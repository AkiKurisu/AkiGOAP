# AkiGOAP Example

1. ``Example1 FSM Like.unity``提供了一个类似于FSM（有限状态机）的AI示例，状态在GOAP被转换为了不同的Goal，当World State改变时Planner会切换当前运行的Goal，即切换不同的状态
2. ``Example2 GOAP Like.unity``提供了一个更像GOAP的AI示例，Goal不变的情况下，由于WorldState变化，Plan的路径规划发生改变

## Eample1说明

```
Goal 1 跟随玩家：
该目标需要Agent拥有能量，当距离小于指定范围后视作完成目标
该目标优先级最高
```
```
Goal 2 恢复能量：
该目标需要Agent没有能量
```

1. 点击开始游戏后，场景内会生成指定数量的Agent，它们会先进入Goal 1跟随玩家并逐渐消耗能量
2. 当能量消耗完后进入Goal 2，移动到Home或Tent，在休息一段时间后恢复满能量，重新进入Goal 1。

## Example2 说明

1. 表现效果同上
2. 仅有一个目标：在玩家附近待机

## 示例脚本说明

- 在Factory中你可以选择AgentPrefab，在Example/Prefab文件夹下有分别使用GOAPPlanner和GOAPPlannerPro的Agent Prefab。如果你不知道两者原理的区别，请阅读```README.md```文档。
- 如果你使用GOAPPlannerPro版本的Agent Prefab, 这些Agent会额外在没有能量时结合位置信息，优先寻找最近的休息点，你可以移动Player来控制它们的行为。

