﻿using System;
using System.Collections.Generic;

public class CommonActionsPlugin : MVRScript, IActionsProvider
{
    private readonly List<object> _actions = new List<object>();

    public override void Init()
    {
        _actions.Clear();

        CreateTextField(new JSONStorableString(
            "Description",
            "This plugin is used for bindings. It offers additional shortcuts not otherwise available using Virt-A-Mate triggers."
        ));

        // Logging
        CreateActionWithParam("LogMessage", SuperController.LogMessage);
        CreateAction(ActionNames.ClearMessageLog, SuperController.singleton.ClearMessages);
        CreateActionWithParam("LogError", SuperController.LogError);
        CreateAction(ActionNames.ClearErrorLog, SuperController.singleton.ClearErrors);

        // Main menu
        CreateAction(ActionNames.SaveScene, SuperController.singleton.SaveSceneDialog);
        CreateAction(ActionNames.LoadScene, SuperController.singleton.LoadSceneDialog);
        CreateAction(ActionNames.MergeLoadScene, SuperController.singleton.LoadMergeSceneDialog);
        CreateAction(ActionNames.Exit, SuperController.singleton.Quit);

        // Selection
        CreateActionWithChoice("SelectAtom",
            val => SuperController.singleton.SelectController(SuperController.singleton.GetAtomByUid(val)
                .freeControllers[0]),
            () => SuperController.singleton.GetAtomUIDs()
        );

        // Broadcast
        BroadcastingUtil.BroadcastActionsAvailable(this);
    }

    private void CreateAction(string jsaName, JSONStorableAction.ActionCallback fn)
    {
        var jsa = new JSONStorableAction(jsaName, fn);
        RegisterAction(jsa);
        _actions.Add(jsa);
    }

    private void CreateActionWithParam(string jssName, Action<string> fn)
    {
        var jss = new JSONStorableString(jssName, null)
        {
            isStorable = false,
            isRestorable = false
        };
        RegisterString(jss);
        jss.setCallbackFunction = val =>
        {
            fn(val);
            jss.valNoCallback = null;
        };
    }

    private void CreateActionWithChoice(string jssName, Action<string> fn, Func<List<string>> genChoices)
    {
        var choices = genChoices();
        var jss = new JSONStorableStringChooser(jssName, choices, null, jssName)
        {
            isStorable = false,
            isRestorable = false
        };
        RegisterStringChooser(jss);
        jss.setCallbackFunction = val =>
        {
            fn(val);
            jss.valNoCallback = null;
        };
        jss.popupOpenCallback += () => jss.choices = genChoices();
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        foreach (var action in _actions)
        {
            bindings.Add(action);
        }
    }
}
