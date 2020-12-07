﻿using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Events;

namespace CustomActions
{
    public interface IActionsRepository : IEnumerable
    {
        UnityEvent onChange { get; }
        IBoundAction AddDiscreteTrigger();
        void Remove(IBoundAction action);
    }

    public class ActionsRepository : IActionsRepository
    {
        public UnityEvent onChange { get; } = new UnityEvent();
        public int count => _actions.Count;

        private readonly Atom _containingAtom;
        private readonly IPrefabManager _prefabManager;
        private readonly List<IBoundAction> _actions = new List<IBoundAction>();

        public ActionsRepository(Atom containingAtom, IPrefabManager prefabManager)
        {
            _containingAtom = containingAtom;
            _prefabManager = prefabManager;
        }

        public IBoundAction AddDiscreteTrigger()
        {
            var action = new DiscreteTriggerBoundAction(_containingAtom, _prefabManager);
            _actions.Add(action);
            onChange.Invoke();
            return action;
        }

        public void Remove(IBoundAction action)
        {
            _actions.Remove(action);
            onChange.Invoke();
        }

        public void Validate()
        {
            foreach (var action in _actions)
                action.Validate();
        }

        public void SyncAtomNames()
        {
            foreach (var action in _actions)
                action.SyncAtomNames();
        }

        public JSONNode GetJSON()
        {
            var actionsJSON = new JSONArray();
            foreach (var action in _actions)
            {
                var actionJSON = action.GetJSON();
                if (actionJSON == null) continue;
                actionJSON["__type"] = action.type;
                actionsJSON.Add(actionJSON);
            }

            return actionsJSON;
        }

        public void RestoreFromJSON(JSONNode actionsJSON)
        {
            if ((actionsJSON?.Count ?? 0) == 0) return;
            _actions.Clear();
            foreach (JSONClass actionJSON in actionsJSON.AsArray)
            {
                IBoundAction action;
                var actionType = actionJSON["__type"];
                switch (actionType)
                {
                    case DebugBoundAction.Type:
                        action = new DebugBoundAction();
                        break;
                    case DiscreteTriggerBoundAction.Type:
                        action = new DiscreteTriggerBoundAction(_containingAtom, _prefabManager);
                        break;
                    default:
                        SuperController.LogError($"Unknown action type {actionType}");
                        continue;
                }

                action.RestoreFromJSON(actionJSON);
                _actions.Add(action);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}
