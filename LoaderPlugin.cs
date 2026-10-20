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
    // ========================================================================
    // Auxiliary Vector2 Class for Lua
    // ========================================================================
    [MoonSharpUserData]
    public class LuaVector2
    {
        public float x;
        public float y;

        public LuaVector2() { x = 0f; y = 0f; }
        public LuaVector2(float x, float y) { this.x = x; this.y = y; }

        public float Magnitude() => Mathf.Sqrt(x * x + y * y);

        public LuaVector2 Normalized()
        {
            float mag = Magnitude();
            return mag > 0.0001f ? new LuaVector2(x / mag, y / mag) : new LuaVector2(0f, 0f);
        }

        public float Dot(LuaVector2 other) => (other == null) ? 0f : (x * other.x + y * other.y);
        public float Distance(LuaVector2 other) => (other == null) ? 0f : Mathf.Sqrt(Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2));
    }

    // ========================================================================
    // Collision Event Bridge MonoBehaviour
    // ========================================================================
    public class LuaItemUseListener : MonoBehaviour
    {
        public Action OnUseAction;

        public void Use()
        {
            OnUseAction?.Invoke();
        }
        public Action<Collision2D> onCollisionEnterAction;
        public Action<Collision2D> onCollisionExitAction;

        private void OnCollisionEnter2D(Collision2D collision) => onCollisionEnterAction?.Invoke(collision);
        private void OnCollisionExit2D(Collision2D collision) => onCollisionExitAction?.Invoke(collision);
    }

    // ========================================================================
    // Immediate 2D Debug Geometry Renderer
    // ========================================================================
    public class LuaDebugRenderer : MonoBehaviour
    {
        private class DebugShape
        {
            public int shapeType; // 0: Line, 1: Circle, 2: Box
            public Vector2 p1, p2;
            public float radius, width, height;
            public float expireTime;
        }

        private static readonly List<DebugShape> activeShapes = new List<DebugShape>();
        private static Material renderMaterial;

        public static void AddLine(Vector2 a, Vector2 b, float duration) =>
            activeShapes.Add(new DebugShape { shapeType = 0, p1 = a, p2 = b, expireTime = Time.time + duration });

        public static void AddCircle(Vector2 center, float radius, float duration) =>
            activeShapes.Add(new DebugShape { shapeType = 1, p1 = center, radius = radius, expireTime = Time.time + duration });

        public static void AddBox(Vector2 center, float width, float height, float duration) =>
            activeShapes.Add(new DebugShape { shapeType = 2, p1 = center, width = width, height = height, expireTime = Time.time + duration });

        private void OnPostRender()
        {
            if (activeShapes.Count == 0) return;

            if (!renderMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                renderMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }

            renderMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            float now = Time.time;
            for (int i = activeShapes.Count - 1; i >= 0; i--)
            {
                var s = activeShapes[i];
                if (now > s.expireTime)
                {
                    activeShapes.RemoveAt(i);
                    continue;
                }

                if (s.shapeType == 0)
                {
                    GL.Vertex3(s.p1.x, s.p1.y, 0f);
                    GL.Vertex3(s.p2.x, s.p2.y, 0f);
                }
                else if (s.shapeType == 1)
                {
                    int segs = 24;
                    for (int j = 0; j < segs; j++)
                    {
                        float a1 = j * Mathf.PI * 2f / segs;
                        float a2 = (j + 1) * Mathf.PI * 2f / segs;
                        GL.Vertex3(s.p1.x + Mathf.Cos(a1) * s.radius, s.p1.y + Mathf.Sin(a1) * s.radius, 0f);
                        GL.Vertex3(s.p1.x + Mathf.Cos(a2) * s.radius, s.p1.y + Mathf.Sin(a2) * s.radius, 0f);
                    }
                }
                else if (s.shapeType == 2)
                {
                    float hw = s.width / 2f, hh = s.height / 2f;
                    Vector2 bl = s.p1 + new Vector2(-hw, -hh), br = s.p1 + new Vector2(hw, -hh);
                    Vector2 tr = s.p1 + new Vector2(hw, hh), tl = s.p1 + new Vector2(-hw, hh);

                    GL.Vertex3(bl.x, bl.y, 0f); GL.Vertex3(br.x, br.y, 0f);
                    GL.Vertex3(br.x, br.y, 0f); GL.Vertex3(tr.x, tr.y, 0f);
                    GL.Vertex3(tr.x, tr.y, 0f); GL.Vertex3(tl.x, tl.y, 0f);
                    GL.Vertex3(tl.x, tl.y, 0f); GL.Vertex3(bl.x, bl.y, 0f);
                }
            }
            GL.End();
            GL.PopMatrix();
        }
    }

    // ========================================================================
    // MoonSharp Lua Entity Wrapper
    // ========================================================================
    [MoonSharpUserData]
    public class LuaEntity
    {
        private readonly GameObject internalObject;
        private readonly HashSet<string> customTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public GameObject InternalObject => internalObject;

        public LuaEntity(GameObject targetObject)
        {
            this.internalObject = targetObject;
        }

        // --- Transform API ---
        public void SetPosition(float x, float y)
        {
            if (internalObject == null) return;
            Vector3 current = internalObject.transform.position;
            internalObject.transform.position = new Vector3(x, y, current.z);
            Physics2D.SyncTransforms();
        }

        public LuaVector2 GetPosition()
        {
            if (internalObject == null) return new LuaVector2(0f, 0f);
            Vector3 pos = internalObject.transform.position;
            return new LuaVector2(pos.x, pos.y);
        }

        public void SetRotation(float angle)
        {
            if (internalObject == null) return;
            internalObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            Physics2D.SyncTransforms();
        }

        public float GetRotation() => internalObject != null ? internalObject.transform.eulerAngles.z : 0f;

        public void SetScale(float x, float y)
        {
            if (internalObject == null) return;
            internalObject.transform.localScale = new Vector3(x, y, 1f);
        }

        public LuaVector2 GetScale()
        {
            if (internalObject == null) return new LuaVector2(0f, 0f);
            Vector3 scale = internalObject.transform.localScale;
            return new LuaVector2(scale.x, scale.y);
        }

        public void SetLocalPosition(float x, float y)
        {
            if (internalObject == null) return;
            internalObject.transform.localPosition = new Vector3(x, y, 0f);
        }

        public LuaVector2 GetLocalPosition()
        {
            if (internalObject == null) return new LuaVector2(0f, 0f);
            Vector3 pos = internalObject.transform.localPosition;
            return new LuaVector2(pos.x, pos.y);
        }

        public void SetParent(LuaEntity parent)
        {
            if (internalObject == null) return;
            internalObject.transform.SetParent(parent?.internalObject != null ? parent.internalObject.transform : null);
        }

        public LuaEntity GetParent()
        {
            if (internalObject?.transform.parent == null) return null;
            return new LuaEntity(internalObject.transform.parent.gameObject);
        }

        // --- Physics API ---
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

        public void AddForceAtPosition(float fx, float fy, float px, float py)
        {
            if (internalObject == null) return;
            Vector2 force = new Vector2(fx, fy);
            Vector2 pos = new Vector2(px, py);
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.AddForceAtPosition(force, pos, ForceMode2D.Impulse);
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

        public LuaVector2 GetVelocity()
        {
            var rb = internalObject != null ? internalObject.GetComponentInChildren<Rigidbody2D>() : null;
            return rb != null ? new LuaVector2(rb.velocity.x, rb.velocity.y) : new LuaVector2(0f, 0f);
        }

        public void AddVelocity(float vx, float vy)
        {
            if (internalObject == null) return;
            Vector2 delta = new Vector2(vx, vy);
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.velocity += delta;
            }
        }

        public void SetAngularVelocity(float speed)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.angularVelocity = speed;
            }
        }

        public float GetAngularVelocity()
        {
            var rb = internalObject?.GetComponentInChildren<Rigidbody2D>();
            return rb != null ? rb.angularVelocity : 0f;
        }

        public void AddTorque(float torque)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.AddTorque(torque, ForceMode2D.Impulse);
            }
        }

        public void SetMass(float mass)
        {
            if (internalObject == null) return;
            var bodies = internalObject.GetComponentsInChildren<Rigidbody2D>();
            if (bodies.Length == 0) return;
            float perBody = Mathf.Max(0.001f, mass / bodies.Length);
            foreach (var rb in bodies) rb.mass = perBody;
        }

        public float GetMass()
        {
            if (internalObject == null) return 0f;
            float total = 0f;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                total += rb.mass;
            }
            return total;
        }

        public void SetGravityScale(float scale)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.gravityScale = scale;
            }
        }

        public float GetGravityScale()
        {
            var rb = internalObject?.GetComponentInChildren<Rigidbody2D>();
            return rb != null ? rb.gravityScale : 1f;
        }

        public void SetKinematic(bool enabled)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.bodyType = enabled ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            }
        }

        public bool IsKinematic()
        {
            var rb = internalObject?.GetComponentInChildren<Rigidbody2D>();
            return rb != null && rb.bodyType == RigidbodyType2D.Kinematic;
        }

        public void SetCollisionEnabled(bool enabled)
        {
            if (internalObject == null) return;
            foreach (var col in internalObject.GetComponentsInChildren<Collider2D>())
            {
                col.enabled = enabled;
            }
        }

        public bool IsCollisionEnabled()
        {
            var col = internalObject?.GetComponentInChildren<Collider2D>();
            return col != null && col.enabled;
        }

        public void SetDrag(float drag)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.drag = drag;
            }
        }

        public void SetAngularDrag(float drag)
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.angularDrag = drag;
            }
        }

        public void Freeze()
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        public void Unfreeze()
        {
            if (internalObject == null) return;
            foreach (var rb in internalObject.GetComponentsInChildren<Rigidbody2D>())
            {
                rb.constraints = RigidbodyConstraints2D.None;
            }
        }

        // --- Lifecycle API ---
        public bool IsAlive()
        {
            if (internalObject == null) return false;

            var person = internalObject.GetComponent<PersonBehaviour>();
            if (person != null)
            {
                return person.Consciousness > 0.01f;
            }

            var limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            if (limbs.Length > 0)
            {
                foreach (var l in limbs)
                {
                    if (l.Health > 0f) return true;
                }
                return false;
            }

            return true;
        }

        public bool IsDeleted() => internalObject == null;

        // Destroy and Delete are aliases for compatibility with both API naming styles.
        public void Destroy()
        {
            if (internalObject != null)
            {
                LoaderPlugin.DispatchGlobalEvent("OnDelete", this);
                UnityEngine.Object.Destroy(internalObject);
            }
        }

        public int GetInstanceId() => internalObject != null ? internalObject.GetInstanceID() : 0;
        public string GetName() => internalObject != null ? internalObject.name : string.Empty;
        public void SetName(string name) { if (internalObject != null) internalObject.name = name; }

        public new string GetType() => internalObject != null ? internalObject.GetType().Name : string.Empty;

        public List<string> GetComponents()
        {
            var list = new List<string>();
            if (internalObject == null) return list;
            foreach (var c in internalObject.GetComponentsInChildren<Component>())
            {
                if (c != null && !list.Contains(c.GetType().Name)) list.Add(c.GetType().Name);
            }
            return list;
        }

        public bool HasComponent(string componentName)
        {
            if (internalObject == null || string.IsNullOrEmpty(componentName)) return false;
            foreach (var c in internalObject.GetComponentsInChildren<Component>())
            {
                if (c != null && c.GetType().Name.Equals(componentName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        // --- State API ---
        public void Ignite()
        {
            if (internalObject == null) return;
            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++) physicalParts[i].Ignite();
        }

        public void Electrocute(float chargeAmount)
        {
            if (internalObject == null) return;
            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++) physicalParts[i].Charge += chargeAmount;
        }

        public void SetTemperature(float degreesCelsius)
        {
            if (internalObject == null) return;
            PhysicalBehaviour[] physicalParts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < physicalParts.Length; i++) physicalParts[i].Temperature = degreesCelsius;
        }

        public float GetTemperature()
        {
            var pb = internalObject?.GetComponentInChildren<PhysicalBehaviour>();
            return pb != null ? pb.Temperature : 20f;
        }

        public void SetHealth(float value)
        {
            if (internalObject == null) return;
            foreach (var limb in internalObject.GetComponentsInChildren<LimbBehaviour>())
            {
                limb.Health = Mathf.Clamp(value, 0f, 100f);
            }
        }

        public float GetHealth()
        {
            if (internalObject == null) return 0f;
            var limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            if (limbs.Length == 0) return 100f;
            float sum = 0f;
            foreach (var l in limbs) sum += l.Health;
            return sum / limbs.Length;
        }

        public void SetMaxHealth(float value)
        {
            if (internalObject == null) return;
            foreach (var limb in internalObject.GetComponentsInChildren<LimbBehaviour>())
            {
                limb.InitialHealth = Mathf.Max(1f, value);
            }
        }

        public float GetMaxHealth()
        {
            var limb = internalObject?.GetComponentInChildren<LimbBehaviour>();
            return limb != null ? limb.InitialHealth : 100f;
        }

        public void SetColor(float r, float g, float b, float a)
        {
            if (internalObject == null) return;
            Color col = new Color(r, g, b, a);
            foreach (var sr in internalObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.color = col;
            }
        }

        public Table GetColor(Script engine)
        {
            Table t = new Table(engine);
            var sr = internalObject?.GetComponentInChildren<SpriteRenderer>();
            Color c = sr != null ? sr.color : Color.white;
            t["r"] = c.r; t["g"] = c.g; t["b"] = c.b; t["a"] = c.a;
            return t;
        }

        public void SetVisible(bool enabled)
        {
            if (internalObject == null) return;
            foreach (var sr in internalObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.enabled = enabled;
            }
        }

        public bool IsVisible()
        {
            var sr = internalObject?.GetComponentInChildren<SpriteRenderer>();
            return sr != null && sr.enabled;
        }

        public void SetLayer(int layer)
        {
            if (internalObject == null) return;
            foreach (var t in internalObject.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = layer;
            }
        }

        public int GetLayer() => internalObject != null ? internalObject.layer : 0;

        public void SetTag(string tag) { if (!string.IsNullOrEmpty(tag)) customTags.Add(tag); }
        public bool HasTag(string tag) => customTags.Contains(tag);
        public void RemoveTag(string tag) => customTags.Remove(tag);

        public void Dismember()
        {
            if (internalObject == null) return;
            Vector2 centerPosition = internalObject.transform.position;
            Joint2D[] joints = internalObject.GetComponentsInChildren<Joint2D>();
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i] != null) joints[i].enabled = false;
            }

            LimbBehaviour[] limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            for (int i = 0; i < limbs.Length; i++)
            {
                Vector2 limbPos = limbs[i].transform.position;
                Vector2 radialDirection = (limbPos - centerPosition).normalized;
                if (radialDirection == Vector2.zero) radialDirection = UnityEngine.Random.insideUnitCircle.normalized;

                Rigidbody2D rb = limbs[i].GetComponent<Rigidbody2D>();
                if (rb != null) rb.AddForce(radialDirection * UnityEngine.Random.Range(10f, 18f), ForceMode2D.Impulse);

                if (limbs[i].CirculationBehaviour != null) limbs[i].CirculationBehaviour.BleedingRate = 15f;
            }
        }

        public void Crush()
        {
            if (internalObject == null) return;
            LimbBehaviour[] limbs = internalObject.GetComponentsInChildren<LimbBehaviour>();
            for (int i = 0; i < limbs.Length; i++) limbs[i].Crush();
        }

        public void Disintegrate()
        {
            if (internalObject == null) return;
            PhysicalBehaviour[] parts = internalObject.GetComponentsInChildren<PhysicalBehaviour>();
            for (int i = 0; i < parts.Length; i++) parts[i].Disintegrate();
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
            if (person != null) person.Consciousness = 1f;
        }

        public void Inject(string liquidName, float amount)
        {
            if (internalObject == null || string.IsNullOrEmpty(liquidName)) return;

            Liquid targetLiquid = Liquid.GetLiquid(liquidName)
                               ?? Liquid.GetLiquid(liquidName.ToUpper())
                               ?? Liquid.GetLiquid(liquidName.ToLower());

            if (targetLiquid == null) return;

            CirculationBehaviour circulation = internalObject.GetComponentInChildren<CirculationBehaviour>();
            if (circulation != null) circulation.AddLiquid(targetLiquid, amount);
        }

        public void Delete()
        {
            if (internalObject != null)
            {
                LoaderPlugin.DispatchGlobalEvent("OnDelete", this);
                UnityEngine.Object.Destroy(internalObject);
            }
        }

        // --- Collision API ---
        public List<string> GetColliders()
        {
            var list = new List<string>();
            if (internalObject == null) return list;
            foreach (var c in internalObject.GetComponentsInChildren<Collider2D>())
            {
                list.Add(c.GetType().Name);
            }
            return list;
        }

        public void SetCollisionLayer(int layer) => SetLayer(layer);
        public int GetCollisionLayer() => GetLayer();

        public void SetCollisionMask(int mask)
        {
            if (internalObject == null) return;
            // Unity 2021 compatibility: Set layer collision interaction
            int myLayer = internalObject.layer;
            for (int i = 0; i < 32; i++)
            {
                bool collide = (mask & (1 << i)) != 0;
                Physics2D.IgnoreLayerCollision(myLayer, i, !collide);
            }
        }

        public int GetCollisionMask() => ~0;

        public void IgnoreCollision(LuaEntity other, bool ignore)
        {
            if (internalObject == null || other?.internalObject == null) return;
            var myCols = internalObject.GetComponentsInChildren<Collider2D>();
            var otherCols = other.internalObject.GetComponentsInChildren<Collider2D>();
            foreach (var c1 in myCols)
            {
                foreach (var c2 in otherCols)
                {
                    Physics2D.IgnoreCollision(c1, c2, ignore);
                }
            }
        }

        public List<LuaEntity> OverlapCircle(float radius)
        {
            var result = new List<LuaEntity>();
            if (internalObject == null) return result;
            var hits = Physics2D.OverlapCircleAll(internalObject.transform.position, radius);
            HashSet<GameObject> roots = new HashSet<GameObject>();
            foreach (var h in hits)
            {
                var root = h.transform.root.gameObject;
                if (root != internalObject && roots.Add(root)) result.Add(new LuaEntity(root));
            }
            return result;
        }

        public List<LuaEntity> OverlapBox(float width, float height)
        {
            var result = new List<LuaEntity>();
            if (internalObject == null) return result;
            var hits = Physics2D.OverlapBoxAll(internalObject.transform.position, new Vector2(width, height), internalObject.transform.eulerAngles.z);
            HashSet<GameObject> roots = new HashSet<GameObject>();
            foreach (var h in hits)
            {
                var root = h.transform.root.gameObject;
                if (root != internalObject && roots.Add(root)) result.Add(new LuaEntity(root));
            }
            return result;
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

    // ========================================================================
    // Main Loader Plugin
    // ========================================================================
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
        private int selectedTab = 0;
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
        public struct LuaEventSubscription
        {
            public Script Engine;
            public DynValue Callback;
        }

        public static readonly Dictionary<string, List<LuaEventSubscription>> RegisteredGlobalEvents =
            new Dictionary<string, List<LuaEventSubscription>>(StringComparer.OrdinalIgnoreCase);

        public static void DispatchGlobalEvent(string eventName, params object[] args)
        {
            if (!RegisteredGlobalEvents.TryGetValue(eventName, out var subscriptions)) return;
            for (int i = subscriptions.Count - 1; i >= 0; i--)
            {
                var sub = subscriptions[i];
                if (sub.Engine != null && sub.Callback != null)
                {
                    try
                    {
                        sub.Engine.Call(sub.Callback, args);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Lua Event Exception - {eventName}]: {ex.Message}");
                    }
                }
            }
        }

        private void Awake()
        {
            Logger.LogInfo("=== [PPG Lua Loader] Final Release: Custom Map & Toybox V3 Engine ===");

            UserData.RegisterType<LuaEntity>();
            UserData.RegisterType<LuaVector2>();

            if (gameObject.GetComponent<LuaDebugRenderer>() == null)
            {
                gameObject.AddComponent<LuaDebugRenderer>();
            }

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

            // Global Key Events Dispatch
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kc)) DispatchGlobalEvent("OnKeyDown", kc.ToString());
                }
            }
            foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyUp(kc)) DispatchGlobalEvent("OnKeyUp", kc.ToString());
            }
            for (int mouseBtn = 0; mouseBtn < 3; mouseBtn++)
            {
                if (Input.GetMouseButtonDown(mouseBtn)) DispatchGlobalEvent("OnMouseDown", mouseBtn);
                if (Input.GetMouseButtonUp(mouseBtn)) DispatchGlobalEvent("OnMouseUp", mouseBtn);
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

        private void DrawMapsPanel()
        {
            GUILayout.Label("Custom Map Presets");
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
            rb.bodyType = RigidbodyType2D.Static;

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
                GameObject spawned = UnityEngine.Object.Instantiate(spawnable.Prefab, worldPos, Quaternion.identity);
                AttachCollisionBridge(spawned);
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
            AttachCollisionBridge(spawnedObject);

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

            DispatchGlobalEvent("OnSpawn", entity);
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

        // ====================================================================
        // Complete Lua API Registry
        // ====================================================================
        private void BindSafeApis(string modName, string modDir, Script engine, string modThumbPath)
        {
            // --- 13. Logging API ---
            engine.Globals["Log"] = (Action<string>)((msg) => Logger.LogInfo($"[{modName}] {msg}"));
            engine.Globals["LogWarning"] = (Action<string>)((msg) => Logger.LogWarning($"[{modName}] {msg}"));
            engine.Globals["LogError"] = (Action<string>)((msg) => Logger.LogError($"[{modName}] {msg}"));

            // --- 9. Event Hooks ---
            string[] eventHooks = { "OnSpawn", "OnDelete", "OnCollision", "OnCollisionEnter", "OnCollisionExit", "OnKeyDown", "OnKeyUp", "OnMouseDown", "OnMouseUp" };
            foreach (var evt in eventHooks)
            {
                engine.Globals[evt] = (Action<DynValue>)((callback) =>
                {
                    if (callback.Type == DataType.Function)
                    {
                        if (!RegisteredGlobalEvents.ContainsKey(evt))
                            RegisteredGlobalEvents[evt] = new List<LuaEventSubscription>();

                        RegisteredGlobalEvents[evt].Add(new LuaEventSubscription
                        {
                            Engine = engine,
                            Callback = callback
                        });
                    }
                });
            }

            // --- 7. Input API ---
            Table inputTable = new Table(engine);
            inputTable["IsKeyDown"] = (Func<string, bool>)((key) => Enum.TryParse(key, true, out KeyCode k) && Input.GetKeyDown(k));
            inputTable["IsKeyHeld"] = (Func<string, bool>)((key) => Enum.TryParse(key, true, out KeyCode k) && Input.GetKey(k));
            inputTable["IsKeyUp"] = (Func<string, bool>)((key) => Enum.TryParse(key, true, out KeyCode k) && Input.GetKeyUp(k));
            inputTable["IsMouseButtonDown"] = (Func<int, bool>)((b) => Input.GetMouseButtonDown(b));
            inputTable["IsMouseButtonHeld"] = (Func<int, bool>)((b) => Input.GetMouseButton(b));
            inputTable["IsMouseButtonUp"] = (Func<int, bool>)((b) => Input.GetMouseButtonUp(b));
            inputTable["GetMouseScrollDelta"] = (Func<float>)(() => Input.mouseScrollDelta.y);
            inputTable["GetMouseWorldPosition"] = (Func<Table>)(() =>
            {
                Vector3 worldPos = GetMouseWorldCoordinates();
                Table posTable = new Table(engine);
                posTable["x"] = worldPos.x;
                posTable["y"] = worldPos.y;
                return posTable;
            });
            engine.Globals["Input"] = inputTable;

            // --- 5. Spawn API ---
            Table spawnTable = new Table(engine);
            spawnTable["Create"] = (Func<string, float, float, LuaEntity>)((itemName, x, y) =>
            {
                var spawnable = ModAPI.FindSpawnable(itemName);
                if (spawnable == null || spawnable.Prefab == null) return null;
                GameObject spawned = UnityEngine.Object.Instantiate(spawnable.Prefab, new Vector3(x, y, 0), Quaternion.identity);
                var ent = new LuaEntity(spawned);
                AttachCollisionBridge(spawned);
                DispatchGlobalEvent("OnSpawn", ent);
                return ent;
            });

            spawnTable["CreateAtRotation"] = (Func<string, float, float, float, LuaEntity>)((itemName, x, y, rot) =>
            {
                var spawnable = ModAPI.FindSpawnable(itemName);
                if (spawnable == null || spawnable.Prefab == null) return null;
                GameObject spawned = UnityEngine.Object.Instantiate(spawnable.Prefab, new Vector3(x, y, 0), Quaternion.Euler(0, 0, rot));
                var ent = new LuaEntity(spawned);
                AttachCollisionBridge(spawned);
                DispatchGlobalEvent("OnSpawn", ent);
                return ent;
            });

            spawnTable["CreateBatch"] = (Func<string, Table, List<LuaEntity>>)((itemName, positions) =>
            {
                var list = new List<LuaEntity>();
                var spawnable = ModAPI.FindSpawnable(itemName);
                if (spawnable == null || spawnable.Prefab == null) return list;

                foreach (var pair in positions.Pairs)
                {
                    Table pt = pair.Value.Table;
                    if (pt != null)
                    {
                        float px = (float)pt.Get("x").Number;
                        float py = (float)pt.Get("y").Number;
                        GameObject spawned = UnityEngine.Object.Instantiate(spawnable.Prefab, new Vector3(px, py, 0), Quaternion.identity);
                        var ent = new LuaEntity(spawned);
                        AttachCollisionBridge(spawned);
                        DispatchGlobalEvent("OnSpawn", ent);
                        list.Add(ent);
                    }
                }
                return list;
            });

            spawnTable["Clone"] = (Func<LuaEntity, float, float, LuaEntity>)((entity, x, y) =>
            {
                if (entity?.InternalObject == null) return null;
                GameObject cloned = UnityEngine.Object.Instantiate(entity.InternalObject, new Vector3(x, y, 0), entity.InternalObject.transform.rotation);
                var ent = new LuaEntity(cloned);
                AttachCollisionBridge(cloned);
                DispatchGlobalEvent("OnSpawn", ent);
                return ent;
            });

            spawnTable["GetAvailableAssets"] = (Func<List<string>>)(() =>
            {
                List<string> assets = new List<string>();
                foreach (var s in Resources.FindObjectsOfTypeAll<SpawnableAsset>())
                {
                    if (s != null && !string.IsNullOrEmpty(s.name) && !assets.Contains(s.name))
                    {
                        assets.Add(s.name);
                    }
                }
                return assets;
            });

            spawnTable["Exists"] = (Func<string, bool>)((assetName) => ModAPI.FindSpawnable(assetName) != null);
            engine.Globals["Spawn"] = spawnTable;

            // --- 6. World API ---
            Table worldTable = new Table(engine);
            worldTable["GetEntities"] = (Func<List<LuaEntity>>)(() =>
            {
                var list = new List<LuaEntity>();
                foreach (var pb in UnityEngine.Object.FindObjectsOfType<PhysicalBehaviour>())
                {
                    if (pb.transform.root != null) list.Add(new LuaEntity(pb.transform.root.gameObject));
                }
                return list;
            });

            worldTable["GetEntitiesInRadius"] = (Func<float, float, float, List<LuaEntity>>)((x, y, radius) =>
            {
                var list = new List<LuaEntity>();
                var colliders = Physics2D.OverlapCircleAll(new Vector2(x, y), radius);
                HashSet<GameObject> roots = new HashSet<GameObject>();
                foreach (var c in colliders)
                {
                    if (roots.Add(c.transform.root.gameObject)) list.Add(new LuaEntity(c.transform.root.gameObject));
                }
                return list;
            });

            worldTable["GetEntitiesInBox"] = (Func<float, float, float, float, List<LuaEntity>>)((x, y, width, height) =>
            {
                var list = new List<LuaEntity>();
                var colliders = Physics2D.OverlapBoxAll(new Vector2(x, y), new Vector2(width, height), 0f);
                HashSet<GameObject> roots = new HashSet<GameObject>();
                foreach (var c in colliders)
                {
                    if (roots.Add(c.transform.root.gameObject)) list.Add(new LuaEntity(c.transform.root.gameObject));
                }
                return list;
            });

            worldTable["GetEntityById"] = (Func<int, LuaEntity>)((instanceId) =>
            {
                foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    if (go.GetInstanceID() == instanceId) return new LuaEntity(go);
                }
                return null;
            });

            worldTable["GetEntitiesByName"] = (Func<string, List<LuaEntity>>)((name) =>
            {
                var list = new List<LuaEntity>();
                foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    if (go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        list.Add(new LuaEntity(go));
                }
                return list;
            });

            worldTable["GetEntitiesByTag"] = (Func<string, List<LuaEntity>>)((tag) =>
            {
                var list = new List<LuaEntity>();
                foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    if (go.CompareTag(tag)) list.Add(new LuaEntity(go));
                }
                return list;
            });

            worldTable["GetEntityCount"] = (Func<int>)(() => UnityEngine.Object.FindObjectsOfType<PhysicalBehaviour>().Length);
            worldTable["GetBounds"] = (Func<Table>)(() =>
            {
                Table b = new Table(engine);
                b["minX"] = -100f; b["maxX"] = 100f; b["minY"] = -50f; b["maxY"] = 50f;
                return b;
            });
            worldTable["GetMouseWorldPosition"] = inputTable["GetMouseWorldPosition"];
            worldTable["GetCameraPosition"] = (Func<Table>)(() =>
            {
                Table t = new Table(engine);
                Vector3 p = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                t["x"] = p.x; t["y"] = p.y;
                return t;
            });
            engine.Globals["World"] = worldTable;

            // --- Existing Physics API ---
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

            // --- 10. Time API ---
            Table timeTable = new Table(engine);
            timeTable["GetDeltaTime"] = (Func<float>)(() => Time.deltaTime);
            timeTable["GetTime"] = (Func<float>)(() => Time.time);
            timeTable["GetFixedDeltaTime"] = (Func<float>)(() => Time.fixedDeltaTime);
            timeTable["GetTimeScale"] = (Func<float>)(() => Time.timeScale);
            timeTable["SetTimeScale"] = (Action<float>)((scale) =>
            {
                isTimeScaleLocked = true;
                lockedTimeScaleValue = Mathf.Clamp(scale, 0.01f, 10f);
                Time.timeScale = lockedTimeScaleValue;
            });
            timeTable["GetFrameRate"] = (Func<float>)(() => 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime));
            engine.Globals["Time"] = timeTable;

            // --- 11. Map API ---
            Table mapTable = new Table(engine);
            mapTable["SetGravity"] = (Action<float, float>)((gx, gy) => Physics2D.gravity = new Vector2(gx, gy));
            mapTable["GetGravity"] = (Func<Table>)(() =>
            {
                Table g = new Table(engine);
                g["x"] = Physics2D.gravity.x; g["y"] = Physics2D.gravity.y;
                return g;
            });
            mapTable["SetTimeScale"] = timeTable["SetTimeScale"];
            mapTable["CreateWall"] = (Func<float, float, float, float, string, LuaEntity>)((x, y, w, h, mat) =>
            {
                string material = string.IsNullOrEmpty(mat) ? "Metal" : mat;
                GameObject wall = CreateStaticWall(x, y, w, h, material);
                activeSpawnedWalls.Add(wall);
                return new LuaEntity(wall);
            });
            mapTable["GetBounds"] = worldTable["GetBounds"];
            mapTable["GetEntities"] = worldTable["GetEntities"];
            mapTable["Register"] = (Action<Table>)((mapConfig) =>
            {
                try
                {
                    string mName = mapConfig.Get("name").String;
                    string desc = mapConfig.Get("description").String ?? "";
                    CustomMapPreset preset = new CustomMapPreset { Name = mName, Description = desc };

                    Table grav = mapConfig.Get("gravity").Table;
                    if (grav != null)
                    {
                        preset.Gravity = new Vector2((float)grav.Get(1).Number, (float)grav.Get(2).Number);
                    }

                    Table structs = mapConfig.Get("structures").Table;
                    if (structs != null)
                    {
                        foreach (var pair in structs.Pairs)
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
                    customMapCatalog[mName] = preset;
                    Logger.LogInfo($"Registered Custom Map Preset: [{mName}]");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed registering map: {ex.Message}");
                }
            });
            engine.Globals["Map"] = mapTable;

            // --- 12. Item API ---
            Table itemTable = new Table(engine);
            Action<Table> registerItem = (configTable) =>
            {
                try
                {
                    string iName = configTable.Get("name").String;
                    string description = configTable.Get("description").String ?? "";
                    string baseItem = configTable.Get("baseItem").String ?? "Sword";
                    string spriteRelPath = configTable.Get("sprite").String;
                    DynValue onSpawnCallback = configTable.Get("onSpawn");

                    customItemCatalog[iName] = new CustomLuaItemTemplate
                    {
                        Name = iName,
                        Description = description,
                        BaseItemName = baseItem,
                        SpritePath = spriteRelPath,
                        ModDirectory = modDir,
                        ModThumbnailPath = modThumbPath,
                        ScriptEngine = engine,
                        OnSpawnCallback = onSpawnCallback
                    };
                    Logger.LogInfo($"Registered Custom Lua Item: [{iName}]");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed registering item: {ex.Message}");
                }
            };

            itemTable["Register"] = registerItem;
            itemTable["Unregister"] = (Action<string>)((name) => customItemCatalog.Remove(name));
            itemTable["GetRegisteredItems"] = (Func<List<string>>)(() => new List<string>(customItemCatalog.Keys));
            itemTable["GetItem"] = (Func<string, Table>)((name) =>
            {
                if (!customItemCatalog.TryGetValue(name, out var item)) return null;
                Table t = new Table(engine);
                t["name"] = item.Name;
                t["description"] = item.Description;
                t["baseItem"] = item.BaseItemName;
                return t;
            });
            itemTable["Spawn"] = (Func<string, float, float, LuaEntity>)((iName, x, y) => InternalSpawnCustomItem(iName, x, y));
            itemTable["OnSpawn"] = (Action<Table, DynValue>)((config, callback) =>
            {
                config["onSpawn"] = callback;
                registerItem(config);
            });
            itemTable["OnUse"] = (Action<Table, DynValue>)((config, callback) =>
            {
                config["onSpawn"] = DynValue.NewCallback((ctx, args) =>
                {
                    if (args.Count > 0 && args[0].UserData?.Object is LuaEntity ent && ent.InternalObject != null)
                    {
                        var listener = ent.InternalObject.GetComponent<LuaItemUseListener>()
                                    ?? ent.InternalObject.AddComponent<LuaItemUseListener>();

                        listener.OnUseAction = () =>
                        {
                            try { engine.Call(callback, ent); }
                            catch (Exception ex) { Debug.LogError($"[Item OnUse Error]: {ex.Message}"); }
                        };
                    }
                    return DynValue.Nil;
                });
                registerItem(config);
            });
            engine.Globals["Item"] = itemTable;

            // --- 13. Debug API ---
            Table debugTable = new Table(engine);
            debugTable["DrawLine"] = (Action<float, float, float, float, float>)((x1, y1, x2, y2, dur) => LuaDebugRenderer.AddLine(new Vector2(x1, y1), new Vector2(x2, y2), dur));
            debugTable["DrawCircle"] = (Action<float, float, float, float>)((x, y, r, dur) => LuaDebugRenderer.AddCircle(new Vector2(x, y), r, dur));
            debugTable["DrawBox"] = (Action<float, float, float, float, float>)((x, y, w, h, dur) => LuaDebugRenderer.AddBox(new Vector2(x, y), w, h, dur));
            debugTable["GetEntityInfo"] = (Func<LuaEntity, string>)((ent) => ent?.InternalObject != null ? $"{ent.GetName()} (ID:{ent.GetInstanceId()}) Pos:{ent.InternalObject.transform.position}" : "null");
            debugTable["IsEnabled"] = (Func<bool>)(() => true);
            engine.Globals["Debug"] = debugTable;

            // --- 14. Mod Management API ---
            Table modApiTable = new Table(engine);
            modApiTable["GetName"] = (Func<string>)(() => modName);
            modApiTable["GetVersion"] = (Func<string>)(() => PluginVersion);
            modApiTable["GetPath"] = (Func<string>)(() => modDir);
            modApiTable["GetAllMods"] = (Func<List<string>>)(() =>
            {
                var list = new List<string>();
                foreach (var m in activeMods) list.Add(m.ModName);
                return list;
            });
            modApiTable["IsLoaded"] = (Func<string, bool>)((name) => activeMods.Exists(m => m.ModName.Equals(name, StringComparison.OrdinalIgnoreCase)));
            modApiTable["Reload"] = (Action<string>)((name) =>
            {
                var target = activeMods.Find(m => m.ModName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (target != null)
                {
                    activeMods.Remove(target);
                    string mainLua = Path.Combine(target.ModDirectory, "main.lua");
                    if (File.Exists(mainLua)) LoadSingleMod(target.ModName, target.ModDirectory, mainLua, "thumb.png");
                }
            });
            modApiTable["GetMetadata"] = (Func<Table>)(() =>
            {
                Table meta = new Table(engine);
                meta["name"] = modName;
                meta["dir"] = modDir;
                return meta;
            });
            engine.Globals["Mod"] = modApiTable;

            // --- 15. Math & Vector2 API ---
            Table mathTable = (Table)engine.Globals["math"];
            mathTable["Clamp"] = (Func<float, float, float, float>)Mathf.Clamp;
            mathTable["Lerp"] = (Func<float, float, float, float>)Mathf.Lerp;
            mathTable["DegToRad"] = (Func<float, float>)((deg) => deg * Mathf.Deg2Rad);
            mathTable["RadToDeg"] = (Func<float, float>)((rad) => rad * Mathf.Rad2Deg);
            mathTable["Distance"] = (Func<float, float, float, float, float>)((x1, y1, x2, y2) => Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)));
            mathTable["Angle"] = (Func<float, float, float, float, float>)((x1, y1, x2, y2) => Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg);
            mathTable["Random"] = (Func<float, float, float>)UnityEngine.Random.Range;
            mathTable["RandomInt"] = (Func<int, int, int>)UnityEngine.Random.Range;

            Table vec2Table = new Table(engine);
            vec2Table["New"] = (Func<float, float, LuaVector2>)((x, y) => new LuaVector2(x, y));
            engine.Globals["Vector2"] = vec2Table;
        }

        // ====================================================================
        // Helpers
        // ====================================================================
        public class LuaCollisionBridge : MonoBehaviour
        {
            public Action<Collision2D> onCollisionEnterAction;
            public Action<Collision2D> onCollisionExitAction;
            private void OnCollisionEnter2D(Collision2D c) => onCollisionEnterAction?.Invoke(c);
            private void OnCollisionExit2D(Collision2D c) => onCollisionExitAction?.Invoke(c);
        }

        private static void AttachCollisionBridge(GameObject obj)
        {
            if (obj == null) return;
            var bridge = obj.GetComponent<LuaCollisionBridge>() ?? obj.AddComponent<LuaCollisionBridge>();
            bridge.onCollisionEnterAction = (collision) =>
            {
                var me = new LuaEntity(obj);
                var other = new LuaEntity(collision.gameObject);
                float impulse = collision.relativeVelocity.magnitude;
                DispatchGlobalEvent("OnCollisionEnter", me, other, impulse);
                DispatchGlobalEvent("OnCollision", me, other, impulse);
            };
            bridge.onCollisionExitAction = (collision) =>
            {
                var me = new LuaEntity(obj);
                var other = new LuaEntity(collision.gameObject);
                DispatchGlobalEvent("OnCollisionExit", me, other, 0f);
            };
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
