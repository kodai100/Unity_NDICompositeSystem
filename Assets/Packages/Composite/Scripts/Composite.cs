using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompositeMode
{
    Additive, Screen, Luminance, Alpha, Bypass
}

public class Composite : MonoBehaviour {

    RenderTexture _inputTexture1;
    RenderTexture _inputTexture2;

    [SerializeField] CompositeMode _compositeMode = CompositeMode.Additive;
    CompositeMode _prevMode;

    [SerializeField] List<Shader> _compositeShaders = new List<Shader>(4);

    [SerializeField] bool _enableDebugView = false;
    [SerializeField] Rect _debugViewRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);

    [SerializeField] InputManager _input1;
    [SerializeField] InputManager _input2;

    Material _compositeMaterial;

    [SerializeField] RenderTexture _texture;
    public RenderTexture GetTexture()
    {
        return _texture;
    }

    public void SetInputTextures(RenderTexture tex1, RenderTexture tex2)
    {
        _inputTexture1 = tex1;
        _inputTexture2 = tex2;
    }
    

	void Start () {

        SetInputTextures(_input1.GetTexture(), _input2.GetTexture());

        _texture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);

        _prevMode = _compositeMode;
        RecreateMaterial(_compositeMode);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_compositeMaterial == null) return;

        if(_compositeMode == CompositeMode.Bypass)
        {
            _texture = _inputTexture2;
            return;
        }
        
        _compositeMaterial.SetTexture("_Layer1Tex", _inputTexture1);
        _compositeMaterial.SetTexture("_Layer2Tex", _inputTexture2);
        

        Graphics.Blit(source, _texture, _compositeMaterial);
    }

    // Update is called once per frame
    void Update () {
		
        if(_prevMode != _compositeMode)
        {
            RecreateMaterial(_compositeMode);
        }

	}

    void RecreateMaterial(CompositeMode mode)
    {
        _compositeMaterial = null;

        switch (mode)
        {
            case CompositeMode.Additive:
                _compositeMaterial = new Material(_compositeShaders[0]);
                break;
            case CompositeMode.Screen:
                _compositeMaterial = new Material(_compositeShaders[1]);
                break;
            case CompositeMode.Luminance:
                _compositeMaterial = new Material(_compositeShaders[2]);
                break;
            case CompositeMode.Alpha:
                _compositeMaterial = new Material(_compositeShaders[3]);
                break;
            default:
                break;
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
