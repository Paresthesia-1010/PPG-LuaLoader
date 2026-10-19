# PPG-LuaLoader

> **開発中（WIP）。** People Playground 用の非公式 Lua MOD ローダーです。個人用のテストプロジェクトです。

**言語：** [English](README.md) · [한국어](README.ko-KR.md) · [简体中文](README.zh-CN.md) · 日本語 · [Русский](README.ru-RU.md)

[![Releases](https://img.shields.io/badge/Release-v3.0_Final-blue.svg)](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📥 ダウンロードとインストール

すぐにローダーを試したい場合は、公式リリースページから最新のコンパイル済みバイナリをダウンロードしてください。

👉 **[PPG-LuaLoader v3.0 Final Release をダウンロード](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)**

### インストール手順

1. Steam 版の **People Playground** がインストールされていることを確認してください。
2. **BepInEx 5 (Mono x64)** をダウンロードし、ゲームのルートディレクトリ（`People Playground/`）に展開してください。
3. [リリースページ](https://github.com/Paresthesia-1010/PPG-LuaLoader/releases/tag/main)から `PPG_LuaLoader.dll` と `MoonSharp.Interpreter.dll` をダウンロードしてください。
4. 2 つの DLL ファイルを **`BepInEx/plugins/`** フォルダーに配置してください。
5. ゲームを起動してください！

---

## 📖 クイックナビゲーションと MOD 制作ガイド

- 🚀 **[クイックスタート：最初の MOD とフォルダー構成](#-クイックスタート最初の-lua-mod-を作成する)**
- 🎮 **[ゲーム内 Toybox の使い方](#-ゲーム内-toybox-の使い方)**
- 📚 **[Lua API 完全リファレンス](#-lua-api-完全リファレンス)**

---

## 🛠️ プロジェクトについて

**PPG-LuaLoader** は、**People Playground** 向けに設計された実験的なサンドボックス型 Lua MOD 実行環境です。

従来の C# DLL MOD に関する最近のセキュリティ上の懸念を受け、本プロジェクトでは **MoonSharp**（純粋な C# 製 Lua インタープリター）を使用した安全な実行レイヤーを検討しています。これにより、システムレベルの脆弱性のリスクを抑えながら、スクリプトベースの MOD 制作を可能にすることを目指しています。

---

## 🚀 クイックスタート：最初の Lua MOD を作成する

### 📁 MOD フォルダーの構成

カスタム MOD を作成するには、ゲームの **`Mods/`** ディレクトリを開き、MOD 専用のフォルダーを作成してください。例：`Mods/Sword/`

MOD フォルダーは次のような構成にします。

```text
People Playground/
 └── Mods/
      └── Sword/
           ├── mod.json       <-- MOD のメタデータと表示名
           ├── thumb.png      <-- カスタムアイコン画像（Toybox メニューのサムネイル）
           └── main.lua       <-- メイン Lua スクリプト
```

### 📄 1. `mod.json`（設定ファイル）

MOD フォルダー内に `mod.json` というファイルを作成してください。

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

### 📜 2. `main.lua`（スクリプト）

設定ファイルと同じフォルダーに `main.lua` を作成してください。

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

## 🎮 ゲーム内 Toybox の使い方

ゲーム内の任意のマップに入ったら、次の操作を行います。

- **`Tab`** または **`F1`** キーを押すと、**Custom Lua Toybox** ウィンドウを開閉できます。
- カスタムアイテムのカードをクリックして**選択**します。選択されたカードはエメラルドグリーンで強調表示されます。
- マウスカーソルを目的の位置に合わせて **`E`** キーを押すと、選択したアイテムがすぐに生成されます。
  - ローダーはキー操作の競合を防ぐため、`E` キーに対するゲーム本来の操作を自動的に抑制します。
- Toybox 内の **`Env`** タブでは、**無重力（Zero Gravity）** の切り替えや、**バレットタイム（Bullet Time、0.2 倍のスローモーション）** の固定ができます。

---

# 📚 Lua API 完全リファレンス

すべての Lua MOD は `main.lua` スクリプトで構成され、次の 2 つのライフサイクル関数を使用します。

- **`Initialize()`**：MOD の読み込み時に 1 回呼び出されます。
- **`Update(dt)`**：ゲームの実行中、毎フレーム呼び出されます。`dt` は秒単位のフレーム間隔時間です。

---

### 1. 📢 グローバルログ API

- `Log(message)`：BepInEx コンソールに情報メッセージを出力します。メッセージには `[ModName]` という接頭辞が付きます。
- `LogWarning(message)`：黄色の警告メッセージを出力します。
- `LogError(message)`：赤色のエラーメッセージを出力します。

---

### 2. 📡 グローバルイベントリスナー

ゲームシミュレーション全体で発生するイベントを監視するためのフックです。

- `OnSpawn(function(entity) ... end)`：エンティティが生成されるたびに呼び出されます。
- `OnDelete(function(entity) ... end)`：エンティティが破棄される直前に呼び出されます。
- `OnCollision(function(me, other, impulse) ... end)`：物理衝突が発生したときに呼び出されます。
- `OnCollisionEnter(function(me, other, impulse) ... end)`：2 つのエンティティが衝突し始めたときに呼び出されます。
- `OnCollisionExit(function(me, other, impulse) ... end)`：2 つのエンティティの衝突が終了したときに呼び出されます。このとき `impulse` は `0` です。
- `OnKeyDown(function(keyName) ... end)`：キーが押されたときに呼び出されます。例：`"Space"`、`"F"`。
- `OnKeyUp(function(keyName) ... end)`：キーが離されたときに呼び出されます。
- `OnMouseDown(function(button) ... end)`：マウスボタンが押されたときに呼び出されます。`0` = 左、`1` = 右、`2` = 中央。
- `OnMouseUp(function(button) ... end)`：マウスボタンが離されたときに呼び出されます。

---

### 3. 🧭 Vector2 API（`LuaVector2`）

- `Vector2.New(x, y)`：新しい 2 次元ベクトルオブジェクトを作成します。
- `vec.x`、`vec.y`：X 座標と Y 座標です。
- `vec:Magnitude()`：ベクトルの長さを返します。
- `vec:Normalized()`：長さが 1 の単位ベクトルを返します。
- `vec:Dot(otherVec)`：2 つのベクトルの内積を返します。
- `vec:Distance(otherVec)`：2 つのベクトル間の距離を返します。

---

### 4. 🦾 Entity API（`LuaEntity`）

生成されたオブジェクト、ラグドール、アイテムに対して呼び出すメソッドです。

#### 変換と階層構造

- `entity:SetPosition(x, y)`：エンティティをワールド座標へ瞬間移動させ、物理状態を同期します。
- `entity:GetPosition()`：現在のワールド座標を `LuaVector2` として返します。
- `entity:SetRotation(angle)`：Z 軸の回転角度を度単位で設定します。
- `entity:GetRotation()`：現在の Z 軸回転角度を度単位で返します。
- `entity:SetScale(x, y)`：ローカルスケールを設定します。
- `entity:GetScale()`：ローカルスケールを `LuaVector2` として返します。
- `entity:SetLocalPosition(x, y)`：親オブジェクトを基準とした位置を設定します。
- `entity:GetLocalPosition()`：親オブジェクトを基準としたローカル位置を `LuaVector2` として返します。
- `entity:SetParent(parentEntity)`：別のエンティティに接続します。接続を解除するには `nil` を渡します。
- `entity:GetParent()`：親の `LuaEntity` または `nil` を返します。

#### 物理と移動

- `entity:AddForce(fx, fy)`：すべての身体パーツに瞬間的な力積を加えます。
- `entity:AddForceAtPosition(fx, fy, px, py)`：指定したワールド座標位置に力を加えます。
- `entity:SetVelocity(vx, vy)`：すべてのパーツの線速度を即座に設定します。
- `entity:GetVelocity()`：主要な身体パーツの速度を `LuaVector2` として返します。
- `entity:AddVelocity(vx, vy)`：すべてのパーツに線速度ベクトルを加算します。
- `entity:SetAngularVelocity(speed)`：回転速度を度/秒単位で設定します。
- `entity:GetAngularVelocity()`：角速度を返します。
- `entity:AddTorque(torque)`：回転方向の力積を加えます。
- `entity:SetMass(mass)`：総質量をすべての身体パーツに均等に分配します。
- `entity:GetMass()`：すべてのパーツの合計質量を返します。
- `entity:SetGravityScale(scale)`：重力倍率を設定します。`0` にすると無重力になります。
- `entity:GetGravityScale()`：現在の重力倍率を返します。
- `entity:SetKinematic(enabled)`：`true` にすると、オブジェクトは外部の物理力の影響を受けなくなります。
- `entity:IsKinematic()`：キネマティックモードが有効な場合に `true` を返します。
- `entity:SetCollisionEnabled(enabled)`：すべてのコライダーを有効または無効にします。
- `entity:IsCollisionEnabled()`：コライダーが有効な場合に `true` を返します。
- `entity:SetDrag(drag)`：線形の空気抵抗を設定します。
- `entity:SetAngularDrag(drag)`：回転に対する空気抵抗を設定します。
- `entity:Freeze()`：位置と回転を完全に固定します。
- `entity:Unfreeze()`：すべての移動制限を解除します。

#### 状態、体力、破棄

- `entity:IsAlive()`：人間に意識がある、または身体パーツに体力が残っている場合に `true` を返します。
- `entity:IsDeleted()`：基になる GameObject が破棄されている場合に `true` を返します。
- `entity:Destroy()`：エンティティを削除し、`OnDelete` を呼び出します。
- `entity:Delete()`：`entity:Destroy()` の別名です。
- `entity:GetInstanceId()`：エンジンのインスタンス ID（整数）を返します。
- `entity:GetName()`：エンティティの GameObject 名を返します。
- `entity:SetName(name)`：エンティティの GameObject 名を変更します。
- `entity:GetType()`：ルートコンポーネントの型名を返します。
- `entity:GetComponents()`：接続されているすべてのコンポーネント名を配列テーブルとして返します。
- `entity:HasComponent(name)`：指定したコンポーネントがエンティティに存在するか確認します。
- `entity:Ignite()`：燃焼可能なすべてのパーツに火をつけます。
- `entity:Electrocute(charge)`：金属フレームまたはアンドロイドに電流を放電します。
- `entity:SetTemperature(celsius)`：身体の温度を強制的に変更します。例：`1000.0` にすると金属が赤熱します。
- `entity:GetTemperature()`：現在の温度を摂氏で返します。
- `entity:SetHealth(amount)`：すべての身体パーツの体力を設定します。範囲は `0` から `100` です。
- `entity:GetHealth()`：身体パーツの平均体力を返します。
- `entity:SetMaxHealth(amount)`：身体パーツの最大体力を上書きします。
- `entity:GetMaxHealth()`：最大体力を返します。
- `entity:SetColor(r, g, b, a)`：スプライトの色調を設定します。各値の範囲は `0.0` から `1.0` です。
- `entity:GetColor()`：`{ r = ..., g = ..., b = ..., a = ... }` 形式のテーブルを返します。
- `entity:SetVisible(enabled)`：スプライトレンダラーを表示または非表示にします。
- `entity:IsVisible()`：スプライトが表示されている場合に `true` を返します。
- `entity:SetLayer(layer)`：子オブジェクトを含む GameObject のレイヤーを設定します。
- `entity:GetLayer()`：現在のレイヤーインデックスを返します。
- `entity:SetTag(tag)`：エンティティにカスタム文字列タグを割り当てます。
- `entity:HasTag(tag)`：指定したカスタムタグがエンティティに存在する場合に `true` を返します。
- `entity:RemoveTag(tag)`：指定したカスタムタグを削除します。
- `entity:Dismember()`：すべての物理ジョイントを大きく破壊し、出血を引き起こします。
- `entity:Crush()`：肉体と骨を押しつぶし、血の混じった破片にします。
- `entity:Disintegrate()`：肉体を蒸発させ、骨格だけを残します。
- `entity:Heal()`：火を消し、温度を 37°C に戻し、骨を修復し、出血を止め、`LIFE` 血清を注入します。
- `entity:Inject(liquidName, amount)`：液体を血流に直接注入します。例：`"LIFE"`、`"ACID"`、`"BLOOD"`。

#### コライダーと重なり判定

- `entity:GetColliders()`：コライダーの種類をテーブルとして返します。
- `entity:SetCollisionLayer(layer)`：衝突レイヤーを設定します。
- `entity:GetCollisionLayer()`：衝突レイヤーを返します。
- `entity:SetCollisionMask(maskBitfield)`：レイヤーの衝突マスクを設定します。
- `entity:GetCollisionMask()`：衝突マスクを返します。
- `entity:IgnoreCollision(otherEntity, ignoreBool)`：2 つのエンティティ間の衝突を無効または有効にします。
- `entity:OverlapCircle(radius)`：このエンティティを中心とする円形範囲内の他のエンティティをテーブルとして返します。
- `entity:OverlapBox(width, height)`：このエンティティを中心とする矩形範囲内の他のエンティティをテーブルとして返します。

---

### 5. ⌨️ 入力 API（`Input`）

- `Input.IsKeyDown(keyName)`：キーが押されたフレームに `true` を返します。例：`"F"`、`"Space"`、`"Tab"`、`"E"`。
- `Input.IsKeyHeld(keyName)`：キーを押し続けている間、継続して `true` を返します。
- `Input.IsKeyUp(keyName)`：キーが離されたフレームに `true` を返します。
- `Input.IsMouseButtonDown(button)`：マウスボタンがクリックされたフレームに `true` を返します。`0` = 左、`1` = 右、`2` = 中央。
- `Input.IsMouseButtonHeld(button)`：マウスボタンを押し続けている間、`true` を返します。
- `Input.IsMouseButtonUp(button)`：マウスボタンが離されたフレームに `true` を返します。
- `Input.GetMouseScrollDelta()`：マウスホイールの垂直スクロール量を返します。
- `Input.GetMouseWorldPosition()`：2 次元ワールド座標でのマウス位置を `{ x = ..., y = ... }` 形式のテーブルとして返します。

---

### 6. 👥 生成 API（`Spawn`）

- `Spawn.Create(assetName, x, y)`：`"Human"` や `"Android"` などの標準エンティティを生成し、`LuaEntity` を返します。
- `Spawn.CreateAtRotation(assetName, x, y, rot)`：指定した回転角度（度）でアセットを生成します。
- `Spawn.CreateBatch(assetName, positionsTable)`：複数のエンティティをまとめて生成します。形式：`{ {x=1, y=2}, {x=3, y=4} }`。
- `Spawn.Clone(entity, x, y)`：既存のエンティティを指定座標に複製します。
- `Spawn.GetAvailableAssets()`：ゲーム内で生成可能なすべてのアセット名をテーブルとして返します。
- `Spawn.Exists(assetName)`：指定した名前のアセットが存在する場合に `true` を返します。

---

### 7. 🌍 ワールド・物理 API（`World` と `Physics`）

- `World.GetEntities()`：シーン内でアクティブなすべてのルートエンティティをテーブルとして返します。
- `World.GetEntitiesInRadius(x, y, radius)`：指定した半径内のエンティティを返します。
- `World.GetEntitiesInBox(x, y, width, height)`：指定した矩形範囲内のエンティティを返します。
- `World.GetEntityById(instanceId)`：Unity のインスタンス ID からエンティティを検索します。
- `World.GetEntitiesByName(name)`：名前に指定した文字列が含まれるエンティティを返します。
- `World.GetEntitiesByTag(tag)`：指定した Unity タグに一致するエンティティを返します。
- `World.GetEntityCount()`：アクティブな物理オブジェクトの総数を返します。
- `World.GetBounds()`：境界情報を `{ minX = -100, maxX = 100, minY = -50, maxY = 50 }` 形式のテーブルとして返します。
- `World.GetMouseWorldPosition()`：マウスのワールド座標を `{ x = ..., y = ... }` 形式で返します。
- `World.GetCameraPosition()`：メインカメラの座標を `{ x = ..., y = ... }` 形式で返します。
- `Physics.Explode(x, y, radius, force)`：物理爆発を発生させ、エンティティを爆心地から外側へ吹き飛ばします。

---

### 8. ⏱️ 時間 API（`Time`）

- `Time.GetDeltaTime()`：フレーム間の経過時間を秒単位で返します。
- `Time.GetTime()`：ゲーム起動後の総経過時間を返します。
- `Time.GetFixedDeltaTime()`：物理更新の時間間隔を返します。
- `Time.GetTimeScale()`：現在のゲームシミュレーション速度を返します。
- `Time.SetTimeScale(scale)`：シミュレーション速度を固定します。例：`0.2` はスローモーション、`1.0` は通常速度です。
- `Time.GetFrameRate()`：現在の FPS を返します。

---

### 9. 🗺️ マップ・環境 API（`Map`）

- `Map.SetGravity(gx, gy)`：ワールドの重力ベクトルを変更します。デフォルト値は `0, -9.81` です。
- `Map.GetGravity()`：`{ x = ..., y = ... }` 形式の重力ベクトルを返します。
- `Map.SetTimeScale(scale)`：`Time.SetTimeScale(scale)` の別名です。
- `Map.CreateWall(x, y, width, height, material)`：破壊不可能な構造壁を生成します。使用できる材質は `"Metal"`、`"Wood"`、`"Concrete"` です。
- `Map.GetBounds()`：マップの境界情報をテーブルとして返します。
- `Map.GetEntities()`：マップ内のすべてのエンティティを返します。
- `Map.Register(mapConfig)`：Toybox の Maps タブにマッププリセットを登録します。

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

### 10. ⚔️ カスタムアイテム API（`Item`）

- `Item.Register(configTable)`：カスタムアイテムを Custom Toybox（`Tab` メニュー）に登録します。
- `Item.Unregister(itemName)`：指定したアイテムの登録を解除します。
- `Item.GetRegisteredItems()`：登録済みのすべてのカスタムアイテム名をテーブルとして返します。
- `Item.GetItem(itemName)`：`{ name, description, baseItem }` 形式のアイテム情報を返します。
- `Item.Spawn(itemName, x, y)`：登録済みのカスタムアイテムを指定したワールド座標に生成します。
- `Item.OnSpawn(configTable, function(entity) ... end)`：アイテムを登録し、生成時に実行するコールバックを設定します。
- `Item.OnUse(configTable, function(entity) ... end)`：アイテムを登録し、アイテムが起動または使用されたときの動作を設定します。

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

### 11. 🛠️ デバッグ描画 API（`Debug`）

- `Debug.DrawLine(x1, y1, x2, y2, duration)`：指定した秒数だけ赤いデバッグ線を描画します。
- `Debug.DrawCircle(cx, cy, radius, duration)`：赤いワイヤーフレームの円を描画します。
- `Debug.DrawBox(cx, cy, width, height, duration)`：`(cx, cy)` を中心とする赤いワイヤーフレームの矩形を描画します。
- `Debug.GetEntityInfo(entity)`：名前、インスタンス ID、座標を含むデバッグ文字列を返します。
- `Debug.IsEnabled()`：`true` を返します。

---

### 12. 📦 MOD 管理 API（`Mod`）

- `Mod.GetName()`：現在の MOD 名を返します。
- `Mod.GetVersion()`：ローダーのバージョンを返します（`"3.0.0"`）。
- `Mod.GetPath()`：現在の MOD ディレクトリの絶対パスを返します。
- `Mod.GetAllMods()`：読み込み済みのすべての MOD 名をテーブルとして返します。
- `Mod.IsLoaded(modName)`：指定した MOD が読み込まれている場合に `true` を返します。
- `Mod.Reload(modName)`：その MOD の `main.lua` を再実行し、ホットリロードします。
- `Mod.GetMetadata()`：`{ name = ..., dir = ... }` 形式のメタデータを返します。

---

### 13. 📐 拡張数学 API（`math`）

Lua 標準の `math` ライブラリに追加されたエンジン拡張機能です。

- `math.Clamp(val, min, max)`：数値を最小値と最大値の範囲内に制限します。
- `math.Lerp(a, b, t)`：係数 `t` を使用して `a` から `b` への線形補間を行います。
- `math.DegToRad(deg)`：角度を度からラジアンに変換します。
- `math.RadToDeg(rad)`：角度をラジアンから度に変換します。
- `math.Distance(x1, y1, x2, y2)`：2 つの 2 次元座標間の距離を計算します。
- `math.Angle(x1, y1, x2, y2)`：1 つ目の点から 2 つ目の点へ向かう角度を度単位で返します。
- `math.Random(min, max)`：ランダムな浮動小数点数を返します。
- `math.RandomInt(min, max)`：最小値から最大値までのランダムな整数を返します。

---

## ⚠️ 免責事項

本プロジェクトは、教育および安全性を重視した MOD 研究を目的として作成された**非公式の実験的テストプロジェクト**です。Studio Minus とは一切関係ありません。利用は自己責任でお願いします。
