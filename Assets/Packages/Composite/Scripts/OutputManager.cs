using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Ndi;
using Klak.Spout;
using PrefsGUI;

public enum OutputMode
{
    NDI, Spout, None
}

[System.Serializable]
public class PrefsOutputMode : PrefsParam<OutputMode>
{
    public PrefsOutputMode(string key, OutputMode defaultValue = default(OutputMode)) : base(key, defaultValue) { }
}

public class OutputManager : MonoBehaviour, IDebuggable {

    [SerializeField] PrefsString _senderName = new PrefsString("Sender Name", "UnityCompositeSender");
    [SerializeField] PrefsBool _alphaSupport = new PrefsBool("Alpha Support", false);

    [SerializeField] NdiSender _ndiSender;
    [SerializeField] SpoutSender _spoutSender;

    [SerializeField] Composite _composite;

    [SerializeField] PrefsOutputMode _outputMode = new PrefsOutputMode("Output Mode", OutputMode.NDI);

    RenderTexture texture;

    private void Awake()
    {
        _spoutSender.senderName = _senderName;
        _ndiSender.senderName = _senderName;
    }

    public void DebugMenu()
    {
        _senderName.OnGUI();
        _alphaSupport.OnGUI();
        _outputMode.OnGUI();
    }

    // Use this for initialization
    void Start () {
        _ndiSender.sourceTexture = _composite.GetTexture();
    }
	
	// Update is called once per frame
	void Update () {

        if (_outputMode == OutputMode.Spout)
        {
            _ndiSender.enabled = false;
            _spoutSender.enabled = true;
        }
        else if (_outputMode == OutputMode.NDI)
        {
            _ndiSender.enabled = true;
            _spoutSender.enabled = false;
        }
        else
        {
            _ndiSender.enabled = false;
            _spoutSender.enabled = false;
        }

        _ndiSender.alphaSupport = _alphaSupport;
        _spoutSender.clearAlpha = !_alphaSupport;
    }
}
