# PPG-LuaLoader

> **В разработке (WIP).** Неофициальный загрузчик Lua-модов для People Playground. Это личный тестовый проект.

**Языки:** [English](README.md) · [한국어](README.ko-KR.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja-JP.md) · Русский

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)  
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 Скачивание и установка

Чтобы сразу попробовать загрузчик, скачайте последнюю скомпилированную версию с официальной страницы релизов.

👉 **[Скачать PPG-LuaLoader v3.0 Final Release](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)**

### Инструкция по установке

1. Убедитесь, что у вас установлена версия **People Playground** из Steam.
2. Скачайте **BepInEx 5 (Mono x64)** и распакуйте его в корневую папку игры (`People Playground/`).
3. Скачайте `PPG_LuaLoader.dll` и `MoonSharp.Interpreter.dll` со [страницы релизов](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main).
4. Поместите оба DLL-файла в папку **`BepInEx/plugins/`**.
5. Запустите игру!

---

## 📖 Быстрая навигация и руководство по созданию модов

- 🚀 **[Быстрый старт: создание первого мода и структура папок](#-быстрый-старт-создание-первого-lua-мода)**
- 🎮 **[Использование внутриигрового Toybox](#-использование-внутриигрового-toybox)**
- 📚 **[Полный справочник Lua API](#-полный-справочник-lua-api)**

---

## 🛠️ О проекте

**PPG-LuaLoader** — это экспериментальная среда для выполнения Lua-модов в песочнице, разработанная для **People Playground**.

В связи с недавними опасениями по поводу безопасности традиционных модов в формате C# DLL проект исследует возможность создания более безопасного уровня выполнения с использованием **MoonSharp** — интерпретатора Lua, написанного на чистом C#. Цель проекта — предоставить возможность создавать модификации на основе скриптов, снижая риск уязвимостей на системном уровне.

---

## 🚀 Быстрый старт: создание первого Lua-мода

### 📁 Структура папки мода

Чтобы создать собственный мод, откройте каталог **`Mods/`** игры и создайте отдельную папку для мода. Например: `Mods/Sword/`

Структура папки мода должна выглядеть следующим образом:

```text
People Playground/
 └── Mods/
      └── Sword/
           ├── mod.json       <-- Метаданные мода и отображаемое название
           ├── thumb.png      <-- Пользовательская иконка (миниатюра в меню Toybox)
           └── main.lua       <-- Основной Lua-скрипт
```

### 📄 1. `mod.json` (файл конфигурации)

Создайте файл с именем `mod.json` в папке мода.

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

### 📜 2. `main.lua` (скрипт)

Создайте файл `main.lua` в той же папке, что и файл конфигурации.

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

## 🎮 Использование внутриигрового Toybox

Загрузите любую карту в игре и выполните следующие действия:

- Нажмите **`Tab`** или **`F1`**, чтобы открыть или закрыть окно **Custom Lua Toybox**.
- Нажмите на карточку пользовательского предмета, чтобы **выбрать его**. Выбранная карточка будет подсвечена изумрудно-зелёным цветом.
- Наведите курсор на нужное место и нажмите **`E`**, чтобы немедленно создать выбранный предмет.
  - Чтобы избежать конфликтов управления, загрузчик автоматически блокирует стандартное действие игры, назначенное на клавишу `E`.
- На вкладке **`Env`** в Toybox можно переключать режим **нулевой гравитации (Zero Gravity)** и включать замедление времени **Bullet Time** (замедление до 0,2 от обычной скорости).

---

# 📚 Полный справочник Lua API

Каждый Lua-мод состоит из скрипта `main.lua` и использует две функции жизненного цикла:

- **`Initialize()`**: вызывается один раз при загрузке мода.
- **`Update(dt)`**: вызывается каждый кадр во время работы игры. `dt` — интервал между кадрами в секундах.

---

### 1. 📢 Глобальный API журналирования

- `Log(message)`: выводит информационное сообщение в консоль BepInEx. Перед сообщением добавляется префикс `[ModName]`.
- `LogWarning(message)`: выводит предупреждение жёлтым цветом.
- `LogError(message)`: выводит сообщение об ошибке красным цветом.

---

### 2. 📡 Глобальные обработчики событий

Эти обработчики позволяют отслеживать события, происходящие в ходе игровой симуляции.

- `OnSpawn(function(entity) ... end)`: вызывается каждый раз при создании сущности.
- `OnDelete(function(entity) ... end)`: вызывается непосредственно перед удалением сущности.
- `OnCollision(function(me, other, impulse) ... end)`: вызывается при физическом столкновении.
- `OnCollisionEnter(function(me, other, impulse) ... end)`: вызывается в момент начала столкновения двух сущностей.
- `OnCollisionExit(function(me, other, impulse) ... end)`: вызывается при завершении столкновения двух сущностей. В этот момент `impulse` равен `0`.
- `OnKeyDown(function(keyName) ... end)`: вызывается при нажатии клавиши. Например: `"Space"`, `"F"`.
- `OnKeyUp(function(keyName) ... end)`: вызывается при отпускании клавиши.
- `OnMouseDown(function(button) ... end)`: вызывается при нажатии кнопки мыши. `0` = левая, `1` = правая, `2` = средняя.
- `OnMouseUp(function(button) ... end)`: вызывается при отпускании кнопки мыши.

---

### 3. 🧭 API Vector2 (`LuaVector2`)

- `Vector2.New(x, y)`: создаёт новый двумерный вектор.
- `vec.x`, `vec.y`: координаты X и Y.
- `vec:Magnitude()`: возвращает длину вектора.
- `vec:Normalized()`: возвращает нормализованный вектор единичной длины.
- `vec:Dot(otherVec)`: возвращает скалярное произведение двух векторов.
- `vec:Distance(otherVec)`: возвращает расстояние между двумя векторами.

---

### 4. 🦾 API Entity (`LuaEntity`)

Методы для работы с созданными объектами, регдоллами и предметами.

#### Преобразования и иерархия

- `entity:SetPosition(x, y)`: мгновенно перемещает сущность в мировые координаты и синхронизирует её физическое состояние.
- `entity:GetPosition()`: возвращает текущие мировые координаты в виде `LuaVector2`.
- `entity:SetRotation(angle)`: задаёт угол поворота вокруг оси Z в градусах.
- `entity:GetRotation()`: возвращает текущий угол поворота вокруг оси Z в градусах.
- `entity:SetScale(x, y)`: задаёт локальный масштаб.
- `entity:GetScale()`: возвращает локальный масштаб в виде `LuaVector2`.
- `entity:SetLocalPosition(x, y)`: задаёт положение относительно родительского объекта.
- `entity:GetLocalPosition()`: возвращает локальное положение относительно родительского объекта в виде `LuaVector2`.
- `entity:SetParent(parentEntity)`: присоединяет сущность к другой сущности. Передайте `nil`, чтобы отсоединить её.
- `entity:GetParent()`: возвращает родительскую сущность `LuaEntity` или `nil`.

#### Физика и движение

- `entity:AddForce(fx, fy)`: прикладывает импульс силы ко всем частям тела.
- `entity:AddForceAtPosition(fx, fy, px, py)`: прикладывает силу в указанной точке мировых координат.
- `entity:SetVelocity(vx, vy)`: немедленно задаёт линейную скорость всех частей объекта.
- `entity:GetVelocity()`: возвращает скорость основной части тела в виде `LuaVector2`.
- `entity:AddVelocity(vx, vy)`: добавляет вектор линейной скорости всем частям объекта.
- `entity:SetAngularVelocity(speed)`: задаёт угловую скорость в градусах в секунду.
- `entity:GetAngularVelocity()`: возвращает угловую скорость.
- `entity:AddTorque(torque)`: прикладывает вращательный импульс.
- `entity:SetMass(mass)`: равномерно распределяет общую массу между всеми частями тела.
- `entity:GetMass()`: возвращает суммарную массу всех частей объекта.
- `entity:SetGravityScale(scale)`: задаёт множитель гравитации. Значение `0` отключает воздействие гравитации.
- `entity:GetGravityScale()`: возвращает текущий множитель гравитации.
- `entity:SetKinematic(enabled)`: при значении `true` объект перестаёт реагировать на внешние физические силы.
- `entity:IsKinematic()`: возвращает `true`, если включён кинематический режим.
- `entity:SetCollisionEnabled(enabled)`: включает или отключает все коллайдеры.
- `entity:IsCollisionEnabled()`: возвращает `true`, если коллайдеры включены.
- `entity:SetDrag(drag)`: задаёт линейное сопротивление воздуха.
- `entity:SetAngularDrag(drag)`: задаёт сопротивление вращению.
- `entity:Freeze()`: полностью фиксирует положение и вращение.
- `entity:Unfreeze()`: снимает все ограничения движения.

#### Состояние, здоровье и удаление

- `entity:IsAlive()`: возвращает `true`, если человек находится в сознании или у частей тела осталось здоровье.
- `entity:IsDeleted()`: возвращает `true`, если базовый `GameObject` был удалён.
- `entity:Destroy()`: удаляет сущность и вызывает `OnDelete`.
- `entity:Delete()`: альтернативное имя для `entity:Destroy()`.
- `entity:GetInstanceId()`: возвращает целочисленный идентификатор экземпляра игрового движка.
- `entity:GetName()`: возвращает имя `GameObject` сущности.
- `entity:SetName(name)`: изменяет имя `GameObject` сущности.
- `entity:GetType()`: возвращает имя типа корневого компонента.
- `entity:GetComponents()`: возвращает таблицу со всеми именами подключённых компонентов.
- `entity:HasComponent(name)`: проверяет, присутствует ли у сущности указанный компонент.
- `entity:Ignite()`: поджигает все части, которые могут гореть.
- `entity:Electrocute(charge)`: разряжает электрический заряд на металлический каркас или андроида.
- `entity:SetTemperature(celsius)`: принудительно изменяет температуру тела. Например, значение `1000.0` раскаляет металл докрасна.
- `entity:GetTemperature()`: возвращает текущую температуру в градусах Цельсия.
- `entity:SetHealth(amount)`: задаёт здоровье всех частей тела. Допустимый диапазон — от `0` до `100`.
- `entity:GetHealth()`: возвращает среднее здоровье частей тела.
- `entity:SetMaxHealth(amount)`: переопределяет максимальное здоровье частей тела.
- `entity:GetMaxHealth()`: возвращает максимальное здоровье.
- `entity:SetColor(r, g, b, a)`: задаёт цветовой оттенок спрайта. Диапазон каждого значения — от `0.0` до `1.0`.
- `entity:GetColor()`: возвращает таблицу в формате `{ r = ..., g = ..., b = ..., a = ... }`.
- `entity:SetVisible(enabled)`: показывает или скрывает рендерер спрайта.
- `entity:IsVisible()`: возвращает `true`, если спрайт видим.
- `entity:SetLayer(layer)`: задаёт слой `GameObject`, включая дочерние объекты.
- `entity:GetLayer()`: возвращает текущий индекс слоя.
- `entity:SetTag(tag)`: присваивает сущности пользовательский строковый тег.
- `entity:HasTag(tag)`: возвращает `true`, если у сущности есть указанный пользовательский тег.
- `entity:RemoveTag(tag)`: удаляет указанный пользовательский тег.
- `entity:Dismember()`: сильно повреждает все физические суставы и вызывает кровотечение.
- `entity:Crush()`: раздавливает плоть и кости, превращая их в окровавленные обломки.
- `entity:Disintegrate()`: испаряет плоть, оставляя только скелет.
- `entity:Heal()`: тушит огонь, возвращает температуру к 37°C, восстанавливает кости, останавливает кровотечение и вводит сыворотку `LIFE`.
- `entity:Inject(liquidName, amount)`: вводит жидкость непосредственно в кровоток. Например: `"LIFE"`, `"ACID"`, `"BLOOD"`.

#### Коллайдеры и проверка пересечений

- `entity:GetColliders()`: возвращает таблицу с типами коллайдеров.
- `entity:SetCollisionLayer(layer)`: задаёт слой столкновений.
- `entity:GetCollisionLayer()`: возвращает слой столкновений.
- `entity:SetCollisionMask(maskBitfield)`: задаёт битовую маску столкновений слоёв.
- `entity:GetCollisionMask()`: возвращает маску столкновений.
- `entity:IgnoreCollision(otherEntity, ignoreBool)`: отключает или включает столкновения между двумя сущностями.
- `entity:OverlapCircle(radius)`: возвращает таблицу других сущностей в круговой области с центром в этой сущности.
- `entity:OverlapBox(width, height)`: возвращает таблицу других сущностей в прямоугольной области с центром в этой сущности.

---

### 5. ⌨️ API ввода (`Input`)

- `Input.IsKeyDown(keyName)`: возвращает `true` в кадре, когда клавиша нажата. Например: `"F"`, `"Space"`, `"Tab"`, `"E"`.
- `Input.IsKeyHeld(keyName)`: возвращает `true`, пока клавиша удерживается.
- `Input.IsKeyUp(keyName)`: возвращает `true` в кадре, когда клавиша отпущена.
- `Input.IsMouseButtonDown(button)`: возвращает `true` в кадре, когда нажата кнопка мыши. `0` = левая, `1` = правая, `2` = средняя.
- `Input.IsMouseButtonHeld(button)`: возвращает `true`, пока кнопка мыши удерживается.
- `Input.IsMouseButtonUp(button)`: возвращает `true` в кадре, когда кнопка мыши отпущена.
- `Input.GetMouseScrollDelta()`: возвращает величину вертикальной прокрутки колеса мыши.
- `Input.GetMouseWorldPosition()`: возвращает положение мыши в двумерных мировых координатах в формате `{ x = ..., y = ... }`.

---

### 6. 👥 API создания объектов (`Spawn`)

- `Spawn.Create(assetName, x, y)`: создаёт стандартную сущность, например `"Human"` или `"Android"`, и возвращает `LuaEntity`.
- `Spawn.CreateAtRotation(assetName, x, y, rot)`: создаёт объект с указанным углом поворота в градусах.
- `Spawn.CreateBatch(assetName, positionsTable)`: создаёт несколько сущностей одновременно. Формат: `{ {x=1, y=2}, {x=3, y=4} }`.
- `Spawn.Clone(entity, x, y)`: копирует существующую сущность в указанные координаты.
- `Spawn.GetAvailableAssets()`: возвращает таблицу со всеми именами объектов, доступных для создания в игре.
- `Spawn.Exists(assetName)`: возвращает `true`, если объект с указанным именем существует.

---

### 7. 🌍 API мира и физики (`World` и `Physics`)

- `World.GetEntities()`: возвращает таблицу всех активных корневых сущностей в сцене.
- `World.GetEntitiesInRadius(x, y, radius)`: возвращает сущности в пределах указанного радиуса.
- `World.GetEntitiesInBox(x, y, width, height)`: возвращает сущности в пределах указанной прямоугольной области.
- `World.GetEntityById(instanceId)`: находит сущность по идентификатору экземпляра Unity.
- `World.GetEntitiesByName(name)`: возвращает сущности, имена которых содержат указанную строку.
- `World.GetEntitiesByTag(tag)`: возвращает сущности, соответствующие указанному тегу Unity.
- `World.GetEntityCount()`: возвращает общее количество активных физических объектов.
- `World.GetBounds()`: возвращает информацию о границах в формате `{ minX = -100, maxX = 100, minY = -50, maxY = 50 }`.
- `World.GetMouseWorldPosition()`: возвращает мировые координаты мыши в формате `{ x = ..., y = ... }`.
- `World.GetCameraPosition()`: возвращает координаты основной камеры в формате `{ x = ..., y = ... }`.
- `Physics.Explode(x, y, radius, force)`: создаёт физический взрыв, отбрасывающий сущности от эпицентра.

---

### 8. ⏱️ API времени (`Time`)

- `Time.GetDeltaTime()`: возвращает время между кадрами в секундах.
- `Time.GetTime()`: возвращает общее время, прошедшее с момента запуска игры.
- `Time.GetFixedDeltaTime()`: возвращает временной интервал между обновлениями физики.
- `Time.GetTimeScale()`: возвращает текущую скорость игровой симуляции.
- `Time.SetTimeScale(scale)`: задаёт скорость симуляции. Например, `0.2` означает замедление, а `1.0` — обычную скорость.
- `Time.GetFrameRate()`: возвращает текущий FPS.

---

### 9. 🗺️ API карты и окружения (`Map`)

- `Map.SetGravity(gx, gy)`: изменяет вектор гравитации мира. Значения по умолчанию — `0, -9.81`.
- `Map.GetGravity()`: возвращает вектор гравитации в формате `{ x = ..., y = ... }`.
- `Map.SetTimeScale(scale)`: альтернативное имя для `Time.SetTimeScale(scale)`.
- `Map.CreateWall(x, y, width, height, material)`: создаёт неразрушаемую структурную стену. Доступные материалы: `"Metal"`, `"Wood"`, `"Concrete"`.
- `Map.GetBounds()`: возвращает таблицу с информацией о границах карты.
- `Map.GetEntities()`: возвращает все сущности на карте.
- `Map.Register(mapConfig)`: регистрирует пресет карты на вкладке Maps в Toybox.

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

### 10. ⚔️ API пользовательских предметов (`Item`)

- `Item.Register(configTable)`: регистрирует пользовательский предмет в Custom Toybox (меню `Tab`).
- `Item.Unregister(itemName)`: отменяет регистрацию указанного предмета.
- `Item.GetRegisteredItems()`: возвращает таблицу со всеми именами зарегистрированных пользовательских предметов.
- `Item.GetItem(itemName)`: возвращает информацию о предмете в формате `{ name, description, baseItem }`.
- `Item.Spawn(itemName, x, y)`: создаёт зарегистрированный пользовательский предмет в указанных мировых координатах.
- `Item.OnSpawn(configTable, function(entity) ... end)`: регистрирует предмет и задаёт функцию обратного вызова, выполняемую при его создании.
- `Item.OnUse(configTable, function(entity) ... end)`: регистрирует предмет и задаёт поведение при его активации или использовании.

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

### 11. 🛠️ API отладочной отрисовки (`Debug`)

- `Debug.DrawLine(x1, y1, x2, y2, duration)`: рисует красную отладочную линию в течение указанного количества секунд.
- `Debug.DrawCircle(cx, cy, radius, duration)`: рисует красную окружность в виде каркаса.
- `Debug.DrawBox(cx, cy, width, height, duration)`: рисует красный прямоугольный каркас с центром в точке `(cx, cy)`.
- `Debug.GetEntityInfo(entity)`: возвращает отладочную строку с именем, идентификатором экземпляра и координатами сущности.
- `Debug.IsEnabled()`: возвращает `true`.

---

### 12. 📦 API управления модами (`Mod`)

- `Mod.GetName()`: возвращает имя текущего мода.
- `Mod.GetVersion()`: возвращает версию загрузчика (`"3.0.0"`).
- `Mod.GetPath()`: возвращает абсолютный путь к каталогу текущего мода.
- `Mod.GetAllMods()`: возвращает таблицу со всеми именами загруженных модов.
- `Mod.IsLoaded(modName)`: возвращает `true`, если указанный мод загружен.
- `Mod.Reload(modName)`: повторно выполняет `main.lua` указанного мода, выполняя горячую перезагрузку.
- `Mod.GetMetadata()`: возвращает метаданные в формате `{ name = ..., dir = ... }`.

---

### 13. 📐 Расширенный математический API (`math`)

Расширения игрового движка для стандартной библиотеки Lua `math`.

- `math.Clamp(val, min, max)`: ограничивает число указанным минимальным и максимальным значениями.
- `math.Lerp(a, b, t)`: выполняет линейную интерполяцию от `a` до `b` с коэффициентом `t`.
- `math.DegToRad(deg)`: преобразует угол из градусов в радианы.
- `math.RadToDeg(rad)`: преобразует угол из радиан в градусы.
- `math.Distance(x1, y1, x2, y2)`: вычисляет расстояние между двумя точками в двумерном пространстве.
- `math.Angle(x1, y1, x2, y2)`: возвращает угол в градусах от первой точки ко второй.
- `math.Random(min, max)`: возвращает случайное число с плавающей точкой.
- `math.RandomInt(min, max)`: возвращает случайное целое число в диапазоне от минимального до максимального значения.

---

## ⚠️ Отказ от ответственности

Этот проект является **неофициальным экспериментальным тестовым проектом**, созданным в образовательных целях и для изучения модификаций с упором на безопасность. Он никак не связан со Studio Minus. Вы используете его на свой страх и риск.
