# KartRacer_Chase

玩家和AI卡丁车手竞速的游戏
玩家在规定的赛道内和不断成长的AI车手竞速
玩家可培养自己的AI车手与别人培养的AI车手竞速

## 下一步要做

- 加入ArcadeKart能够普通驾驶
- 搭建简单赛道(拥有赛道外墙壁 使赛车无法穿越)
- 自动检测墙壁及赛道的简单AI驾驶框架车辆
- 开始进行自动驾驶训练(时间越少奖励越多)
- 扩展ArcadeKart的操作(漂移 手刹)
- 修复赛道内Prefab 制作更多赛道
- 中断后继续训练等训练指令及参数记录文档
- AI车手可以互相竞速训练
- 将场景中环境(比如树和楼房替换为Prefab) 并将整体环境制作为Prefab
- 简化ArcadeKart控制脚本
- 扩展车辆高级控制(赛车竞速策略)
- 扩展更多种类车辆(麦克纳姆轮胎车辆 平板小推车 滑板车 快递车 外卖电动车 飞思卡尔小车)
- 人类选手和AI组队(AI可以有不同的策略 比如倒着开专门辅助人类选手撞对方车辆 )
- 人类选手辅助训练(人跑AI追 人辅助AI追 AI和人类一起追)

---

Sensor|占用|解释
--|:--:|--
Car type|1|0~1 每种车辆一个数值(卡丁车0.1 SUV0.2)
Car forward|3|车身的方向
Agent velocity|3|车辆当前的速度
射线检测器(每个)|3+1+1+1|检测器指向角度+碰撞距离+车身边缘+检测到的碰撞类型(墙壁 对方车辆 己方车辆0.3f)
比赛完成度|1|0~1
前面一辆车的比赛完成度|1|0~1
后面一辆车的比赛完成度|1|0~1
当前排名|1|赛车数量-当前排名-1/赛车数量
目标排名|1|赛车数量-目标排名-1/赛车数量
当前道路方向|4|道路位置(当前位置 0)+道路方向
前面一段道路方向|4|道路位置(向前检测线总长一半位置 0.5)+道路方向
再前面一段道路方向|4|道路位置(向前检测线总长位置 1)+道路方向

### 高级车辆可在行车过程中动态规划传感器检测方向以及调整检测不同位置的路面方向

- 射线检测器动态扫描
  - 车辆前进方向速度越大 地面坡度检测器检测范围越远
  - 一个射线检测器不停地绕车旋转(可包含上下角度变化)
- 停车时根据车辆方向与当前路面方向夹角调整检测路面方向的位置
  - 路面方向与车厢方向夹角绝对值 [0,45°]       ` 0   0.5   1 `
  - 路面方向与车厢方向夹角绝对值 (45°,135°)    ` 0   0.5  -0.5 `
  - 路面方向与车厢方向夹角绝对值 [135°,180°]   ` 0  -0.5  -1 `

>- 甚至由第二个AI来辅助协助动态调整检测器方向和路面位置

---

## 疑问

- 对于车辆射线检测器的位置设置
  1. 将多个射线检测器的起始位置设置为同一个位置 调整不同的角度 并设置不同的碰撞阈值(如果多个检测器包含了全部车身 可以有自己多大的概念)
  2. 将多个射线检测器的起始位置设置在不同的地方 调整不同的角度 当碰撞距离为0时算为阈值(没有自己大小的概念)
