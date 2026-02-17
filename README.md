# Unity 弹弹堂类客户端快速搭建说明

本项目是一个 Unity 前端项目，用于实现类似第七大道《弹弹堂》的 2D 回合制射击玩法。  
当前仓库已包含一套最小可运行的单人本地 Demo 流程：**Lobby 场景 → 按按钮 → 进入 Battle 场景 → 控制角色移动与发射炮弹**。

本文档说明在 Unity 中需要完成的操作与预制体/物体上需要挂载的脚本，使项目变为可运行的 Demo。

---

## 1. 目录与脚本结构（只列出当前 Demo 相关）

```text
Assets/
  Scenes/
    Lobby.unity          # 大厅场景
    Battle.unity         # 战斗场景

  Scripts/
    Common/
      GameEntry.cs
    Lobby/
      LobbyManager.cs
    Battle/
      BattleManager.cs
      PlayerController.cs
      Projectile.cs
```

> 说明：以上 C# 文件已经在仓库中创建好，无需手动新建代码，只需在 Unity 场景/预制体中挂载对应脚本即可。

---

## 2. 场景设置

### 2.1 创建并配置场景

1. 在 `Assets/Scenes` 下确认或新建两个场景：
   - `Lobby.unity`
   - `Battle.unity`
2. 打开 `File > Build Settings...`：
   - 将 `Lobby.unity` 和 `Battle.unity` 拖入 `Scenes In Build` 列表。
   - 推荐顺序：
     - `Lobby` 在上（索引 0）
     - `Battle` 在下（索引 1）

### 2.2 设置启动场景

- 在 `Build Settings` 中将 `Lobby` 设为第 0 个场景，或者直接在编辑器中打开 `Lobby.unity` 后点击播放，作为游戏入口。

---

## 3. Lobby 场景配置（大厅）

目标：在 Lobby 中点击一个按钮，加载 Battle 场景。

### 3.1 挂载 `GameEntry`

1. 在 `Lobby` 场景中新建一个空物体：
   - 名称：`GameEntry`
   - 挂载脚本：`GameEntry`（来自 `Assets/Scripts/Common/GameEntry.cs`）
2. 该物体会在 `Awake` 时设置为单例并 `DontDestroyOnLoad`，后续可在此初始化全局系统。

### 3.2 挂载 `LobbyManager` 并配置按钮

1. 在 `Lobby` 场景中新建一个空物体：
   - 名称：`LobbyManager`
   - 挂载脚本：`LobbyManager`（来自 `Assets/Scripts/Lobby/LobbyManager.cs`）
2. 创建简单 UI：
   - 菜单：`GameObject > UI > Canvas` 创建一个 `Canvas`。
   - 在 `Canvas` 下创建：`UI > Button`，命名为 `StartBattleButton`。
   - 在按钮的文本上写：`开始单人战斗`（或任意文字）。
3. 设置按钮点击事件：
   - 选中 `StartBattleButton`。
   - 在 `Button (Script)` 组件的 `On Click ()` 列表中点击 `+`。
   - 将场景中的 `LobbyManager` 物体拖入新添加的事件对象槽。
   - 在右侧函数下拉菜单中选择：`LobbyManager -> OnClickStartSingleBattle()`。

此时运行 `Lobby` 场景并点击按钮，即可加载 `Battle` 场景。

---

## 4. Battle 场景配置（代码动态生成的本地单人战斗 Demo）

目标：在 Battle 场景中 **不手工摆放地面和玩家**，而是通过代码在运行时动态生成最小可玩战斗场景。

当前方案中，Battle 场景只需要一个挂载了 `BattleBootstrap` 脚本的空物体即可，其余内容（地面、玩家、BattleManager、炮弹逻辑）全部由代码生成。

### 4.1 BattleBootstrap 脚本说明

- 脚本位置：`Assets/Scripts/Battle/BattleBootstrap.cs`
- 主要职责：
  - 在 `Start()` 中通过代码创建：
    - 一个带 `BoxCollider2D` 的简易地面。
    - 一个带 `Rigidbody2D + BoxCollider2D + PlayerController` 的玩家对象，并自动创建 `FirePoint` 子物体。
    - 一个 `BattleManager` 对象，并把玩家引用绑定到 `BattleManager.player`。
  - 可选：从 inspector 中接收一个自定义 `Projectile` 预制体，如果留空，则由 `PlayerController` 代码动态生成一个最简版炮弹对象。

> 说明：`PlayerController` 已经支持「无预制体情况下，完全由代码创建炮弹」（见 `FireProjectile()`）。  
> 后续接入真实关卡时，只需替换为从配置/服务器数据驱动的生成逻辑。

### 4.2 在 Battle 场景中挂载 BattleBootstrap

1. 打开 `Battle.unity` 场景。
2. 新建一个空物体：
   - 名称建议：`BattleBootstrap`（或任意名称）。
   - 挂载脚本：`BattleBootstrap`（来自 `Assets/Scripts/Battle/BattleBootstrap.cs`）。
3. 在 `BattleBootstrap` 组件中，可根据需要调整参数：
   - `Player Spawn Position`：玩家出生位置。
   - `Ground Y`：地面在 Y 轴的位置。
   - `Ground Width`：地面的宽度（影响碰撞范围）。
   - `Ground Thickness`：地面的厚度（碰撞盒高度）。
   - `Projectile Prefab`（可选）：自定义炮弹预制体，若留空则使用代码默认创建的简单炮弹（仅刚体+碰撞体，无贴图）。

完成上述操作后，Battle 场景在运行时会自动生成：

- 一个不可见但有碰撞的地面。
- 一个可受重力影响的玩家对象：
  - 通过 A/D 或 左右方向键移动。
  - 通过上/下方向键调整炮口角度。
  - 通过空格键蓄力并发射炮弹。
- 一个 `BattleManager` 对象，用于后续扩展战斗逻辑。

### 4.3（可选）自定义可视化炮弹预制体

如果你希望炮弹有明确的贴图/特效，可以按以下步骤创建预制体，并交由 `BattleBootstrap` 与 `PlayerController` 使用：

1. 在场景中临时创建一个小的 2D 精灵：
   - 名称：`Projectile`
2. 为 `Projectile` 添加组件：
   - `Rigidbody2D`
     - Body Type：`Dynamic`
   - `CircleCollider2D`（或其他合适的 2D Collider）
   - 脚本：`Projectile`（来自 `Assets/Scripts/Battle/Projectile.cs`）
   - 可选：`SpriteRenderer`，设置你喜欢的炮弹贴图。
3. 将该 `Projectile` 拖拽到 `Assets/Prefabs/` 目录中，生成预制体 `Projectile.prefab`。
4. 删除场景中的临时 `Projectile` 实例（场景中不需要常驻一个炮弹）。
5. 选中 `Battle` 场景中的 `BattleBootstrap` 对象：
   - 将刚才创建的 `Projectile.prefab` 拖到其 `Projectile Prefab` 字段。

> 提示：如果你只想快速验证逻辑，不创建预制体也没问题，代码会自动创建一个仅包含物理行为的炮弹对象。

---

## 5. 输入与运行验证

### 5.1 输入说明（默认 Unity Input）

当前 Demo 使用 Unity 的默认输入映射：

- 移动：`Horizontal`（A/D 或 左右方向键）
- 调角度：`UpArrow` / `DownArrow`
- 蓄力与发射：`Space`（空格键，按下开始蓄力，松开发射）

如需修改按键，可在 `Edit > Project Settings > Input`（或新输入系统中对应设置）进行调整，并在代码中同步修改按键检测逻辑。

### 5.2 运行流程检查

1. 打开 `Lobby.unity`，点击播放：
   - 界面中看到一个按钮（例如“开始单人战斗”）。
   - 点击按钮，场景切换到 `Battle`。
2. 在 `Battle` 中：
   - 使用 A/D 或 左右方向键，玩家应能左右移动。
   - 使用上/下方向键，看到炮口(`FirePoint`)朝向变化。
   - 按住空格后松开，生成一个炮弹，从炮口方向飞出，受到重力影响，并在碰到地面或其他碰撞体后销毁。

如果上述流程正常，就说明项目已经是一个**可运行的最小单人本地战斗 Demo**。

---

## 6. 后续可扩展方向（建议）

- 在 Lobby 增加玩家信息展示、伪造房间列表，模拟多人房间 UI。
- 在 Battle 中加入：
  - 回合系统（多名玩家轮流开火）。
  - 风力系统（对炮弹水平加速度产生影响）。
  - HUD（角度、力度、风向、血量等 UI）。
- 引入简单的网络层封装，逐步从本地单人过渡到联机对战（仅在客户端实现接口，与真实服务端对接）。

如需进一步指导（例如 HUD、回合系统或风力系统的具体实现），可以在此 README 的基础上再扩展相应模块与文档。

