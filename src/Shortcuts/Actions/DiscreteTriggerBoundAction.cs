﻿using SimpleJSON;

public class DiscreteTriggerBoundAction : TriggerBoundAction, IBoundAction
{
    public const string Type = "discreteTrigger";
    public string type => Type;

    private readonly TriggerActionDiscrete _triggerAction;

    public DiscreteTriggerBoundAction(IPrefabManager prefabManager)
        : base(prefabManager)
    {
        _triggerAction = trigger.CreateDiscreteActionStartInternal();
    }

    public void Invoke()
    {
        _triggerAction.Trigger();
    }

    protected override void Open()
    {
        _triggerAction.OpenDetailPanel();
    }

    public override JSONClass GetJSON()
    {
        return _triggerAction.GetJSON();
    }

    public override void RestoreFromJSON(JSONClass json)
    {
        _triggerAction.RestoreFromJSON(json);
    }
}
