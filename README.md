# PPG-LuaLoader

> **WIP.** Non-official Lua mod loader for People Playground. This is a personal test project.

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

Every Lua mod resides in its own folder inside `People Playground/Mods/<ModName>/`.
The engine boots from **`main.lua`** and reads metadata from an optional **`mod.json`** (`Name`, `ThumbnailPath`).

---

## 1. 🔄 Lifecycle Hooks

Define these functions globally in your `main.lua`. The loader calls them automatically:

```lua
function Initialize()
    -- Called ONCE when the mod is loaded and verified
    Log("Mod initialized successfully.")
end

function Update(dt)
    -- Called EVERY FRAME
    -- dt (number): Time.deltaTime in seconds
end
```

---

## 2. 📢 Global Logging

Direct logging to the BepInEx console and game log output, automatically tagged with your mod name (`[YourModName]`):

| Function | Parameters | Description |
| :--- | :--- | :--- |
| `Log(msg)` | `msg` *(string)* | Writes an informational message to the log. |
| `LogWarning(msg)` | `msg` *(string)* | Writes a warning message to the log. |
| `LogError(msg)` | `msg` *(string)* | Writes an error message to the log. |

---

## 3. 📡 Global Event Listeners

Register callbacks to intercept engine-level events across all entities and input devices.

```lua
-- Entity Events
OnSpawn(function(entity) ... end)
OnDelete(function(entity) ... end)

-- Collision Events
OnCollision(function(me, other, impulse) ... end)
OnCollisionEnter(function(me, other, impulse) ... end)
OnCollisionExit(function(me, other, impulse) ... end)

-- Input Events
OnKeyDown(function(keyCodeString) ... end)
OnKeyUp(function(keyCodeString) ... end)
OnMouseDown(function(buttonIndex) ... end)
OnMouseUp(function(buttonIndex) ... end)
```

| Event Listener | Callback Signature | Description |
| :--- | :--- | :--- |
| `OnSpawn(fn)` | `fn(entity: LuaEntity)` | Fired whenever an entity is instantiated via Spawn/Item APIs. |
| `OnDelete(fn)` | `fn(entity: LuaEntity)` | Fired right before an entity is destroyed via `:Destroy()` or `:Delete()`. |
| `OnCollision(fn)` | `fn(me: LuaEntity, other: LuaEntity, impulse: number)` | Fired on collision contact. |
| `OnCollisionEnter(fn)` | `fn(me: LuaEntity, other: LuaEntity, impulse: number)` | Fired when two colliders first make physical contact. |
| `OnCollisionExit(fn)` | `fn(me: LuaEntity, other: LuaEntity, impulse: number)` | Fired when two colliders separate (`impulse` is `0`). |
| `OnKeyDown(fn)` | `fn(key: string)` | Fired on any keyboard key down (e.g., `"Space"`, `"F"`, `"Alpha1"`). |
| `OnKeyUp(fn)` | `fn(key: string)` | Fired on any keyboard key release. |
| `OnMouseDown(fn)` | `fn(btn: number)` | Fired on mouse press (`0` = Left, `1` = Right, `2` = Middle). |
| `OnMouseUp(fn)` | `fn(btn: number)` | Fired on mouse button release. |

---

## 4. 🧭 Vector2 & `LuaVector2`

A dedicated 2D vector object providing arithmetic and spatial helper functions.

### Constructor
* `Vector2.New(x, y)` $\rightarrow$ Returns a new `LuaVector2` instance.

### Fields & Methods
* `vec.x`, `vec.y` *(number)*: Vector coordinates.
* `vec:Magnitude()` $\rightarrow$ *(number)* Length of the vector $\sqrt{x^2 + y^2}$.
* `vec:Normalized()` $\rightarrow$ *(LuaVector2)* Unit vector (length of 1). Returns `(0, 0)` if magnitude is near zero.
* `vec:Dot(otherVec)` $\rightarrow$ *(number)* Dot product of two vectors.
* `vec:Distance(otherVec)` $\rightarrow$ *(number)* Euclidean distance between two vectors.

```lua
local v1 = Vector2.New(3, 4)
Log("Magnitude: " .. tostring(v1:Magnitude())) -- 5

local v2 = Vector2.New(0, 0)
Log("Distance: " .. tostring(v1:Distance(v2)))  -- 5
```

---

## 5. 🦾 Entity API (`LuaEntity`)

The core object wrapping spawned ragdolls, props, weapons, and custom objects.

### 5.1 Transform & Hierarchy
| Method | Returns | Description |
| :--- | :--- | :--- |
| `entity:SetPosition(x, y)` | `nil` | Teleports the root object to world coordinates and syncs 2D physics. |
| `entity:GetPosition()` | `LuaVector2` | Returns the current world position. |
| `entity:SetRotation(angle)` | `nil` | Sets the Z-axis rotation in degrees and syncs physics. |
| `entity:GetRotation()` | `number` | Returns current Z-axis angle in degrees. |
| `entity:SetScale(x, y)` | `nil` | Sets local scale. |
| `entity:GetScale()` | `LuaVector2` | Returns local scale. |
| `entity:SetLocalPosition(x, y)`| `nil` | Sets position relative to its parent. |
| `entity:GetLocalPosition()` | `LuaVector2` | Returns position relative to its parent. |
| `entity:SetParent(parentEntity)` | `nil` | Sets parent entity hierarchy (pass `nil` to unparent). |
| `entity:GetParent()` | `LuaEntity` | Returns parent `LuaEntity`, or `nil` if root. |

### 5.2 Physics
Applies across all child `Rigidbody2D` instances in the ragdoll or composite structure:

| Method | Returns | Description |
| :--- | :--- | :--- |
| `entity:AddForce(fx, fy)` | `nil` | Applies an instant impulse force to all parts. |
| `entity:AddForceAtPosition(fx, fy, px, py)` | `nil` | Applies an impulse force at a specific world coordinate. |
| `entity:SetVelocity(vx, vy)` | `nil` | Sets the linear velocity of all body parts directly. |
| `entity:GetVelocity()` | `LuaVector2` | Returns linear velocity of the primary body part. |
| `entity:AddVelocity(vx, vy)` | `nil` | Adds a velocity delta vector to all rigidbodies. |
| `entity:SetAngularVelocity(speed)` | `nil` | Sets rotational speed in degrees/sec. |
| `entity:GetAngularVelocity()` | `number` | Returns current rotational velocity. |
| `entity:AddTorque(torque)` | `nil` | Applies an instant rotational impulse. |
| `entity:SetMass(totalMass)` | `nil` | Divides `totalMass` equally across all physical parts. |
| `entity:GetMass()` | `number` | Returns the combined mass of all rigidbodies. |
| `entity:SetGravityScale(scale)` | `nil` | Multiplies global gravity (`0` = weightless). |
| `entity:GetGravityScale()` | `number` | Returns current gravity multiplier. |
| `entity:SetKinematic(enabled)` | `nil` | Toggles Rigidbody between `Kinematic` and `Dynamic`. |
| `entity:IsKinematic()` | `boolean` | Checks if physics calculations are disabled (kinematic). |
| `entity:SetCollisionEnabled(enabled)` | `nil` | Enables or disables all attached `Collider2D` components. |
| `entity:IsCollisionEnabled()` | `boolean` | Checks if collision is active. |
| `entity:SetDrag(drag)` | `nil` | Sets linear air resistance. |
| `entity:SetAngularDrag(drag)` | `nil` | Sets rotational air resistance. |
| `entity:Freeze()` | `nil` | Freezes position and rotation (`RigidbodyConstraints2D.FreezeAll`). |
| `entity:Unfreeze()` | `nil` | Clears all movement constraints. |

### 5.3 Identity, State & Vitals
| Method | Returns | Description |
| :--- | :--- | :--- |
| `entity:IsAlive()` | `boolean` | Checks whether consciousness > 0.01 or limb health > 0. |
| `entity:IsDeleted()` | `boolean` | Checks if the underlying Unity GameObject has been destroyed. |
| `entity:Destroy()` | `nil` | Dispatches `OnDelete` and destroys the GameObject immediately. |
| `entity:Delete()` | `nil` | Alias for `entity:Destroy()`. |
| `entity:GetInstanceId()` | `number` | Returns Unity's unique instance ID. |
| `entity:GetName()` | `string` | Gets GameObject name. |
| `entity:SetName(name)` | `nil` | Renames GameObject. |
| `entity:GetType()` | `string` | Returns the class name of the root component. |
| `entity:GetComponents()` | `table` | Returns an array of component type names attached to this entity. |
| `entity:HasComponent(name)` | `boolean` | Checks if a component exists on this entity (case-insensitive). |

### 5.4 Health, Combat & Damage Effects
| Method | Returns | Description |
| :--- | :--- | :--- |
| `entity:Ignite()` | `nil` | Ignites all flammable parts (`PhysicalBehaviour.Ignite()`). |
| `entity:Electrocute(charge)` | `nil` | Adds electric charge, triggering sparks, shocks, or Android failure. |
| `entity:SetTemperature(celsius)` | `nil` | Forces physical temperature (e.g., `1000` for red-hot metal). |
| `entity:GetTemperature()` | `number` | Returns current body/part temperature in Celsius. |
| `entity:SetHealth(value)` | `nil` | Clamps and sets health (0 to 100) across all limbs. |
| `entity:GetHealth()` | `number` | Returns the average health across all limbs. |
| `entity:SetMaxHealth(value)` | `nil` | Sets `InitialHealth` for all limbs. |
| `entity:GetMaxHealth()` | `number` | Returns max health of the base limb. |
| `entity:SetColor(r, g, b, a)` | `nil` | Sets `SpriteRenderer.color` (values from `0.0` to `1.0`). |
| `entity:GetColor()` | `table` | Returns `{ r = ..., g = ..., b = ..., a = ... }`. |
| `entity:SetVisible(bool)` | `nil` | Enables/disables all SpriteRenderers. |
| `entity:IsVisible()` | `boolean` | Returns sprite visibility state. |
| `entity:SetLayer(layerIndex)` | `nil` | Sets GameObject layer recursively across all children. |
| `entity:GetLayer()` | `number` | Returns root layer index. |
| `entity:SetTag(tag)` | `nil` | Registers a custom string tag on this entity. |
| `entity:HasTag(tag)` | `boolean` | Checks whether the custom tag is present. |
| `entity:RemoveTag(tag)` | `nil` | Removes the custom tag. |
| `entity:Dismember()` | `nil` | Detaches joints, adds explosive radial force, and sets bleeding rate to 15. |
| `entity:Crush()` | `nil` | Crushes flesh, bones, and organs into bloody pulp. |
| `entity:Disintegrate()` | `nil` | Vaporizes flesh down to the clean skeleton. |
| `entity:Heal()` | `nil` | Extinguishes fire, sets temperature to 37°C, mends bones, stops bleeding, injects 20 units of `LIFE` serum, and restores consciousness. |
| `entity:Inject(liquid, amount)` | `nil` | Injects fluids directly into the circulatory system (e.g., `"LIFE"`, `"ACID"`, `"BLOOD"`). |

### 5.5 Collision Masking & Spatial Queries
| Method | Returns | Description |
| :--- | :--- | :--- |
| `entity:GetColliders()` | `table` | Returns an array of attached collider class names. |
| `entity:SetCollisionLayer(layer)` | `nil` | Alias for `SetLayer(layer)`. |
| `entity:GetCollisionLayer()` | `number` | Returns current collision layer. |
| `entity:SetCollisionMask(mask)` | `nil` | Sets layer collision interaction using a 32-bit integer bitfield. |
| `entity:GetCollisionMask()` | `number` | Returns full collision mask (`~0`). |
| `entity:IgnoreCollision(other, bool)` | `nil` | Ignores or restores physics collision against another specific `LuaEntity`. |
| `entity:OverlapCircle(radius)` | `table` | Returns array of `LuaEntity` instances whose colliders overlap this circle. |
| `entity:OverlapBox(w, h)` | `table` | Returns array of `LuaEntity` instances whose colliders overlap this oriented box. |

---

## 6. ⌨️ Input API (`Input`)

Polled inputs for keyboards and mouse actions:

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Input.IsKeyDown(keyName)` | `boolean` | `true` only on the frame the key was pressed down (e.g. `"F"`, `"Space"`, `"E"`). |
| `Input.IsKeyHeld(keyName)` | `boolean` | `true` while the key is being held down. |
| `Input.IsKeyUp(keyName)` | `boolean` | `true` only on the frame the key was released. |
| `Input.IsMouseButtonDown(button)`| `boolean` | `true` on mouse press down (`0` = Left, `1` = Right, `2` = Middle). |
| `Input.IsMouseButtonHeld(button)`| `boolean` | `true` while mouse button is held down. |
| `Input.IsMouseButtonUp(button)` | `boolean` | `true` on mouse button release. |
| `Input.GetMouseScrollDelta()` | `number` | Vertical scroll delta of the mouse wheel. |
| `Input.GetMouseWorldPosition()` | `table` | Returns `{ x = ..., y = ... }` world coordinates under the cursor. |

---

## 7. 👥 Spawn API (`Spawn`)

Spawns native game assets (such as `"Human"`, `"Android"`, `"Sword"`, etc.):

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Spawn.Create(assetName, x, y)` | `LuaEntity` | Instantiates a native prefab at world coordinates `(x, y)`. |
| `Spawn.CreateAtRotation(name, x, y, rot)` | `LuaEntity` | Instantiates a prefab with a specific Z rotation angle in degrees. |
| `Spawn.CreateBatch(name, posTable)` | `table` | Instantiates multiple entities. `posTable = { {x=1, y=2}, {x=3, y=4} }`. |
| `Spawn.Clone(entity, x, y)` | `LuaEntity` | Clones an existing `LuaEntity` and places it at `(x, y)`. |
| `Spawn.GetAvailableAssets()` | `table` | Returns an array of all native spawnable asset names found in game memory. |
| `Spawn.Exists(assetName)` | `boolean` | Checks if a spawnable asset exists by name. |

---

## 8. 🌍 World & Physics API (`World` & `Physics`)

### 8.1 World Queries
| Method | Returns | Description |
| :--- | :--- | :--- |
| `World.GetEntities()` | `table` | Returns all active root `LuaEntity` objects in the scene. |
| `World.GetEntitiesInRadius(x, y, r)` | `table` | Returns entities within radius `r` of `(x, y)`. |
| `World.GetEntitiesInBox(x, y, w, h)` | `table` | Returns entities inside an axis-aligned box. |
| `World.GetEntityById(instanceId)` | `LuaEntity` | Finds a GameObject by its Unity Instance ID. |
| `World.GetEntitiesByName(name)` | `table` | Finds entities containing `name` in their GameObject name. |
| `World.GetEntitiesByTag(tag)` | `table` | Finds entities matching Unity tag. |
| `World.GetEntityCount()` | `number` | Returns total number of active physical bodies. |
| `World.GetBounds()` | `table` | Returns simulation boundaries: `{ minX = -100, maxX = 100, minY = -50, maxY = 50 }`. |
| `World.GetMouseWorldPosition()` | `table` | Alias for `Input.GetMouseWorldPosition()`. |
| `World.GetCameraPosition()` | `table` | Returns main camera coordinates `{ x = ..., y = ... }`. |

### 8.2 Physics
* `Physics.Explode(x, y, radius, force)`: Creates an explosion at `(x, y)`. Propels all rigidbodies within `radius` outward using distance-attenuated impulse force.

```lua
-- Detonate an explosive shockwave at mouse location on Right Click
if Input.IsMouseButtonDown(1) then
    local m = Input.GetMouseWorldPosition()
    Physics.Explode(m.x, m.y, 8.0, 50.0)
end
```

---

## 9. ⏱️ Time API (`Time`)

Control simulation speed and query frame times:

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Time.GetDeltaTime()` | `number` | Elapsed time since last frame in seconds (`Time.deltaTime`). |
| `Time.GetTime()` | `number` | Total time elapsed since game launch (`Time.time`). |
| `Time.GetFixedDeltaTime()` | `number` | Physics step interval (`Time.fixedDeltaTime`). |
| `Time.GetTimeScale()` | `number` | Current simulation time scale. |
| `Time.SetTimeScale(scale)` | `nil` | Locks game speed (clamped between `0.01` and `10.0`). Prevents the engine from resetting slow-mo. |
| `Time.GetFrameRate()` | `number` | Current instantaneous Frames Per Second (FPS). |

---

## 10. 🗺️ Map & Environment API (`Map`)

Construct custom levels, change physics constants, or register Toybox presets:

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Map.SetGravity(gx, gy)` | `nil` | Sets 2D world gravity vector (Default: `0, -9.81`). |
| `Map.GetGravity()` | `table` | Returns `{ x = ..., y = ... }` gravity. |
| `Map.SetTimeScale(scale)` | `nil` | Alias for `Time.SetTimeScale(scale)`. |
| `Map.CreateWall(x, y, w, h, mat)` | `LuaEntity` | Instantly spawns a static, indestructible structural wall. `mat`: `"Metal"`, `"Wood"`, or `"Concrete"`. |
| `Map.GetBounds()` | `table` | Returns `{ minX, maxX, minY, maxY }`. |
| `Map.GetEntities()` | `table` | Alias for `World.GetEntities()`. |
| `Map.Register(config)` | `nil` | Registers a map layout into the Toybox **Maps** tab. |

### Map Registration Example:
```lua
Map.Register({
    name = "Zero-G Killbox",
    description = "Enclosed metal arena with zero gravity.",
    gravity = { 0, 0 },
    structures = {
        { x = 0, y = -10, width = 30, height = 1, material = "Metal" },
        { x = 0, y = 10, width = 30, height = 1, material = "Metal" },
        { x = -15, y = 0, width = 1, height = 20, material = "Metal" },
        { x = 15, y = 0, width = 1, height = 20, material = "Metal" }
    }
})
```

---

## 11. ⚔️ Custom Item API (`Item`)

Registers items into the Custom Toybox (`Tab` menu) and handles usage callbacks.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Item.Register(configTable)` | `nil` | Registers an item template in the Toybox. |
| `Item.Unregister(itemName)` | `nil` | Removes an item from the registry. |
| `Item.GetRegisteredItems()` | `table` | Returns array of all registered custom item names. |
| `Item.GetItem(itemName)` | `table` | Returns `{ name, description, baseItem }` or `nil`. |
| `Item.Spawn(itemName, x, y)` | `LuaEntity` | Spawns the custom item at world coordinates. |
| `Item.OnSpawn(configTable, fn)` | `nil` | Shortcut to assign `configTable.onSpawn = fn` and register. |
| `Item.OnUse(configTable, fn)` | `nil` | Registers an item and attaches an activation listener triggered when the player uses the item. |

### Configuration Table Schema:
```lua
Item.Register({
    name = "Plasma Blade",
    description = "Slices and superheats targets upon contact.",
    baseItem = "Sword",             -- Native prefab used as base
    sprite = "sprites/plasma.png",  -- Path relative to mod directory
    onSpawn = function(entity)
        entity:SetColor(0.2, 0.8, 1.0, 1.0)
        entity:SetTemperature(500)
    end
})

-- Item with Activation Callback (F / Context Action)
Item.OnUse({
    name = "Overcharge Cell",
    description = "Press Use to discharge massive electricity.",
    baseItem = "Flashlight"
}, function(entity)
    local pos = entity:GetPosition()
    Physics.Explode(pos.x, pos.y, 5, 20)
    entity:Electrocute(100)
end)
```

---

## 12. 🛠️ Debug API (`Debug`)

Visual primitives drawn in world-space using immediate-mode rendering (`OnPostRender`):

| Method | Parameters | Description |
| :--- | :--- | :--- |
| `Debug.DrawLine(x1, y1, x2, y2, duration)` | Numbers | Draws a red debug line from point A to point B for `duration` seconds. |
| `Debug.DrawCircle(cx, cy, radius, duration)` | Numbers | Draws a red wireframe circle. |
| `Debug.DrawBox(cx, cy, width, height, duration)` | Numbers | Draws a red wireframe box centered at `(cx, cy)`. |
| `Debug.GetEntityInfo(entity)` | `entity` | Returns a debug string: `"Name (ID:...) Pos:(x,y,z)"`. |
| `Debug.IsEnabled()` | None | Returns `true`. |

```lua
-- Visualize an entity's bounding box and trajectory line every frame
function Update(dt)
    local m = Input.GetMouseWorldPosition()
    Debug.DrawCircle(m.x, m.y, 1.5, 0.02)
end
```

---

## 13. 📦 Mod Management API (`Mod`)

Access metadata and manage loaded mods dynamically:

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Mod.GetName()` | `string` | Returns the current mod's directory/display name. |
| `Mod.GetVersion()` | `string` | Returns the Lua Loader runtime version (`"3.0.0"`). |
| `Mod.GetPath()` | `string` | Returns the absolute file path to the mod folder. |
| `Mod.GetAllMods()` | `table` | Returns array of names of all currently loaded Lua mods. |
| `Mod.IsLoaded(modName)` | `boolean` | Checks if a given mod is active. |
| `Mod.Reload(modName)` | `nil` | Hot-reloads the target mod by re-executing its `main.lua`. |
| `Mod.GetMetadata()` | `table` | Returns `{ name = ..., dir = ... }`. |

---

## 14. 📐 Math Extensions (`math`)

The engine extends Lua’s standard `math` library with Unity-specific math calculations:

| Function | Parameters | Returns | Description |
| :--- | :--- | :--- | :--- |
| `math.Clamp(val, min, max)` | `(number, number, number)` | `number` | Clamps value between `min` and `max`. |
| `math.Lerp(a, b, t)` | `(number, number, number)` | `number` | Linear interpolation between `a` and `b` by factor `t`. |
| `math.DegToRad(deg)` | `(number)` | `number` | Converts degrees to radians. |
| `math.RadToDeg(rad)` | `(number)` | `number` | Converts radians to degrees. |
| `math.Distance(x1, y1, x2, y2)` | `(number, number, number, number)` | `number` | Calculates 2D Euclidean distance. |
| `math.Angle(x1, y1, x2, y2)` | `(number, number, number, number)` | `number` | Returns angle in degrees from point 1 to point 2. |
| `math.Random(min, max)` | `(number, number)` | `number` | Returns a random float between `min` and `max`. |
| `math.RandomInt(min, max)` | `(int, int)` | `number` | Returns a random integer between `min` (inclusive) and `max` (exclusive). |

---

## ⚠️ Disclaimer
This is an **unofficial, experimental test project** created for educational and safety-oriented modding research. It is not affiliated with Studio Minus. Use at your own risk.
