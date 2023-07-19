using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPNodeStack : StackNode
    {
        public GOAPNodeStack()
        {
            capabilities&=~Capabilities.Copiable;
            capabilities &=~Capabilities.Deletable;//不可删除
            capabilities &=~Capabilities.Movable;//不可删除
            headerContainer.style.flexDirection=FlexDirection.Row;
            headerContainer.style.justifyContent=Justify.Center;
        }
    }
}
