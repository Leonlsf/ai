# 压力怪兽 (Stress Monster) - 运行指南

## 📋 项目介绍

「压力怪兽」是一款以「吞噬压力」为核心的减压游戏，玩家将烦心事输入游戏，它们会变成食物被可爱的怪兽吞噬。结合了喂食爽感、情绪转化、Boss战和进化系统。

---

## 🚀 快速开始指南

### 第一步：安装 Unity Hub 和 Unity Editor

1. 前往 [Unity官网](https://unity.com/download) 下载并安装 **Unity Hub**
2. 打开 Unity Hub，点击 `Installs` → `Install Editor`
3. 选择安装版本：**2021.3 LTS** 或 **2022.3 LTS**（稳定版）
4. 在安装组件中勾选：
   - ✅ **Windows Build Support**（PC）或 **Mac Build Support**（macOS）
   - ✅ **Android Build Support**（可选，手机版）
   - ✅ **TextMeshPro**（默认已勾选）

### 第二步：打开项目

1. 打开 Unity Hub
2. 点击 `Projects` → `Add`
3. 选择文件夹：`/workspace/StressMonster`
4. Unity会自动初始化项目（首次打开可能需要几分钟）

### 第三步：打开主场景

项目打开后，在 `Project` 窗口中找到：
```
Assets/Scenes/MainScene.unity
```
双击打开场景。

### 第四步：配置项目引用 (关键步骤！)

在场景中，你会看到 `GameBootstrapper` GameObject，需要配置引用：

1. 在 `Hierarchy` 窗口中选中 `GameBootstrapper`
2. 在 Inspector 中会看到 `GameBootstrapper (Script)` 组件
3. 需要在场景中创建以下子对象并引用：

#### 4.1 创建 Core 管理器

在 `Hierarchy` 中右键 → `Create Empty` → 命名为 `GameManager`，挂载 [GameManager.cs](file:///workspace/StressMonster/Assets/Scripts/Core/GameManager.cs)
同样方式创建：
- `InputManager` → 挂载 [InputManager.cs](file:///workspace/StressMonster/Assets/Scripts/Core/InputManager.cs)
- `AudioManager` → 挂载 [AudioManager.cs](file:///workspace/StressMonster/Assets/Scripts/Core/AudioManager.cs)

#### 4.2 创建怪兽

创建 `Monster` GameObject：
1. 右键 → `Create Empty` → 命名为 `Monster`
2. 添加 `Sprite Renderer` 组件（创建简单占位精灵即可）
3. 添加 `Animator` 组件（暂时不用动画控制器）
4. 添加子对象 `MouthPosition` (Transform)
5. 挂载 [MonsterController.cs](file:///workspace/StressMonster/Assets/Scripts/Monster/MonsterController.cs)
6. 挂载 [MonsterAnimation.cs](file:///workspace/StressMonster/Assets/Scripts/Monster/MonsterAnimation.cs)
7. 挂载 [MonsterEvolution.cs](file:///workspace/StressMonster/Assets/Scripts/Monster/MonsterEvolution.cs)

#### 4.3 创建喂食系统

创建 `FoodGenerator` GameObject：
1. 挂载 [FoodGenerator.cs](file:///workspace/StressMonster/Assets/Scripts/Feeding/FoodGenerator.cs)
2. 创建 `DragHandler` GameObject → 挂载 [DragHandler.cs](file:///workspace/StressMonster/Assets/Scripts/Feeding/DragHandler.cs)
3. 创建 `ComboSystem` GameObject → 挂载 [ComboSystem.cs](file:///workspace/StressMonster/Assets/Scripts/Feeding/ComboSystem.cs)

#### 4.4 创建情绪系统

右键 → `Create → Scriptable Object` → 搜索：
- `EmotionKeywordDB` → 命名为 `DefaultEmotionDB`
- `EmotionConverter` → 命名为 `DefaultEmotionConverter`

创建 `EmotionEffectPlayer` GameObject → 挂载 [EmotionEffectPlayer.cs](file:///workspace/StressMonster/Assets/Scripts/Emotion/EmotionEffectPlayer.cs)

#### 4.5 创建 Boss 系统

创建 `BossBattleManager` GameObject → 挂载 [BossBattleManager.cs](file:///workspace/StressMonster/Assets/Scripts/Boss/BossBattleManager.cs)

#### 4.6 创建 UI

右键 → `UI → Canvas` → 命名为 `GameCanvas`

在 Canvas 下创建：
- `InputPanel` → 挂载 [InputPanel.cs](file:///workspace/StressMonster/Assets/Scripts/UI/InputPanel.cs)
- `EmotionSelector` → 挂载 [EmotionSelector.cs](file:///workspace/StressMonster/Assets/Scripts/UI/EmotionSelector.cs)
- `ComboDisplay` → 挂载 [ComboDisplay.cs](file:///workspace/StressMonster/Assets/Scripts/UI/ComboDisplay.cs)
- `EvolutionPanel` → 挂载 [EvolutionPanel.cs](file:///workspace/StressMonster/Assets/Scripts/UI/EvolutionPanel.cs)
- `BossHealthBar` → 挂载 [BossHealthBar.cs](file:///workspace/StressMonster/Assets/Scripts/UI/BossHealthBar.cs)

#### 4.7 连接所有引用

最后回到 `GameBootstrapper`，在 Inspector 中把刚创建的 GameObject 拖到对应的字段中。

### 第五步：创建占位精灵 (快速测试)

无需专业美术，创建简单的占位图：

1. 在 `Assets/Art/Sprites` 右键 → `Create → Sprite (Square)`
2. 创建以下精灵：
   - `MonsterPlaceholder` (任何颜色，比如绿色)
   - `FoodAnger` (红色)
   - `FoodAnxiety` (黄色)
   - `FoodSadness` (蓝色)
   - `FoodIrritation` (橙色)
3. 把这些精灵拖到对应的组件引用中

### 第六步：运行！

点击 Unity Editor 顶部的 **▶ Play** 按钮（或按 `Ctrl+P` / `Cmd+P`）

---

## 🎯 极简测试方式 (推荐)

如果觉得上面太复杂，可以先做极简测试：

### 1. 创建基础结构

在 `MainScene` 中只保留：
- `Main Camera`
- `GameBootstrapper`
- `GameManager`
- `Monster` (简单精灵)

### 2. 运行基础流程

即使没有完整的引用，Unity也会编译脚本并运行，你可以：
- 检查控制台是否有报错
- 验证基础框架是否工作

---

## 📂 项目结构总览

```
StressMonster/
├── Assets/
│   ├── Scripts/              # 26个核心C#脚本
│   │   ├── Core/            # 游戏管理器
│   │   ├── Monster/         # 怪兽系统
│   │   ├── Feeding/         # 喂食系统
│   │   ├── Emotion/         # 情绪系统
│   │   ├── Boss/            # Boss战系统
│   │   └── UI/              # UI系统
│   ├── Data/                # JSON配置文件
│   ├── Prefabs/             # 预制体目录
│   ├── Scenes/              # 主场景
│   └── ...
└── ProjectSettings/         # Unity项目配置
```

---

## 🐛 常见问题

### Q: Unity打开时报错？
A: 确保 Unity 版本是 2021.3+，并且打开项目时让 Unity 完成自动生成工作。

### Q: 脚本编译错误？
A: 检查所有脚本的命名空间是否正确，确保没有缺失的引用。

### Q: 精灵不显示？
A: 确保 `Sprite Renderer` 组件已添加，且精灵不为空。

---

## 📝 开发计划

- [ ] 添加美术资源
- [ ] 添加音效
- [ ] 完成全功能测试
- [ ] 制作安装包

---

## 📄 许可证

MIT License