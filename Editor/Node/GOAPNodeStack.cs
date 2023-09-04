using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPNodeStack : StackNode
    {
        public GOAPNodeStack()
        {
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.Center;
        }
    }
    public class GOAPActionStack : GOAPNodeStack
    {
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            return element is GOAPActionNode;
        }
    }
    public class GOAPGoalStack : GOAPNodeStack
    {
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            return element is GOAPGoalNode;
        }
    }
}
