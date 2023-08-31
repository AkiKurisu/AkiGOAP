# AkiGOAP Example

Example provides a simple usage example

## Instructions for use

Among them, the Factory will generate batches of Agents, and the goals and actions of these Agents have been defined in the ExampleSet.

```
Goal 1 follows the player:
This goal requires the Agent to have energy. When the distance is less than the specified range, the goal is considered to be completed
This target has the highest priority

Goal 2 restore energy:
This goal requires the agent to have no energy
```

1. After clicking to start the game, a specified number of Agents will be generated in the scene, and they will first enter the Goal 1 to follow the player and gradually consume energy
2. Enter Goal 2 after the energy is exhausted, move to Home or Tent, recover full energy after a period of rest, and re-enter Goal 1.


- In Factory, you can choose Agent Prefab, and there are Agent Prefabs using GOAPPlanner and GOAPPlannerPro respectively under the Example/Prefab folder. If you don't know the difference between the two principles, please read the ```README.md``` document.
- If you use the Agent Prefab of the GOAPPlannerPro version, these Agents will additionally combine location information when there is no energy, and give priority to finding the nearest rest point, and you can move the Player to control their behavior.