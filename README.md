# PPG-LuaLoader

> **WIP.** Non-official Lua mod loader for People Playground. This is a personal test project.

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia/PPG-LuaLoader/releases)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 Download & Installation

Looking to test the loader right away? Grab the latest compiled binaries from the official release page.

👉 **[Download PPG-LuaLoader v3.0 Final Release](https://github.com/Paresthesia/PPG-LuaLoader/releases/tag/main)**

### Installation Steps
1. Make sure you have **People Playground** installed via Steam.
2. Download and extract **BepInEx 5 (Mono x64)** into your game root directory (`People Playground/`).
3. Download `PPG_LuaLoader.dll` and `MoonSharp.Interpreter.dll` from the [Releases Page](https://github.com/Paresthesia/PPG-LuaLoader/releases).
4. Drop both DLL files into your **`BepInEx/plugins/`** folder.
5. Launch the game!

---

## 📖 Quick Navigation & Modding Guide
* 🚀 **[Quickstart: Your First Mod](#-quickstart-writing-your-first-lua-mod)**
* 📚 **[Complete Lua API Reference](#-complete-lua-api-reference)**

---

## 🛠️ About Project
**PPG-LuaLoader** is an experimental, sandboxed Lua modding runtime designed for **People Playground**. 
Following recent security concerns regarding traditional C# DLL mods, this project explores a safe execution layer using **MoonSharp** (pure C# Lua interpreter) to allow secure, script-based modding without risking system-level vulnerabilities.

---

## 🚀 Quickstart: Writing Your First Lua Mod

To create a custom mod, navigate to your game's **`Mods/`** directory and create a new folder (e.g., `Mods/MyFirstLuaMod/`). Inside that folder, create a file named **`main.lua`**.

Here is a complete template showcasing item registration, spawning, and gore manipulation:

```lua
-- =========================================================
-- My First PPG Lua Mod Template
-- =========================================================

function Initialize()
    Log("MyFirstLuaMod initialized successfully!")

    -- 1. Register a custom weapon into the Custom Toybox (Press Tab in-game)
    Item.Register({
        name = "Electric Katana",
        description = "A high-voltage custom blade scripted in Lua.",
        baseItem = "Sword",
        sprite = "katana_icon.png", -- Optional: Place PNG in your mod folder
        
        -- Triggered automatically when spawned from the Toybox
        onSpawn = function(swordEntity)
            Log("Electric Katana spawned and charged!")
            swordEntity:Electrocute(50.0)
        end
    })
end

function Update(dt)
    local mouse = Input.GetMouseWorldPosition()

    -- Press [F] to spawn a test Human
    if Input.IsKeyDown("F") then
        local human = Spawn.Create("Human", mouse.x, mouse.y)
        if human ~= nil then
            Log("Spawned test dummy!")
        end
    end
end
```

---

## 📚 Complete Lua API Reference

Every Lua mod consists of a **`main.lua`** script containing two lifecycle hooks:
* **`Initialize()`**: Called once when the mod is loaded.
* **`Update(dt)`**: Called every frame while the game is running.

### 1. ⌨️ Input API
* `Input.IsKeyDown(keyName)`: Returns `true` on the single frame a key (e.g., `"F"`, `"Space"`, `"Tab"`, `"E"`, `"G"`) is pressed.
* `Input.GetMouseWorldPosition()`: Returns a table `{x, y}` representing 2D world mouse coordinates.

### 2. 👥 Spawn API
* `Spawn.Create(assetName, x, y)`: Spawns native entities like `"Human"` or `"Android"` and returns a `LuaEntity` wrapper.

### 3. 🦾 Entity API (`LuaEntity`)
Methods called on spawned entities:
* `entity:AddForce(fx, fy)`: Applies an impulse vector to all limbs.
* `entity:Ignite()`: Sets flammable entities on fire.
* `entity:Electrocute(volts)`: Discharges electricity into metal frames or Androids.
* `entity:SetTemperature(celsius)`: Forces body temperature change (e.g., `1000.0` for red-hot metal).
* `entity:Dismember()`: Severely shatters all physics joints for clean dismemberment.
* `entity:Crush()`: Crushes flesh and bones into bloody chunks.
* `entity:Disintegrate()`: Vaporizes flesh down to the skeleton.
* `entity:Heal()`: Extinguishes fire, resets body temperature to 37°C, mends bones, and injects native `LIFE` serum.
* `entity:Inject(liquidName, amount)`: Injects fluids like `"ACID"`, `"LIFE"`, or `"BLOOD"` directly into the bloodstream.
* `entity:Delete()`: Instantly removes the entity from the world.

### 4. ⚔️ Custom Item API
* `Item.Register(configTable)`: Registers a custom item into the Custom Toybox (`Tab` menu).
  * Properties inside `configTable`: `name`, `description`, `baseItem` (default: `"Sword"`), `sprite` (relative PNG path), and `onSpawn = function(entity) ... end`.

### 5. 🌍 Map & Environment API
* `Map.SetGravity(gx, gy)`: Dynamically changes 2D physics gravity.
* `Map.SetTimeScale(scale)`: Locks simulation speed for bullet-time effects (e.g., `0.2` for slow-mo).
* `Map.CreateWall(x, y, width, height, material)`: Instantly spawns a solid, indestructible structural wall (`"Metal"`, `"Wood"`, `"Concrete"`).
* `Map.Register(mapConfig)`: Registers a complete map preset with custom structures into the Toybox Maps tab.

---

## ⚠️ Disclaimer
This is an **unofficial, experimental test project** created for educational and safety-oriented modding research. It is not affiliated with Studio Minus. Use at your own risk.
```
