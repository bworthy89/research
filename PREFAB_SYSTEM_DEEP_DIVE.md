# PrefabSystem Deep Dive - Complete Reference

## Architecture Overview

### Core Components

```
PrefabSystem (GameSystemBase)
    ↓ manages
m_Prefabs: List<PrefabBase>        // All prefab instances
m_Entities: Dictionary<PrefabBase, Entity>  // Prefab to Entity mapping
m_PrefabIndices: Dictionary<PrefabID, int>  // ID to list index
```

## PrefabID Structure

**Definition:**
```csharp
public struct PrefabID {
    private string m_Type;  // e.g., "NetPrefab", "RoadPrefab"
    private string m_Name;  // e.g., "Small Road", "Highway"

    public PrefabID(string type, string name);
    public PrefabID(PrefabBase prefab);

    public override string ToString() => $"{m_Type}:{m_Name}";
}
```

**Usage:**
```csharp
// Create PrefabID for a road
var roadID = new PrefabID("NetPrefab", "Small Road");

// Or from existing prefab
var roadID = new PrefabID(somePrefabInstance);
```

## PrefabBase Hierarchy

```
ComponentBase
    ↓
PrefabBase (abstract)
    ↓
NetPrefab (roads, rails, etc.)
    ↓
RoadPrefab? (needs verification)
```

**Key Properties:**
```csharp
public class PrefabBase : ComponentBase {
    public List<ComponentBase> components;  // All attached components
    public bool isDirty;                    // Has changes?
    public string thumbnailUrl;             // Preview image
    public bool builtin;                    // Is it a built-in asset?
    public PrefabAsset asset;               // Associated asset
    public string uiTag;                    // Display name

    // Component management
    public T AddComponent<T>();
    public T AddOrGetComponent<T>();
    public bool TryGet<T>(out T component);
    public bool Has<T>();
    public void Remove<T>();

    // Utility
    public PrefabID GetPrefabID();
    public PrefabBase Clone();
}
```

## NetPrefab Structure

```csharp
public class NetPrefab : PrefabBase {
    // Methods (no explicit properties in decompiled code)
    public override void GetPrefabComponents(HashSet<ComponentType> components);
    public override void GetArchetypeComponents(HashSet<ComponentType> components);
    public override void LateInitialize(...);
}
```

**Components it adds:**
- `NetData` (read-write)
- `ConnectedEdge` (if has Node component)
- `ConnectedNode` (if has Edge component)

**Archetypes created:**
- Node archetype (with Created, Updated components)
- Edge archetype

## PrefabSystem Query Methods

### 1. Query by PrefabID

```csharp
public bool TryGetPrefab(PrefabID id, out PrefabBase prefab)
{
    if (m_PrefabIndices.TryGetValue(id, out int index)) {
        prefab = m_Prefabs[index];
        return true;
    }
    prefab = null;
    return false;
}
```

**Usage:**
```csharp
PrefabID roadID = new PrefabID("NetPrefab", "Small Road");
if (prefabSystem.TryGetPrefab(roadID, out PrefabBase prefab)) {
    NetPrefab roadPrefab = prefab as NetPrefab;
    // Use it
}
```

### 2. Query by Entity

```csharp
public T GetPrefab<T>(Entity entity) where T : PrefabBase
{
    PrefabData data = EntityManager.GetComponentData<PrefabData>(entity);
    return m_Prefabs[data.m_Index] as T;
}

public bool TryGetPrefab<T>(Entity entity, out T prefab) where T : PrefabBase
{
    if (EntityManager.TryGetComponent<PrefabData>(entity, out var data)) {
        prefab = m_Prefabs[data.m_Index] as T;
        return true;
    }
    prefab = null;
    return false;
}
```

### 3. Query by PrefabData

```csharp
public T GetPrefab<T>(PrefabData prefabData) where T : PrefabBase
{
    return m_Prefabs[prefabData.m_Index] as T;
}
```

### 4. Query by PrefabRef

```csharp
public T GetPrefab<T>(PrefabRef refData) where T : PrefabBase
{
    PrefabData data = EntityManager.GetComponentData<PrefabData>(refData.m_Prefab);
    return GetPrefab<T>(data);
}
```

### 5. Get Entity from Prefab

```csharp
public Entity GetEntity(PrefabBase prefab)
{
    return m_Entities[prefab];
}

public bool TryGetEntity(PrefabBase prefab, out Entity entity)
{
    return m_Entities.TryGetValue(prefab, out entity);
}
```

## NetToolSystem Prefab Usage

### In CreateDefinitionsJob

```csharp
public struct CreateDefinitionsJob {
    [ReadOnly]
    public Entity m_NetPrefab;  // The road type being placed

    [ReadOnly]
    public Entity m_LanePrefab;

    [ReadOnly]
    public ComponentLookup<NetGeometryData> m_NetGeometryData;

    [ReadOnly]
    public ComponentLookup<PlaceableNetData> m_PlaceableData;

    // Access prefab data
    NetGeometryData geomData = m_NetGeometryData[m_NetPrefab];
    PlaceableNetData placeableData = m_PlaceableData[m_NetPrefab];
}
```

### When Creating Roads

```csharp
CreationDefinition component = new CreationDefinition {
    m_Prefab = m_NetPrefab,  // Entity reference to road prefab
    m_SubPrefab = m_LanePrefab,
    m_RandomSeed = random.NextInt()
};
```

## Road Prefab Naming (Needs Verification)

**Hypothesis based on patterns:**

| Road Type | Likely PrefabID | Entity Name |
|-----------|-----------------|-------------|
| Small Road | `NetPrefab:Small Road` | ? |
| Medium Road | `NetPrefab:Medium Road` | ? |
| Large Road | `NetPrefab:Large Road` | ? |
| Highway | `NetPrefab:Highway` | ? |

**Need to verify:**
- Exact type string ("NetPrefab", "RoadPrefab", or other?)
- Exact name strings
- How to enumerate available road prefabs

## Unknown / Still Need to Find

### 1. How NetToolSystem Gets m_NetPrefab

**Question:** Where is `m_NetPrefab` set?

**Hypotheses:**
- A) User selects from UI, sets `prefab` property
- B) Tool inherits from base class with prefab selection
- C) PrefabSystem is queried at tool initialization

**Need to find:** Main NetToolSystem properties/fields

### 2. Road Prefab Categories

**Question:** How are roads categorized?

**Possibilities:**
- RoadData component has flags/type?
- NetGeometryData.m_DefaultWidth determines category?
- Separate RoadPrefab subclass?

**Need to check:**
- `Game.Prefabs/RoadData.cs` (if exists)
- `Game.Net/RoadData.cs`

### 3. Enumerating Available Roads

**Question:** How to get list of all available road prefabs?

**Approach:**
```csharp
// Query all NetPrefab entities?
EntityQuery query = GetEntityQuery(typeof(NetPrefab), typeof(RoadData));
// Then filter/categorize
```

### 4. Prefab Selection UI

**Question:** How does the UI show road selection?

**Need to find:**
- `ToolUISystem.cs` - Generic tool UI
- Road picker UI component
- How prefab selection triggers

## Next Steps for Implementation

### Phase 1: Prefab Lookup
1. ✅ Understand PrefabID structure
2. ✅ Understand query methods
3. ❌ Find exact road prefab names
4. ❌ Test PrefabSystem.TryGetPrefab()
5. ❌ Verify road Entity retrieval

### Phase 2: Multi-Prefab Support
1. Store multiple road Entity references
2. Select prefab based on road type (arterial vs local)
3. Pass correct prefab to CreateDefinitionsJob

### Phase 3: Dynamic Discovery
1. Query all available road prefabs at runtime
2. Categorize by width/type
3. Auto-select appropriate prefab

## Code Examples for Mod

### Getting a Road Prefab by Name

```csharp
public Entity GetRoadPrefab(PrefabSystem prefabSystem, string roadName) {
    // Try common type names
    string[] typeNames = { "NetPrefab", "RoadPrefab", "Road" };

    foreach (string typeName in typeNames) {
        PrefabID id = new PrefabID(typeName, roadName);
        if (prefabSystem.TryGetPrefab(id, out PrefabBase prefab)) {
            return prefabSystem.GetEntity(prefab);
        }
    }

    return Entity.Null;
}

// Usage
Entity smallRoad = GetRoadPrefab(prefabSystem, "Small Road");
Entity largeRoad = GetRoadPrefab(prefabSystem, "Large Road");
```

### Selecting Prefab Based on Road Type

```csharp
public Entity SelectRoadPrefab(bool isArterial, Entity arterialPrefab, Entity localPrefab) {
    return isArterial ? arterialPrefab : localPrefab;
}

// In grid generation
for (int x = 0; x <= gridCount.x; x++) {
    bool isArterial = (arterialSpacing > 0) && (x % arterialSpacing == 0);
    Entity prefab = SelectRoadPrefab(isArterial, arterialPrefab, localPrefab);

    // Create road with selected prefab
    CreateRoad(prefab, ...);
}
```

### Querying Available Road Prefabs

```csharp
public List<Entity> GetAllRoadPrefabs(EntityQuery roadQuery) {
    NativeArray<Entity> entities = roadQuery.ToEntityArray(Allocator.Temp);
    List<Entity> roads = new List<Entity>();

    foreach (Entity entity in entities) {
        if (EntityManager.HasComponent<RoadData>(entity)) {
            roads.Add(entity);
        }
    }

    entities.Dispose();
    return roads;
}
```

## Summary

### ✅ What We Know
- PrefabID structure and construction
- PrefabSystem query methods
- NetPrefab architecture
- How prefabs are referenced in jobs

### ❌ What We Still Need
- Exact road prefab names (empirical testing needed)
- How NetToolSystem prefab property is set
- Road categorization mechanism
- UI integration details

### 🎯 Can Implement Now
- Basic prefab lookup by hardcoded names
- Multi-prefab storage in mod
- Prefab selection logic

### ⏳ Need More RE
- Dynamic prefab discovery
- UI integration
- Perfect prefab name matching

---

**Next:** Find exact road prefab names through in-game testing or further code analysis.
