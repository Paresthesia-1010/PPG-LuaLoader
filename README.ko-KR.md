# PPG-LuaLoader

> **개발 진행 중(WIP).** People Playground용 비공식 Lua 모드 로더입니다. 개인 테스트 프로젝트입니다.

**언어:** [English](README.md) · 한국어 · [简体中文](README.zh-CN.md) · [日本語](README.ja-JP.md) · [Русский](README.ru-RU.md)

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 다운로드 및 설치

로더를 바로 테스트하고 싶으신가요? 공식 릴리스 페이지에서 최신 컴파일 버전을 다운로드하세요.

👉 **[PPG-LuaLoader v3.0 최종 릴리스 다운로드](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)**

### 설치 단계

1. Steam을 통해 **People Playground**가 설치되어 있는지 확인합니다.
2. **BepInEx 5 (Mono x64)**를 다운로드한 다음 게임의 루트 디렉터리(`People Playground/`)에 압축을 풉니다.
3. [릴리스 페이지](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)에서 `PPG_LuaLoader.dll`과 `MoonSharp.Interpreter.dll`을 다운로드합니다.
4. 두 DLL 파일을 **`BepInEx/plugins/`** 폴더에 넣습니다.
5. 게임을 실행합니다!

---

## 📖 빠른 탐색 및 모드 제작 가이드

- 🚀 **[빠른 시작: 첫 번째 모드 만들기 및 폴더 구조](#-빠른-시작-첫-번째-lua-모드-만들기)**
- 🎮 **[게임 내 Toybox 사용법](#-게임-내-toybox-사용법)**
- 📚 **[전체 Lua API 레퍼런스](#-전체-lua-api-레퍼런스)**

---

## 🛠️ 프로젝트 소개

**PPG-LuaLoader**는 **People Playground**를 위한 실험적인 샌드박스형 Lua 모드 실행 환경입니다.

기존 C# DLL 모드와 관련된 최근 보안 우려에 따라, 이 프로젝트는 **MoonSharp**(순수 C# Lua 인터프리터)를 활용한 안전한 실행 계층을 연구합니다. 이를 통해 시스템 수준의 보안 취약점을 유발할 위험을 줄이면서 스크립트 기반 모딩을 지원합니다.

---

## 🚀 빠른 시작: 첫 번째 Lua 모드 만들기

### 📁 모드 폴더 구조

사용자 지정 모드를 만들려면 게임의 **`Mods/`** 디렉터리로 이동한 뒤, 모드를 위한 전용 폴더를 만듭니다. 예를 들어 `Mods/Sword/`와 같이 만들 수 있습니다.

모드 폴더는 다음과 같은 구조로 구성해야 합니다.

```text
People Playground/
 └── Mods/
      └── Sword/
           ├── mod.json       <-- 모드 메타데이터 및 표시 이름
           ├── thumb.png      <-- 사용자 지정 아이콘 이미지(Toybox 메뉴의 썸네일)
           └── main.lua       <-- 기본 Lua 실행 스크립트
```

### 📄 1. `mod.json` (설정)

모드 폴더 안에 `mod.json` 파일을 만듭니다.

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

### 📜 2. `main.lua` (스크립트)

설정 파일과 같은 폴더에 `main.lua` 파일을 만듭니다.

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

## 🎮 게임 내 Toybox 사용법

게임 맵에 들어간 상태에서 다음과 같이 사용할 수 있습니다.

- **`Tab`** 또는 **`F1`** 키를 눌러 **Custom Lua Toybox** 창을 열거나 닫습니다.
- 사용자 지정 아이템 카드를 클릭해 **선택**합니다. 선택된 카드는 에메랄드 그린 색상으로 강조 표시됩니다.
- 마우스 커서를 원하는 위치에 놓고 **`E`** 키를 눌러 선택한 아이템을 즉시 생성합니다.
  - 로더는 키 입력 충돌을 방지하기 위해 `E` 키에 대한 게임 기본 동작을 자동으로 차단합니다.
- Toybox 내부의 **`Env`** 탭에서 **무중력(Zero Gravity)**을 켜거나 끌 수 있으며, **불릿 타임(Bullet Time, 0.2배 슬로 모션)**을 고정할 수도 있습니다.

---

# 📚 전체 Lua API 레퍼런스

모든 Lua 모드는 `main.lua` 스크립트로 구성되며, 다음 두 가지 생명주기 함수를 포함합니다.

- **`Initialize()`**: 모드가 로드될 때 한 번 호출됩니다.
- **`Update(dt)`**: 게임이 실행되는 동안 매 프레임 호출됩니다. (`dt`는 초 단위의 프레임 경과 시간입니다.)

---

### 1. 📢 전역 로그 API

- `Log(message)`: BepInEx 콘솔에 `[ModName]` 접두사가 붙은 정보 메시지를 출력합니다.
- `LogWarning(message)`: 노란색 경고 메시지를 출력합니다.
- `LogError(message)`: 빨간색 오류 메시지를 출력합니다.

---

### 2. 📡 전역 이벤트 리스너

게임 시뮬레이션 전체에서 발생하는 이벤트를 감지하기 위한 훅입니다.

- `OnSpawn(function(entity) ... end)`: 엔티티가 생성될 때마다 호출됩니다.
- `OnDelete(function(entity) ... end)`: 엔티티가 제거되기 직전에 호출됩니다.
- `OnCollision(function(me, other, impulse) ... end)`: 물리 충돌이 발생했을 때 호출됩니다.
- `OnCollisionEnter(function(me, other, impulse) ... end)`: 두 엔티티가 처음 충돌하기 시작할 때 호출됩니다.
- `OnCollisionExit(function(me, other, impulse) ... end)`: 두 엔티티의 충돌이 끝날 때 호출됩니다. (`impulse`는 `0`입니다.)
- `OnKeyDown(function(keyName) ... end)`: 키를 누를 때 호출됩니다. 예: `"Space"`, `"F"`.
- `OnKeyUp(function(keyName) ... end)`: 키에서 손을 뗄 때 호출됩니다.
- `OnMouseDown(function(button) ... end)`: 마우스 버튼을 누를 때 호출됩니다. (`0` = 왼쪽, `1` = 오른쪽, `2` = 가운데)
- `OnMouseUp(function(button) ... end)`: 마우스 버튼에서 손을 뗄 때 호출됩니다.

---

### 3. 🧭 Vector2 API (`LuaVector2`)

- `Vector2.New(x, y)`: 새로운 2차원 벡터 객체를 생성합니다.
- `vec.x`, `vec.y`: X 및 Y 좌표입니다.
- `vec:Magnitude()`: 벡터의 길이를 반환합니다.
- `vec:Normalized()`: 길이가 1인 단위 벡터를 반환합니다.
- `vec:Dot(otherVec)`: 두 벡터의 내적을 반환합니다.
- `vec:Distance(otherVec)`: 두 벡터 사이의 거리를 반환합니다.

---

### 4. 🦾 Entity API (`LuaEntity`)

생성된 오브젝트, 래그돌 및 아이템에 호출하는 메서드입니다.

#### 변환 및 계층 구조

- `entity:SetPosition(x, y)`: 엔티티를 월드 좌표로 순간 이동시키고 물리 상태를 동기화합니다.
- `entity:GetPosition()`: 현재 월드 좌표를 `LuaVector2`로 반환합니다.
- `entity:SetRotation(angle)`: Z축 회전 각도를 도 단위로 설정합니다.
- `entity:GetRotation()`: 현재 Z축 회전 각도를 도 단위로 반환합니다.
- `entity:SetScale(x, y)`: 로컬 스케일을 설정합니다.
- `entity:GetScale()`: 로컬 스케일을 `LuaVector2`로 반환합니다.
- `entity:SetLocalPosition(x, y)`: 부모 엔티티를 기준으로 위치를 설정합니다.
- `entity:GetLocalPosition()`: 부모 엔티티를 기준으로 한 로컬 위치를 `LuaVector2`로 반환합니다.
- `entity:SetParent(parentEntity)`: 다른 엔티티에 연결합니다. 연결을 해제하려면 `nil`을 전달합니다.
- `entity:GetParent()`: 부모 `LuaEntity` 또는 `nil`을 반환합니다.

#### 물리 및 움직임

- `entity:AddForce(fx, fy)`: 모든 신체 부위에 순간적인 힘을 적용합니다.
- `entity:AddForceAtPosition(fx, fy, px, py)`: 지정된 월드 위치에 힘을 적용합니다.
- `entity:SetVelocity(vx, vy)`: 모든 부위의 선형 속도를 즉시 설정합니다.
- `entity:GetVelocity()`: 기본 신체 부위의 속도를 `LuaVector2`로 반환합니다.
- `entity:AddVelocity(vx, vy)`: 모든 신체 부위에 선형 속도 벡터를 더합니다.
- `entity:SetAngularVelocity(speed)`: 초당 도 단위로 회전 속도를 설정합니다.
- `entity:GetAngularVelocity()`: 회전 속도를 반환합니다.
- `entity:AddTorque(torque)`: 회전 충격량을 적용합니다.
- `entity:SetMass(mass)`: 전체 질량을 모든 신체 부위에 균등하게 분배합니다.
- `entity:GetMass()`: 모든 부위의 총 질량을 반환합니다.
- `entity:SetGravityScale(scale)`: 중력 배율을 설정합니다. (`0`은 무중력입니다.)
- `entity:GetGravityScale()`: 현재 중력 배율을 반환합니다.
- `entity:SetKinematic(enabled)`: `true`로 설정하면 외부 물리력의 영향을 받지 않습니다.
- `entity:IsKinematic()`: 키네마틱 모드가 활성화되어 있으면 `true`를 반환합니다.
- `entity:SetCollisionEnabled(enabled)`: 모든 충돌체를 활성화하거나 비활성화합니다.
- `entity:IsCollisionEnabled()`: 충돌체가 활성화되어 있으면 `true`를 반환합니다.
- `entity:SetDrag(drag)`: 선형 공기 저항을 설정합니다.
- `entity:SetAngularDrag(drag)`: 회전 공기 저항을 설정합니다.
- `entity:Freeze()`: 위치와 회전을 완전히 고정합니다.
- `entity:Unfreeze()`: 모든 움직임 제한을 해제합니다.

#### 상태, 생명력 및 제거

- `entity:IsAlive()`: 인간이 의식이 있거나 신체 부위에 체력이 남아 있으면 `true`를 반환합니다.
- `entity:IsDeleted()`: 기반 GameObject가 파괴되었으면 `true`를 반환합니다.
- `entity:Destroy()`: 엔티티를 삭제하고 `OnDelete`를 호출합니다.
- `entity:Delete()`: `entity:Destroy()`와 동일한 별칭입니다.
- `entity:GetInstanceId()`: 엔진의 인스턴스 ID 정수를 반환합니다.
- `entity:GetName()`: 엔티티 GameObject의 이름을 반환합니다.
- `entity:SetName(name)`: 엔티티 GameObject의 이름을 변경합니다.
- `entity:GetType()`: 루트 컴포넌트의 타입 이름을 반환합니다.
- `entity:GetComponents()`: 연결된 모든 컴포넌트 이름을 배열 테이블로 반환합니다.
- `entity:HasComponent(name)`: 해당 컴포넌트가 엔티티에 존재하는지 확인합니다.
- `entity:Ignite()`: 불이 붙을 수 있는 모든 부위에 불을 붙입니다.
- `entity:Electrocute(charge)`: 금속 프레임 또는 안드로이드에 전기를 방전합니다.
- `entity:SetTemperature(celsius)`: 신체 온도를 강제로 변경합니다. 예: 금속을 빨갛게 달아오르게 하려면 `1000.0`을 사용합니다.
- `entity:GetTemperature()`: 현재 온도를 섭씨로 반환합니다.
- `entity:SetHealth(amount)`: 모든 신체 부위의 체력을 설정합니다. 범위는 `0`부터 `100`입니다.
- `entity:GetHealth()`: 신체 부위 체력의 평균을 반환합니다.
- `entity:SetMaxHealth(amount)`: 신체 부위의 최대 체력을 재설정합니다.
- `entity:GetMaxHealth()`: 최대 체력을 반환합니다.
- `entity:SetColor(r, g, b, a)`: 스프라이트 색조를 설정합니다. 각 값의 범위는 `0.0`부터 `1.0`입니다.
- `entity:GetColor()`: `{ r = ..., g = ..., b = ..., a = ... }` 형식의 테이블을 반환합니다.
- `entity:SetVisible(enabled)`: 스프라이트 렌더러를 표시하거나 숨깁니다.
- `entity:IsVisible()`: 스프라이트가 표시되고 있으면 `true`를 반환합니다.
- `entity:SetLayer(layer)`: 모든 자식 오브젝트를 포함한 GameObject 레이어를 설정합니다.
- `entity:GetLayer()`: 현재 레이어 인덱스를 반환합니다.
- `entity:SetTag(tag)`: 엔티티에 사용자 지정 문자열 태그를 지정합니다.
- `entity:HasTag(tag)`: 엔티티에 지정한 사용자 지정 태그가 있으면 `true`를 반환합니다.
- `entity:RemoveTag(tag)`: 지정한 사용자 지정 태그를 제거합니다.
- `entity:Dismember()`: 모든 물리 관절을 심하게 파괴하고 출혈을 일으킵니다.
- `entity:Crush()`: 살과 뼈를 으스러뜨려 피가 섞인 파편으로 만듭니다.
- `entity:Disintegrate()`: 살을 증발시켜 골격만 남깁니다.
- `entity:Heal()`: 불을 끄고, 온도를 37°C로 초기화하고, 뼈를 치료하고, 출혈을 멈추며, `LIFE` 혈청을 주입합니다.
- `entity:Inject(liquidName, amount)`: 혈류에 액체를 직접 주입합니다. 예: `"LIFE"`, `"ACID"`, `"BLOOD"`.

#### 충돌체 및 겹침 감지

- `entity:GetColliders()`: 충돌체 유형 이름을 테이블로 반환합니다.
- `entity:SetCollisionLayer(layer)`: 충돌 레이어를 설정합니다.
- `entity:GetCollisionLayer()`: 충돌 레이어를 반환합니다.
- `entity:SetCollisionMask(maskBitfield)`: 레이어 충돌 마스크를 설정합니다.
- `entity:GetCollisionMask()`: 충돌 마스크를 반환합니다.
- `entity:IgnoreCollision(otherEntity, ignoreBool)`: 두 엔티티 사이의 충돌을 비활성화하거나 다시 활성화합니다.
- `entity:OverlapCircle(radius)`: 엔티티를 중심으로 한 원 안에 있는 다른 엔티티를 테이블로 반환합니다.
- `entity:OverlapBox(width, height)`: 엔티티를 중심으로 한 사각형 안에 있는 다른 엔티티를 테이블로 반환합니다.

---

### 5. ⌨️ 입력 API (`Input`)

- `Input.IsKeyDown(keyName)`: 키를 누른 순간의 프레임에 `true`를 반환합니다. 예: `"F"`, `"Space"`, `"Tab"`, `"E"`.
- `Input.IsKeyHeld(keyName)`: 키를 누르고 있는 동안 계속 `true`를 반환합니다.
- `Input.IsKeyUp(keyName)`: 키에서 손을 뗀 프레임에 `true`를 반환합니다.
- `Input.IsMouseButtonDown(button)`: 마우스 버튼을 클릭한 프레임에 `true`를 반환합니다. (`0` = 왼쪽, `1` = 오른쪽, `2` = 가운데)
- `Input.IsMouseButtonHeld(button)`: 마우스 버튼을 누르고 있는 동안 `true`를 반환합니다.
- `Input.IsMouseButtonUp(button)`: 마우스 버튼에서 손을 뗀 프레임에 `true`를 반환합니다.
- `Input.GetMouseScrollDelta()`: 마우스 휠의 세로 스크롤 이동량을 반환합니다.
- `Input.GetMouseWorldPosition()`: 2차원 월드 마우스 좌표를 `{ x = ..., y = ... }` 형식의 테이블로 반환합니다.

---

### 6. 👥 생성 API (`Spawn`)

- `Spawn.Create(assetName, x, y)`: `"Human"` 또는 `"Android"` 같은 기본 게임 엔티티를 생성하고 `LuaEntity`를 반환합니다.
- `Spawn.CreateAtRotation(assetName, x, y, rot)`: 지정한 회전 각도(도 단위)로 에셋을 생성합니다.
- `Spawn.CreateBatch(assetName, positionsTable)`: 여러 엔티티를 생성합니다. 형식: `{ {x=1, y=2}, {x=3, y=4} }`.
- `Spawn.Clone(entity, x, y)`: 기존 엔티티를 지정된 좌표에 복제합니다.
- `Spawn.GetAvailableAssets()`: 게임에서 생성할 수 있는 모든 에셋 이름을 테이블로 반환합니다.
- `Spawn.Exists(assetName)`: 해당 이름의 에셋이 존재하면 `true`를 반환합니다.

---

### 7. 🌍 월드 및 물리 API (`World` 및 `Physics`)

- `World.GetEntities()`: 장면에 있는 모든 활성 루트 엔티티를 테이블로 반환합니다.
- `World.GetEntitiesInRadius(x, y, radius)`: 지정된 반경 안에 있는 엔티티를 반환합니다.
- `World.GetEntitiesInBox(x, y, width, height)`: 지정된 사각형 안에 있는 엔티티를 반환합니다.
- `World.GetEntityById(instanceId)`: Unity 인스턴스 ID로 엔티티를 찾습니다.
- `World.GetEntitiesByName(name)`: 이름에 지정한 문자열이 포함된 엔티티를 반환합니다.
- `World.GetEntitiesByTag(tag)`: Unity 태그가 일치하는 엔티티를 반환합니다.
- `World.GetEntityCount()`: 현재 활성화된 물리 오브젝트의 총개수를 반환합니다.
- `World.GetBounds()`: 경계 정보를 다음 형식의 테이블로 반환합니다: `{ minX = -100, maxX = 100, minY = -50, maxY = 50 }`.
- `World.GetMouseWorldPosition()`: 마우스의 월드 좌표를 `{ x = ..., y = ... }` 형식으로 반환합니다.
- `World.GetCameraPosition()`: 메인 카메라의 좌표를 `{ x = ..., y = ... }` 형식으로 반환합니다.
- `Physics.Explode(x, y, radius, force)`: 물리 폭발을 발생시켜 중심점에서 엔티티를 바깥쪽으로 날려 보냅니다.

---

### 8. ⏱️ 시간 API (`Time`)

- `Time.GetDeltaTime()`: 프레임 경과 시간을 초 단위로 반환합니다.
- `Time.GetTime()`: 게임 실행 이후 경과한 총시간을 반환합니다.
- `Time.GetFixedDeltaTime()`: 물리 업데이트 간격을 반환합니다.
- `Time.GetTimeScale()`: 현재 게임 시뮬레이션 속도를 반환합니다.
- `Time.SetTimeScale(scale)`: 시뮬레이션 속도를 고정합니다. 예: 슬로 모션은 `0.2`, 일반 속도는 `1.0`입니다.
- `Time.GetFrameRate()`: 현재 FPS를 반환합니다.

---

### 9. 🗺️ 맵 및 환경 API (`Map`)

- `Map.SetGravity(gx, gy)`: 월드 중력 벡터를 변경합니다. 기본값은 `0, -9.81`입니다.
- `Map.GetGravity()`: `{ x = ..., y = ... }` 형식의 중력 벡터를 반환합니다.
- `Map.SetTimeScale(scale)`: `Time.SetTimeScale(scale)`의 별칭입니다.
- `Map.CreateWall(x, y, width, height, material)`: 파괴할 수 없는 구조용 벽을 생성합니다. 재질은 `"Metal"`, `"Wood"`, `"Concrete"`를 사용할 수 있습니다.
- `Map.GetBounds()`: 맵의 경계 정보를 테이블로 반환합니다.
- `Map.GetEntities()`: 맵에 있는 모든 엔티티를 반환합니다.
- `Map.Register(mapConfig)`: Toybox의 Maps 탭에 맵 프리셋을 등록합니다.

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

### 10. ⚔️ 사용자 지정 아이템 API (`Item`)

- `Item.Register(configTable)`: 사용자 지정 아이템을 Custom Toybox(`Tab` 메뉴)에 등록합니다.
- `Item.Unregister(itemName)`: 아이템 등록을 해제합니다.
- `Item.GetRegisteredItems()`: 등록된 모든 사용자 지정 아이템 이름을 테이블로 반환합니다.
- `Item.GetItem(itemName)`: `{ name, description, baseItem }` 형식의 아이템 정보를 반환합니다.
- `Item.Spawn(itemName, x, y)`: 등록된 사용자 지정 아이템을 월드 좌표에 생성합니다.
- `Item.OnSpawn(configTable, function(entity) ... end)`: 아이템을 등록하고 생성 시 실행할 콜백을 설정합니다.
- `Item.OnUse(configTable, function(entity) ... end)`: 아이템을 등록하고 활성화하거나 사용할 때 실행할 동작을 연결합니다.

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

### 11. 🛠️ 디버그 그리기 API (`Debug`)

- `Debug.DrawLine(x1, y1, x2, y2, duration)`: 지정된 시간(초) 동안 빨간색 디버그 선을 그립니다.
- `Debug.DrawCircle(cx, cy, radius, duration)`: 빨간색 와이어프레임 원을 그립니다.
- `Debug.DrawBox(cx, cy, width, height, duration)`: `(cx, cy)`를 중심으로 빨간색 와이어프레임 사각형을 그립니다.
- `Debug.GetEntityInfo(entity)`: 이름, 인스턴스 ID 및 좌표가 포함된 디버그 문자열을 반환합니다.
- `Debug.IsEnabled()`: `true`를 반환합니다.

---

### 12. 📦 모드 관리 API (`Mod`)

- `Mod.GetName()`: 현재 모드의 이름을 반환합니다.
- `Mod.GetVersion()`: 로더 버전을 반환합니다. (`"3.0.0"`)
- `Mod.GetPath()`: 현재 모드 디렉터리의 절대 경로를 반환합니다.
- `Mod.GetAllMods()`: 로드된 모든 모드의 이름을 테이블로 반환합니다.
- `Mod.IsLoaded(modName)`: 지정한 모드가 로드되어 있으면 `true`를 반환합니다.
- `Mod.Reload(modName)`: 해당 모드의 `main.lua`를 다시 실행하여 모드를 핫 리로드합니다.
- `Mod.GetMetadata()`: `{ name = ..., dir = ... }` 형식의 메타데이터를 반환합니다.

---

### 13. 📐 확장 수학 API (`math`)

Lua 표준 `math` 라이브러리에 추가된 엔진 기능입니다.

- `math.Clamp(val, min, max)`: 숫자를 최소값과 최대값 사이로 제한합니다.
- `math.Lerp(a, b, t)`: 계수 `t`를 사용해 `a`에서 `b`까지 선형 보간합니다.
- `math.DegToRad(deg)`: 각도를 도에서 라디안으로 변환합니다.
- `math.RadToDeg(rad)`: 각도를 라디안에서 도로 변환합니다.
- `math.Distance(x1, y1, x2, y2)`: 두 2차원 점 사이의 거리를 반환합니다.
- `math.Angle(x1, y1, x2, y2)`: 첫 번째 점에서 두 번째 점을 향하는 각도를 도 단위로 반환합니다.
- `math.Random(min, max)`: 무작위 부동소수점 숫자를 반환합니다.
- `math.RandomInt(min, max)`: 최소값과 최대값 사이의 무작위 정수를 반환합니다.

---

## ⚠️ 면책 조항

이 프로젝트는 교육 및 안전 중심의 모딩 연구를 위해 제작된 **비공식 실험용 테스트 프로젝트**입니다. Studio Minus와 어떠한 제휴 관계도 없습니다. 사용에 따른 책임은 사용자에게 있습니다.
