using System;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class NodeMenuAction : DropdownMenuAction
    {
        public NodeMenuAction(string actionName, Action<DropdownMenuAction> actionCallback, Func<DropdownMenuAction, Status> actionStatusCallback, object userData = null) : base(actionName, actionCallback, actionStatusCallback, userData)
        {
        }
    }
}
