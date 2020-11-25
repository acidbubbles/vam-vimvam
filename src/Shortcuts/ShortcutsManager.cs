﻿using System.Collections.Generic;
using SimpleJSON;

public interface IShortcutsManager
{
}

public class ShortcutsManager : IShortcutsManager
{
    private readonly Atom _containingAtom;
    private readonly PrefabManager _prefabManager;
    public Binding rootBinding { get; } = new Binding();
    private readonly Dictionary<string, IBoundAction> _actions = new Dictionary<string, IBoundAction>();

    public ShortcutsManager(Atom containingAtom, PrefabManager prefabManager)
    {
        _containingAtom = containingAtom;
        _prefabManager = prefabManager;
    }

    public void Add(string name, IBoundAction action)
    {
        _actions.Add(name, action);
    }

    public Binding Add(Binding binding)
    {
        return rootBinding.Add(binding);
    }

    public void Execute(string actionName)
    {
        IBoundAction boundAction;
        if (!_actions.TryGetValue(actionName, out boundAction))
        {
            SuperController.LogError(
                $"Binding was mapped to {actionName} but there was no action matching this name available.");
            return;
        }

        boundAction.Invoke();
    }

    public void Validate()
    {
        foreach (var kvp in _actions)
            kvp.Value.Validate();
    }

    public void SyncAtomNames()
    {
        foreach (var kvp in _actions)
            kvp.Value.SyncAtomNames();
    }

    public JSONClass GetJSON()
    {
        var actionsJSON = new JSONClass();
        foreach (var action in _actions)
        {
            var actionJSON = action.Value.GetJSON();
            if (actionJSON == null) continue;
            actionJSON["__type"] = action.Value.type;
            actionsJSON[action.Key] = actionJSON;
        }

        return actionsJSON;
    }

    public void RestoreFromJSON(JSONClass actionsJSON)
    {
        _actions.Clear();
        foreach (var key in actionsJSON.AsObject.Keys)
        {
            var actionJSON = actionsJSON[key].AsObject;
            IBoundAction action;
            var actionType = actionJSON["__type"];
            SuperController.LogMessage($"{key} = {actionType}: {actionJSON}");
            switch (actionType)
            {
                case DebugBoundAction.Type:
                    action = new DebugBoundAction();
                    break;
                case DiscreteTriggerBoundAction.Type:
                    action = new DiscreteTriggerBoundAction(_prefabManager, _containingAtom);
                    break;
                default:
                    SuperController.LogError($"Unknown action type {actionType}");
                    continue;
            }

            action.RestoreFromJSON(actionJSON);
            _actions.Add(key, action);
        }
    }
}