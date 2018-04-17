using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrefsGUI;

interface IDebuggable
{
    void DebugMenu();
}

public class DebugMenu : SingletonMonoBehaviour<DebugMenu> {

    [SerializeField] bool _menuEnabled = false;

    Rect _windowRect = new Rect(0, 0, 300, 200);
    
    [SerializeField] InputManager _inputManager1;
    [SerializeField] InputManager _inputManager2;
    [SerializeField] Composite _compositeManager;
    [SerializeField] OutputManager _outputManager;

    GUIUtil.Folds folds = new GUIUtil.Folds();


	// Use this for initialization
	void Start () {

        folds.Add("Input1", () => {
            _inputManager1.DebugMenu();
        });

        folds.Add("Input2", () =>
        {
            _inputManager2.DebugMenu();
        });

        folds.Add("Composition", () => {
            _compositeManager.DebugMenu();
        });

        folds.Add("Output", () => {
            _outputManager.DebugMenu();
        });

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (_menuEnabled) Prefs.Save();

            _menuEnabled = !_menuEnabled;
        }

	}

    private void OnGUI()
    {
        if (_menuEnabled)
        {
            _windowRect = GUILayout.Window(GetHashCode(), _windowRect, (id) => { OnGUIInternal();  GUI.DragWindow(); }, "Debug Menu");
        }

       
    }

    void OnGUIInternal()
    {
        folds.OnGUI();

        if (GUILayout.Button("Save"))
        {
            Prefs.Save();
        }

    }

}
