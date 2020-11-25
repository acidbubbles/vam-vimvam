using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class Shortcuts : MVRScript
{
    private const float _timeoutLen = 1.0f; // http://vimdoc.sourceforge.net/htmldoc/options.html#'timeoutlen'

    private Binding _rootBindings = new Binding();
    private Binding _current;
    private Coroutine _coroutine;
    private readonly Dictionary<string, IBoundAction> _actions = new Dictionary<string, IBoundAction>();
    private PrefabManager _prefabManager;
    private bool _loaded;

    public override void Init()
    {
        try
        {
            _prefabManager = new PrefabManager();
            StartCoroutine(_prefabManager.LoadUIAssets());
            SuperController.singleton.StartCoroutine(DeferredInit());
            SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;

            // TODO: Replace by a dynamically generated list
            _actions.Add("save", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actions.Add("print.1", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actions.Add("print.2", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actions.Add("print.3", new DiscreteTriggerBoundAction(_prefabManager, containingAtom));
            _actions.Add("print.3.4", new DebugBoundAction("debug 3.4"));
            _actions.Add("print.3.5", new DebugBoundAction("debug 3.5"));

            // TODO: Build from a key mappings JSON
            _rootBindings = new Binding {action = null};
            _rootBindings.Add(new Binding
            {
                modifier = KeyCode.LeftControl,
                key = KeyCode.S,
                action = "save"
            });
            _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha1,
                action = "print.1"
            });
            _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha2,
                action = "print.2"
            });
            var b3 = _rootBindings.Add(new Binding
            {
                key = KeyCode.Alpha3,
                action = "print.3"
            });
            b3.Add(new Binding
            {
                key = KeyCode.Alpha4,
                action = "print.3.4"
            });
            b3.Add(new Binding
            {
                key = KeyCode.Alpha5,
                action = "print.3.5"
            });

            CreateTextField(new JSONStorableString("", "Actions"), false);
            foreach (var action in _actions)
            {
                var a = action;
                CreateButton($"Edit {a.Key}", false).button.onClick.AddListener(() => { a.Value.Edit(); });
            }

            CreateTextField(new JSONStorableString("", "Bindings"), true);
            foreach (var action in _actions)
            {
                var a = action;
                CreateButton($"Map {a.Key}", true).button.onClick.AddListener(() => { SuperController.LogMessage("Not implemented"); });
            }
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(Init)}: {e}");
        }
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if(this == null) yield break;
        if(!_loaded) containingAtom.RestoreFromLast(this);
    }

    public override void InitUI()
    {
        base.InitUI();
        if (UITransform != null)
        {
            _prefabManager.triggerActionsParent = UITransform;
        }
    }

    public void Update()
    {
        try
        {
            if (!Input.anyKeyDown) return;
            if (LookInputModule.singleton.inputFieldActive && !Input.GetKey(KeyCode.LeftControl)) return;

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            var next = (_current ?? _rootBindings).DoMatch();

            if (next == null)
            {
                if (_current != _rootBindings)
                    next = _rootBindings.DoMatch();
                _current = null;
                if (next == null)
                    return;
            }

            if (next.Count == 0)
            {
                if (next.action != null)
                    Execute(next);
                _current = null;
                return;
            }

            _current = next;
            _coroutine = StartCoroutine(TimeoutCoroutine());
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(Update)}: {e}");
        }
    }

    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(_timeoutLen);
        if (_current == null) yield break;
        try
        {
            if (_current.action != null)
            {
                Execute(_current);
                _current = _rootBindings;
            }

            _coroutine = null;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(TimeoutCoroutine)}: {e}");
        }
    }

    public override void Validate()
    {
        base.Validate();
        foreach (var kvp in _actions)
            kvp.Value.Validate();
    }

    public void OnAtomRename(string oldid, string newid)
    {
        foreach (var kvp in _actions)
            kvp.Value.SyncAtomNames();
    }

    public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
    {
        var json = base.GetJSON(includePhysical, includeAppearance, forceStore);

        try
        {
            var actionsJSON = new JSONClass();
            foreach (var action in _actions)
            {
                var actionJSON = action.Value.GetJSON();
                if (actionJSON == null) continue;
                actionJSON["__type"] = action.Value.type;
                actionsJSON[action.Key] = actionJSON;
            }

            json["actions"] = actionsJSON;
            needsStore = true;
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(GetJSON)} (Serialize): {exc}");
        }

        return json;
    }

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

        try
        {
            _loaded = true;
            var actionsJSON = jc["actions"]?.AsObject;
            if ((actionsJSON?.Count ?? 0) == 0) return;
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
                        action = new DiscreteTriggerBoundAction(_prefabManager, containingAtom);
                        break;
                    default:
                        SuperController.LogError($"Unknown action type {actionType}");
                        continue;
                }
                action.RestoreFromJSON(actionJSON);
                _actions.Add(key, action);
            }
        }
        catch (Exception exc)
        {
            SuperController.LogError($"{nameof(Shortcuts)}.{nameof(RestoreFromJSON)}: {exc}");
        }
    }

    public void OnDestroy()
    {
        if (SuperController.singleton != null)
        {
            SuperController.singleton.onAtomUIDRenameHandlers -= OnAtomRename;
        }
    }

    private void Execute(Binding current)
    {
        IBoundAction boundAction;
        if (!_actions.TryGetValue(current.action, out boundAction))
        {
            SuperController.LogError(
                $"Binding was mapped to {current.action} but there was no action matching this name available.");
            return;
        }

        boundAction.Invoke();
    }
}
