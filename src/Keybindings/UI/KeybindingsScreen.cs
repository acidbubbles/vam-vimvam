﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingsScreen : MonoBehaviour
{
    private const string _notBoundButtonLabel = "";

    private class CommandBindingRow
    {
        public string commandName;
        public GameObject container;
        public UIDynamicButton bindingBtn;
    }

    public IPrefabManager prefabManager { get; set; }
    public IKeyMapManager keyMapManager { get; set; }
    public IAnalogMapManager analogMapManager { get; set; }
    public RemoteCommandsManager remoteCommandsManager { get; set; }
    public KeybindingsStorage storage { get; set; }
    public IKeybindingsSettings settings { get; set; }

    public bool isRecording;
    private readonly List<CommandBindingRow> _rows = new List<CommandBindingRow>();
    private Coroutine _setKeybindingCoroutine;

    public void Configure()
    {
        var layoutElement = gameObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 100;
        layoutElement.minHeight = 100;

        var group = gameObject.AddComponent<VerticalLayoutGroup>();
        group.spacing = 10f;
        group.padding = new RectOffset(10, 10, 10, 10);

        var bg = gameObject.AddComponent<Image>();
        bg.raycastTarget = false;
        bg.color = new Color(0.721f, 0.682f, 0.741f);
    }

    public void Awake()
    {
        var optionsTitle = prefabManager.CreateText(transform, "<b>Options</b>");
        optionsTitle.fontSize = 32;
        optionsTitle.alignment = TextAnchor.MiddleCenter;

        var showKeypresses = prefabManager.CreateToggle(transform, "Show key presses as you type them (desktop)");
        showKeypresses.backgroundColor = Color.clear;
        settings.showKeyPressesJSON.toggle = showKeypresses.toggle;

        prefabManager.CreateSpacer(transform, 10f);

        var importTitle = prefabManager.CreateText(transform, "<b>Import / Export</b>");
        importTitle.fontSize = 32;
        importTitle.alignment = TextAnchor.MiddleCenter;

        var importSubtitle = prefabManager.CreateText(transform, "<i>Import an existing scheme, and use 'Make default' to save your current mappings.</i>");
        importSubtitle.alignment = TextAnchor.UpperCenter;
        importSubtitle.GetComponent<LayoutElement>().preferredHeight = 70;

        var toolbarGo = new GameObject();
        toolbarGo.transform.SetParent(transform, false);

        var group = toolbarGo.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 20;

        var importBtn = prefabManager.CreateButton(toolbarGo.transform, "Import (overwrite)");
        importBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenImportDialog(true); } });

        var addBtn = prefabManager.CreateButton(toolbarGo.transform, "Import (add)");
        addBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenImportDialog(false); } });

        var exportBtn = prefabManager.CreateButton(toolbarGo.transform, "Export");
        exportBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.OpenExportDialog(); } });

        var makeDefaultBtn = prefabManager.CreateButton(toolbarGo.transform, "Make default");
        makeDefaultBtn.button.onClick.AddListener(() => { if (!isRecording) { storage.ExportDefault(); } });

        prefabManager.CreateSpacer(transform, 10f);

        var keybindingsTitle = prefabManager.CreateText(transform, "<b>Keybindings</b>");
        keybindingsTitle.fontSize = 32;
        keybindingsTitle.alignment = TextAnchor.MiddleCenter;

        var keybindingsSubtitle = prefabManager.CreateText(transform, "<i>You can configure custom trigger shortcuts in the CustomCommands plugin</i>");
        keybindingsSubtitle.alignment = TextAnchor.UpperCenter;
        keybindingsSubtitle.GetComponent<LayoutElement>().preferredHeight = 30;

        var noteSubtitle = prefabManager.CreateText(transform, "<i>Commands with an <color=blue>asterisk</color> can be bound to joysticks and mouse axis</i>");
        noteSubtitle.alignment = TextAnchor.UpperCenter;
        noteSubtitle.GetComponent<LayoutElement>().preferredHeight = 50;

        {
            var searchGo = new GameObject();
            searchGo.transform.SetParent(transform, false);

            var layout = searchGo.AddComponent<LayoutElement>();
            layout.preferredHeight = 40f;

            var searchBackground = searchGo.AddComponent<Image>();
            searchBackground.raycastTarget = false;
            searchBackground.color = Color.white;

            var searchFieldGo = new GameObject();
            searchFieldGo.transform.SetParent(searchGo.transform, false);

            var searchFieldRect = searchFieldGo.AddComponent<RectTransform>();
            searchFieldRect.anchorMin = new Vector2(0f, 0f);
            searchFieldRect.anchorMax = new Vector2(1f, 1f);
            searchFieldRect.sizeDelta = new Vector2(1f, 1f);

            var text = searchFieldGo.AddComponent<Text>();
            text.font = prefabManager.font;
            text.color = Color.black;
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleLeft;

            var placeholderGo = new GameObject();
            placeholderGo.transform.SetParent(searchGo.transform, false);

            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0f, 0f);
            placeholderRect.anchorMax = new Vector2(1f, 1f);
            placeholderRect.sizeDelta = new Vector2(1f, 1f);

            var placeholderText = placeholderGo.AddComponent<Text>();
            placeholderText.raycastTarget = false;
            placeholderText.font = prefabManager.font;
            placeholderText.color = Color.gray;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.fontSize = 30;
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.text = "Search commands...";

            var input = searchFieldGo.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.onValueChanged.AddListener(OnSearchValueChanged);
        }
    }

    private void OnSearchValueChanged(string query)
    {
        for (var i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            if (row.commandName == null) continue;
            row.container.SetActive(FuzzyFinder.Match(row.commandName, query));
        }
    }

    public void OnEnable()
    {
        CreateRows();

        keyMapManager.onChanged.AddListener(OnKeybindingsChanged);
        analogMapManager.onChanged.AddListener(OnKeybindingsChanged);
    }

    private void CreateRows()
    {
        var unloadedCommands = keyMapManager.maps.Select(m => m.commandName).Concat(analogMapManager.maps.Select(m => m.commandName)).ToList();

        // ReSharper disable RedundantEnumerableCastCall
        var commands = remoteCommandsManager.actionCommands.Cast<ICommandInvoker>().Concat(remoteCommandsManager.analogCommands.Cast<ICommandInvoker>());
        // ReSharper restore RedundantEnumerableCastCall
        foreach (var group in commands.GroupBy(c => c.ns))
        {
            AddGroupRow(@group.Key);

            foreach (var command in @group.OrderBy(g => g.localName))
            {
                AddEditRow(command);
                unloadedCommands.Remove(command.commandName);
            }
        }

        if (unloadedCommands.Count <= 0) return;

        var disabledCommandInvokers = new List<ICommandInvoker>();
        foreach (var unloadedCommand in unloadedCommands)
        {
            var map = ((IMap) keyMapManager.maps.FirstOrDefault(m => m.commandName == unloadedCommand)) ?? analogMapManager.maps.FirstOrDefault(m => m.commandName == unloadedCommand);
            if (map == null) continue;
            disabledCommandInvokers.Add(new DisabledCommandInvoker(unloadedCommand, map.GetPrettyString(), map.slot));
        }

        foreach (var group in disabledCommandInvokers.GroupBy(c => c.ns))
        {
            AddGroupRow($"[unloaded] {@group.Key}");

            foreach (var command in @group)
            {
                AddEditRow(command);
                unloadedCommands.Remove(command.commandName);
            }
        }
    }

    public void OnDisable()
    {
        keyMapManager.onChanged.RemoveListener(OnKeybindingsChanged);
        analogMapManager.onChanged.RemoveListener(OnKeybindingsChanged);

        if (_setKeybindingCoroutine != null) StopCoroutine(_setKeybindingCoroutine);
        _setKeybindingCoroutine = null;
        isRecording = false;
        ClearRows();
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            Destroy(row.container);
        _rows.Clear();
    }

    private void OnKeybindingsChanged()
    {
        // TODO: Instead we could try to just update existing bindings and create/remove rows but this is easier
        ClearRows();
        CreateRows();
    }

    private void AddGroupRow(string groupName)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var row = new CommandBindingRow {container = go};
        _rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 1);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = 70;

        var text = go.AddComponent<Text>();
        text.text = groupName;
        text.color = new Color(0.3f, 0.2f, 0.2f);
        text.raycastTarget = false;
        text.font = prefabManager.font;
        text.fontSize = 30;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
    }

    private void AddEditRow(ICommandInvoker commandInvoker)
    {
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var row = new CommandBindingRow {container = go, commandName = commandInvoker.commandName};
        _rows.Add(row);

        go.transform.SetSiblingIndex(transform.childCount - 1);

        var group = go.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;

        var isAnalog = commandInvoker is IAnalogCommandInvoker;

        var displayNameText = prefabManager.CreateText(go.transform, commandInvoker.localName + (isAnalog ? " *" : ""));
        if (isAnalog) displayNameText.color = new Color(0, 0.3f, 0.4f);
        var displayNameLayout = displayNameText.GetComponent<LayoutElement>();
        displayNameLayout.flexibleWidth = 1000f;

        CreateBindingButton(commandInvoker, go, row, 0);
        CreateBindingButton(commandInvoker, go, row, 1);
    }

    private void CreateBindingButton(ICommandInvoker commandInvoker, GameObject go, CommandBindingRow row, int slot)
    {
        var bindingBtn = prefabManager.CreateButton(go.transform, GetMappedBinding(commandInvoker, slot));
        bindingBtn.button.onClick.AddListener(() =>
        {
            if (isRecording) return;
            if (commandInvoker is IActionCommandInvoker)
                _setKeybindingCoroutine = StartCoroutine(RecordKeys(bindingBtn, commandInvoker, bindingBtn.buttonColor, slot));
            else if (commandInvoker is IAnalogCommandInvoker)
                _setKeybindingCoroutine = StartCoroutine(RecordAnalog(bindingBtn, commandInvoker, bindingBtn.buttonColor, slot));
            bindingBtn.buttonColor = new Color(0.9f, 0.6f, 0.65f);
            bindingBtn.label = "Recording...";
        });
        row.bindingBtn = bindingBtn;
        var bindingLayout = bindingBtn.GetComponent<LayoutElement>();
        bindingLayout.minWidth = 300f;
        bindingLayout.preferredWidth = 300f;
    }

    private void StopRecording(UIDynamicButton btn, Color btnColor, ICommandInvoker commandInvoker, int slot)
    {
        isRecording = false;
        btn.buttonColor = btnColor;
        btn.label = GetMappedBinding(commandInvoker, slot);
        if (_setKeybindingCoroutine != null) StopCoroutine(_setKeybindingCoroutine);
        _setKeybindingCoroutine = null;
    }

    private static readonly string[] _knownAxisNames = new[] {"Mouse X", "Mouse Y", "Mouse ScrollWheel", "LeftStickX", "LeftStickY", "RightStickX", "RightStickY", "Triggers", "DPadX", "DPadY", "Axis6", "Axis7", "Axis8"};

    private IEnumerator RecordAnalog(UIDynamicButton btn, ICommandInvoker commandInvoker, Color btnColor, int slot)
    {
        isRecording = true;
        while (true)
        {
            yield return 0;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var mapped = analogMapManager.GetMapByName(commandInvoker.commandName, slot);
                if (mapped != null)
                    analogMapManager.maps.Remove(mapped);
                break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                break;

            var ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            foreach (var axisName in _knownAxisNames)
            {
                if (Mathf.Abs(Input.GetAxis(axisName)) < 0.75f) continue;
                var key = KeyCodes.bindableKeyCodes.GetCurrent();
                // We don't want to take over the mouse!
                if (axisName.StartsWith("Mouse") && key == KeyCode.None && !ctrlDown && !shiftDown && !altDown) continue;
                var binding = new KeyChord(key, ctrlDown, altDown, shiftDown);

                var previousMap = analogMapManager.maps.FirstOrDefault(m => m.commandName == commandInvoker.commandName);
                if (previousMap != null)
                {
                    remoteCommandsManager.UpdateValue(previousMap.commandName, 0);
                    analogMapManager.maps.Remove(previousMap);
                }
                var conflictMap = analogMapManager.maps.FirstOrDefault(m => m.chord.Equals(binding) && m.axisName == axisName);
                if (conflictMap != null)
                {
                    remoteCommandsManager.UpdateValue(conflictMap.commandName, 0);
                    analogMapManager.maps.Remove(conflictMap);
                    var conflictRow = _rows.FirstOrDefault(r => r.commandName == conflictMap.commandName);
                    if (conflictRow != null)
                        conflictRow.bindingBtn.label = _notBoundButtonLabel;
                    SuperController.LogError($"Keybindings: Reassigned binding from {conflictMap.commandName} to {commandInvoker.commandName}");
                }
                analogMapManager.maps.Add(new AnalogMap(binding, axisName, commandInvoker.commandName, slot));
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }
        }

        StopRecording(btn, btnColor, commandInvoker, slot);
    }

    private IEnumerator RecordKeys(UIDynamicButton btn, ICommandInvoker commandInvoker, Color btnColor, int slot)
    {
        isRecording = true;
        var expire = float.MaxValue;
        var setKeybindingList = new List<KeyChord>();
        while (Time.unscaledTime < expire)
        {
            yield return 0;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // Apply
                ApplyRecordedKeybinding(setKeybindingList, commandInvoker, slot);
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Cancel
                StopRecording(btn, btnColor, commandInvoker, slot);
                yield break;
            }

            var key = KeyCodes.bindableKeyCodes.GetCurrentDown();
            if (key == KeyCode.None) continue;

            var ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var binding = new KeyChord(key, ctrlDown, altDown, shiftDown);
            setKeybindingList.Add(binding);
            btn.label = setKeybindingList.GetKeyChordsAsString();
            expire = Time.unscaledTime + Settings.TimeoutLen;
        }
        ApplyRecordedKeybinding(setKeybindingList, commandInvoker, slot);
        StopRecording(btn, btnColor, commandInvoker, slot);
    }

    private void ApplyRecordedKeybinding(List<KeyChord> setKeybindingList, ICommandInvoker commandInvoker, int slot)
    {
        if (setKeybindingList.Count > 0)
        {
            var bindings = setKeybindingList.ToArray();
            var previousMap = keyMapManager.maps.FirstOrDefault(m => m.commandName == commandInvoker.commandName);
            if (previousMap != null)
                keyMapManager.maps.Remove(previousMap);
            var conflictMap = keyMapManager.maps.FirstOrDefault(m => m.chords.SameBinding(bindings));
            if (conflictMap != null)
            {
                keyMapManager.maps.Remove(conflictMap);
                var conflictRow = _rows.FirstOrDefault(r => r.commandName == conflictMap.commandName);
                if (conflictRow != null)
                    conflictRow.bindingBtn.label = _notBoundButtonLabel;
                SuperController.LogError($"Keybindings: Reassigned binding from {conflictMap.commandName} to {commandInvoker.commandName}");
            }
            keyMapManager.maps.Add(new KeyMap(bindings, commandInvoker.commandName, slot));
            keyMapManager.RebuildTree();
        }
        else
        {
            var mapped = keyMapManager.GetMapByName(commandInvoker.commandName, slot);
            if (mapped != null)
                keyMapManager.maps.Remove(mapped);
        }
    }

    private string GetMappedBinding(ICommandInvoker commandInvoker, int slot)
    {
        if (commandInvoker is IActionCommandInvoker)
        {
            var mapped = keyMapManager.GetMapByName(commandInvoker.commandName, slot);
            return mapped?.GetPrettyString() ?? _notBoundButtonLabel;
        }

        if (commandInvoker is IAnalogCommandInvoker)
        {
            var mapped = analogMapManager.GetMapByName(commandInvoker.commandName, slot);
            return mapped?.GetPrettyString() ?? _notBoundButtonLabel;
        }

        var disabledCommandInvoker = commandInvoker as DisabledCommandInvoker;
        if (disabledCommandInvoker != null && disabledCommandInvoker.slot == slot)
        {
            return disabledCommandInvoker.prettyString;
        }

        return _notBoundButtonLabel;
    }
}
