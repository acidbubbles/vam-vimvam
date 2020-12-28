﻿using System;
using System.Collections;
using System.Linq;
using DefaultNamespace;
using SimpleJSON;
using UnityEngine.UI;

public class GlobalCommands
{
    private readonly JSONStorable _owner;
    private readonly Atom _containingAtom;
    private readonly ISelectionHistoryManager _selectionHistoryManager;
    private readonly RemoteCommandsManager _remoteCommandsManager;
    private JSONStorableFloat _moveX;
    private JSONStorableFloat _moveY;
    private JSONStorableFloat _moveZ;
    private JSONStorableFloat _moveCameraX;
    private JSONStorableFloat _moveCameraY;
    private JSONStorableFloat _moveCameraZ;
    private JSONStorableFloat _rotateX;
    private JSONStorableFloat _rotateY;
    private JSONStorableFloat _rotateZ;
    private JSONStorableFloat _cameraOrbitX;
    private JSONStorableFloat _cameraOrbitY;
    private JSONStorableFloat _cameraDollyZoom;
    private JSONStorableFloat _cameraPanX;
    private JSONStorableFloat _cameraPanY;

    public GlobalCommands(JSONStorable owner, Atom containingAtom, ISelectionHistoryManager selectionHistoryManager, RemoteCommandsManager remoteCommandsManager)
    {
        _owner = owner;
        _containingAtom = containingAtom;
        _selectionHistoryManager = selectionHistoryManager;
        _remoteCommandsManager = remoteCommandsManager;
    }

    public void Init()
    {
        // Mode
        CreateAction("GameMode", "PlayMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Play);
        CreateAction("GameMode", "EditMode", () => SuperController.singleton.gameMode = SuperController.GameMode.Edit);

        // Main menu
        CreateAction("Global", "SaveScene", SuperController.singleton.SaveSceneDialog);
        CreateAction("Global", "LoadScene", SuperController.singleton.LoadSceneDialog);
        CreateAction("Global", "MergeLoadScene", SuperController.singleton.LoadMergeSceneDialog);
        CreateAction("Global", "Exit", SuperController.singleton.Quit);
        CreateAction("Global", "ScreenshotMode", SuperController.singleton.SelectModeScreenshot);
        CreateAction("Global", "OnlineBrowser", () => SuperController.singleton.activeUI = SuperController.ActiveUI.OnlineBrowser);
        CreateAction("Global", "OpenMainMenu", () => SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu);
        CreateAction("Global", "ToggleShowHiddenAtoms", SuperController.singleton.ToggleShowHiddenAtoms);

        // Menu
        CreateAction("AtomUI", "CloseAllPanels", CloseAllPanels);
        CreateAction("AtomUI", "Open", () => OpenTab(null));
        CreateAction("AtomUI", "ControlTab", () => OpenTab(type => type == "Person" ? "ControlAndPhysics1" : "Control"));
        CreateAction("AtomUI", "PresetTab", () => OpenTab(_ => "Preset"));
        CreateAction("AtomUI", "MoveTab", () => OpenTab(_ => "Move"));
        CreateAction("AtomUI", "AnimationTab", () => OpenTab(_ => "Animation"));
        CreateAction("AtomUI", "PhysicsControlTab", () => OpenTab(_ => "Physics Control"));
        CreateAction("AtomUI", "PhysicsObjectTab", () => OpenTab(_ => "Physics Object"));
        CreateAction("AtomUI", "CollisionTriggerTab", () => OpenTab(_ => "Collision Trigger"));
        CreateAction("AtomUI", "MaterialTab", () => OpenTab(_ => "Material"));
        CreateAction("AtomUI", "PluginsTab", () => OpenTab(_ => "Plugins"));
        CreateAction("AtomUI", "PluginsTab_Plugin#0", () => OpenPlugin(0));
        CreateAction("AtomUI", "PluginsTab_Plugin#1", () => OpenPlugin(1));
        CreateAction("AtomUI", "PluginsTab_Plugin#2", () => OpenPlugin(2));
        CreateAction("AtomUI", "PluginsTab_Plugin#3", () => OpenPlugin(3));
        CreateAction("AtomUI", "PluginsTab_Plugin#4", () => OpenPlugin(4));
        CreateAction("AtomUI", "PluginsTab_Plugin#5", () => OpenPlugin(5));
        CreateAction("AtomUI", "PluginsTab_Plugin#6", () => OpenPlugin(6));
        CreateAction("AtomUI", "PluginsTab_Plugin#7", () => OpenPlugin(7));
        CreateAction("AtomUI", "PluginsTab_Plugin#8", () => OpenPlugin(8));
        CreateAction("AtomUI", "PluginsTab_Plugin#9", () => OpenPlugin(9));
        // Animation Pattern
        CreateAction("AtomUI", "AnimationPatternTab", () => OpenTab(_ => "Animation Pattern", "AnimationPattern"));
        CreateAction("AtomUI", "AnimationTriggersTab", () => OpenTab(_ => "Animation Triggers", "AnimationPattern"));
        // Custom Unity Asset
        CreateAction("AtomUI", "AssetTab", () => OpenTab(_ => "Asset", "CustomUnityAsset"));
        // Audio Source
        CreateAction("AtomUI", "AudioSourceTab", () => OpenTab(_ => "Audio Source", "AudioSource"));
        // Person
        CreateAction("AtomUI", "ClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("AtomUI", "ClothingTab", () => OpenTab(_ => "Clothing", "Person"));
        CreateAction("AtomUI", "HairTab", () => OpenTab(_ => "Hair", "Person"));
        CreateAction("AtomUI", "AppearancePresetsTab", () => OpenTab(_ => "Appearance Presets", "Person"));
        CreateAction("AtomUI", "GeneralPresetsTab", () => OpenTab(_ => "General Presets", "Person"));
        CreateAction("AtomUI", "PosePresetsTab", () => OpenTab(_ => "Pose Presets", "Person"));
        CreateAction("AtomUI", "SkinPresetsTab", () => OpenTab(_ => "Skin Presets", "Person"));
        CreateAction("AtomUI", "PluginsPresetsTab", () => OpenTab(_ => "Plugins Presets", "Person"));
        CreateAction("AtomUI", "MorphsPresetsTab", () => OpenTab(_ => "Morphs Presets", "Person"));
        CreateAction("AtomUI", "HairPresetsTab", () => OpenTab(_ => "Hair Presets", "Person"));
        CreateAction("AtomUI", "ClothingPresetsTab", () => OpenTab(_ => "Clothing Presets", "Person"));
        CreateAction("AtomUI", "MaleMorphsTab", () => OpenTab(_ => "Male Morphs", "Person"));
        CreateAction("AtomUI", "FemaleMorphsTab", () => OpenTab(_ => "Female Morphs", "Person"));

        // Main Tabs
        CreateAction("MainUI", "Open", () => OpenMainTab(null));
        CreateAction("MainUI", "FileTab", () => OpenMainTab("TabFile"));
        CreateAction("MainUI", "UserPrefsTab", () => OpenMainTab("TabUserPrefs"));
        CreateAction("MainUI", "NavigationTab", () => OpenMainTab("TabNavigation"));
        CreateAction("MainUI", "SelectTab", () => OpenMainTab("TabSelect"));
        CreateAction("MainUI", "SessionPluginPresetsTab", () => OpenMainTab("TabSessionPluginPresets"));
        CreateAction("MainUI", "SessionPluginsTab", () => OpenMainTab("TabSessionPlugins"));
        CreateAction("MainUI", "ScenePluginsTab", () => OpenMainTab("TabScenePlugins"));
        CreateAction("MainUI", "ScenePluginPresetsTab", () => OpenMainTab("TabScenePluginPresets"));
        CreateAction("MainUI", "SceneLightingTab", () => OpenMainTab("TabSceneLighting"));
        CreateAction("MainUI", "SceneMiscTab", () => OpenMainTab("TabSceneMisc"));
        CreateAction("MainUI", "SceneAnimationTab", () => OpenMainTab("TabAnimation"));
        CreateAction("MainUI", "AddAtomTab", () => OpenMainTab("TabAddAtom"));
        CreateAction("MainUI", "AudioTab", () => OpenMainTab("TabAudio"));
        // CreateAction("OpenMainMenuDebugTab", () => OpenMainTab("TabDebug"));
        // TODO: Next/Previous tab

        // Selection
        CreateAction("Select", "Deselect", () => SuperController.singleton.SelectController(null));
        CreateAction("Select", "HistoryBack", SelectHistoryBack);
        CreateAction("Select", "PreviousAtom", () => SelectPreviousAtom());
        CreateAction("Select", "NextAtom", () => SelectNextAtom());
        CreateAction("Select", "PreviousPersonAtom", () => SelectPreviousAtom("Person"));
        CreateAction("Select", "NextPersonAtom", () => SelectNextAtom("Person"));
        CreateAction("Select", "RootControl", () => SelectControllerByName("control"));
        CreateAction("Select", "HipControl", () => SelectControllerByName("hipControl"));
        CreateAction("Select", "PelvisControl", () => SelectControllerByName("pelvisControl"));
        CreateAction("Select", "ChestControl", () => SelectControllerByName("chestControl"));
        CreateAction("Select", "HeadControl", () => SelectControllerByName("headControl"));
        CreateAction("Select", "RHandControl", () => SelectControllerByName("rHandControl"));
        CreateAction("Select", "LHandControl", () => SelectControllerByName("lHandControl"));
        CreateAction("Select", "RFootControl", () => SelectControllerByName("rFootControl"));
        CreateAction("Select", "LFootControl", () => SelectControllerByName("lFootControl"));
        CreateAction("Select", "NeckControl", () => SelectControllerByName("neckControl"));
        CreateAction("Select", "EyeTargetControl", () => SelectControllerByName("eyeTargetControl"));
        CreateAction("Select", "RNippleControl", () => SelectControllerByName("rNippleControl"));
        CreateAction("Select", "LNippleControl", () => SelectControllerByName("lNippleControl"));
        CreateAction("Select", "TestesControl", () => SelectControllerByName("testesControl"));
        CreateAction("Select", "PenisBaseControl", () => SelectControllerByName("penisBaseControl"));
        CreateAction("Select", "PenisMidControl", () => SelectControllerByName("penisMidControl"));
        CreateAction("Select", "PenisTipControl", () => SelectControllerByName("penisTipControl"));
        CreateAction("Select", "RElbowControl", () => SelectControllerByName("rElbowControl"));
        CreateAction("Select", "LElbowControl", () => SelectControllerByName("lElbowControl"));
        CreateAction("Select", "RKneeControl", () => SelectControllerByName("rKneeControl"));
        CreateAction("Select", "LKneeControl", () => SelectControllerByName("lKneeControl"));
        CreateAction("Select", "RToeControl", () => SelectControllerByName("rToeControl"));
        CreateAction("Select", "LToeControl", () => SelectControllerByName("lToeControl"));
        CreateAction("Select", "AbdomenControl", () => SelectControllerByName("abdomenControl"));
        CreateAction("Select", "Abdomen2Control", () => SelectControllerByName("abdomen2Control"));
        CreateAction("Select", "RThighControl", () => SelectControllerByName("rThighControl"));
        CreateAction("Select", "LThighControl", () => SelectControllerByName("lThighControl"));
        CreateAction("Select", "LArmControl", () => SelectControllerByName("lArmControl"));
        CreateAction("Select", "RArmControl", () => SelectControllerByName("rArmControl"));
        CreateAction("Select", "RShoulderControl", () => SelectControllerByName("rShoulderControl"));
        CreateAction("Select", "LShoulderControl", () => SelectControllerByName("lShoulderControl"));

        // Dev
        CreateAction("Plugins", "ReloadAllScenePlugins", ReloadAllScenePlugins);

        // Edit atom
        CreateAction("Atom", "EnableCollisions", () => OnSelectedAtom(atom => atom.collisionEnabled = true));
        CreateAction("Atom", "DisableCollisions", () => OnSelectedAtom(atom => atom.collisionEnabled = true));

        // Add atom
        CreateAction("Add", "AnimationPattern", () => SuperController.singleton.AddAtomByType("AnimationPattern", true, true, true));
        CreateAction("Add", "FloorsAndWalls_AtomSlate", () => SuperController.singleton.AddAtomByType("Slate", true, true, true));
        CreateAction("Add", "FloorsAndWalls_AtomWall", () => SuperController.singleton.AddAtomByType("Wall", true, true, true));
        CreateAction("Add", "FloorsAndWalls_AtomWoodPanel", () => SuperController.singleton.AddAtomByType("WoodPanel", true, true, true));
        CreateAction("Add", "Force_CycleForce", () => SuperController.singleton.AddAtomByType("CycleForce", true, true, true));
        CreateAction("Add", "Force_GrabPoint", () => SuperController.singleton.AddAtomByType("GrabPoint", true, true, true));
        CreateAction("Add", "Force_RhythmForce", () => SuperController.singleton.AddAtomByType("RhythmForce", true, true, true));
        CreateAction("Add", "Force_SyncForce", () => SuperController.singleton.AddAtomByType("SyncForce", true, true, true));
        CreateAction("Add", "Light_InvisibleLight", () => SuperController.singleton.AddAtomByType("InvisibleLight", true, true, true));
        CreateAction("Add", "Misc_ClothGrabSphere", () => SuperController.singleton.AddAtomByType("ClothGrabSphere", true, true, true));
        CreateAction("Add", "Misc_CustomUnityAsset", () => SuperController.singleton.AddAtomByType("CustomUnityAsset", true, true, true));
        CreateAction("Add", "Misc_Empty", () => SuperController.singleton.AddAtomByType("Empty", true, true, true));
        CreateAction("Add", "Misc_ImagePanel", () => SuperController.singleton.AddAtomByType("ImagePanel", true, true, true));
        CreateAction("Add", "Misc_SimpleSign", () => SuperController.singleton.AddAtomByType("SimpleSign", true, true, true));
        CreateAction("Add", "Misc_SubScene", () => SuperController.singleton.AddAtomByType("SubScene", true, true, true));
        CreateAction("Add", "Misc_UIText", () => SuperController.singleton.AddAtomByType("UIText", true, true, true));
        CreateAction("Add", "Misc_VaMLogo", () => SuperController.singleton.AddAtomByType("VaMLogo", true, true, true));
        CreateAction("Add", "Misc_WebBrowser", () => SuperController.singleton.AddAtomByType("WebBrowser", true, true, true));
        CreateAction("Add", "Misc_WebPanel", () => SuperController.singleton.AddAtomByType("WebPanel", true, true, true));
        CreateAction("Add", "People_Person", () => SuperController.singleton.AddAtomByType("Person", true, true, true));
        CreateAction("Add", "Reflective_Glass", () => SuperController.singleton.AddAtomByType("Glass", true, true, true));
        CreateAction("Add", "Reflective_ReflectiveSlate", () => SuperController.singleton.AddAtomByType("ReflectiveSlate", true, true, true));
        CreateAction("Add", "Reflective_ReflectiveWoodPanel", () => SuperController.singleton.AddAtomByType("ReflectiveWoodPanel", true, true, true));
        CreateAction("Add", "Shapes_Cube", () => SuperController.singleton.AddAtomByType("Cube", true, true, true));
        CreateAction("Add", "Shapes_Sphere", () => SuperController.singleton.AddAtomByType("Sphere", true, true, true));
        CreateAction("Add", "Shapes_Capsule", () => SuperController.singleton.AddAtomByType("Capsule", true, true, true));
        CreateAction("Add", "Sound_AudioSource", () => SuperController.singleton.AddAtomByType("AudioSource", true, true, true));
        CreateAction("Add", "Toys_Dildo", () => SuperController.singleton.AddAtomByType("Dildo", true, true, true));
        CreateAction("Add", "Triggers_CollisionTrigger", () => SuperController.singleton.AddAtomByType("CollisionTrigger", true, true, true));
        CreateAction("Add", "Triggers_LookAtTrigger", () => SuperController.singleton.AddAtomByType("LookAtTrigger", true, true, true));
        CreateAction("Add", "Triggers_UIButton", () => SuperController.singleton.AddAtomByType("UIButton", true, true, true));
        CreateAction("Add", "Triggers_UISlider", () => SuperController.singleton.AddAtomByType("UISlider", true, true, true));
        CreateAction("Add", "Triggers_UIToggle", () => SuperController.singleton.AddAtomByType("UIToggle", true, true, true));
        CreateAction("Add", "Triggers_VariableTrigger", () => SuperController.singleton.AddAtomByType("VariableTrigger", true, true, true));
        CreateAction("Add", "Clone_CurrentAtom", () => _owner.StartCoroutine(CloneCurrentAtom()));

        // Remove atom
        CreateAction("Remove", "SelectedAtom", () => SuperController.singleton.RemoveAtom(SuperController.singleton.GetSelectedAtom()));

        // Edit controller
        CreateAction("Controller", "PositionState_On", () => OnSelectedController(c => c.currentPositionState = FreeControllerV3.PositionState.On));
        CreateAction("Controller", "PositionState_Off", () => OnSelectedController(c => c.currentPositionState = FreeControllerV3.PositionState.Off));
        CreateAction("Controller", "RotationState_On", () => OnSelectedController(c => c.currentRotationState = FreeControllerV3.RotationState.On));
        CreateAction("Controller", "RotationState_Off", () => OnSelectedController(c => c.currentRotationState = FreeControllerV3.RotationState.Off));
        CreateAction("Controller", "PositionAndRotationState_On", () => OnSelectedController(c => { c.currentPositionState = FreeControllerV3.PositionState.On; c.currentRotationState = FreeControllerV3.RotationState.On; }));
        CreateAction("Controller", "PositionAndRotationState_Off", () => OnSelectedController(c => { c.currentPositionState = FreeControllerV3.PositionState.Off; c.currentRotationState = FreeControllerV3.RotationState.Off; }));

        // Animation
        CreateAction("Animations", "StartPlayback", () => SuperController.singleton.motionAnimationMaster.StartPlayback());
        CreateAction("Animations", "StopPlayback", () => SuperController.singleton.motionAnimationMaster.StopPlayback());
        CreateAction("Animations", "Reset", () => SuperController.singleton.motionAnimationMaster.ResetAnimation());
        // TODO: Add more options

        // Time
        CreateAction("Time", "TimeScale_Set_Normal", () => TimeControl.singleton.currentScale = 1f);
        CreateAction("Time", "TimeScale_Set_Half", () => TimeControl.singleton.currentScale = 0.5f);
        CreateAction("Time", "TimeScale_Set_Quarter", () => TimeControl.singleton.currentScale = 0.25f);
        CreateAction("Time", "TimeScale_Set_Minimum", () => TimeControl.singleton.currentScale = 0.1f);
        CreateAction("Time", "Toggle_FreezeMotionAndSound", () => SuperController.singleton.freezeAnimationToggle.isOn = !SuperController.singleton.freezeAnimationToggle.isOn);

        // Movement
        _moveX = CreateAnalog("Move", "Move_Absolute_X");
        _moveY = CreateAnalog("Move", "Move_Absolute_Y");
        _moveZ = CreateAnalog("Move", "Move_Absolute_Z");
        _moveCameraX = CreateAnalog("Move", "Move_RelativeToCamera_X");
        _moveCameraY = CreateAnalog("Move", "Move_RelativeToCamera_Y");
        _moveCameraZ = CreateAnalog("Move", "Move_RelativeToCamera_Z");
        _rotateX = CreateAnalog("Rotate", "Rotate_Absolute_X");
        _rotateY = CreateAnalog("Rotate", "Rotate_Absolute_Y");
        _rotateZ = CreateAnalog("Rotate", "Rotate_Absolute_Z");

        // Camera
        CreateAction("Camera", "FocusOnSelectedController", () => SuperController.singleton.FocusOnSelectedController());
        _cameraPanX = CreateAnalog("Camera", "Pan_X");
        _cameraPanY = CreateAnalog("Camera", "Pan_Y");
        _cameraOrbitX = CreateAnalog("Camera", "Orbit_X");
        _cameraOrbitY = CreateAnalog("Camera", "Orbit_Y");
        _cameraDollyZoom = CreateAnalog("Camera", "DollyZoom");

        // Logging
        CreateAction("Logs", "ClearMessageLog", SuperController.singleton.ClearMessages);
        CreateAction("Logs", "ClearErrorLog", SuperController.singleton.ClearErrors);
        CreateAction("Logs", "OpenErrorLog", SuperController.singleton.OpenErrorLogPanel);
        CreateAction("Logs", "OpenMessageLog", SuperController.singleton.OpenMessageLogPanel);
        CreateAction("Logs", "CloseErrorLog", SuperController.singleton.CloseErrorLogPanel);
        CreateAction("Logs", "CloseMessageLog", SuperController.singleton.CloseMessageLogPanel);
        CreateAction("Logs", "ToggleErrorLog", () => SuperController.singleton.errorLogPanel.gameObject.SetActive(!SuperController.singleton.errorLogPanel.gameObject.activeSelf));
        CreateAction("Logs", "ToggleMessageLog", () => SuperController.singleton.msgLogPanel.gameObject.SetActive(!SuperController.singleton.msgLogPanel.gameObject.activeSelf));

        // Settings
        CreateAction("Settings", "TogglePerformanceMonitor", TogglePerformanceMonitor);
        // TODO: Got permission from LFE to check out what he thought off, take a look and make sure to double-credit him! :)
    }

    private static void OnSelectedAtom(Action<Atom> fn)
    {
        var atom = SuperController.singleton.GetSelectedAtom();
        if (atom == null) return;
        fn(atom);
    }

    private static void OnSelectedController(Action<FreeControllerV3> fn)
    {
        var fc = SuperController.singleton.GetSelectedController();
        if (fc == null) return;
        fn(fc);
    }

    private const float _moveMultiplier = 1f; // TODO: Based on camera distance from model?
    private const float _rotateMultiplier = 1f;

    public void Update()
    {
        if(_moveX.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.X, _moveX.val * _moveMultiplier);
        if(_moveY.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.Y, _moveY.val * _moveMultiplier);
        if(_moveZ.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.Z, _moveZ.val * _moveMultiplier);
        if(_moveCameraX.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.CameraRight, _moveCameraX.val * _moveMultiplier);
        if(_moveCameraY.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.CameraUp, -_moveCameraY.val * _moveMultiplier);
        if(_moveCameraZ.val != 0) SuperController.singleton.GetSelectedController()?.MoveAxis(FreeControllerV3.MoveAxisnames.CameraForward, _moveCameraZ.val * _moveMultiplier);
        if(_rotateX.val != 0) SuperController.singleton.GetSelectedController()?.RotateAxis(FreeControllerV3.RotateAxisnames.X, _rotateX.val * _rotateMultiplier);
        if(_rotateY.val != 0) SuperController.singleton.GetSelectedController()?.RotateAxis(FreeControllerV3.RotateAxisnames.Y, _rotateY.val * _rotateMultiplier);
        if(_rotateZ.val != 0) SuperController.singleton.GetSelectedController()?.RotateAxis(FreeControllerV3.RotateAxisnames.Z, _rotateZ.val * _rotateMultiplier);
        if (_cameraPanX.val != 0) SuperController.singleton.CameraPan(_cameraPanX.val, -SuperController.singleton.MonitorCenterCamera.transform.right);
        if (_cameraPanY.val != 0) SuperController.singleton.CameraPan(_cameraPanY.val, SuperController.singleton.MonitorCenterCamera.transform.up);
        if (_cameraDollyZoom.val != 0) SuperController.singleton.CameraDollyZoom(_cameraDollyZoom.val);
        if (_cameraOrbitX.val != 0) SuperController.singleton.CameraOrbitX(_cameraOrbitX.val);
        if (_cameraOrbitY.val != 0) SuperController.singleton.CameraOrbitY(_cameraOrbitY.val);
    }

    private static void TogglePerformanceMonitor()
    {
        var toggle = UserPreferences.singleton.transform
            .GetComponentsInChildren<Toggle>(true)?
            .FirstOrDefault(c => c.name == "PerfMon Toggle");
        if (toggle == null) return;
        toggle.isOn = !toggle.isOn;
    }

    private void CreateAction(string ns, string jsaName, Action fn)
    {
        _remoteCommandsManager.Add(new ActionCommandInvoker(_owner, ns, jsaName, fn));
    }

    private JSONStorableFloat CreateAnalog(string ns, string jsfName)
    {
        var jsf = new JSONStorableFloat(jsfName, 0f, -1f, 1f);
        _remoteCommandsManager.Add(new JSONStorableFloatCommandInvoker(_owner, ns, jsfName, jsf));
        return jsf;
    }

    private void ReloadAllScenePlugins()
    {
        foreach (var atom in SuperController.singleton.GetAtoms().Where(a => !ReferenceEquals(a, _containingAtom)))
        {
            if (atom.UITransform == null) continue;
            if (atom.UITransform
                .GetChild(0)
                .ReloadPlugins("Canvas", "Plugins", null))
                continue;
            foreach (var script in atom
                .GetStorableIDs()
                .Select(id => atom.GetStorableByID(id))
                .OfType<MVRScript>())
            {
                atom.RestoreFromLast(script);
            }
        }
    }

    private static void CloseAllPanels()
    {
        SuperController.singleton.activeUI = SuperController.ActiveUI.None;
        SuperController.singleton.CloseMessageLogPanel();
        SuperController.singleton.CloseErrorLogPanel();
    }

    private static void OpenMainTab(string tabName)
    {
        SuperController.singleton.SetActiveUI("MainMenu");
        if (tabName != null)
            SuperController.singleton.SetMainMenuTab(tabName);
    }

    private Atom OpenTab(Func<string, string> getTabName, string type = null)
    {
        var selectedAtom = _selectionHistoryManager.GetLastSelectedAtomOfType(type);
        if (ReferenceEquals(selectedAtom, null)) return null;

        SuperController.singleton.SelectController(selectedAtom.mainController);
        SuperController.singleton.SetActiveUI("SelectedOptions");

        var tabName = getTabName?.Invoke(selectedAtom.type);
        if (tabName == null) return null;

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>(true);
        if (selector == null) return null;

        /*
        foreach (Transform t in selector.toggleContainer)
            SuperController.LogMessage(t.name);
        */
        selector.SetActiveTab(tabName);
        return selectedAtom;
    }

    private void OpenPlugin(int i)
    {
        var atom = OpenTab(_ => "Plugins");
        if (atom == null) return;
        var uid = atom.GetStorableIDs().FirstOrDefault(s => s.StartsWith($"plugin#{i}_"));
        if (uid == null) return;
        var plugin = atom.GetStorableByID(uid) as MVRScript;
        if (plugin == null) return;
        plugin.UITransform.gameObject.SetActive(true);
    }

    private void SelectHistoryBack()
    {
        var selectedController = SuperController.singleton.GetSelectedController();
        var mainController = SuperController.singleton.GetSelectedAtom()?.mainController;
        if (selectedController != mainController)
        {
            SuperController.singleton.SelectController(mainController);
            return;
        }

        if (_selectionHistoryManager.history.Count <= 1) return;

        SuperController.singleton.SelectController(_selectionHistoryManager.history[_selectionHistoryManager.history.Count - 2].mainController);
        _selectionHistoryManager.history.RemoveAt(_selectionHistoryManager.history.Count - 1);
    }

    private static void SelectPreviousAtom(string type = null)
    {
        var atoms = SuperController.singleton
            .GetAtoms()
            .Where(a => type == null || a.type == type)
            .Where(a => a.mainController != null)
            .ToList();
        if (atoms.Count == 0) return;
        var index = atoms.IndexOf(SuperController.singleton.GetSelectedAtom());
        if (index == -1)
        {
            SuperController.singleton.SelectController(atoms[atoms.Count - 1].mainController);
            return;
        }
        if (index == 0)
        {
            SuperController.singleton.SelectController(atoms[atoms.Count - 1].mainController);
            return;
        }
        SuperController.singleton.SelectController(atoms[index - 1].mainController);
    }

    private void SelectNextAtom(string type = null)
    {
        var atoms = SuperController.singleton
            .GetAtoms()
            .Where(a => type == null || a.type == type)
            .Where(a => a.mainController != null)
            .ToList();
        if (atoms.Count == 0) return;
        var index = atoms.IndexOf(SuperController.singleton.GetSelectedAtom());
        if (index == -1)
        {
            SuperController.singleton.SelectController(atoms[0].mainController);
            return;
        }
        if (index == atoms.Count - 1)
        {
            SuperController.singleton.SelectController(atoms[0].mainController);
            return;
        }
        SuperController.singleton.SelectController(atoms[index + 1].mainController);
    }

    private void SelectControllerByName(string controllerName)
    {
        for (var i = _selectionHistoryManager.history.Count - 1; i >= 0; i--)
        {
            var atom = _selectionHistoryManager.history[i];

            var controller = atom.freeControllers.FirstOrDefault(fc => fc.name == controllerName);
            if (controller != null)
            {
                SuperController.singleton.SelectController(controller);
                return;
            }
        }

        foreach(var atom in SuperController.singleton.GetAtoms())
        {
            var controller = atom.freeControllers.FirstOrDefault(fc => fc.name == controllerName);
            if (controller != null)
            {
                SuperController.singleton.SelectController(controller);
                return;
            }
        }
    }

    private static IEnumerator CloneCurrentAtom()
    {
        var atom = SuperController.singleton.GetSelectedAtom();
        if (atom == null) yield break;
        if (atom.type == null) yield break;
        var json = new JSONArray();
        atom.Store(json);
        var uid = SuperController.singleton.CreateUID(atom.uid);
        var enumerator = SuperController.singleton.AddAtomByType(atom.type, uid, true, true, false);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
        var clone = SuperController.singleton.GetAtomByUid(uid);
        if (clone == null) throw new NullReferenceException($"Could not create new atom for clone for type '{atom.type}'");
        SuperController.LogMessage(json.ToString());
        if (json.Count == 1)
        {
            var jc = json[0].AsObject;
            clone.ClearParentAtom();
            clone.RestoreTransform(jc);
            clone.RestoreParentAtom(jc);
            clone.Restore(jc, true, true, true, null, true);
            clone.LateRestore(jc);
            clone.PostRestore();
        }
        clone.collisionEnabled = false;
    }
}
