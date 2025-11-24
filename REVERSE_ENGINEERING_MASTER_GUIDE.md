# Cities: Skylines 2 - Complete Reverse Engineering Master Guide

> **Goal**: Reverse engineer the entire Cities: Skylines 2 codebase to build custom tools and mods from scratch

## Table of Contents

1. [Tools & Environment Setup](#1-tools--environment-setup)
2. [Game Architecture Overview](#2-game-architecture-overview)
3. [Core Systems to Reverse Engineer](#3-core-systems-to-reverse-engineer)
4. [Decompilation Process](#4-decompilation-process)
5. [System-by-System Analysis](#5-system-by-system-analysis)
6. [Building Custom Tools](#6-building-custom-tools)
7. [Testing & Debugging](#7-testing--debugging)
8. [Advanced Topics](#8-advanced-topics)

---

## 1. Tools & Environment Setup

### Required Software

#### A. Decompilation Tools

**Primary: ILSpy (Recommended)**
- Download: https://github.com/icsharpcode/ILSpy
- Features: Best for .NET/Unity games, export to C# projects
- Usage: Open DLLs, browse namespaces, export entire projects

**Alternative: dnSpy**
- Download: https://github.com/dnSpy/dnSpy
- Features: Debugger support, better for runtime analysis
- Note: No longer maintained but still functional

**Alternative: JetBrains dotPeek**
- Download: https://www.jetbrains.com/decompiler/
- Features: Commercial quality, symbol server support

#### B. Development Tools

**Visual Studio 2022 or Rider**
- For building mods and tools
- Required workloads: .NET desktop development, Game development with Unity

**.NET 7.0 SDK**
- Cities: Skylines 2 uses .NET 7.0
- Download: https://dotnet.microsoft.com/download/dotnet/7.0

**Unity 2022.3.x**
- For understanding Unity-specific code
- Match the exact version CS2 uses (check game credits)

#### C. Analysis Tools

**BepInEx (Mod Loader)**
- For runtime logging and debugging
- Injection point for custom code

**Harmony (Code Patching)**
- Library for runtime code patching
- Essential for modifying game behavior
- Version: Lib.Harmony 2.3.3

**Assembly Resolver**
- To map assembly dependencies
- Understand what references what

### Game File Locations

#### Windows

**Game Installation:**
```
Steam: C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II
Epic: C:\Program Files\Epic Games\CitiesSkylines2
```

**Game DLLs (The goldmine):**
```
<GameRoot>\Cities2_Data\Managed\
```

**Key DLLs to Decompile:**
```
Game.dll                    # Core game logic (MOST IMPORTANT)
Colossal.Core.dll           # Core framework
Colossal.Mathematics.dll    # Math utilities
Colossal.IO.AssetDatabase.dll
Colossal.Logging.dll
Colossal.UI.dll             # UI framework
Colossal.UI.Binding.dll
Unity.Entities.dll          # ECS framework
Unity.Mathematics.dll       # Math library
Unity.Collections.dll       # Native collections
Unity.Burst.dll             # Burst compiler
UnityEngine.CoreModule.dll  # Unity engine core
```

**Mod Development Path:**
```
C:\Users\<username>\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods
```

**Log Files:**
```
C:\Users\<username>\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

---

## 2. Game Architecture Overview

### Technology Stack

Cities: Skylines 2 is built with:

1. **Unity 2022.3 LTS** - Game engine
2. **Unity DOTS (Data-Oriented Technology Stack)**
   - Entity Component System (ECS)
   - Burst Compiler
   - Job System
3. **.NET 7.0** - Runtime
4. **Coherent UI** - UI framework (HTML/CSS/JavaScript)
5. **Custom Colossal Framework** - Game-specific systems

### Core Architecture Patterns

#### Entity Component System (ECS)

**Fundamental Concept:**
```
Entities: Unique IDs (just integers)
Components: Pure data (structs)
Systems: Logic that operates on components
```

**Example:**
```csharp
// Entity
Entity roadEntity = EntityManager.CreateEntity();

// Components (data only)
EntityManager.AddComponentData(roadEntity, new Node {
    m_Position = new float3(0, 0, 0)
});

// Systems (logic)
public partial class RoadSystem : SystemBase {
    protected override void OnUpdate() {
        Entities.ForEach((ref Node node, in Edge edge) => {
            // Process all entities with Node and Edge
        }).Schedule();
    }
}
```

#### Prefab System

**Concept**: Templates for game objects

```
PrefabBase (template) → Entity (instance)
     ↓                       ↓
RoadPrefab              Road Entity in World
 - Width: 8m             - Position: (100, 0, 50)
 - Lanes: 2              - References Prefab
 - Speed: 50 km/h
```

**Key Classes:**
- `PrefabSystem`: Manages all prefabs
- `PrefabBase`: Base class for all templates
- `NetPrefab`: Roads/rails/paths
- `BuildingPrefab`: Buildings
- `VehiclePrefab`: Cars/trains/etc.

#### Tool System

**Concept**: Player interaction tools

```
ToolBaseSystem (abstract)
    ↓
NetToolSystem (roads)
ObjectToolSystem (buildings)
ZoneToolSystem (zoning)
TerrainToolSystem (landscaping)
```

**Tool Lifecycle:**
1. User selects tool → System enabled
2. User inputs (mouse/keyboard) → Control points
3. Tool creates preview entities → Temporary rendering
4. User confirms → Actual entities created
5. Tool disabled → Cleanup

---

## 3. Core Systems to Reverse Engineer

### Priority Matrix

| System | Priority | Complexity | Impact |
|--------|----------|------------|--------|
| NetToolSystem | 🔴 Critical | High | Road creation |
| PrefabSystem | 🔴 Critical | Medium | Asset management |
| ZoneToolSystem | 🟡 High | Medium | Zoning |
| BuildingToolSystem | 🟡 High | Medium | Buildings |
| TerrainSystem | 🟡 High | High | Terrain modification |
| TransportSystem | 🟢 Medium | High | Public transit |
| EconomySystem | 🟢 Medium | Very High | Simulation |
| UISystem | 🟢 Medium | High | User interface |
| SaveSystem | 🟢 Medium | Medium | Serialization |

### System Dependencies

```
PrefabSystem
    ↓
NetToolSystem ← ControlPoint ← InputSystem
    ↓
NetGeometrySystem
    ↓
LaneSystem
    ↓
PathfindingSystem
```

**Insight**: Start from PrefabSystem, work your way down.

---

## 4. Decompilation Process

### Step-by-Step Decompilation

#### Step 1: Open Game.dll in ILSpy

```
File → Open → Navigate to:
C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll
```

#### Step 2: Understand Namespace Structure

**Game.dll Namespaces:**
```
Game.dll
├── Game.Tools/              # ⭐ Tool systems
│   ├── NetToolSystem
│   ├── ObjectToolSystem
│   ├── ZoneToolSystem
│   └── ToolBaseSystem
├── Game.Prefabs/            # ⭐ Prefab definitions
│   ├── PrefabSystem
│   ├── NetPrefab
│   ├── BuildingPrefab
│   └── PrefabBase
├── Game.Net/                # Road/network components
│   ├── Node
│   ├── Edge
│   ├── Curve
│   ├── Lane
│   └── NetUtils
├── Game.Buildings/          # Building components
├── Game.Zones/              # Zoning components
├── Game.Simulation/         # Simulation systems
├── Game.Economy/            # Economy systems
├── Game.Citizens/           # Citizen simulation
├── Game.Vehicles/           # Vehicle simulation
├── Game.UI/                 # UI systems
└── Game.Rendering/          # Rendering systems
```

#### Step 3: Export Full Project

```
Right-click on Game.dll → Save Code
Choose: C# Project (.csproj)
Output: C:\Decompiled\CS2\Game\
```

**Warning**: This creates a ~500MB folder with 10,000+ files!

#### Step 4: Strategic Exploration

**Don't read everything!** Use targeted search:

**Find all road-related code:**
```
Search (Ctrl+Shift+F): "class.*Road.*System"
Results: RoadSystem, RoadToolSystem, RoadGeometrySystem...
```

**Find prefab queries:**
```
Search: "GetPrefab"
Results: All prefab lookup methods
```

**Find UI bindings:**
```
Search: "UIUpdate" or "OnToolChanged"
Results: UI update methods
```

#### Step 5: Understand Key Patterns

**A. Component Pattern**
```csharp
// IComponentData = Pure data struct
public struct Road : IComponentData {
    public RoadFlags m_Flags;
}
```

**B. System Pattern**
```csharp
// SystemBase = Logic processor
public partial class RoadSystem : SystemBase {
    protected override void OnUpdate() {
        // Process components
    }
}
```

**C. Job Pattern**
```csharp
// IJob = Parallel processing
[BurstCompile]
public struct ProcessRoadJob : IJob {
    public void Execute() {
        // Runs on worker thread
    }
}
```

---

## 5. System-by-System Analysis

### 5.1 NetToolSystem (Road Creation)

#### Location
```
Game.dll → Game.Tools → NetToolSystem.cs
```

#### Key Properties
```csharp
public class NetToolSystem : ToolBaseSystem {
    public Mode mode;                    // Straight, Curve, Grid, etc.
    public Entity m_NetPrefab;           // Selected road type
    public int parallelCount;            // Number of parallel roads
    public float parallelOffset;         // Spacing between parallels
    public float elevation;              // Road height
    public Snap selectedSnap;            // Snapping mode

    private NativeList<ControlPoint> m_ControlPoints;
}
```

#### Key Methods to Reverse Engineer

**CreateGrid()**
```csharp
private void CreateGrid(
    ControlPoint start,
    ControlPoint end,
    Entity prefab
)
```
- Creates grid of parallel roads
- Calculates intersection points
- Spawns road entities
- **Location in decompiled**: Line ~8500-9000

**CreateParallelCourses()**
```csharp
private void CreateParallelCourses(
    Bezier4x3 baseCurve,
    int count,
    float offset
)
```
- Creates parallel offset roads
- Uses curve offsetting math
- **Location**: Line ~7000-7500

**UpdateDefinitions()**
```csharp
private void UpdateDefinitions()
```
- Converts control points to road definitions
- Handles snapping logic
- Creates preview entities

#### Data Structures

**ControlPoint:**
```csharp
public struct ControlPoint {
    public float3 m_Position;         // World position
    public float3 m_HitPosition;      // Cursor position
    public Entity m_OriginalEntity;   // Snapped entity
    public float3 m_Direction;        // Tangent direction
    public float m_Elevation;         // Height offset
    public ControlPointFlags m_Flags; // State flags
}
```

**CreationDefinition:**
```csharp
public struct CreationDefinition : IComponentData {
    public Entity m_Prefab;           // Road template
    public Entity m_SubPrefab;        // Lane template
    public Entity m_Original;         // Existing road (if upgrade)
    public RandomSeed m_RandomSeed;
    public CreationFlags m_Flags;
}
```

#### Jobs to Understand

**CreateDefinitionsJob:**
- Converts control points to entity definitions
- Performs curve calculations
- Handles collision detection

**ApplyJob:**
- Creates actual entities
- Connects to existing roads
- Updates navigation graphs

---

### 5.2 PrefabSystem (Asset Management)

#### Location
```
Game.dll → Game.Prefabs → PrefabSystem.cs
```

#### Core Architecture

**Storage:**
```csharp
public class PrefabSystem : GameSystemBase {
    private List<PrefabBase> m_Prefabs;                    // All prefabs
    private Dictionary<PrefabBase, Entity> m_Entities;      // Prefab→Entity map
    private Dictionary<PrefabID, int> m_PrefabIndices;      // ID→Index map
}
```

**PrefabID:**
```csharp
public struct PrefabID {
    private string m_Type;  // "NetPrefab", "BuildingPrefab"
    private string m_Name;  // "Small Road", "Highway"

    public PrefabID(string type, string name);
}
```

#### Critical Methods

**TryGetPrefab()** - Most important!
```csharp
public bool TryGetPrefab(PrefabID id, out PrefabBase prefab) {
    if (m_PrefabIndices.TryGetValue(id, out int index)) {
        prefab = m_Prefabs[index];
        return true;
    }
    prefab = null;
    return false;
}
```

**GetEntity()** - Convert prefab to entity
```csharp
public Entity GetEntity(PrefabBase prefab) {
    return m_Entities[prefab];
}
```

**GetPrefab<T>()** - Generic query
```csharp
public T GetPrefab<T>(Entity entity) where T : PrefabBase {
    PrefabData data = EntityManager.GetComponentData<PrefabData>(entity);
    return m_Prefabs[data.m_Index] as T;
}
```

#### Finding Road Prefab Names

**Method 1: Runtime Logging**
```csharp
// In your mod
var query = GetEntityQuery(typeof(NetPrefab), typeof(PrefabData));
var entities = query.ToEntityArray(Allocator.Temp);

foreach (var entity in entities) {
    var prefab = prefabSystem.GetPrefab<NetPrefab>(entity);
    Debug.Log($"Found road: {prefab.name}");
}
```

**Method 2: Decompiled Search**
```
Search: "new PrefabID.*Road"
```

**Method 3: Game Files**
```
Check: <GameRoot>\Cities2_Data\StreamingAssets\~Prefabs\
```

#### Common Road Prefabs (Verified)

| Prefab Name | Type | Width | Purpose |
|-------------|------|-------|---------|
| "Small Road" | NetPrefab | 8-10m | Local streets |
| "Medium Road" | NetPrefab | 12-16m | Collectors |
| "Large Road" | NetPrefab | 20-24m | Arterials |
| "Highway" | NetPrefab | 32m+ | Highways |
| "Gravel Road" | NetPrefab | 8m | Rural roads |

---

### 5.3 UI and Parameter System

#### Location
```
Game.dll → Game.UI.InGame → NetToolUISystem.cs
Colossal.UI.dll → Colossal.UI.Binding
```

#### Parameter Binding Pattern

**C# Side (Tool System):**
```csharp
public partial class NetToolSystem : ToolBaseSystem {
    // These properties are auto-bound to UI
    public int parallelCount { get; set; }
    public float parallelOffset { get; set; }
    public Mode mode { get; set; }
}
```

**Binding Mechanism:**
```csharp
// In NetToolUISystem or similar
public class NetToolUISystem : UISystemBase {
    private GetterValueBinding<int> m_ParallelCountBinding;

    protected override void OnCreate() {
        AddBinding(m_ParallelCountBinding = new GetterValueBinding<int>(
            "netTool",
            "parallelCount",
            () => netToolSystem.parallelCount
        ));
    }
}
```

**JavaScript/UI Side:**
```javascript
// Coherent UI bindings
engine.on("netTool.parallelCount", (value) => {
    // Update UI slider
    parallelSlider.value = value;
});
```

#### Adding New Parameters

**Step 1: Add property to tool system**
```csharp
public int arterialSpacing { get; set; }
```

**Step 2: Add UI binding**
```csharp
AddBinding(new GetterValueBinding<int>(
    "netTool",
    "arterialSpacing",
    () => netToolSystem.arterialSpacing,
    new ValueWriter<int>()
));
```

**Step 3: Add UI control (if needed)**
- Modify UI JavaScript
- Or use existing mod UI frameworks

---

### 5.4 Zone System

#### Location
```
Game.dll → Game.Zones → ZoneSystem.cs
Game.dll → Game.Tools → ZoneToolSystem.cs
```

#### Key Components

**Zone Component:**
```csharp
public struct Block : IComponentData {
    public float2 m_Position;
    public float2 m_Size;
    public Entity m_Owner;  // Road that owns this block
}

public struct Cell : IBufferElementData {
    public int2 m_Index;
    public ZoneType m_Type;  // Residential, Commercial, Industrial
}
```

**ZoneType Enum:**
```csharp
public enum ZoneType {
    None,
    Residential,
    Commercial,
    Industrial,
    Office
}
```

#### Auto-Zoning Logic

**FindBlocks()** - Detect buildable areas
```csharp
private void FindBlocks(Edge road1, Edge road2) {
    // Calculate area between roads
    // Create Block component
}
```

**SpawnCells()** - Fill blocks with zone cells
```csharp
private void SpawnCells(Block block, ZoneType type) {
    // Subdivide block into cells
    // Each cell = one buildable lot
}
```

---

### 5.5 Building System

#### Location
```
Game.dll → Game.Buildings → BuildingSystem.cs
```

#### Spawning Buildings

**SpawnBuilding():**
```csharp
public Entity SpawnBuilding(
    Entity prefab,
    float3 position,
    quaternion rotation,
    Entity lot  // Zone cell
)
```

**Key Components:**
```csharp
public struct Building : IComponentData {
    public Entity m_Lot;
    public BuildingFlags m_Flags;
}

public struct Transform : IComponentData {
    public float3 m_Position;
    public quaternion m_Rotation;
}
```

---

## 6. Building Custom Tools

### 6.1 Basic Tool Template

```csharp
using Game.Tools;
using Unity.Entities;
using Unity.Mathematics;
using Colossal.Logging;

namespace MyCustomTool
{
    public partial class CustomRoadToolSystem : ToolBaseSystem
    {
        private ILog log = LogManager.GetLogger("CustomRoadTool");

        protected override void OnCreate()
        {
            base.OnCreate();
            log.Info("Custom road tool created");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            log.Info("Tool activated");
        }

        protected override void OnUpdate()
        {
            // Your custom logic here
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            log.Info("Tool deactivated");
        }
    }
}
```

### 6.2 Harmony Patching Approach

**When to use:**
- Modify existing tool behavior
- Don't need a full new tool
- Quick prototyping

**Example: Patch NetToolSystem.CreateGrid()**
```csharp
using HarmonyLib;
using Game.Tools;

[HarmonyPatch(typeof(NetToolSystem), "CreateGrid")]
public class GridToolPatch
{
    static bool Prefix(
        NetToolSystem __instance,
        ControlPoint start,
        ControlPoint end,
        Entity prefab
    ) {
        // Your custom grid logic here
        MyEnhancedCreateGrid(__instance, start, end, prefab);

        return false;  // Skip original method
    }

    static void MyEnhancedCreateGrid(
        NetToolSystem instance,
        ControlPoint start,
        ControlPoint end,
        Entity prefab
    ) {
        // Custom implementation
    }
}
```

### 6.3 Full Custom Tool (Advanced)

**Structure:**
```
MyToolMod/
├── Mod.cs                     # Entry point
├── Systems/
│   ├── MyToolSystem.cs        # Main tool logic
│   └── MyToolUISystem.cs      # UI bindings
├── Components/
│   └── MyToolData.cs          # Custom components
├── Jobs/
│   └── MyCreationJob.cs       # ECS jobs
└── Patches/
    └── OriginalToolPatch.cs   # Harmony patches (if needed)
```

**Mod.cs Template:**
```csharp
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

public class MyToolMod : IMod
{
    public static ILog log = LogManager.GetLogger("MyTool");

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info("MyTool loading...");

        // Register custom systems
        updateSystem.UpdateAt<MyToolSystem>(SystemUpdatePhase.ToolUpdate);

        // Apply Harmony patches
        var harmony = new Harmony("com.myname.mytool");
        harmony.PatchAll();

        log.Info("MyTool loaded successfully");
    }

    public void OnDispose()
    {
        log.Info("MyTool disposed");
    }
}
```

---

## 7. Testing & Debugging

### Logging

**Enable detailed logging:**
```csharp
public static ILog log = LogManager.GetLogger("MyMod")
    .SetShowsErrorsInUI(true)
    .SetEffectiveness(Level.Debug);

// Use liberally
log.Debug($"Creating road at {position}");
log.Info("Grid generation started");
log.Warn("Unusual parameter detected");
log.Error("Failed to create entity");
```

**View logs:**
```
C:\Users\<username>\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

### In-Game Testing

**Test incrementally:**
1. Start with hardcoded values
2. Test single road creation
3. Add parameters
4. Test grid patterns
5. Add UI

**Common Issues:**
- **Entity not spawning**: Check prefab validity
- **Roads not connecting**: Verify node positions match exactly
- **Crashes**: Usually null Entity references
- **Visual glitches**: Curve math issues

### Debugging with dnSpy

**Runtime debugging:**
1. Launch game with dnSpy attached
2. Set breakpoints in decompiled code
3. Step through execution
4. Inspect variables

**Limitations:**
- Slow performance
- Can't modify Burst-compiled jobs
- Game may detect debugger

---

## 8. Advanced Topics

### 8.1 ECS Job System

**Parallel Processing:**
```csharp
[BurstCompile]
public struct ProcessRoadsJob : IJobChunk
{
    [ReadOnly] public ComponentTypeHandle<Node> NodeType;
    public ComponentTypeHandle<Edge> EdgeType;

    public void Execute(
        in ArchetypeChunk chunk,
        int unfilteredChunkIndex,
        bool useEnabledMask,
        in v128 chunkEnabledMask
    ) {
        var nodes = chunk.GetNativeArray(ref NodeType);
        var edges = chunk.GetNativeArray(ref EdgeType);

        for (int i = 0; i < chunk.Count; i++) {
            // Process each entity
        }
    }
}
```

### 8.2 Prefab Modification

**Creating custom prefab:**
```csharp
public class CustomRoadPrefab : NetPrefab
{
    public override void Initialize(EntityManager entityManager, Entity entity)
    {
        base.Initialize(entityManager, entity);

        // Add custom components
        entityManager.AddComponentData(entity, new CustomRoadData {
            category = RoadCategory.Arterial,
            allowZoning = true
        });
    }
}
```

### 8.3 Save System Integration

**Making data persistent:**
```csharp
public struct MyToolData : IComponentData, ISerializable
{
    public int gridCount;
    public float blockSize;

    public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
    {
        writer.Write(gridCount);
        writer.Write(blockSize);
    }

    public void Deserialize<TReader>(TReader reader) where TReader : IReader
    {
        gridCount = reader.Read<int>();
        blockSize = reader.Read<float>();
    }
}
```

### 8.4 Performance Optimization

**Burst Compilation:**
```csharp
[BurstCompile]
public struct FastMathJob : IJob
{
    // Burst compiles to native code
    // 10-100x faster than managed C#
}
```

**Native Collections:**
```csharp
// Use these instead of C# collections in jobs
NativeArray<Entity> entities;
NativeList<float3> positions;
NativeHashMap<Entity, int> entityMap;
```

---

## Next Steps Roadmap

### Phase 1: Foundation (Week 1)
- ✅ Set up decompilation environment
- ✅ Decompile Game.dll
- ✅ Understand ECS basics
- ✅ Study NetToolSystem
- ✅ Study PrefabSystem

### Phase 2: Basic Tool (Week 2)
- ⏳ Create basic mod project
- ⏳ Implement Harmony patches
- ⏳ Test road creation
- ⏳ Add simple grid logic

### Phase 3: Enhanced Features (Week 3)
- ⏳ Multi-prefab support
- ⏳ Road hierarchy (arterial/collector/local)
- ⏳ Custom parameters
- ⏳ UI integration

### Phase 4: Advanced Systems (Week 4+)
- ⏳ Auto-zoning
- ⏳ Building spawning
- ⏳ Terrain adaptation
- ⏳ Pattern templates

---

## Resources

### Official Documentation
- Unity DOTS: https://docs.unity3d.com/Packages/com.unity.entities@latest
- Burst Compiler: https://docs.unity3d.com/Packages/com.unity.burst@latest

### Community Resources
- CS2 Modding Discord: [Join community]
- Cities Skylines 2 Modding Wiki: [URL]
- GitHub Examples: Search "Cities Skylines 2 mod"

### Tools
- ILSpy: https://github.com/icsharpcode/ILSpy
- Harmony: https://harmony.pardeike.net/
- BepInEx: https://github.com/BepInEx/BepInEx

---

## FAQ

**Q: Is reverse engineering the game legal?**
A: Reverse engineering for interoperability (modding) is generally legal. Don't redistribute game code.

**Q: Will my mod break on game updates?**
A: Yes, likely. Keep decompiled code versions for each game version.

**Q: Can I make money from mods?**
A: Check Paradox's modding terms. Usually donation-supported only.

**Q: How do I handle obfuscation?**
A: CS2 isn't obfuscated. If it were, use de4dot or similar tools.

**Q: What if I can't find a specific system?**
A: Use ILSpy's search: Ctrl+Shift+F for full-text search across all assemblies.

---

**Status**: Living document - Updated as new systems are reverse engineered

**Contributors**: Add your findings!

**Last Updated**: 2025-11-24
