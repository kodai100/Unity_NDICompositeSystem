using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Ndi;
using Klak.Spout;

public enum InputMode
{
    NDI, Spout, Bypass
}

public class InputManager : MonoBehaviour {

    [SerializeField] NdiReceiver _ndiReceiever;
    [SerializeField] SpoutReceiver _spoutReceiever;

    [SerializeField] bool _enableDebugView = false;
    [SerializeField] Rect _debugViewRext = new Rect(0, 0, Screen.width / 2, Screen.height / 2);

    [SerializeField] Vector2Int _resolution = new Vector2Int(1920, 1080);

    [SerializeField] InputMode _inputMode = InputMode.NDI;
    [SerializeField] string _nameFilter = "UnitySender";
    [SerializeField] Rect _crop;

    RenderTexture _texture;

    public RenderTexture GetTexture()
    {
        return _texture;
    }
    
	void Start () {
        _texture = new RenderTexture(_resolution.x, _resolution.y, 24, RenderTextureFormat.ARGB32);

        _ndiReceiever.targetTexture = _texture;
        _spoutReceiever.targetTexture = _texture;
	}
	
	// Update is called once per frame
	void Update () {

        _ndiReceiever.nameFilter = _nameFilter;
        _spoutReceiever.nameFilter = _nameFilter;

        if(_inputMode == InputMode.Spout)
        {
            _ndiReceiever.enabled = false;
            _spoutReceiever.enabled = true;
        }
        else if(_inputMode == InputMode.NDI)
        {
            _ndiReceiever.enabled = true;
            _spoutReceiever.enabled = false;
        }
        else
        {
            _ndiReceiever.enabled = false;
            _spoutReceiever.enabled = false;
        }


		
	}

    void RecreateTexture(int width, int height)
    {
        if(_texture != null)
        {
            _texture.Release();
            _texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            _ndiReceiever.targetTexture = _texture;
            _spoutReceiever.targetTexture = _texture;
        }
    }

    private void OnGUI()
    {
        if (_enableDebugView)
        {
            GUI.DrawTexture(_debugViewRext, _texture, ScaleMode.StretchToFill, false);
        }
        
    }

    private void OnDestroy()
    {
        _texture.Release();
    }
}
