# KartRacer_Chase

玩家和AI卡丁车手竞速的游戏
玩家培养自己的AI车手与别人培养的AI车手竞速
多个AI车手互相驾驶
玩家在赛道内驾驶车辆AI车手模仿并学习驾驶

## 下一步要做

- 训练新的车手
- 增加触屏驾驶操作
- 在5个车手时第二和第三在起点前都是第二名的问题
- 菜单界面增加设置训练信息是否显示等参数
- 调整ArcadeKart碰到墙壁后马上按住前进会有动作延迟
- 增加行驶中获得奖励及用时UI显示(记录数据并能够恢复查看)
  - 单圈时间 总时间
  - 当前奖励 总奖励
  - 查看当前操作(开车的油门刹车方向)
- 根据车辆速度快慢控制摄像机远近
- 赛道序列的训练进程(赛道循环 赛道随机)
- 调整墙壁碰撞效果(前速 反弹 碰撞检测到的类型)
- 增加编辑赛道
- 修复赛道内Prefab 制作更多赛道
- 自动检测墙壁及赛道的简单AI驾驶框架车辆
- 中断后继续训练等训练指令及参数记录文档
- AI车手可以互相竞速训练
- 将场景中环境(比如树和楼房替换为Prefab) 并将整体环境制作为Prefab
- 简化ArcadeKart控制脚本
- 扩展车辆高级控制(赛车竞速策略)
- 扩展更多种类车辆(麦克纳姆轮胎车辆 平板小推车 滑板车 快递车 大脚车 飞思卡尔小车)
- 人类选手和AI组队(AI可以有不同的策略 比如倒着开专门辅助人类选手撞对方车辆 )
- 人类选手辅助训练(人跑AI追 人辅助AI追 AI和人类一起追)
- 扩展ArcadeKart的操作(漂移 手刹)

- 可选玩法玩家训练的AI车手在赛道的最短时间达标后开启新的赛道
- 玩家可以配置自己的车手有多少个射线检测器(什么角度) 以及其它的传感器 但不付费的车手大脑只能容纳有限的传感器(每个类型有各自的限制 整体也有限制)

---

Sensor|占用|解释
--|:--:|--
Car type|1|0~1每种车辆一个数值(卡丁车0.1 SUV0.2)
Car forward|3|车身的朝向
Car velocity|3|车辆当前的三维速度
Car velocity forward|1|车辆向前方向速度
射线检测器(每个)|3+1+1+1|检测器指向角度+碰撞距离+车身边缘+检测到的碰撞类型(墙壁 对方车辆 己方车辆0.3f)
当前圈完成进度|1|0~1
单圈占比赛总圈数比例|1|1/总圈数(以此来代替总圈数)
比赛完成进度|1|0~1
前面一辆车的比赛完成度|1|0~1
后面一辆车的比赛完成度|1|0~1
当前排名|1|当前排名/赛车数量
目标排名|1|目标排名/赛车数量
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

## 命令

cd ml-agents_config
activate ml-agents_release2
mlagents-learn trainer_config.yaml --run-id=AI_Racer1 --train --resume
mlagents-learn trainer_config.yaml --run-id=AI_Racer1 --train --force

## 使用插件

[Curvy Splines](https://assetstore.unity.com/packages/tools/utilities/curvy-splines-7038)
[unity-ugui-XCharts](https://github.com/monitor1394/unity-ugui-XCharts)
