using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class NodeResolver
    {
        private const string NodeStyleSheetPath = "AkiGOAP/Node";
        private StyleSheet styleSheetCache;
        public GOAPNode CreateNodeInstance(Type type, GOAPView view)
        {
            GOAPNode node;
            if (type.IsSubclassOf(typeof(GOAPGoal)))
            {
                node = new GOAPGoalNode();
            }
            else
            {
                node = new GOAPActionNode();
            }
            node.SetBehavior(type, view);
            if (styleSheetCache == null) styleSheetCache = Resources.Load<StyleSheet>(NodeStyleSheetPath);
            node.styleSheets.Add(styleSheetCache);
            return node;
        }
    }
}
