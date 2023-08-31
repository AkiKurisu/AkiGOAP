# AkiGOAP Example

Example提供了一个简单的使用样例

## 使用说明

其中Factory会生成批量Agent，这些Agent的目标Goal和行为Action已经定义在ExampleSet中。

```
目标1 跟随玩家：
该目标需要Agent拥有能量，当距离小于指定范围后视作完成目标
该目标优先级最高

目标2 恢复能量：
该目标需要Agent没有能量
```

1. 点击开始游戏后，场景内会生成指定数量的Agent，它们会先进入目标1跟随玩家并逐渐消耗能量
2. 当能量消耗完后进入目标2，移动到Home或Tent，在休息一段时间后恢复满能量，重新进入目标1。


- 在Factory中你可以选择AgentPrefab，在Example/Prefab文件夹下有分别使用GOAPPlanner和GOAPPlannerPro的Agent Prefab。如果你不知道两者原理的区别，请阅读```README.md```文档。
- 如果你使用GOAPPlannerPro版本的Agent Prefab, 这些Agent会额外在没有能量时结合位置信息，优先寻找最近的休息点，你可以移动Player来控制它们的行为。

