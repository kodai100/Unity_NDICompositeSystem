using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Ndi;
using Klak.Spout;

public enum OutputMode
{
    NDI, Spout, None
}

public class OutputManager : MonoBehaviour {

    [SerializeField] string _senderName;
    [SerializeField] NdiSender _ndiSender;
    [SerializeField] SpoutSender _spoutSender;

    [SerializeField] Composite _composite;

    [SerializeField] OutputMode _outputMode = OutputMode.NDI;

    RenderTexture texture;

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
    }
}
