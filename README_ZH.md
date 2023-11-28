# AkiGOAP

AkiGOAP是一个支持可视化、模块化编辑，支持多线程的Goal Oriented Action Planner（目标导向的行为规划）Unity插件，同时集成了多个开源GOAP插件的功能。

AkiGOAP is a Goal Oriented Action Planner unity plugin that supports visualization, modular editing, and multi-threading, which integrates the functions of multiple open source GOAP plugins.
## 特点 Features

1. 多线程, 使用Unity Job System

2. 可视化编辑器

<img src="Images/GraphEditor.png" />

- 支持Runtime时Debug

<img src="Images/GraphEditorDebug.png"/>

3. 自定义输出决策细节

<img src="Images/Log.png" />

4. 决策快照

<img src="Images/SnapShot.png" />


## 支持的版本

* Unity 2021.3 or Later

## 安装
1. 在Unity PackageManager中输入Git URL下载 ```https://github.com/AkiKurisu/AkiGOAP.git```
#

## 如何使用 How To Use

由于GOAP AI的设计需要一定门槛，我只介绍如何使用插件的核心功能，具体的设计可以参考插件提供的Example样例。

1. 在Asset文件夹内右键菜单```Create/AkiGOAP/GOAPSet```创建GOAPSet
2. 点击```Open GOAP Editor```打开编辑器
3. 右键创建Goal结点或Action结点，将两种结点分别拖入```GOAP Goal Stack```和```Action Stack```中
4. 创建GameObject挂载GOAPPlanner或GOAPPlannerPro，同时会挂载GOAPWorldState
5. 编写Agent脚本，示例如下：
    ```c#
    using UnityEngine;
    using UnityEngine.AI;
    using System.Linq;
    namespace Kurisu.GOAP.Example
    {
        public class ExampleAgent : MonoBehaviour
        {
            //此处的IPlanner也可以为GOAPPlanner或GOAPPlannerPro
            private IPlanner planner;
            private NavMeshAgent navMeshAgent;
            public NavMeshAgent NavMeshAgent=>navMeshAgent;
            [SerializeField]
            private GOAPSet dataSet;
            [SerializeField]
            internal Transform player;
            public Transform Player=>player;
            private void Start() {
                navMeshAgent=GetComponent<NavMeshAgent>();
                planner=GetComponent<IPlanner>();
                //你可以通关继承并使用Linq的Cast或OfType获取自定义的子类并进行依赖的注入
                var goals=dataSet.GetGoals();
                foreach(var goal in goals.OfType<ExampleGoal>())
                {
                    goal.Inject(this);
                }
                var actions=dataSet.GetActions();
                foreach(var action in actions.OfType<ExampleAction>())
                {
                    action.Inject(this);
                }
                //最后你需要将Goal和Action注入Planner中
                planner.InjectGoals(goals);
                planner.InjectActions(actions);
            }
        }
    }

    ```
6. 在上述GameObejct上挂载Agent脚本，并拖入之前制作的GOAPSet
7. 点击Play，在Start时所有Goal和Action均会获取其依赖进行初始化
8. 点击GOAPPlanner或GOAPPlannerPro的```Open GOAP Editor```打开编辑器查看当前所有Goal的Priority优先级和所有Action的Cost代价
9. 点击GOAPPlanner或GOAPPlannerPro的```Open Planner Snapshot```打开快照查看当前的Plan计划（即抵达当前Goal的一串Action序列）

## 说明 Explanation
1. GOAPPlanner没有使用JobSystem，仅使用对象池减少了GC开销，适用于复杂度较低的任务，优化自 https://github.com/toastisme/OpenGOAP 

    特点：根据优先级最高的Goal搜索路径，没有搜索到则FallBack至下一优先级的Goal

2. GOAPPlannerPro使用了JobSystem，参考自 https://github.com/crashkonijn/GOAP ，创建的Job可以将Position加入Cost计算，使用样例如下：
    ```C#
    using UnityEngine;
    namespace Kurisu.GOAP.Example
    {
        public class GoToHome : ExampleAction
        {
            protected override void SetupDerived()
            {
                //注册该结点所绑定的Transform
                worldState.RegisterNodeTarget(this,agent.Home);
            }
        }
    }
    ```
    特点：将所有Goal和Action加入搜索的图，为每一个Goal创建Job进行路径搜索。根据优先级采纳搜索结果。

3. 关于GOAPPlanner和GOAPPlannerPro的```TickType```，由于GOAP使用比较费，我们可以考虑在不需要它的时候关闭Plan的搜索。勾选```ManualUpdateGoal```则Goal的更新都变更为手动调用。勾选```ManualActivatePlanner```则Planner不再自动搜索Plan，需要手动调用```ManualActivate()```激活，并且Planner在激活后第一次失去Plan时再次关闭。该选项适用于一些回合制游戏，通常这些游戏的AI仅需在特定回合或特定时间段进行Plan的搜索。
4. 关于GOAPPlannerPro的```Skip Search When Action Running```, Pro版本默认会在每帧获取全部Goal情况下的Plan（即Action序列）, 典型例子为：目标A需要一个物品，而获得物品需要先进行移动行为B再进行采集行为C。搜索到当前的Action为B后，AI将进行B行为。如果要让AI在B完成后进行采集行为C，我们应当在B完成后通知Planner重新搜索或者每帧进行搜索。如果勾选该选项，Planner就会在拥有Action时不再搜索，你需要让Action主动关闭自己，例如使该Action处于Precondition不满足的状态。