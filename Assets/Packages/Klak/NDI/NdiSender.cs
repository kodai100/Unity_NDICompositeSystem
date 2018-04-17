// KlakNDI - NDI plugin for Unity
// https://github.com/keijiro/KlakNDI

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Klak.Ndi
{
    public class NdiSender : MonoBehaviour
    {
        #region Source texture

        [SerializeField] string _senderName;
        public string senderName {
            get { return _senderName; }
            set { _senderName = value; }
        }

        [SerializeField] RenderTexture _sourceTexture;

        public RenderTexture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Format option

        [SerializeField] bool _alphaSupport;

        public bool alphaSupport {
            get { return _alphaSupport; }
            set { _alphaSupport = value; }
        }

        #endregion

        #region Conversion shader

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;
        RenderTexture _converted;
         
        #endregion

        #region Frame readback queue

        struct Frame
        {
            public int width, height;
            public bool alpha;
            public AsyncGPUReadbackRequest readback;
        }

        Queue<Frame> _frameQueue;

        void QueueFrame(RenderTexture source)
        {
            if (_frameQueue.Count > 3)
            {
                Debug.LogWarning("Too many GPU readback requests.");
                return;
            }

            // Return the old render texture to the pool.
            if (_converted != null) RenderTexture.ReleaseTemporary(_converted);

            // Allocate a new render texture.
            _converted = RenderTexture.GetTemporary(
                source.width / 2, (_alphaSupport ? 3 : 2) * source.height / 2, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
            );

            // Apply the conversion shader.
            Graphics.Blit(source, _converted, _material, _alphaSupport ? 1 : 0);

            // Request readback.
            _frameQueue.Enqueue(new Frame{
                width = source.width, height = source.height,
                alpha = _alphaSupport,
                readback = AsyncGPUReadback.Request(_converted)
            });
        }

        #endregion

        #region Misc variables

        IntPtr _plugin;
        bool _hasCamera;

        #endregion

        #region MonoBehaviour implementation

        IEnumerator Start()
        {
            _material = new Material(_shader);
            _frameQueue = new Queue<Frame>(4);
            _plugin = PluginEntry.NDI_CreateSender(_senderName);
            _hasCamera = (GetComponent<Camera>() != null);

            // Synchronize with async send at the end of every frame.
            var wait = new WaitForEndOfFrame();
            while (true)
            {
                yield return wait;
                if (enabled) PluginEntry.NDI_SyncSender(_plugin);
            }
        }

        void OnDestroy()
        {
            Destroy(_material);
            if (_converted != null) RenderTexture.ReleaseTemporary(_converted);
            PluginEntry.NDI_DestroySender(_plugin);
        }

        void Update()
        {
            // Process the readback queue.
            while (_frameQueue.Count > 0)
            {
                var frame = _frameQueue.Peek();

                if (frame.readback.hasError)
                {
                    Debug.LogWarning("GPU readback error was detected.");
                    _frameQueue.Dequeue();
                }
                else if (frame.readback.done)
                {
                    var array = frame.readback.GetData<Byte>();
                    unsafe {
                        PluginEntry.NDI_SendFrame(
                            _plugin, (IntPtr)array.GetUnsafeReadOnlyPtr(),
                            frame.width, frame.height,
                            frame.alpha ? FourCC.UYVA : FourCC.UYVY
                        );
                    }
                    _frameQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            // Request frame readback when in render texture mode.
            if (!_hasCamera && _sourceTexture != null) QueueFrame(_sourceTexture);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            QueueFrame(_sourceTexture);
            Graphics.Blit(_sourceTexture, destination);
        }

        #endregion
    }
}
