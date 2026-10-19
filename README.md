# PPG-LuaLoader

> **WIP.** Non-official Lua mod loader for People Playground. This is a personal test project.

**Languages:** English · [한국어](README.ko-KR.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja-JP.md) · [Русский](README.ru-RU.md)

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 Download & Installation

Looking to test the loader right away? Grab the latest compiled binaries from the official release page.

👉 **[Download PPG-LuaLoader v3.0 Final Release](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)**

### Installation Steps
1. Make sure you have **People Playground** installed via Steam.
2. Download and extract **BepInEx 5 (Mono x64)** into your game root directory (`People Playground/`).
3. Download `PPG_LuaLoader.dll` and `MoonSharp.Interpreter.dll` from the [Releases Page](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main).
4. Drop both DLL files into your **`BepInEx/plugins/`** folder.
5. Launch the game!

---

## 📖 Quick Navigation & Modding Guide
* 🚀 **[Quickstart: Your First Mod & Folder Structure](#-quickstart-writing-your-first-lua-mod)**
* 🎮 **[In-Game Toybox Usage](#-in-game-toybox-usage)**
* 📚 **[Complete Lua API Reference](#-complete-lua-api-reference)**

---

## 🛠️ About Project
**PPG-LuaLoader** is an experimental, sandboxed Lua modding runtime designed for **People Playground**. 
Following recent security concerns regarding traditional C# DLL mods, this project explores a safe execution layer using **MoonSharp** (pure C# Lua interpreter) to allow secure, script-based modding without risking system-level vulnerabilities.

---

## 🚀 Quickstart: Writing Your First Lua Mod

### 📁 Mod Folder Structure
To create a custom mod, navigate to your game's **`Mods/`** directory and create a dedicated folder for your mod (e.g., `Mods/Sword/`). 

Your mod folder should be structured like this:
```text
People Playground/
 └── Mods/
      └── Sword/
           ├── mod.json       <-- Mod metadata & display name
           ├── thumb.png      <-- Custom icon image (Thumbnail for the Toybox menu)
           └── main.lua       <-- Main Lua execution script
```

### 📄 1. `mod.json` (Configuration)
Create a file named `mod.json` inside your mod folder:
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

### 📜 2. `main.lua` (Script)
Create a file named `main.lua` alongside your config:
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

## 🎮 In-Game Toybox Usage

Once you are inside any game map:
* Press **`Tab`** or **`F1`** to toggle open/close the **Custom Lua Toybox** window.
* Click on any custom item card to **select it** (it will highlight in emerald green).
* Aim your mouse cursor and press **`E`** to spawn the selected item instantly! 
  *(The loader automatically suppresses native game actions on the `E` key to prevent conflicts).*
* Use the **`Env`** tab inside the Toybox to toggle **Zero Gravity** or lock **Bullet Time (0.2x Slow-Mo)**.

---

# 📚 Complete Lua API Reference

Every Lua mod consists of a **`main.lua`** script containing two lifecycle hooks:
* **`Initialize()`**: Called once when the mod is loaded.
* **`Update(dt)`**: Called every frame while the game is running (`dt` = frame delta time in seconds).

---

### 1. 📢 Global Logging API
* `Log(message)`: Prints an informational message to the BepInEx console prefixed with `[ModName]`.
* `LogWarning(message)`: Prints a yellow warning message.
* `LogError(message)`: Prints a red error message.

---

### 2. 📡 Global Event Listeners
Hooks to listen for global events across the entire game simulation:
* `OnSpawn(function(entity) ... end)`: Called whenever any entity is created.
* `OnDelete(function(entity) ... end)`: Called right before an entity is destroyed.
* `OnCollision(function(me, other, impulse) ... end)`: Called on physics collision contact.
* `OnCollisionEnter(function(me, other, impulse) ... end)`: Called when two entities begin colliding.
* `OnCollisionExit(function(me, other, impulse) ... end)`: Called when two entities stop colliding (`impulse` is `0`).
* `OnKeyDown(function(keyName) ... end)`: Called when any key is pressed down (e.g., `"Space"`, `"F"`).
* `OnKeyUp(function(keyName) ... end)`: Called when any key is released.
* `OnMouseDown(function(button) ... end)`: Called when a mouse button is pressed (`0` = Left, `1` = Right, `2` = Middle).
* `OnMouseUp(function(button) ... end)`: Called when a mouse button is released.

---

### 3. 🧭 Vector2 API (`LuaVector2`)
* `Vector2.New(x, y)`: Creates a new 2D vector object.
* `vec.x`, `vec.y`: X and Y coordinates.
* `vec:Magnitude()`: Returns vector length.
* `vec:Normalized()`: Returns unit vector with length 1.
* `vec:Dot(otherVec)`: Returns the dot product.
* `vec:Distance(otherVec)`: Returns the distance between two vectors.

---

### 4. 🦾 Entity API (`LuaEntity`)
Methods called on spawned objects, ragdolls, and items:

#### Transform & Hierarchy
* `entity:SetPosition(x, y)`: Teleports entity to world coordinates and syncs physics.
* `entity:GetPosition()`: Returns current world coordinates as a `LuaVector2`.
* `entity:SetRotation(angle)`: Sets Z-axis rotation angle in degrees.
* `entity:GetRotation()`: Returns current Z-axis angle in degrees.
* `entity:SetScale(x, y)`: Sets local scale.
* `entity:GetScale()`: Returns local scale as a `LuaVector2`.
* `entity:SetLocalPosition(x, y)`: Sets position relative to parent.
* `entity:GetLocalPosition()`: Returns local position relative to parent as a `LuaVector2`.
* `entity:SetParent(parentEntity)`: Attaches to another entity (pass `nil` to detach).
* `entity:GetParent()`: Returns parent `LuaEntity` or `nil`.

#### Physics & Motion
* `entity:AddForce(fx, fy)`: Applies an instant force impulse vector to all limbs.
* `entity:AddForceAtPosition(fx, fy, px, py)`: Applies force at a specific world position.
* `entity:SetVelocity(vx, vy)`: Instantly sets linear velocity on all parts.
* `entity:GetVelocity()`: Returns primary limb velocity as a `LuaVector2`.
* `entity:AddVelocity(vx, vy)`: Adds linear velocity vector to all limbs.
* `entity:SetAngularVelocity(speed)`: Sets rotation speed in degrees per second.
* `entity:GetAngularVelocity()`: Returns rotational velocity.
* `entity:AddTorque(torque)`: Applies rotational impulse.
* `entity:SetMass(mass)`: Distributes total mass evenly across all body parts.
* `entity:GetMass()`: Returns combined total mass of all parts.
* `entity:SetGravityScale(scale)`: Sets gravity multiplier (`0` for zero gravity).
* `entity:GetGravityScale()`: Returns current gravity multiplier.
* `entity:SetKinematic(enabled)`: When `true`, object ignores external physics forces.
* `entity:IsKinematic()`: Returns `true` if kinematic mode is enabled.
* `entity:SetCollisionEnabled(enabled)`: Enables or disables all colliders.
* `entity:IsCollisionEnabled()`: Returns `true` if colliders are active.
* `entity:SetDrag(drag)`: Sets linear air resistance.
* `entity:SetAngularDrag(drag)`: Sets rotational air resistance.
* `entity:Freeze()`: Freezes position and rotation completely in place.
* `entity:Unfreeze()`: Removes all movement constraints.

#### State, Vitals & Destruction
* `entity:IsAlive()`: Returns `true` if human is conscious or limbs have health.
* `entity:IsDeleted()`: Returns `true` if the underlying GameObject is destroyed.
* `entity:Destroy()`: Deletes the entity and triggers `OnDelete`.
* `entity:Delete()`: Alias for `entity:Destroy()`.
* `entity:GetInstanceId()`: Returns engine instance ID integer.
* `entity:GetName()`: Returns the entity GameObject name.
* `entity:SetName(name)`: Renames the entity GameObject.
* `entity:GetType()`: Returns root component type name.
* `entity:GetComponents()`: Returns an array table of all component names attached.
* `entity:HasComponent(name)`: Checks if a component exists on this entity.
* `entity:Ignite()`: Sets all flammable parts on fire.
* `entity:Electrocute(charge)`: Discharges electricity into metal frames or Androids.
* `entity:SetTemperature(celsius)`: Forces body temperature change (e.g., `1000.0` for red-hot metal).
* `entity:GetTemperature()`: Returns current temperature in Celsius.
* `entity:SetHealth(amount)`: Sets health on all limbs (`0` to `100`).
* `entity:GetHealth()`: Returns average limb health.
* `entity:SetMaxHealth(amount)`: Overrides maximum limb health.
* `entity:GetMaxHealth()`: Returns max limb health.
* `entity:SetColor(r, g, b, a)`: Sets sprite tint color (`0.0` to `1.0`).
* `entity:GetColor()`: Returns table `{ r = ..., g = ..., b = ..., a = ... }`.
* `entity:SetVisible(enabled)`: Shows or hides sprite renderers.
* `entity:IsVisible()`: Returns `true` if sprites are visible.
* `entity:SetLayer(layer)`: Sets GameObject layer across all children.
* `entity:GetLayer()`: Returns current layer index.
* `entity:SetTag(tag)`: Assigns a custom string tag to the entity.
* `entity:HasTag(tag)`: Returns `true` if entity contains the specified custom tag.
* `entity:RemoveTag(tag)`: Removes the specified custom tag.
* `entity:Dismember()`: Severely shatters all physics joints and causes bleeding.
* `entity:Crush()`: Crushes flesh and bones into bloody chunks.
* `entity:Disintegrate()`: Vaporizes flesh down to the skeleton.
* `entity:Heal()`: Extinguishes fire, resets temperature to 37°C, mends bones, stops bleeding, and injects `LIFE` serum.
* `entity:Inject(liquidName, amount)`: Injects fluids directly into the bloodstream (e.g., `"LIFE"`, `"ACID"`, `"BLOOD"`).

#### Colliders & Overlaps
* `entity:GetColliders()`: Returns a table of collider type names.
* `entity:SetCollisionLayer(layer)`: Sets collision layer.
* `entity:GetCollisionLayer()`: Returns collision layer.
* `entity:SetCollisionMask(maskBitfield)`: Sets layer collision mask.
* `entity:GetCollisionMask()`: Returns collision mask.
* `entity:IgnoreCollision(otherEntity, ignoreBool)`: Disables or enables collision between two entities.
* `entity:OverlapCircle(radius)`: Returns a table of other entities inside a circle around this entity.
* `entity:OverlapBox(width, height)`: Returns a table of other entities inside a box around this entity.

---

### 5. ⌨️ Input API (`Input`)
* `Input.IsKeyDown(keyName)`: Returns `true` on the single frame a key (e.g., `"F"`, `"Space"`, `"Tab"`, `"E"`) is pressed down.
* `Input.IsKeyHeld(keyName)`: Returns `true` continuously while the key is held down.
* `Input.IsKeyUp(keyName)`: Returns `true` on the frame the key is released.
* `Input.IsMouseButtonDown(button)`: Returns `true` on click (`0` = Left, `1` = Right, `2` = Middle).
* `Input.IsMouseButtonHeld(button)`: Returns `true` while mouse button is held.
* `Input.IsMouseButtonUp(button)`: Returns `true` on mouse button release.
* `Input.GetMouseScrollDelta()`: Returns vertical scroll wheel movement.
* `Input.GetMouseWorldPosition()`: Returns a table `{ x = ..., y = ... }` of 2D world mouse coordinates.

---

### 6. 👥 Spawn API (`Spawn`)
* `Spawn.Create(assetName, x, y)`: Spawns native entities like `"Human"` or `"Android"` and returns a `LuaEntity`.
* `Spawn.CreateAtRotation(assetName, x, y, rot)`: Spawns an asset with rotation in degrees.
* `Spawn.CreateBatch(assetName, positionsTable)`: Spawns multiple entities. Format: `{ {x=1, y=2}, {x=3, y=4} }`.
* `Spawn.Clone(entity, x, y)`: Duplicates an existing entity at specified coordinates.
* `Spawn.GetAvailableAssets()`: Returns a table list of all spawnable asset names in the game.
* `Spawn.Exists(assetName)`: Returns `true` if an asset exists with that name.

---

### 7. 🌍 World & Physics API (`World` & `Physics`)
* `World.GetEntities()`: Returns a table of all active root entities in the scene.
* `World.GetEntitiesInRadius(x, y, radius)`: Returns entities within a radius.
* `World.GetEntitiesInBox(x, y, width, height)`: Returns entities inside a box.
* `World.GetEntityById(instanceId)`: Finds an entity by its Unity instance ID.
* `World.GetEntitiesByName(name)`: Returns entities whose names contain the string.
* `World.GetEntitiesByTag(tag)`: Returns entities matching a Unity tag.
* `World.GetEntityCount()`: Returns total count of active physical objects.
* `World.GetBounds()`: Returns boundaries table: `{ minX = -100, maxX = 100, minY = -50, maxY = 50 }`.
* `World.GetMouseWorldPosition()`: Returns `{ x = ..., y = ... }`.
* `World.GetCameraPosition()`: Returns main camera coordinates `{ x = ..., y = ... }`.
* `Physics.Explode(x, y, radius, force)`: Triggers a physics explosion that throws entities away from the center point.

---

### 8. ⏱️ Time API (`Time`)
* `Time.GetDeltaTime()`: Returns frame elapsed time in seconds.
* `Time.GetTime()`: Returns total running time since launch.
* `Time.GetFixedDeltaTime()`: Returns physics step time.
* `Time.GetTimeScale()`: Returns current game simulation speed.
* `Time.SetTimeScale(scale)`: Locks simulation speed (e.g., `0.2` for slow-mo, `1.0` for normal).
* `Time.GetFrameRate()`: Returns current FPS.

---

### 9. 🗺️ Map & Environment API (`Map`)
* `Map.SetGravity(gx, gy)`: Changes world gravity vector (Default: `0, -9.81`).
* `Map.GetGravity()`: Returns `{ x = ..., y = ... }`.
* `Map.SetTimeScale(scale)`: Alias for `Time.SetTimeScale(scale)`.
* `Map.CreateWall(x, y, width, height, material)`: Spawns an indestructible structural wall (`"Metal"`, `"Wood"`, `"Concrete"`).
* `Map.GetBounds()`: Returns map boundary table.
* `Map.GetEntities()`: Returns all entities in the map.
* `Map.Register(mapConfig)`: Registers a map preset into the Toybox Maps tab.

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

### 10. ⚔️ Custom Item API (`Item`)
* `Item.Register(configTable)`: Registers an item into the Custom Toybox (`Tab` menu).
* `Item.Unregister(itemName)`: Unregisters an item.
* `Item.GetRegisteredItems()`: Returns a table of all registered custom item names.
* `Item.GetItem(itemName)`: Returns item info `{ name, description, baseItem }`.
* `Item.Spawn(itemName, x, y)`: Spawns a registered custom item at world coordinates.
* `Item.OnSpawn(configTable, function(entity) ... end)`: Registers item and sets spawn callback.
* `Item.OnUse(configTable, function(entity) ... end)`: Registers item and binds activation action when activated/used.

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

### 11. 🛠️ Debug Drawing API (`Debug`)
* `Debug.DrawLine(x1, y1, x2, y2, duration)`: Draws a red debug line for `duration` seconds.
* `Debug.DrawCircle(cx, cy, radius, duration)`: Draws a red wireframe circle.
* `Debug.DrawBox(cx, cy, width, height, duration)`: Draws a red wireframe box centered at `(cx, cy)`.
* `Debug.GetEntityInfo(entity)`: Returns debug string containing name, instance ID, and coordinates.
* `Debug.IsEnabled()`: Returns `true`.

---

### 12. 📦 Mod Management API (`Mod`)
* `Mod.GetName()`: Returns current mod name.
* `Mod.GetVersion()`: Returns loader version (`"3.0.0"`).
* `Mod.GetPath()`: Returns absolute path to current mod directory.
* `Mod.GetAllMods()`: Returns table of all loaded mod names.
* `Mod.IsLoaded(modName)`: Returns `true` if the specified mod is loaded.
* `Mod.Reload(modName)`: Hot-reloads the mod by re-running its `main.lua`.
* `Mod.GetMetadata()`: Returns `{ name = ..., dir = ... }`.

---

### 13. 📐 Extended Math API (`math`)
Engine additions to Lua's standard `math` library:
* `math.Clamp(val, min, max)`: Clamps number between min and max.
* `math.Lerp(a, b, t)`: Linear interpolation between `a` and `b` by factor `t`.
* `math.DegToRad(deg)`: Converts degrees to radians.
* `math.RadToDeg(rad)`: Converts radians to degrees.
* `math.Distance(x1, y1, x2, y2)`: Distance between two 2D points.
* `math.Angle(x1, y1, x2, y2)`: Angle in degrees from point 1 to point 2.
* `math.Random(min, max)`: Returns a random floating-point number.
* `math.RandomInt(min, max)`: Returns a random integer between min and max.

---

## ⚠️ Disclaimer
This is an **unofficial, experimental test project** created for educational and safety-oriented modding research. It is not affiliated with Studio Minus. Use at your own risk.
