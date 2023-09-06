# AkiGOAP Example

1. ``Example1 FSM Like.unity`` provides an AI example similar to FSM (Finite State Machine). The state is converted into different goals in GOAP. We switch the currently running goal by switching the World State, that is, switch between different goals. status
2. ``Example2 GOAP Like.unity`` provides a more GOAP-like AI example. When the Goal remains unchanged, the path planning of the Plan changes due to changes in the WorldState

## Eample1 Description

```
Goal 1 Follow Player:
This goal requires the Agent to have energy. When the distance is less than the specified range, the goal is considered to be completed
This target has the highest priority
```
```
Goal 2 Restore Energy:
This goal requires the Agent to have no energy
```

1. After clicking to start the game, a specified number of Agents will be generated in the scene, and they will first enter the Goal 1 to follow the player and gradually consume energy
2. Enter Goal 2 after the energy is exhausted, move to Home or Tent, recover full energy after a period of rest, and re-enter Goal 1.

## Example2 Description

1. The display effect is the same as above
2. Only one goal: Idle Close to Player 

## Example Script Description

- In Factory, you can choose AgentPrefab, and there are Agent Prefabs using GOAPPlanner and GOAPPlannerPro respectively under the Example/Prefab folder. If you don't know the difference between the two principles, please read the ```README.md``` document.
- If you use the Agent Prefab of the GOAPPlannerPro version, these Agents will additionally combine location information when there is no energy, and give priority to finding the nearest rest point, and you can move the Player to control their behavior.