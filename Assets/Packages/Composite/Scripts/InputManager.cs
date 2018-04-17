using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Ndi;
using Klak.Spout;
using PrefsGUI;

public enum InputMode
{
    NDI, Spout, Bypass
}

[System.Serializable]
public class PrefsInputMode : PrefsParam<InputMode>
{
    public PrefsInputMode(string key, InputMode defaultValue = default(InputMode)) : base(key, defaultValue) { }
}

public class InputManager : MonoBehaviour, IDebuggable {

    [SerializeField] NdiReceiver _ndiReceiever;
    [SerializeField] SpoutReceiver _spoutReceiever;

    [SerializeField] PrefsBool _enableDebugView = new PrefsBool("Enable Debug View", false);
    [SerializeField] PrefsRect _debugViewRect = new PrefsRect("Debug View Rect", new Rect(0, 0, Screen.width / 2, Screen.height / 2));

    [SerializeField] PrefsVector2Int _resolution = new PrefsVector2Int("Resolution", new Vector2Int(1920, 1080));

    [SerializeField] PrefsInputMode _inputMode = new PrefsInputMode("Input Mode", InputMode.NDI);
    [SerializeField] PrefsString _nameFilter = new PrefsString("Name Filter", "UnitySender");
    [SerializeField] PrefsRect _crop = new PrefsRect("Crop Rect (UV)", new Rect(0, 0, 1, 1));

    RenderTexture _texture;

    public RenderTexture GetTexture()
    {
        return _texture;
    }

    public void DebugMenu()
    {
        _enableDebugView.OnGUI();
        _debugViewRect.OnGUI();
        _resolution.OnGUI();
        _inputMode.OnGUI();
        _nameFilter.OnGUI();
        _crop.OnGUI();
    }
    
	void Start () {
        _texture = new RenderTexture(_resolution.Get().x, _resolution.Get().y, 24, RenderTextureFormat.ARGB32);

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
            GUI.DrawTexture(_debugViewRect, _texture, ScaleMode.StretchToFill, false);
        }
        
    }

    private void OnDestroy()
    {
        _texture.Release();
    }
}
