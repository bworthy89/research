# Reverse Engineering Checklist for Harmony Patch

## What We Need for Enhanced Grid Tool

### ✅ Already Reverse-Engineered

1. **NetToolSystem.CreateGrid()** - Core grid generation logic
2. **CreateParallelCourses()** - Parallel road creation
3. **NetCourse, ControlPoint** - Data structures
4. **ECS Components** - Node, Edge, Curve, Road, Composition
5. **NetUtils** - Utility methods (curves, offsets, rotations)

### ❌ Still Need to Reverse Engineer

## 1. PrefabSystem ⭐ CRITICAL

**Why:** To select different road types (arterial vs local)

**What we need:**
- How to query available road prefabs
- How to get Entity reference by name/type
- Road prefab categories/properties

**Questions:**
- How does NetToolSystem get `m_NetPrefab`?
- How to find "Small Road", "Medium Road", "Large Road" prefabs?
- Are there prefab tags/categories?

**Files to check:**
- `Game.Prefabs/PrefabSystem.cs`
- `Game.Prefabs/NetPrefab.cs`
- `Game.Prefabs/RoadPrefab.cs`

## 2. Tool Parameters & UI ⭐ HIGH PRIORITY

**Why:** To add our new parameters (manual grid count, arterial spacing)

**What we need:**
- How tool parameters are defined
- How they're exposed to UI
- Where they're stored (save/load)

**Questions:**
- How does `parallelCount` appear in UI?
- How to add new sliders/dropdowns?
- Parameter binding system?

**Files to check:**
- `Game.Tools/NetToolSystem.cs` (parameter declarations)
- `Game.UI.InGame/NetToolUISystem.cs` (likely)
- Tool configuration storage

**Current parameters we found:**
```csharp
public int2 parallelCount;
public float parallelOffset;
public Mode mode;
public Snap selectedSnap;
public float elevation;
```

**Parameters we need to add:**
```csharp
public bool useManualGridCount;      // NEW
public int2 manualGridCount;          // NEW
public int arterialSpacing;           // NEW
public Entity arterialRoadPrefab;     // NEW
```

## 3. UI System (Optional for Phase 1)

**Why:** To add controls for new parameters

**What we need:**
- UI binding framework
- How to create sliders, checkboxes, dropdowns
- Where UI definitions live (C# or TypeScript?)

**Can we skip?**
- YES for Phase 1 - Use console commands or config file
- YES for testing - Hardcode values initially
- NO for production - Users need UI

**Files to check:**
- `Game.UI/` - UI framework
- `Game.UI.InGame/` - In-game UI systems
- TypeScript files (if UI is in TS)

## 4. Entity Prefab Reference System

**Why:** To store multiple road prefab references

**What we need:**
- How prefabs are referenced
- How to store prefab Entity in tool state
- Prefab selection/switching

**Currently:**
```csharp
public Entity m_NetPrefab;  // Single road type
```

**We need:**
```csharp
public Entity m_ArterialPrefab;
public Entity m_CollectorPrefab;
public Entity m_LocalPrefab;
```

## 5. Tool State Serialization (Low Priority)

**Why:** To save/load settings between sessions

**What we need:**
- State save/load mechanism
- Where settings are persisted

**Found:**
```csharp
public struct State {
    public void Save(NetToolSystem netTool);
    public void Load(NetToolSystem netTool);
}
```

**Can extend this for our parameters**

## 6. Zoning System (Phase 3 Only)

**Why:** For auto-zoning feature

**Not needed yet** - Can defer to Phase 3

---

## Priority Order for Implementation

### Phase 1A: Minimal Viable Patch (No UI)
**Can implement with just:**
1. Understanding of `m_NetPrefab` selection
2. Harmony patch to `CreateGrid()`
3. Hardcoded parameters

**Approach:**
```csharp
[HarmonyPatch(typeof(NetToolSystem), "CreateGrid")]
class GridPatch {
    static bool Prefix(NetToolSystem __instance, ...) {
        // Hardcode for testing:
        int2 manualCount = new int2(10, 10);
        int arterialSpacing = 4;

        // Call our enhanced CreateGrid
        EnhancedCreateGrid(__instance, manualCount, arterialSpacing);
        return false; // Skip original
    }
}
```

✅ **Can start now without more RE!**

### Phase 1B: Add PrefabSystem Support
**Need to RE:**
1. PrefabSystem.GetPrefab() or similar
2. Road prefab lookup by name

**Approach:**
```csharp
// Get different road types
Entity arterialPrefab = prefabSystem.GetPrefab("Large Road");
Entity localPrefab = prefabSystem.GetPrefab("Small Road");

// Use in grid generation
Entity selectedPrefab = isArterial ? arterialPrefab : localPrefab;
```

⚠️ **Need to RE PrefabSystem first**

### Phase 1C: Add UI Controls
**Need to RE:**
1. Tool UI system
2. Parameter binding
3. UI widget creation

**Approach:**
- Add fields to NetToolSystem
- Bind to UI controls
- Save/load state

⚠️ **Need to RE UI system first**

---

## What to Reverse Engineer Next?

### Option 1: PrefabSystem (Recommended)
**Why:** Enables road type selection
**Impact:** Can implement road hierarchy
**Effort:** Medium (1-2 hours)

**Action:**
- Find `Game.Prefabs/PrefabSystem.cs`
- Look for prefab query methods
- Understand road prefab categorization

### Option 2: Tool Parameters
**Why:** Understand parameter exposure
**Impact:** Know where to add our params
**Effort:** Low (30 min)

**Action:**
- Study existing parameters in NetToolSystem
- Find parameter binding mechanism
- Look for UI connection

### Option 3: Skip RE, Start Coding
**Why:** Test Harmony patching works
**Impact:** Proof of concept
**Effort:** Low (1 hour)

**Action:**
- Create basic Harmony patch
- Hardcode manual grid count
- Test in-game

---

## Recommended Next Steps

**I suggest:**

1. **Quick test** (30 min):
   - Create basic Harmony patch with hardcoded values
   - Verify patching works
   - Confirm grid generation can be modified

2. **RE PrefabSystem** (1-2 hours):
   - Find road prefab lookup
   - Implement road type selection
   - Test arterial vs local roads

3. **Add parameters** (1 hour):
   - Study parameter system
   - Add our new fields
   - Wire up to patch

4. **UI integration** (optional, can defer):
   - Add controls later
   - Use config file for now

---

## What do you want to do?

**A.** Start with quick Harmony patch test (prove it works)
**B.** Reverse engineer PrefabSystem first (enable road selection)
**C.** Study the parameter system (understand architecture)
**D.** Something else?

