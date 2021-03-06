﻿public class JSONStorableActionCommandInvoker : CommandInvokerBase, IActionCommandInvoker
{
    private readonly JSONStorableAction _action;

    public JSONStorableActionCommandInvoker(JSONStorable storable, string ns, string localName, JSONStorableAction action)
        : base(storable, ns, localName)
    {
        _action = action;
    }

    public ICommandReleaser Invoke()
    {
        _action.actionCallback.Invoke();
        return null;
    }
}
