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
To create a custom mod, navigate to your game's **`Mods/`** directory and create a dedicated folder for your mod (e.g., `Mods/PlasmaBladeMod/`). 

Your mod folder should be structured like this:
```text
People Playground/
 └── Mods/
      └── PlasmaBladeMod/
           ├── mod.json       <-- Mod metadata & display name
           ├── thumb.png      <-- Custom icon image (Thumbnail for the Toybox menu)
           └── main.lua       <-- Main Lua execution script
```

### 📄 1. `mod.json` (Configuration)
Create a file named `mod.json` inside your mod folder:
```json
{
  "Name": "Flaming Plasma Blade",
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
-- =========================================================
-- Flaming Plasma Blade Mod Template
-- =========================================================

function Initialize()
    Log("Initializing Flaming Plasma Blade...")

    -- Register a custom weapon into the Custom Toybox
    Item.Register({
        name = "Flaming Plasma Blade",
        description = "A blazing custom blade created purely in Lua!",
        baseItem = "Sword",
        sprite = "thumb.png", -- Optional: Use your PNG as the item's world texture
        
        -- Triggered automatically when spawned from the Custom Toybox
        onSpawn = function(swordEntity)
            Log("Plasma Blade materialized in physical space!")
            swordEntity:Ignite()
            swordEntity:SetTemperature(1000.0)
        end
    })
end

function Update(dt)
    -- Frame tick logic here
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
