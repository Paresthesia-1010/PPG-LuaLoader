# PPG-LuaLoader

> **开发中（WIP）。** 适用于 People Playground 的非官方 Lua 模组加载器。这是一个个人测试项目。

**语言：** [English](README.md) · [한국어](README.ko-KR.md) · 简体中文 · [日本語](README.ja-JP.md) · [Русский](README.ru-RU.md)

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 下载与安装

想立即测试加载器吗？请前往官方发布页面，下载最新的已编译版本。

👉 **[下载 PPG-LuaLoader v3.0 最终版](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)**

### 安装步骤

1. 确保已通过 Steam 安装 **People Playground**。
2. 下载 **BepInEx 5 (Mono x64)**，并将其解压到游戏根目录（`People Playground/`）。
3. 从[发布页面](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)下载 `PPG_LuaLoader.dll` 和 `MoonSharp.Interpreter.dll`。
4. 将这两个 DLL 文件放入 **`BepInEx/plugins/`** 文件夹。
5. 启动游戏！

---

## 📖 快速导航与模组制作指南

- 🚀 **[快速入门：制作你的第一个模组与文件夹结构](#-快速开始编写你的第一个-lua-模组)**
- 🎮 **[游戏内 Toybox 使用方法](#-游戏内-toybox-使用方法)**
- 📚 **[完整 Lua API 参考](#-完整-lua-api-参考)**

---

## 🛠️ 项目介绍

**PPG-LuaLoader** 是一个专为 **People Playground** 设计的实验性沙盒 Lua 模组运行环境。

鉴于传统 C# DLL 模组近期引发的安全问题，本项目尝试使用 **MoonSharp**（纯 C# Lua 解释器）构建更安全的执行层，让玩家能够通过脚本制作模组，同时降低系统级安全漏洞的风险。

---

## 🚀 快速开始：编写你的第一个 Lua 模组

### 📁 模组文件夹结构

要创建自定义模组，请进入游戏的 **`Mods/`** 目录，并为模组创建一个专用文件夹，例如 `Mods/Sword/`。

模组文件夹应采用以下结构：

```text
People Playground/
 └── Mods/
      └── Sword/
           ├── mod.json       <-- 模组元数据与显示名称
           ├── thumb.png      <-- 自定义图标（Toybox 菜单中的缩略图）
           └── main.lua       <-- Lua 主执行脚本
```

### 📄 1. `mod.json`（配置文件）

在模组文件夹中创建名为 `mod.json` 的文件：

```json
{
  "Name": "1000°C Sword",
  "Author": "YourName",
  "Description": "Custom burning sword powered by Lua!",
  "ModVersion": "1.0",
  "GameVersion": "1.27+",
  "ThumbnailPath": "thumb.png",
  "Active": true
}
```

### 📜 2. `main.lua`（脚本）

在配置文件所在的文件夹中创建 `main.lua`：

```lua
local swords = {}

function Initialize()
    Log("Initializing 1000°C Sword Mod...")

    Item.Register({
        name = "1000°C Sword",
        description = "A sword permanently maintained at 1000°C.",
        baseItem = "Sword",

        onSpawn = function(swordEntity)
            swordEntity:SetTemperature(1000.0)
            table.insert(swords, swordEntity)

            Log("Sword temperature set to 1000°C.")
        end
    })
end

function Update(dt)
    for i = #swords, 1, -1 do
        local sword = swords[i]

        if sword ~= nil then
            sword:SetTemperature(1000.0)
        else
            table.remove(swords, i)
        end
    end
end
```

---

## 🎮 游戏内 Toybox 使用方法

进入任意游戏地图后：

- 按 **`Tab`** 或 **`F1`** 键，打开或关闭 **Custom Lua Toybox** 窗口。
- 点击任意自定义物品卡片以**选中该物品**。选中的卡片会以翠绿色高亮显示。
- 将鼠标指针移动到目标位置，然后按 **`E`** 键，立即生成选中的物品。
  - 加载器会自动屏蔽 `E` 键对应的游戏原生操作，以避免输入冲突。
- 在 Toybox 中使用 **`Env`** 选项卡，可以切换**零重力（Zero Gravity）**，或锁定**子弹时间（Bullet Time，0.2 倍慢动作）**。

---

# 📚 完整 Lua API 参考

每个 Lua 模组都包含一个名为 **`main.lua`** 的脚本，其中有两个生命周期函数：

- **`Initialize()`**：模组加载时调用一次。
- **`Update(dt)`**：游戏运行期间每一帧都会调用。`dt` 表示以秒为单位的帧间隔时间。

---

### 1. 📢 全局日志 API

- `Log(message)`：向 BepInEx 控制台输出信息，并在消息前添加 `[ModName]` 前缀。
- `LogWarning(message)`：输出黄色警告消息。
- `LogError(message)`：输出红色错误消息。

---

### 2. 📡 全局事件监听器

用于监听整个游戏模拟过程中的全局事件：

- `OnSpawn(function(entity) ... end)`：每当有实体被创建时调用。
- `OnDelete(function(entity) ... end)`：实体被销毁前调用。
- `OnCollision(function(me, other, impulse) ... end)`：发生物理碰撞时调用。
- `OnCollisionEnter(function(me, other, impulse) ... end)`：两个实体开始碰撞时调用。
- `OnCollisionExit(function(me, other, impulse) ... end)`：两个实体结束碰撞时调用，此时 `impulse` 为 `0`。
- `OnKeyDown(function(keyName) ... end)`：按下任意键时调用，例如 `"Space"`、`"F"`。
- `OnKeyUp(function(keyName) ... end)`：松开任意键时调用。
- `OnMouseDown(function(button) ... end)`：按下鼠标按钮时调用。`0` = 左键，`1` = 右键，`2` = 中键。
- `OnMouseUp(function(button) ... end)`：松开鼠标按钮时调用。

---

### 3. 🧭 Vector2 API（`LuaVector2`）

- `Vector2.New(x, y)`：创建一个新的二维向量对象。
- `vec.x`、`vec.y`：X 和 Y 坐标。
- `vec:Magnitude()`：返回向量的长度。
- `vec:Normalized()`：返回长度为 1 的单位向量。
- `vec:Dot(otherVec)`：返回两个向量的点积。
- `vec:Distance(otherVec)`：返回两个向量之间的距离。

---

### 4. 🦾 Entity API（`LuaEntity`）

用于对生成的对象、布娃娃和物品调用的方法。

#### 变换与层级结构

- `entity:SetPosition(x, y)`：将实体传送到世界坐标，并同步物理状态。
- `entity:GetPosition()`：以 `LuaVector2` 的形式返回当前世界坐标。
- `entity:SetRotation(angle)`：设置 Z 轴旋转角度，单位为度。
- `entity:GetRotation()`：返回当前 Z 轴旋转角度，单位为度。
- `entity:SetScale(x, y)`：设置局部缩放。
- `entity:GetScale()`：以 `LuaVector2` 的形式返回局部缩放。
- `entity:SetLocalPosition(x, y)`：设置相对于父对象的位置。
- `entity:GetLocalPosition()`：以 `LuaVector2` 的形式返回相对于父对象的局部位置。
- `entity:SetParent(parentEntity)`：将实体附加到另一个实体上。传入 `nil` 可解除连接。
- `entity:GetParent()`：返回父级 `LuaEntity` 或 `nil`。

#### 物理与运动

- `entity:AddForce(fx, fy)`：对所有肢体施加瞬时冲量。
- `entity:AddForceAtPosition(fx, fy, px, py)`：在指定的世界坐标位置施加力。
- `entity:SetVelocity(vx, vy)`：立即设置所有部件的线性速度。
- `entity:GetVelocity()`：以 `LuaVector2` 的形式返回主要肢体的速度。
- `entity:AddVelocity(vx, vy)`：为所有肢体增加线性速度向量。
- `entity:SetAngularVelocity(speed)`：设置旋转速度，单位为度/秒。
- `entity:GetAngularVelocity()`：返回角速度。
- `entity:AddTorque(torque)`：施加旋转冲量。
- `entity:SetMass(mass)`：将总质量平均分配到所有身体部件。
- `entity:GetMass()`：返回所有部件的总质量。
- `entity:SetGravityScale(scale)`：设置重力倍率。`0` 表示零重力。
- `entity:GetGravityScale()`：返回当前重力倍率。
- `entity:SetKinematic(enabled)`：设为 `true` 时，对象将不受外部物理力影响。
- `entity:IsKinematic()`：启用运动学模式时返回 `true`。
- `entity:SetCollisionEnabled(enabled)`：启用或禁用所有碰撞体。
- `entity:IsCollisionEnabled()`：碰撞体处于启用状态时返回 `true`。
- `entity:SetDrag(drag)`：设置线性空气阻力。
- `entity:SetAngularDrag(drag)`：设置旋转空气阻力。
- `entity:Freeze()`：完全冻结位置和旋转。
- `entity:Unfreeze()`：解除所有运动限制。

#### 状态、生命值与销毁

- `entity:IsAlive()`：当人类仍有意识或肢体仍有生命值时返回 `true`。
- `entity:IsDeleted()`：底层 GameObject 被销毁时返回 `true`。
- `entity:Destroy()`：删除实体并触发 `OnDelete`。
- `entity:Delete()`：`entity:Destroy()` 的别名。
- `entity:GetInstanceId()`：返回引擎实例 ID 整数。
- `entity:GetName()`：返回实体 GameObject 的名称。
- `entity:SetName(name)`：重命名实体 GameObject。
- `entity:GetType()`：返回根组件的类型名称。
- `entity:GetComponents()`：以数组表的形式返回附加到实体上的所有组件名称。
- `entity:HasComponent(name)`：检查实体是否包含指定组件。
- `entity:Ignite()`：点燃所有可燃部位。
- `entity:Electrocute(charge)`：向金属框架或机器人释放电流。
- `entity:SetTemperature(celsius)`：强制改变身体温度，例如设置为 `1000.0` 可使金属变得通红。
- `entity:GetTemperature()`：返回当前温度，单位为摄氏度。
- `entity:SetHealth(amount)`：设置所有肢体的生命值，范围为 `0` 到 `100`。
- `entity:GetHealth()`：返回各肢体生命值的平均值。
- `entity:SetMaxHealth(amount)`：覆盖肢体的最大生命值。
- `entity:GetMaxHealth()`：返回最大生命值。
- `entity:SetColor(r, g, b, a)`：设置精灵图的颜色叠加值，各参数范围为 `0.0` 到 `1.0`。
- `entity:GetColor()`：返回格式为 `{ r = ..., g = ..., b = ..., a = ... }` 的表。
- `entity:SetVisible(enabled)`：显示或隐藏精灵渲染器。
- `entity:IsVisible()`：精灵可见时返回 `true`。
- `entity:SetLayer(layer)`：设置 GameObject 及其所有子对象的图层。
- `entity:GetLayer()`：返回当前图层索引。
- `entity:SetTag(tag)`：为实体分配自定义字符串标签。
- `entity:HasTag(tag)`：实体包含指定自定义标签时返回 `true`。
- `entity:RemoveTag(tag)`：移除指定的自定义标签。
- `entity:Dismember()`：严重破坏所有物理关节并导致出血。
- `entity:Crush()`：将肉体和骨骼压碎成带血的碎块。
- `entity:Disintegrate()`：使肉体汽化，只留下骨骼。
- `entity:Heal()`：扑灭火焰、将温度重置为 37°C、修复骨骼、止血，并注入 `LIFE` 血清。
- `entity:Inject(liquidName, amount)`：将液体直接注入血液循环系统，例如 `"LIFE"`、`"ACID"`、`"BLOOD"`。

#### 碰撞体与重叠检测

- `entity:GetColliders()`：以表的形式返回碰撞体类型名称。
- `entity:SetCollisionLayer(layer)`：设置碰撞图层。
- `entity:GetCollisionLayer()`：返回碰撞图层。
- `entity:SetCollisionMask(maskBitfield)`：设置图层碰撞掩码。
- `entity:GetCollisionMask()`：返回碰撞掩码。
- `entity:IgnoreCollision(otherEntity, ignoreBool)`：禁用或启用两个实体之间的碰撞。
- `entity:OverlapCircle(radius)`：以表的形式返回该实体周围圆形区域内的其他实体。
- `entity:OverlapBox(width, height)`：以表的形式返回该实体周围矩形区域内的其他实体。

---

### 5. ⌨️ 输入 API（`Input`）

- `Input.IsKeyDown(keyName)`：当某个按键刚被按下时返回 `true`，例如 `"F"`、`"Space"`、`"Tab"`、`"E"`。
- `Input.IsKeyHeld(keyName)`：按键持续按住期间始终返回 `true`。
- `Input.IsKeyUp(keyName)`：当某个按键刚被松开时返回 `true`。
- `Input.IsMouseButtonDown(button)`：鼠标按钮被点击时返回 `true`。`0` = 左键，`1` = 右键，`2` = 中键。
- `Input.IsMouseButtonHeld(button)`：鼠标按钮持续按住期间返回 `true`。
- `Input.IsMouseButtonUp(button)`：鼠标按钮被松开时返回 `true`。
- `Input.GetMouseScrollDelta()`：返回鼠标滚轮的垂直滚动量。
- `Input.GetMouseWorldPosition()`：以 `{ x = ..., y = ... }` 的形式返回二维世界坐标中的鼠标位置。

---

### 6. 👥 生成 API（`Spawn`）

- `Spawn.Create(assetName, x, y)`：生成 `"Human"` 或 `"Android"` 等原生实体，并返回 `LuaEntity`。
- `Spawn.CreateAtRotation(assetName, x, y, rot)`：以指定的旋转角度（度）生成资源。
- `Spawn.CreateBatch(assetName, positionsTable)`：批量生成多个实体。格式：`{ {x=1, y=2}, {x=3, y=4} }`。
- `Spawn.Clone(entity, x, y)`：在指定坐标复制现有实体。
- `Spawn.GetAvailableAssets()`：以表的形式返回游戏中所有可生成的资源名称。
- `Spawn.Exists(assetName)`：指定名称的资源存在时返回 `true`。

---

### 7. 🌍 世界与物理 API（`World` 和 `Physics`）

- `World.GetEntities()`：以表的形式返回场景中所有处于活动状态的根实体。
- `World.GetEntitiesInRadius(x, y, radius)`：返回指定半径范围内的实体。
- `World.GetEntitiesInBox(x, y, width, height)`：返回指定矩形区域内的实体。
- `World.GetEntityById(instanceId)`：通过 Unity 实例 ID 查找实体。
- `World.GetEntitiesByName(name)`：返回名称中包含指定字符串的实体。
- `World.GetEntitiesByTag(tag)`：返回具有指定 Unity 标签的实体。
- `World.GetEntityCount()`：返回当前活动物理对象的总数。
- `World.GetBounds()`：返回边界表：`{ minX = -100, maxX = 100, minY = -50, maxY = 50 }`。
- `World.GetMouseWorldPosition()`：返回 `{ x = ..., y = ... }` 格式的鼠标世界坐标。
- `World.GetCameraPosition()`：返回主摄像机坐标，格式为 `{ x = ..., y = ... }`。
- `Physics.Explode(x, y, radius, force)`：触发物理爆炸，将实体从爆炸中心向外抛开。

---

### 8. ⏱️ 时间 API（`Time`）

- `Time.GetDeltaTime()`：返回以秒为单位的帧间隔时间。
- `Time.GetTime()`：返回游戏启动后的总运行时间。
- `Time.GetFixedDeltaTime()`：返回物理更新的时间间隔。
- `Time.GetTimeScale()`：返回当前游戏模拟速度。
- `Time.SetTimeScale(scale)`：锁定模拟速度。例如，`0.2` 表示慢动作，`1.0` 表示正常速度。
- `Time.GetFrameRate()`：返回当前 FPS。

---

### 9. 🗺️ 地图与环境 API（`Map`）

- `Map.SetGravity(gx, gy)`：更改世界重力向量。默认值为 `0, -9.81`。
- `Map.GetGravity()`：返回 `{ x = ..., y = ... }` 格式的重力向量。
- `Map.SetTimeScale(scale)`：`Time.SetTimeScale(scale)` 的别名。
- `Map.CreateWall(x, y, width, height, material)`：生成不可破坏的结构墙体。可用材质包括 `"Metal"`、`"Wood"`、`"Concrete"`。
- `Map.GetBounds()`：返回地图边界表。
- `Map.GetEntities()`：返回地图中的所有实体。
- `Map.Register(mapConfig)`：将地图预设注册到 Toybox 的 Maps 选项卡中。

```lua
Map.Register({
    name = "Bunker",
    description = "Reinforced concrete chamber.",
    gravity = { 0, -9.81 },
    structures = {
        { x = 0, y = -5, width = 20, height = 1, material = "Concrete" },
        { x = -10, y = 0, width = 1, height = 10, material = "Metal" },
        { x = 10, y = 0, width = 1, height = 10, material = "Metal" }
    }
})
```

---

### 10. ⚔️ 自定义物品 API（`Item`）

- `Item.Register(configTable)`：将物品注册到 Custom Toybox（`Tab` 菜单）。
- `Item.Unregister(itemName)`：取消注册指定物品。
- `Item.GetRegisteredItems()`：以表的形式返回所有已注册的自定义物品名称。
- `Item.GetItem(itemName)`：返回物品信息 `{ name, description, baseItem }`。
- `Item.Spawn(itemName, x, y)`：在指定世界坐标生成已注册的自定义物品。
- `Item.OnSpawn(configTable, function(entity) ... end)`：注册物品并设置生成时执行的回调函数。
- `Item.OnUse(configTable, function(entity) ... end)`：注册物品，并绑定物品激活或使用时执行的操作。

```lua
Item.Register({
    name = "Energy Saber",
    description = "Hot energy weapon.",
    baseItem = "Sword",
    sprite = "blade.png",
    onSpawn = function(entity)
        entity:SetColor(0.2, 0.8, 1.0, 1.0)
        entity:SetTemperature(800)
    end
})

Item.OnUse({
    name = "Healing Beacon",
    description = "Heals everyone nearby when used.",
    baseItem = "Flashlight"
}, function(entity)
    local pos = entity:GetPosition()
    local nearby = World.GetEntitiesInRadius(pos.x, pos.y, 6)
    for i = 1, #nearby do
        nearby[i]:Heal()
    end
end)
```

---

### 11. 🛠️ 调试绘图 API（`Debug`）

- `Debug.DrawLine(x1, y1, x2, y2, duration)`：绘制一条红色调试线，持续指定秒数。
- `Debug.DrawCircle(cx, cy, radius, duration)`：绘制红色线框圆。
- `Debug.DrawBox(cx, cy, width, height, duration)`：以 `(cx, cy)` 为中心绘制红色线框矩形。
- `Debug.GetEntityInfo(entity)`：返回包含名称、实例 ID 和坐标的调试字符串。
- `Debug.IsEnabled()`：返回 `true`。

---

### 12. 📦 模组管理 API（`Mod`）

- `Mod.GetName()`：返回当前模组名称。
- `Mod.GetVersion()`：返回加载器版本（`"3.0.0"`）。
- `Mod.GetPath()`：返回当前模组目录的绝对路径。
- `Mod.GetAllMods()`：以表的形式返回所有已加载模组的名称。
- `Mod.IsLoaded(modName)`：指定模组已加载时返回 `true`。
- `Mod.Reload(modName)`：重新运行该模组的 `main.lua`，以热重载模组。
- `Mod.GetMetadata()`：返回 `{ name = ..., dir = ... }` 格式的元数据。

---

### 13. 📐 扩展数学 API（`math`）

为 Lua 标准 `math` 库添加的引擎扩展功能：

- `math.Clamp(val, min, max)`：将数值限制在最小值和最大值之间。
- `math.Lerp(a, b, t)`：根据系数 `t` 在 `a` 和 `b` 之间进行线性插值。
- `math.DegToRad(deg)`：将角度从度转换为弧度。
- `math.RadToDeg(rad)`：将角度从弧度转换为度。
- `math.Distance(x1, y1, x2, y2)`：计算两个二维点之间的距离。
- `math.Angle(x1, y1, x2, y2)`：返回从第一个点指向第二个点的角度，单位为度。
- `math.Random(min, max)`：返回一个随机浮点数。
- `math.RandomInt(min, max)`：返回最小值与最大值之间的随机整数。

---

## ⚠️ 免责声明

这是一个为教育目的及安全导向的模组研究而创建的**非官方实验性测试项目**。本项目与 Studio Minus 没有任何关联。使用风险由用户自行承担。
