using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPG_LuaLoader
{
    [MoonSharpUserData]
    public class LuaEntity
    {
        private readonly GameObject internalObject;

        public LuaEntity(GameObject targetObject)
        {
            this.internalObject = targetObject;
        }

        public Table GetPosition(Script engine)
        {
            if (internalObject == null) return null;

            Table posTable = new Table(engine);
            Vector3 pos = internalObject.transform.position;
            posTable["x"] = pos.x;
            posTable["y"] = pos.y;
            return posTable;
        }

        public void AddForce(float forceX, float forceY)
        {
            if (internalObject == null) return;

            Rigidbody2D[] bodies = internalObject.GetComponentsInChildren<Rigidbody2D>();
            Vector2 forceVector = new Vector2(forceX, forceY);

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].AddForce(forceVector, ForceMode2D.Impulse);
            }
        }

        public void SetVelocity(float velocityX, float velocityY)
        {
            if (internalObject == null) return;

            Rigidbody2D[] bodies = internalObject.GetComponentsInChildren<Rigidbody2D>();
            Vector2 velocityVector = new Vector2(velocityX, velocityY);

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].velocity = velocityVector;
            }
        }

        public void Ignite()
        {
            if (internalObject == null) return;

            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++)
            {
                physicalParts[i].Ignite();
            }
        }

        public void Electrocute(float chargeAmount)
        {
            if (internalObject == null) return;

            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++)
            {
                physicalParts[i].Charge += chargeAmount;
            }
        }

        public void SetTemperature(float degreesCelsius)
        {
            if (internalObject == null) return;

            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++)
            {
                physicalParts[i].Temperature = degreesCelsius;
            }
        }

        public void Dismember()
        {
            if (internalObject == null) return;

            Vector2 centerPosition = internalObject.transform.position;
            Joint2D[] joints = internalObject.GetComponentsInChildren<Joint2D>();
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i] != null)
                {
                    joints[i].enabled = false;
                }
            }

            LimbBehaviour[] limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            for (int i = 0; i < limbs.Length; i++)
            {
                Vector2 limbPos = limbs[i].transform.position;
                Vector2 radialDirection = (limbPos - centerPosition).normalized;

                if (radialDirection == Vector2.zero)
                {
                    radialDirection = UnityEngine.Random.insideUnitCircle.normalized;
                }

                Rigidbody2D rb = limbs[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(radialDirection * UnityEngine.Random.Range(10f, 18f), ForceMode2D.Impulse);
                }

                if (limbs[i].CirculationBehaviour != null)
                {
                    limbs[i].CirculationBehaviour.BleedingRate = 15f;
                }
            }
        }

        public void Crush()
        {
            if (internalObject == null) return;

            LimbBehaviour[] limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            for (int i = 0; i < limbs.Length; i++)
            {
                limbs[i].Crush();
            }
        }

        public void Disintegrate()
        {
            if (internalObject == null) return;

            PhysicalBehaviour[] parts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].Disintegrate();
            }
        }

        public void Heal()
        {
            if (internalObject == null) return;

            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++)
            {
                physicalParts[i].Extinguish();
                physicalParts[i].Temperature = 37f;
            }

            LimbBehaviour[] limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            for (int i = 0; i < limbs.Length; i++)
            {
                limbs[i].HealBone();
                limbs[i].Health = 100f;

                if (limbs[i].CirculationBehaviour != null)
                {
                    limbs[i].CirculationBehaviour.HealBleeding();
                    limbs[i].CirculationBehaviour.BleedingRate = 0f;
                }
            }

            Inject("LIFE", 20f);

            PersonBehaviour person = internalObject.GetComponent<PersonBehaviour>();
            if (person != null)
            {
                person.Consciousness = 1f;
            }
        }

        public void Inject(string liquidName, float amount)
        {
            if (internalObject == null || string.IsNullOrEmpty(liquidName)) return;

            Liquid targetLiquid = Liquid.GetLiquid(liquidName)
                               ?? Liquid.GetLiquid(liquidName.ToUpper())
                               ?? Liquid.GetLiquid(liquidName.ToLower());

            if (targetLiquid == null)
            {
                Debug.LogWarning($"[Lua Loader] Liquid '{liquidName}' not found in registry.");
                return;
            }

            CirculationBehaviour circulation = internalObject.GetComponentInChildren<CirculationBehaviour>();
            if (circulation != null)
            {
                circulation.AddLiquid(targetLiquid, amount);
            }
        }

        public void Delete()
        {
            if (internalObject != null)
            {
                UnityEngine.Object.Destroy(internalObject);
            }
        }
    }

    public class CustomLuaItemTemplate
    {
        public string Name;
        public string Description;
        public string BaseItemName;
        public string SpritePath;
        public string ModDirectory;
        public string ModThumbnailPath;
        public Script ScriptEngine;
        public DynValue OnSpawnCallback;
        public Texture2D CachedIconTexture;
    }

    public class CustomMapStructureDefinition
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public string Material;
    }

    public class CustomMapPreset
    {
        public string Name;
        public string Description;
        public Vector2 Gravity = new Vector2(0f, -9.81f);
        public List<CustomMapStructureDefinition> Structures = new List<CustomMapStructureDefinition>();
    }

    public class LuaModContainer
    {
        public string ModName { get; private set; }
        public string ModDirectory { get; private set; }
        public Script ScriptEngine { get; private set; }
        public DynValue UpdateCallback { get; private set; }

        public LuaModContainer(string modName, string modDirectory, Script engine)
        {
            ModName = modName;
            ModDirectory = modDirectory;
            ScriptEngine = engine;

            DynValue updateFunc = engine.Globals.Get("Update");
            if (updateFunc.Type == DataType.Function)
            {
                UpdateCallback = updateFunc;
            }
        }

        public void DisableUpdate()
        {
            UpdateCallback = null;
        }
    }

    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class LoaderPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.user.ppg.lualoader";
        public const string PluginName = "Temporary PPG Lua Loader";
        public const string PluginVersion = "3.0.0";

        private readonly List<LuaModContainer> activeMods = new List<LuaModContainer>();
        private readonly Dictionary<string, CustomLuaItemTemplate> customItemCatalog = new Dictionary<string, CustomLuaItemTemplate>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CustomMapPreset> customMapCatalog = new Dictionary<string, CustomMapPreset>(StringComparer.OrdinalIgnoreCase);
        private readonly List<GameObject> activeSpawnedWalls = new List<GameObject>();

        private string officialModsDirectory;
        private Camera cachedCamera;

        // Custom Toybox UI States
        private bool isToyboxVisible = false;
        private Rect toyboxWindowRect = new Rect(40, 50, 420, 720);
        private Vector2 scrollPosition = Vector2.zero;
        private int selectedTab = 0; // 0 = Custom Mods, 1 = Entities, 2 = Maps, 3 = Environment
        private string hoveredItemTitle = "Lua Toybox";
        private string hoveredItemDesc = "Hover over an item to view description.";

        private float cardSlotSize = 105f;

        private string currentlySelectedItem = null;
        private bool isSelectedNativeAsset = false;

        private bool isTimeScaleLocked = false;
        private float lockedTimeScaleValue = 1.0f;

        // Visual Textures
        private Texture2D windowBgTex;
        private Texture2D cardSlotTex;
        private Texture2D cardSlotHoverTex;
        private Texture2D cardSlotSelectedTex;
        private Texture2D solidWallTex;
        private GUIStyle windowStyle;
        private GUIStyle cardButtonStyle;
        private GUIStyle selectedCardButtonStyle;

        private static IDictionary cachedInputDict = null;
        private static List<object> suppressedActionKeys = new List<object>();
        private static bool isGameEKeySuppressed = false;

        private void Awake()
        {
            Logger.LogInfo("=== [PPG Lua Loader] Final Release: Custom Map & Toybox V3 Engine ===");

            UserData.RegisterType<LuaEntity>();

            officialModsDirectory = Path.Combine(Paths.GameRootPath, "Mods");
            EnsureDirectoryExists(officialModsDirectory);

            InitStyles();
            ScanAndBootstrapNativeMods();
        }

        private void InitStyles()
        {
            windowBgTex = CreateSolidColorTexture(2, 2, new Color(0.08f, 0.08f, 0.09f, 0.97f));
            cardSlotTex = CreateSolidColorTexture(2, 2, new Color(0.13f, 0.13f, 0.15f, 1.0f));
            cardSlotHoverTex = CreateSolidColorTexture(2, 2, new Color(0.25f, 0.25f, 0.29f, 1.0f));
            cardSlotSelectedTex = CreateSolidColorTexture(2, 2, new Color(0.18f, 0.48f, 0.24f, 1.0f));
            solidWallTex = CreateSolidColorTexture(2, 2, new Color(0.22f, 0.24f, 0.28f, 1.0f));
        }

        private void Update()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == "Menu" || currentSceneName == "Disclaimer")
            {
                isToyboxVisible = false;
                ClearSelection();
                return;
            }

            if (isTimeScaleLocked)
            {
                Time.timeScale = lockedTimeScaleValue;
            }

            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.F1))
            {
                isToyboxVisible = !isToyboxVisible;
                if (!isToyboxVisible)
                {
                    ClearSelection();
                }
            }

            if (!string.IsNullOrEmpty(currentlySelectedItem))
            {
                SetGameEKeySuppressed(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Vector3 worldPos = GetMouseWorldCoordinates();

                    if (isSelectedNativeAsset)
                    {
                        SpawnNativeAssetAtMouse(currentlySelectedItem);
                    }
                    else
                    {
                        InternalSpawnCustomItem(currentlySelectedItem, worldPos.x, worldPos.y);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ClearSelection();
                }
            }
            else
            {
                SetGameEKeySuppressed(false);
            }

            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }

            float deltaTime = Time.deltaTime;

            for (int i = 0; i < activeMods.Count; i++)
            {
                var mod = activeMods[i];
                if (mod.UpdateCallback != null)
                {
                    try
                    {
                        mod.ScriptEngine.Call(mod.UpdateCallback, deltaTime);
                    }
                    catch (ScriptRuntimeException ex)
                    {
                        Logger.LogError($"[Runtime Error in '{mod.ModName}']: {ex.DecoratedMessage}");
                        mod.DisableUpdate();
                    }
                }
            }
        }

        private void ClearSelection()
        {
            currentlySelectedItem = null;
            isSelectedNativeAsset = false;
            SetGameEKeySuppressed(false);
        }

        private void SetGameEKeySuppressed(bool suppress)
        {
            if (suppress && CatalogBehaviour.Main != null)
            {
                var fields = typeof(CatalogBehaviour).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (f.FieldType == typeof(SpawnableAsset))
                    {
                        f.SetValue(CatalogBehaviour.Main, null);
                    }
                }
            }

            if (suppress == isGameEKeySuppressed) return;

            try
            {
                if (cachedInputDict == null)
                {
                    var fields = typeof(InputSystem).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var f in fields)
                    {
                        if (typeof(IDictionary).IsAssignableFrom(f.FieldType))
                        {
                            var dict = f.GetValue(null) as IDictionary;
                            if (dict != null)
                            {
                                cachedInputDict = dict;
                                break;
                            }
                        }
                    }
                }

                if (cachedInputDict != null)
                {
                    if (suppress)
                    {
                        suppressedActionKeys.Clear();
                        foreach (DictionaryEntry entry in cachedInputDict)
                        {
                            if (entry.Value is InputAction act && act.Key == KeyCode.E)
                            {
                                suppressedActionKeys.Add(entry.Key);
                                act.Key = KeyCode.None;
                                cachedInputDict[entry.Key] = act;
                            }
                        }
                    }
                    else
                    {
                        foreach (var key in suppressedActionKeys)
                        {
                            if (cachedInputDict.Contains(key))
                            {
                                var act = (InputAction)cachedInputDict[key];
                                act.Key = KeyCode.E;
                                cachedInputDict[key] = act;
                            }
                        }
                        suppressedActionKeys.Clear();
                    }

                    isGameEKeySuppressed = suppress;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[Input Suppression Error]: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            if (!isToyboxVisible) return;

            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Menu" || sceneName == "Disclaimer") return;

            SetupDynamicStyles();

            toyboxWindowRect = GUI.Window(8888, toyboxWindowRect, DrawToyboxWindowContent, "", windowStyle);
        }

        private void SetupDynamicStyles()
        {
            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(GUI.skin.window);
                windowStyle.normal.background = windowBgTex;
                windowStyle.padding = new RectOffset(12, 12, 12, 12);
            }

            if (cardButtonStyle == null)
            {
                cardButtonStyle = new GUIStyle(GUI.skin.button);
                cardButtonStyle.normal.background = cardSlotTex;
                cardButtonStyle.hover.background = cardSlotHoverTex;
                cardButtonStyle.active.background = cardSlotHoverTex;
            }

            if (selectedCardButtonStyle == null)
            {
                selectedCardButtonStyle = new GUIStyle(GUI.skin.button);
                selectedCardButtonStyle.normal.background = cardSlotSelectedTex;
                selectedCardButtonStyle.hover.background = cardSlotSelectedTex;
                selectedCardButtonStyle.active.background = cardSlotSelectedTex;
            }
        }

        private void DrawToyboxWindowContent(int windowId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("LUA TOYBOX V3", GUILayout.Height(24));
            if (GUILayout.Button("X", GUILayout.Width(28), GUILayout.Height(22)))
            {
                isToyboxVisible = false;
                ClearSelection();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Slot Size:", GUILayout.Width(65));
            cardSlotSize = GUILayout.HorizontalSlider(cardSlotSize, 70f, 150f, GUILayout.Width(180));
            GUILayout.Label($"{Mathf.RoundToInt(cardSlotSize)}px", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // 4 Navigation Tabs (Including New MAPS Tab!)
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Mods", selectedTab == 0 ? cardButtonStyle : GUI.skin.button, GUILayout.Height(26))) selectedTab = 0;
            if (GUILayout.Button("Entities", selectedTab == 1 ? cardButtonStyle : GUI.skin.button, GUILayout.Height(26))) selectedTab = 1;
            if (GUILayout.Button("Maps", selectedTab == 2 ? cardButtonStyle : GUI.skin.button, GUILayout.Height(26))) selectedTab = 2;
            if (GUILayout.Button("Env", selectedTab == 3 ? cardButtonStyle : GUI.skin.button, GUILayout.Height(26))) selectedTab = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            if (Event.current.type == EventType.Repaint)
            {
                if (!string.IsNullOrEmpty(currentlySelectedItem))
                {
                    hoveredItemTitle = $"★ SELECTED: {currentlySelectedItem}";
                    hoveredItemDesc = "Aim mouse and press [E] to spawn! (Click again or ESC to deselect)";
                }
                else
                {
                    hoveredItemTitle = "Lua Toybox V3";
                    hoveredItemDesc = "Select an item and press [E] to spawn, or build custom maps!";
                }
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(490));

            if (selectedTab == 0)
            {
                DrawCustomItemsGrid();
            }
            else if (selectedTab == 1)
            {
                DrawEntitiesGrid();
            }
            else if (selectedTab == 2)
            {
                DrawMapsPanel();
            }
            else if (selectedTab == 3)
            {
                DrawEnvironmentPanel();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(8);

            DrawBottomInfoBar();

            GUI.DragWindow(new Rect(0, 0, 10000, 24));
        }

        private void DrawCustomItemsGrid()
        {
            if (customItemCatalog.Count == 0)
            {
                GUILayout.Label("No custom Lua items registered.");
                return;
            }

            float currentSlot = cardSlotSize;
            float availableWidth = toyboxWindowRect.width - 40;
            int columns = Mathf.Max(2, Mathf.FloorToInt(availableWidth / (currentSlot + 8)));
            int colIndex = 0;

            GUILayout.BeginHorizontal();

            foreach (var itemPair in customItemCatalog)
            {
                var item = itemPair.Value;

                if (item.CachedIconTexture == null)
                {
                    item.CachedIconTexture = ResolveExplicitModThumbnail(item);
                }

                Rect cardRect = GUILayoutUtility.GetRect(currentSlot, currentSlot, GUILayout.Width(currentSlot), GUILayout.Height(currentSlot));

                if (cardRect.Contains(Event.current.mousePosition))
                {
                    hoveredItemTitle = item.Name;
                    hoveredItemDesc = item.Description;
                }

                bool isThisSelected = (currentlySelectedItem == item.Name && !isSelectedNativeAsset);
                GUIStyle activeStyle = isThisSelected ? selectedCardButtonStyle : cardButtonStyle;

                if (GUI.Button(cardRect, GUIContent.none, activeStyle))
                {
                    if (isThisSelected)
                    {
                        ClearSelection();
                    }
                    else
                    {
                        currentlySelectedItem = item.Name;
                        isSelectedNativeAsset = false;
                    }
                }

                if (item.CachedIconTexture != null)
                {
                    float padding = currentSlot * 0.12f;
                    Rect iconRect = new Rect(cardRect.x + padding, cardRect.y + padding, cardRect.width - (padding * 2), cardRect.height - (padding * 2));
                    GUI.DrawTexture(iconRect, item.CachedIconTexture, ScaleMode.ScaleToFit);
                }

                colIndex++;
                if (colIndex >= columns)
                {
                    colIndex = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(8);
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Space(8);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawEntitiesGrid()
        {
            float currentSlot = cardSlotSize;
            GUILayout.BeginHorizontal();

            DrawEntitySlot("Human", "Human", "A standard human ragdoll. Fragile, flammable, and bleeds.", currentSlot);
            GUILayout.Space(8);
            DrawEntitySlot("Android", "Android", "A humanoid robot. Durable metal frame, immune to fire, vulnerable to electricity.", currentSlot);

            GUILayout.EndHorizontal();
        }

        private void DrawEntitySlot(string assetName, string displayName, string desc, float slotSize)
        {
            Rect cardRect = GUILayoutUtility.GetRect(slotSize, slotSize, GUILayout.Width(slotSize), GUILayout.Height(slotSize));
            if (cardRect.Contains(Event.current.mousePosition))
            {
                hoveredItemTitle = displayName;
                hoveredItemDesc = desc;
            }

            bool isThisSelected = (currentlySelectedItem == assetName && isSelectedNativeAsset);
            GUIStyle activeStyle = isThisSelected ? selectedCardButtonStyle : cardButtonStyle;

            if (GUI.Button(cardRect, GUIContent.none, activeStyle))
            {
                if (isThisSelected)
                {
                    ClearSelection();
                }
                else
                {
                    currentlySelectedItem = assetName;
                    isSelectedNativeAsset = true;
                }
            }

            var spawnable = ModAPI.FindSpawnable(assetName);
            if (spawnable != null && spawnable.Prefab != null)
            {
                var sr = spawnable.Prefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null && sr.sprite.texture != null)
                {
                    float padding = slotSize * 0.12f;
                    Rect iconRect = new Rect(cardRect.x + padding, cardRect.y + padding, cardRect.width - (padding * 2), cardRect.height - (padding * 2));
                    GUI.DrawTexture(iconRect, sr.sprite.texture, ScaleMode.ScaleToFit);
                }
            }
        }

        // Phase 6: Map Builder Tab
        private void DrawMapsPanel()
        {
            GUILayout.Label("Custom Map Presets (Phase 6):");
            if (customMapCatalog.Count == 0)
            {
                GUILayout.Label("(No custom maps registered via Lua yet)");
            }
            else
            {
                foreach (var mapPair in customMapCatalog)
                {
                    var mapPreset = mapPair.Value;
                    if (GUILayout.Button($"[Build Map] {mapPreset.Name}", GUILayout.Height(34)))
                    {
                        BuildCustomMap(mapPreset);
                    }
                }
            }

            GUILayout.Space(14);
            GUILayout.Label("Map Management:");
            if (GUILayout.Button("Clear All Spawned Walls", GUILayout.Height(30)))
            {
                ClearAllSpawnedWalls();
            }
        }

        private void DrawEnvironmentPanel()
        {
            GUILayout.Label("Physics Gravity:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zero Gravity", GUILayout.Height(32)))
            {
                Physics2D.gravity = Vector2.zero;
                hoveredItemTitle = "Zero Gravity";
                hoveredItemDesc = "Objects and ragdolls float weightlessly in space.";
            }
            if (GUILayout.Button("Normal Gravity", GUILayout.Height(32)))
            {
                Physics2D.gravity = new Vector2(0f, -9.81f);
                hoveredItemTitle = "Normal Gravity";
                hoveredItemDesc = "Standard Earth gravity restored (-9.81 m/s^2).";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(14);

            GUILayout.Label("Bullet Time (Slow-Motion):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Slow-Mo (0.2x)", GUILayout.Height(32)))
            {
                isTimeScaleLocked = true;
                lockedTimeScaleValue = 0.2f;
                Time.timeScale = 0.2f;
                hoveredItemTitle = "Slow Motion (Locked)";
                hoveredItemDesc = "Game speed locked at 20% bullet time (will not reset!).";
            }
            if (GUILayout.Button("Normal Speed (1.0x)", GUILayout.Height(32)))
            {
                isTimeScaleLocked = false;
                lockedTimeScaleValue = 1.0f;
                Time.timeScale = 1.0f;
                hoveredItemTitle = "Normal Speed";
                hoveredItemDesc = "Simulation speed unlocked and restored to 100%.";
            }
            GUILayout.EndHorizontal();
        }

        private void DrawBottomInfoBar()
        {
            GUI.Box(GUILayoutUtility.GetRect(390, 72), GUIContent.none, cardButtonStyle);
            Rect barRect = GUILayoutUtility.GetLastRect();

            GUI.Label(new Rect(barRect.x + 10, barRect.y + 6, barRect.width - 20, 22), hoveredItemTitle);
            GUI.Label(new Rect(barRect.x + 10, barRect.y + 28, barRect.width - 20, 42), hoveredItemDesc);
        }

        // Builds solid static walls and applies environmental gravity
        private void BuildCustomMap(CustomMapPreset map)
        {
            Physics2D.gravity = map.Gravity;
            Logger.LogInfo($"Building custom map: [{map.Name}]");

            foreach (var s in map.Structures)
            {
                GameObject wallObj = CreateStaticWall(s.X, s.Y, s.Width, s.Height, s.Material);
                activeSpawnedWalls.Add(wallObj);
            }

            hoveredItemTitle = $"Constructed: {map.Name}";
            hoveredItemDesc = map.Description;
        }

        private void ClearAllSpawnedWalls()
        {
            for (int i = 0; i < activeSpawnedWalls.Count; i++)
            {
                if (activeSpawnedWalls[i] != null)
                {
                    Destroy(activeSpawnedWalls[i]);
                }
            }
            activeSpawnedWalls.Clear();
            Logger.LogInfo("Cleared all custom spawned map walls.");
        }

        private GameObject CreateStaticWall(float x, float y, float width, float height, string materialName)
        {
            GameObject wallObj = new GameObject("CustomWall_" + materialName);
            wallObj.layer = LayerMask.NameToLayer("Objects");
            wallObj.transform.position = new Vector3(x, y, 0f);
            wallObj.transform.localScale = new Vector3(width, height, 1f);

            Rigidbody2D rb = wallObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static; // Completely immovable in physical space!

            BoxCollider2D col = wallObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            SpriteRenderer sr = wallObj.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(solidWallTex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
            sr.color = materialName.Equals("Metal", StringComparison.OrdinalIgnoreCase) ? new Color(0.2f, 0.23f, 0.28f) : new Color(0.35f, 0.35f, 0.35f);

            PhysicalBehaviour pb = wallObj.AddComponent<PhysicalBehaviour>();
            pb.Properties = ModAPI.FindPhysicalProperties(materialName) ?? ModAPI.FindPhysicalProperties("Metal");

            return wallObj;
        }

        private Texture2D ResolveExplicitModThumbnail(CustomLuaItemTemplate item)
        {
            if (!string.IsNullOrEmpty(item.SpritePath))
            {
                Texture2D customTex = LoadPointFilteredTexture(item.ModDirectory, item.SpritePath);
                if (customTex != null) return customTex;
            }

            string modThumbnailFile = !string.IsNullOrEmpty(item.ModThumbnailPath) ? item.ModThumbnailPath : "thumb.png";
            Texture2D modThumbTex = LoadPointFilteredTexture(item.ModDirectory, modThumbnailFile);
            if (modThumbTex != null)
            {
                return modThumbTex;
            }

            return cardSlotHoverTex;
        }

        private Texture2D LoadPointFilteredTexture(string modFolder, string relativePath)
        {
            string combinedPath = Path.Combine(modFolder, relativePath);
            string fullPath = Path.GetFullPath(combinedPath);

            if (!File.Exists(fullPath)) return null;

            byte[] fileBytes = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            if (ImageConversion.LoadImage(tex, fileBytes))
            {
                return tex;
            }

            return null;
        }

        private Texture2D CreateSolidColorTexture(int width, int height, Color col)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void SpawnNativeAssetAtMouse(string assetName)
        {
            var spawnable = ModAPI.FindSpawnable(assetName);
            if (spawnable != null && spawnable.Prefab != null)
            {
                Vector3 worldPos = GetMouseWorldCoordinates();
                UnityEngine.Object.Instantiate(spawnable.Prefab, worldPos, Quaternion.identity);
            }
        }

        private Vector3 GetMouseWorldCoordinates()
        {
            if (cachedCamera == null) cachedCamera = Camera.main;
            Vector3 mouseScreen = Input.mousePosition;
            Vector3 world = (cachedCamera != null) ? cachedCamera.ScreenToWorldPoint(mouseScreen) : Vector3.zero;
            world.z = 0f;
            return world;
        }

        private LuaEntity InternalSpawnCustomItem(string itemName, float x, float y)
        {
            if (!customItemCatalog.TryGetValue(itemName, out CustomLuaItemTemplate template))
            {
                Logger.LogWarning($"[Item.Spawn] No custom template named '{itemName}'");
                return null;
            }

            var baseSpawnable = ModAPI.FindSpawnable(template.BaseItemName);
            if (baseSpawnable == null || baseSpawnable.Prefab == null)
            {
                Logger.LogError($"[Item.Spawn] Base item '{template.BaseItemName}' not found.");
                return null;
            }

            Vector3 spawnPos = new Vector3(x, y, 0f);
            GameObject spawnedObject = UnityEngine.Object.Instantiate(baseSpawnable.Prefab, spawnPos, Quaternion.identity);
            spawnedObject.name = template.Name;

            if (!string.IsNullOrEmpty(template.SpritePath))
            {
                Sprite customSprite = LoadSafeSpriteFromModFolder(template.ModDirectory, template.SpritePath);
                if (customSprite != null)
                {
                    SpriteRenderer renderer = spawnedObject.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sprite = customSprite;
                    }
                }
            }

            LuaEntity entity = new LuaEntity(spawnedObject);

            if (template.OnSpawnCallback != null && template.OnSpawnCallback.Type == DataType.Function)
            {
                try
                {
                    template.ScriptEngine.Call(template.OnSpawnCallback, entity);
                }
                catch (ScriptRuntimeException ex)
                {
                    Logger.LogError($"[onSpawn Error in '{template.Name}']: {ex.DecoratedMessage}");
                }
            }

            return entity;
        }

        private void ScanAndBootstrapNativeMods()
        {
            activeMods.Clear();
            customItemCatalog.Clear();
            customMapCatalog.Clear();

            string[] directories = Directory.GetDirectories(officialModsDirectory);

            foreach (string dir in directories)
            {
                string mainLuaPath = Path.Combine(dir, "main.lua");
                string modJsonPath = Path.Combine(dir, "mod.json");

                if (!File.Exists(mainLuaPath))
                {
                    continue;
                }

                string modName = Path.GetFileName(dir);
                string modThumbPath = "thumb.png";

                if (File.Exists(modJsonPath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(modJsonPath);
                        if (TryParseModJsonExtended(jsonContent, out string parsedName, out string parsedThumb))
                        {
                            if (!string.IsNullOrEmpty(parsedName)) modName = parsedName;
                            if (!string.IsNullOrEmpty(parsedThumb)) modThumbPath = parsedThumb;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Could not parse mod.json for [{modName}]: {ex.Message}");
                    }
                }

                LoadSingleMod(modName, dir, mainLuaPath, modThumbPath);
            }

            Logger.LogInfo($"Toybox Engine active. Synchronized {activeMods.Count} Lua mod(s).");
        }

        private static bool TryParseModJsonExtended(string json, out string name, out string thumbPath)
        {
            name = null;
            thumbPath = "thumb.png";

            if (string.IsNullOrWhiteSpace(json)) return false;

            name = ExtractJsonStringValue(json, "\"Name\"");
            string parsedThumb = ExtractJsonStringValue(json, "\"ThumbnailPath\"");
            if (!string.IsNullOrEmpty(parsedThumb)) thumbPath = parsedThumb;

            return true;
        }

        private static string ExtractJsonStringValue(string json, string key)
        {
            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex >= 0)
            {
                int colonIndex = json.IndexOf(':', keyIndex);
                if (colonIndex >= 0)
                {
                    int firstQuote = json.IndexOf('"', colonIndex + 1);
                    if (firstQuote >= 0)
                    {
                        int secondQuote = json.IndexOf('"', firstQuote + 1);
                        if (secondQuote >= 0)
                        {
                            return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim();
                        }
                    }
                }
            }
            return null;
        }

        private void LoadSingleMod(string modName, string modDir, string entryFilePath, string modThumbPath)
        {
            try
            {
                CoreModules safeModules = CoreModules.Basic |
                                          CoreModules.Table |
                                          CoreModules.String |
                                          CoreModules.Math;

                Script engine = new Script(safeModules);

                engine.Globals["dofile"] = null;
                engine.Globals["loadfile"] = null;
                engine.Globals["load"] = null;

                BindSafeApis(modName, modDir, engine, modThumbPath);

                string luaCode = File.ReadAllText(entryFilePath);
                engine.DoString(luaCode);

                DynValue initFunc = engine.Globals.Get("Initialize");
                if (initFunc.Type == DataType.Function)
                {
                    engine.Call(initFunc);
                }

                activeMods.Add(new LuaModContainer(modName, modDir, engine));
                Logger.LogInfo($"[Toybox Sync] Loaded mod package: [{modName}]");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading mod [{modName}]: {ex.Message}");
            }
        }

        private void BindSafeApis(string modName, string modDir, Script engine, string modThumbPath)
        {
            engine.Globals["Log"] = (Action<string>)((msg) =>
            {
                Logger.LogInfo($"[{modName}] {msg}");
            });

            Table inputTable = new Table(engine);

            inputTable["IsKeyDown"] = (Func<string, bool>)((keyName) =>
            {
                if (Enum.TryParse(keyName, true, out KeyCode parsedKey))
                {
                    return Input.GetKeyDown(parsedKey);
                }
                return false;
            });

            inputTable["GetMouseWorldPosition"] = (Func<Table>)(() =>
            {
                Vector3 worldPos = GetMouseWorldCoordinates();
                Table posTable = new Table(engine);
                posTable["x"] = worldPos.x;
                posTable["y"] = worldPos.y;
                return posTable;
            });

            engine.Globals["Input"] = inputTable;

            Table spawnTable = new Table(engine);

            spawnTable["Create"] = (Func<string, float, float, LuaEntity>)((itemName, x, y) =>
            {
                try
                {
                    var spawnable = ModAPI.FindSpawnable(itemName);
                    if (spawnable == null || spawnable.Prefab == null)
                    {
                        Logger.LogWarning($"[Spawn.Create] Asset '{itemName}' not found.");
                        return null;
                    }

                    Vector3 targetPosition = new Vector3(x, y, 0f);
                    GameObject spawnedInstance = UnityEngine.Object.Instantiate(spawnable.Prefab, targetPosition, Quaternion.identity);

                    return new LuaEntity(spawnedInstance);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Spawn.Create Exception]: {ex.Message}");
                    return null;
                }
            });

            engine.Globals["Spawn"] = spawnTable;

            Table physicsTable = new Table(engine);

            physicsTable["Explode"] = (Action<float, float, float, float>)((originX, originY, radius, forceMagnitude) =>
            {
                Vector2 center = new Vector2(originX, originY);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
                HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>();

                for (int i = 0; i < colliders.Length; i++)
                {
                    Rigidbody2D rb = colliders[i].attachedRigidbody;
                    if (rb != null && !affectedBodies.Contains(rb))
                    {
                        affectedBodies.Add(rb);

                        Vector2 direction = rb.position - center;
                        float distance = direction.magnitude;

                        if (distance > 0.01f)
                        {
                            float falloff = 1f - Mathf.Clamp01(distance / radius);
                            rb.AddForce(direction.normalized * (forceMagnitude * falloff), ForceMode2D.Impulse);
                        }
                    }
                }
            });

            engine.Globals["Physics"] = physicsTable;

            // Phase 6: Map & Structure Builder APIs
            Table mapTable = new Table(engine);

            mapTable["SetGravity"] = (Action<float, float>)((gx, gy) =>
            {
                Physics2D.gravity = new Vector2(gx, gy);
            });

            mapTable["SetTimeScale"] = (Action<float>)((scale) =>
            {
                isTimeScaleLocked = true;
                lockedTimeScaleValue = Mathf.Clamp(scale, 0.01f, 10.0f);
                Time.timeScale = lockedTimeScaleValue;
            });

            // Single wall spawner
            mapTable["CreateWall"] = (Func<float, float, float, float, string, LuaEntity>)((x, y, w, h, mat) =>
            {
                string material = string.IsNullOrEmpty(mat) ? "Metal" : mat;
                GameObject wall = CreateStaticWall(x, y, w, h, material);
                activeSpawnedWalls.Add(wall);
                return new LuaEntity(wall);
            });

            // Register full custom map preset
            mapTable["Register"] = (Action<Table>)((mapConfig) =>
            {
                try
                {
                    string mapName = mapConfig.Get("name").String;
                    string description = mapConfig.Get("description").String ?? "";

                    CustomMapPreset preset = new CustomMapPreset
                    {
                        Name = mapName,
                        Description = description
                    };

                    Table grav = mapConfig.Get("gravity").Table;
                    if (grav != null)
                    {
                        float gx = (float)grav.Get(1).Number;
                        float gy = (float)grav.Get(2).Number;
                        preset.Gravity = new Vector2(gx, gy);
                    }

                    Table structuresTable = mapConfig.Get("structures").Table;
                    if (structuresTable != null)
                    {
                        foreach (var pair in structuresTable.Pairs)
                        {
                            Table s = pair.Value.Table;
                            if (s != null)
                            {
                                preset.Structures.Add(new CustomMapStructureDefinition
                                {
                                    X = (float)s.Get("x").Number,
                                    Y = (float)s.Get("y").Number,
                                    Width = (float)s.Get("width").Number,
                                    Height = (float)s.Get("height").Number,
                                    Material = s.Get("material").String ?? "Metal"
                                });
                            }
                        }
                    }

                    customMapCatalog[mapName] = preset;
                    Logger.LogInfo($"Registered Custom Map Preset: [{mapName}]");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed registering custom map: {ex.Message}");
                }
            });

            engine.Globals["Map"] = mapTable;

            Table itemTable = new Table(engine);

            itemTable["Register"] = (Action<Table>)((configTable) =>
            {
                try
                {
                    string itemName = configTable.Get("name").String;
                    string description = configTable.Get("description").String ?? "";
                    string baseItemName = configTable.Get("baseItem").String ?? "Sword";
                    string spriteRelativePath = configTable.Get("sprite").String;
                    DynValue onSpawnCallback = configTable.Get("onSpawn");

                    customItemCatalog[itemName] = new CustomLuaItemTemplate
                    {
                        Name = itemName,
                        Description = description,
                        BaseItemName = baseItemName,
                        SpritePath = spriteRelativePath,
                        ModDirectory = modDir,
                        ModThumbnailPath = modThumbPath,
                        ScriptEngine = engine,
                        OnSpawnCallback = onSpawnCallback
                    };

                    Logger.LogInfo($"Mounted to Toybox with custom thumb: [{itemName}]");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed registering custom item: {ex.Message}");
                }
            });

            itemTable["Spawn"] = (Func<string, float, float, LuaEntity>)((itemName, x, y) =>
            {
                return InternalSpawnCustomItem(itemName, x, y);
            });

            engine.Globals["Item"] = itemTable;
        }

        private Sprite LoadSafeSpriteFromModFolder(string modFolder, string relativePath)
        {
            string combinedPath = Path.Combine(modFolder, relativePath);
            string fullPath = Path.GetFullPath(combinedPath);
            string fullModFolderPath = Path.GetFullPath(modFolder);

            if (!fullPath.StartsWith(fullModFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogError($"Security Violation: Path Traversal blocked for '{relativePath}'");
                return null;
            }

            if (!File.Exists(fullPath)) return null;

            byte[] fileBytes = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            if (ImageConversion.LoadImage(texture, fileBytes))
            {
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }

            return null;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}