# PPG-LuaLoader

> **WIP.** Non-official Lua mod loader for People Playground. This is a personal test project.

---

## 🛠️ About Project
**PPG-LuaLoader** is an experimental, sandboxed Lua modding runtime designed for **People Playground**. 
Following recent security concerns regarding traditional C# DLL mods, this project explores a safe execution layer using **MoonSharp** (pure C# Lua interpreter) to allow secure, script-based modding without risking system-level vulnerabilities.

---

## ✨ Features (V3.0 Final)
- 🔒 **Hard Sandboxing:** Completely strips dangerous OS, IO, and dynamic package execution layers.
- 🎮 **Live Interactive API:** Binds Unity's frame loops, keyboard/mouse inputs, and physics directly to Lua.
- 🩸 **Gore & Medical Integration:** Full control over PPG's native limb joints, crushing, acid disintegration, and `LIFE` serum mechanics.
- 🖼️ **Custom Toybox UI:** A dedicated, custom in-game `Tab` menu supporting adjustable slot sizes, pixel-art PNG loaders, and single-item `E`-key spawning.
- 🌍 **Phase 6 Map Builder:** Dynamic gravity controls, slow-motion bullet time, and runtime structural wall generation (`Map.CreateWall`).

---

## 🚀 Getting Started (For Testers)

1. **Install BepInEx:** Ensure `BepInEx 5 (Mono x64)` is correctly installed in your People Playground directory.
2. **Deploy Loader:** Place `PPG_LuaLoader.dll` and `MoonSharp.Interpreter.dll` into your `BepInEx/plugins/` folder.
3. **Install Lua Mods:** Create mod folders inside the official `Mods/` directory (e.g., `Mods/PlasmaBladeMod/`), featuring a `main.lua` entry script.

---

## 📝 Example `main.lua`

```lua
function Initialize()
    Log("Initializing Custom Mod Suite...")

    -- Register a custom weapon
    Item.Register({
        name = "Flaming Plasma Blade",
        description = "A blazing custom blade created purely in Lua!",
        baseItem = "Sword",
        onSpawn = function(sword)
            sword:Ignite()
            sword:SetTemperature(1000.0)
        end
    })
end

function Update(dt)
    -- Frame tick logic here
end
