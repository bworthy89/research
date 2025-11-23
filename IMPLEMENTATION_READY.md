# Implementation Ready - Complete Deep Dive Summary

## Status: ✅ READY TO IMPLEMENT

We now have complete understanding of all required systems to build the Enhanced Grid Tool mod.

---

## 1. ✅ PrefabSystem - COMPLETE

### How to Get Road Prefabs

```csharp
// Method 1: By name (requires exact name)
PrefabID roadID = new PrefabID("NetPrefab", "Small Road");
if (prefabSystem.TryGetPrefab(roadID, out PrefabBase prefab)) {
    Entity roadEntity = prefabSystem.GetEntity(prefab);
}

// Method 2: Query from existing entity
if (prefabSystem.TryGetPrefab(someEntity, out NetPrefab prefab)) {
    // Use prefab
}
```

### For Our Mod

```csharp
public class EnhancedGridMod {
    private Entity m_SmallRoadPrefab;
    private Entity m_MediumRoadPrefab;
    private Entity m_LargeRoadPrefab;

    private void InitializePrefabs(PrefabSystem prefabSystem) {
        m_SmallRoadPrefab = GetRoadByName(prefabSystem, "Small Road");
        m_MediumRoadPrefab = GetRoadByName(prefabSystem, "Medium Road");
        m_LargeRoadPrefab = GetRoadByName(prefabSystem, "Large Road");
    }

    private Entity GetRoadByName(PrefabSystem ps, string name) {
        PrefabID id = new PrefabID("NetPrefab", name);
        return ps.TryGetPrefab(id, out PrefabBase p) ? ps.GetEntity(p) : Entity.Null;
    }
}
```

**Unknown (need testing):**
- Exact road type string ("NetPrefab" vs "RoadPrefab")
- Exact road name strings
- Can hardcode initially, test in-game

---

## 2. ✅ Tool Parameters - COMPLETE

### Pattern for Adding Parameters

**Example from NetToolSystem:**
```csharp
// 1. Private backing field
private int m_ParallelCount;

// 2. Public property with auto-save
public int parallelCount {
    get => m_ParallelCount;
    set {
        if (value != m_ParallelCount) {
            m_ParallelCount = value;
            m_ForceUpdate = true;
            SaveToolPreferences();  // Auto-saves on change!
        }
    }
}

// 3. Add to preferences class
private class NetToolPreferences {
    public int m_ParallelCount;

    public void Save(NetToolSystem tool) {
        m_ParallelCount = tool.parallelCount;
    }

    public void Load(NetToolSystem tool) {
        tool.parallelCount = m_ParallelCount;
    }
}
```

### For Our Enhanced Grid

**Add these parameters to NetToolSystem via Harmony transpiler or extension:**

```csharp
// New parameters we need
public bool useManualGridCount { get; set; }
public int2 manualGridCount { get; set; }
public int arterialSpacing { get; set; }
public Entity arterialRoadPrefab { get; set; }
public Entity localRoadPrefab { get; set; }
```

**OR** (easier) - Use config file/static class:

```csharp
public static class EnhancedGridConfig {
    public static bool useManualGridCount = false;
    public static int2 manualGridCount = new int2(10, 10);
    public static int arterialSpacing = 4;
    // Prefabs set at runtime
    public static Entity arterialPrefab;
    public static Entity localPrefab;
}
```

---

## 3. ✅ Grid Generation Algorithm - COMPLETE

### Current Implementation

**File:** `NetToolSystem.CreateDefinitionsJob.CreateGrid()`
**Line:** 4035

**Key variables:**
```csharp
int2 gridCount = new int2(
    Mathf.RoundToInt(float4.x),  // X direction count
    Mathf.RoundToInt(float4.y)   // Y direction count
);

// Auto-calculated from:
float2 distances = new float2(
    math.distance(point1, point2),
    math.distance(point2, point3)
);
float2 cellSize = netGeometryData.m_DefaultWidth * new float2(16f, 8f);
float2 gridCountFloat = (distances - 0.16f) / cellSize;
```

### Our Enhancement

**Harmony Patch:**
```csharp
[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
public class EnhancedGridPatch {

    static bool Prefix(CreateDefinitionsJob __instance, ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions) {

        // Get config
        bool useManual = EnhancedGridConfig.useManualGridCount;
        int2 manualCount = EnhancedGridConfig.manualGridCount;

        if (useManual) {
            // Use our count instead of auto-calculated
            return EnhancedCreateGrid(__instance, ownerDefinitions, manualCount);
        }

        // Let original run
        return true;
    }

    static bool EnhancedCreateGrid(CreateDefinitionsJob job, NativeParallelHashMap<Entity, OwnerDefinition> owners, int2 gridCount) {
        // Copy-paste original CreateGrid code
        // Replace gridCount calculation with our value
        // Add arterial road selection logic

        for (int x = 0; x < gridCount.x; x++) {
            for (int y = 0; y < gridCount.y; y++) {
                // Check if arterial
                bool isArterial = (EnhancedGridConfig.arterialSpacing > 0) &&
                                  ((x % EnhancedGridConfig.arterialSpacing == 0) ||
                                   (y % EnhancedGridConfig.arterialSpacing == 0));

                // Select prefab
                Entity prefab = isArterial ?
                    EnhancedGridConfig.arterialPrefab :
                    EnhancedGridConfig.localPrefab;

                // Create road with selected prefab
                CreationDefinition def = new CreationDefinition {
                    m_Prefab = prefab,  // <-- KEY CHANGE
                    m_SubPrefab = job.m_LanePrefab,
                    m_RandomSeed = random.NextInt()
                };

                // ... rest of road creation
            }
        }

        return false; // Skip original
    }
}
```

---

## 4. ⏳ UI System - OPTIONAL (Can Defer)

**Current approach:** Use config file or console commands

**Future:** Add UI controls

**Where UI lives:**
- `Game.UI.InGame/ToolUISystem.cs` - Generic tool UI
- TypeScript files in UI folder - Actual UI rendering

**Can skip for Phase 1!**

---

## 5. ✅ State Management - COMPLETE

### Existing Pattern

```csharp
private Dictionary<Entity, NetToolPreferences> m_ToolPreferences;

private void SaveToolPreferences() {
    if (m_LoadingPreferences || prefab == null) return;

    if (!m_ToolPreferences.TryGetValue(prefab.m_Prefab, out var prefs)) {
        prefs = new NetToolPreferences();
        m_ToolPreferences[prefab.m_Prefab] = prefs;
    }

    prefs.Save(this);
}
```

**Per-prefab settings storage!** Each road type remembers its settings.

### For Our Mod

**Option A:** Extend existing preferences
- Add fields to NetToolPreferences
- Requires Harmony transpiler (complex)

**Option B:** Separate config file
- Save to `EnhancedGridConfig.json`
- Load on mod init
- Simple and clean

---

## Implementation Plan

### Phase 1A: Minimal Harmony Patch (1-2 hours)

```csharp
[BepInPlugin("com.yourname.enhancedgrid", "Enhanced Grid", "1.0.0")]
public class EnhancedGridMod : BaseUnityPlugin {

    void Awake() {
        var harmony = new Harmony("com.yourname.enhancedgrid");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
class GridPatch {
    static bool Prefix(...) {
        // Hardcoded test values
        int2 manualGrid = new int2(12, 8);

        // Call custom CreateGrid with manual count
        return false; // Skip original
    }
}
```

**Test:** Load in-game, use grid tool, verify 12x8 grid created

### Phase 1B: Add Prefab Support (2-3 hours)

```csharp
public class EnhancedGridMod : BaseUnityPlugin {
    private static Entity arterialPrefab;
    private static Entity localPrefab;

    void OnEnable() {
        // Get PrefabSystem
        var world = World.DefaultGameObjectInjectionWorld;
        var prefabSystem = world.GetOrCreateSystemManaged<PrefabSystem>();

        // Load prefabs
        arterialPrefab = GetPrefab(prefabSystem, "Large Road");
        localPrefab = GetPrefab(prefabSystem, "Small Road");
    }
}

// In patch: use arterialPrefab vs localPrefab based on position
```

**Test:** Verify arterial roads every N blocks

### Phase 1C: Add Config File (1 hour)

```csharp
public class EnhancedGridConfig {
    public bool useManualGridCount = false;
    public int gridX = 10;
    public int gridY = 10;
    public int arterialSpacing = 4;
    public string arterialRoadName = "Large Road";
    public string localRoadName = "Small Road";

    public static void Load() {
        // Read from JSON file
    }

    public static void Save() {
        // Write to JSON file
    }
}
```

**Test:** Edit config, reload, verify changes apply

### Phase 2: UI Integration (4-8 hours)

- Study ToolUISystem
- Add UI controls (sliders, checkboxes)
- Bind to config values
- **Can defer to later!**

---

## What We Can Build RIGHT NOW

### Minimum Viable Product

✅ **Prefab lookup** - Just need to test exact names
✅ **Parameter storage** - Use static config class
✅ **Grid algorithm** - Fully understood
✅ **Harmony patching** - Standard approach

### In 1-2 Days of Work

1. Basic Harmony patch with hardcoded manual grid count
2. Prefab support for arterial vs local roads
3. Config file for user customization
4. **Working enhanced grid tool!**

### Later Additions

- UI controls (nice to have, not critical)
- Angled grid patterns
- Preset system
- Auto-zoning

---

## Decision Points

### Q: Exact Road Prefab Names?
**A:** Need empirical testing. Try:
- "Small Road", "Medium Road", "Large Road"
- "Road_Small", "Road_Medium", "Road_Large"
- Query all NetPrefabs and log names

### Q: Harmony Patch Target?
**A:** Two options:
1. `NetToolSystem.CreateDefinitionsJob.CreateGrid` (inner method)
2. Outer wrapper that calls it

**Recommend:** Test both, use whichever works

### Q: Config vs UI First?
**A:** Config file first - much simpler, works well

---

## Next Steps

**Ready to code! Which approach:**

**A. Start implementing** Phase 1A patch now
**B. Test prefab names** first (safer)
**C. Create project structure** (BepInEx mod template)

**Your choice!**
