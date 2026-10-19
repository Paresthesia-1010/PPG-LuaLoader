# PPG-LuaLoader

> **WIP.** Non-official Lua mod loader for People Playground. This is a personal test project.

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia/PPG-LuaLoader/releases)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 Download & Installation

Looking to test the loader right away? Grab the latest compiled binaries from the official release page.

👉 **[Download PPG-LuaLoader v3.0 Final Release](https://github.com/Paresthesia/PPG-LuaLoader/releases)**

### Installation Steps
1. Make sure you have **People Playground** installed via Steam.
2. Download and extract **BepInEx 5 (Mono x64)** into your game root directory (`People Playground/`).
3. Download `PPG_LuaLoader.dll` and `MoonSharp.Interpreter.dll` from the [Releases Page](https://github.com/Paresthesia/PPG-LuaLoader/releases).
4. Drop both DLL files into your **`BepInEx/plugins/`** folder.
5. Launch the game!

---

## 📖 Quick Navigation & Modding Guide
Want to write your own Lua mods? Skip straight to the guides below:
* 🚀 **[Quickstart: Your First Mod](#-quickstart-writing-your-first-lua-mod)**
* 🎛️ **[API Reference Overview](#-lua-api-reference)**

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
