# AkiGOAP

AkiGOAP is a Goal Oriented Action Planner unity plugin that supports visualization, modular editing, and multi-threading, which integrates the functions of multiple open source GOAP plugins.

## Features

1. Multi-thread, using Unity Job System

2. Visual Graph Editor

<img src="Images/GraphEditor.png" />

- Allow debug in runtime play mode

<img src="Images/GraphEditorDebug.png"/>

3. Log detail with customized level

<img src="Images/Log.png" />

4. Take snapShot of current plan searching

<img src="Images/SnapShot.png" />

## Supported version

* Unity 2021.3 or Later

## Set Up
1. Using git URL to download package by Unity PackageManager ```https://github.com/AkiKurisu/AkiGOAP.git```
#

## How To Use

Since the design of GOAP AI requires a certain threshold, I only introduce how to use the core functions of the plugin. For the specific design, please refer to the Example sample provided by the plugin.
1. In the Asset folder, right-click the menu ```Create/AkiGOAP/GOAPSet``` to create a GOAPSet
2. Click ```Open GOAP Editor``` to open the editor
3. Right-click to create a Goal node or Action node, and drag the two nodes into ```GOAP Goal Stack``` and ```Action Stack``` respectively
4. Create a GameObject to mount GOAPPlanner or GOAPPlannerPro, and also mount GOAPWorldState
5. Write the Agent script, the example is as follows:
    ```c#
    using UnityEngine;
    using UnityEngine.AI;
    using System.Linq;
    namespace Kurisu.GOAP.Example
    {
        public class ExampleAgent : MonoBehaviour
        {
            //You can use GOAPPlanner or GOAPPlannerPro
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
                //You can pass inheritance and use Linq's Cast or OfType to get custom subclasses and perform dependency injection
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
                //Finally you need to inject Goal and Action into Planner
                planner.InjectGoals(goals);
                planner.InjectActions(actions);
            }
        }
    }

    ```
6. Mount the Agent script on the GameObject above, and drag in the previously created GOAPSet
7. Click Play, and all Goals and Actions will obtain their dependencies for initialization at Start
8. Click ```Open GOAP Editor``` of GOAPPlanner or GOAPPlannerPro to open the editor to view the Priority of all current Goals and the Cost of all Actions
9. Click ```Open Planner Snapshot``` of GOAPPlanner or GOAPPlannerPro to open the snapshot to view the current Plan plan (that is, a series of Action sequences that reach the current Goal)

## Explanation
1. GOAPPlanner does not use JobSystem, only uses object pool to reduce GC overhead, suitable for tasks with low complexity, optimized from https://github.com/toastisme/OpenGOAP

     Features: According to the Goal search path with the highest priority, if not found, FallBack to the Goal with the next priority

2. GOAPPlannerPro uses JobSystem, refer to https://github.com/crashkonijn/GOAP , the created Job can add Position to Cost calculation, the usage example is as follows:
    ```C#
    using UnityEngine;
    namespace Kurisu.GOAP.Example
    {
        public class GoToHome : ExampleAction
        {
            protected override void SetupDerived()
            {
                //Register Transform bound to this node
                worldState.RegisterNodeTarget(this,agent.Home);
            }
        }
    }
    ```
    Features: Add all Goals and Actions to the search graph, and create a Job for each Goal to search for paths. Feed search results based on priority.

3. Regarding ```TickType``` of GOAPPlanner and GOAPPlannerPro, since GOAP is relatively expensive to use, we can consider turning off the search of Plan when it is not needed. Check ```ManualUpdateGoal``` to change the Goal update to manual call. Check ```ManualActivatePlanner```, the Planner will no longer automatically search for the plan, you need to manually call ```ManualActivate()``` to activate, and the planner will be closed again when it loses the plan for the first time after activation. This option is suitable for some turn-based games. Usually, the AI of these games only needs to search for a plan in a specific round or a specific time period.
   
4. Regarding the ```Skip Search When Action Running``` of GOAPPlannerPro, the Pro version will obtain the Plan (that is, the Action sequence) of all Goal cases in each frame by default. A typical example is: Goal A needs an item, but gets the item It is necessary to perform the movement behavior B first and then the collection behavior C. After finding that the current Action is B, the AI will perform B. If we want AI to perform acquisition behavior C after B is completed, we should notify Planner to search again or search every frame after B is completed. If this option is checked, Planner will no longer search when it has an Action. You need to let the Action close itself actively, for example, make the Action in a state where the Precondition is not satisfied.