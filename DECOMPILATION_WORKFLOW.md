# Cities: Skylines 2 - Practical Decompilation Workflow

> **Goal**: Step-by-step guide to decompile and analyze specific game systems

## Quick Start Checklist

- [ ] Install ILSpy
- [ ] Locate game DLLs
- [ ] Open Game.dll
- [ ] Export system of interest
- [ ] Analyze code
- [ ] Document findings

---

## Workflow 1: Decompiling a Specific System

### Example: Reverse Engineering the Transport System

#### Step 1: Locate the DLL

**Game Installation Path:**
```
Windows: C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II
macOS: ~/Library/Application Support/Steam/steamapps/common/Cities Skylines II
Linux: ~/.steam/steam/steamapps/common/Cities Skylines II
```

**DLL Location:**
```
<GameRoot>\Cities2_Data\Managed\Game.dll
```

#### Step 2: Open in ILSpy

1. Launch ILSpy
2. **File → Open** → Select `Game.dll`
3. Wait for analysis (30-60 seconds)

#### Step 3: Navigate to System

**Using Namespace Tree:**
```
Game.dll
└── Game
    └── Simulation
        └── TransportLineSystem
```

**Using Search (Faster):**
1. Press **Ctrl+Shift+F**
2. Search: `class TransportLineSystem`
3. Double-click result

#### Step 4: Export the System

**Option A: Export Single Class**
```
1. Right-click on TransportLineSystem
2. Save Code...
3. Choose format: C# (.cs)
4. Save to: C:\CS2_Decompiled\TransportLineSystem.cs
```

**Option B: Export Entire Namespace**
```
1. Right-click on Game.Simulation namespace
2. Save Code...
3. Choose format: C# Project (.csproj)
4. Save to: C:\CS2_Decompiled\Game.Simulation\
```

#### Step 5: Analyze Dependencies

**Find what it references:**
```csharp
// At top of decompiled file
using Game.Routes;          // ← Other systems it uses
using Game.Vehicles;
using Game.Pathfind;
```

**Find what references it:**
```
1. Right-click on TransportLineSystem
2. Analyze
3. Used By → See all callers
```

#### Step 6: Document Key Findings

Create a markdown file: `TransportLineSystem_Analysis.md`

```markdown
# TransportLineSystem Analysis

## Purpose
Manages public transport lines (bus, tram, metro, etc.)

## Key Components
- `TransportLine` - Line definition
- `TransportStop` - Stop entity
- `RouteVehicle` - Vehicle assigned to route

## Key Methods
### CreateLine()
- **Purpose**: Creates a new transport line
- **Parameters**: stops[], vehicleCount, color
- **Returns**: Entity (line entity)

### UpdateLine()
- **Purpose**: Modifies existing line
- **Called by**: TransportUISystem

## Dependencies
- RouteSystem: Route pathfinding
- VehicleSpawnSystem: Spawning vehicles
- PassengerSystem: Boarding/alighting

## Questions
- [ ] How are stops automatically suggested?
- [ ] Vehicle assignment algorithm?
- [ ] Revenue calculation location?
```

---

## Workflow 2: Finding Specific Functionality

### Example: "How do road upgrades work?"

#### Step 1: Keyword Search

**Search across all assemblies:**
```
Ctrl+Shift+F in ILSpy
Search: "upgrade"
```

**Results might include:**
```
NetToolSystem.UpgradeMode
NetToolSystem.UpgradeDefinition
NetUpgradeSystem.ProcessUpgrades()
```

#### Step 2: Narrow Down

**Search with context:**
```
Search: "class.*Upgrade.*System"
```

**Better results:**
```
NetUpgradeSystem          ← Likely main system
NetToolSystem.UpgradeState ← Tool integration
```

#### Step 3: Read the Code

```csharp
public partial class NetUpgradeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Job definitions
        Entities
            .WithAll<Upgraded>()
            .ForEach((Entity entity, ref Edge edge) => {
                // Upgrade logic here
            })
            .Schedule();
    }
}
```

#### Step 4: Trace Data Flow

**Find the component:**
```csharp
public struct Upgraded : IComponentData
{
    public Entity m_NewPrefab;  // What to upgrade to
}
```

**Find who adds it:**
```
Right-click Upgraded → Analyze → Written By
Results: NetToolSystem.CreateDefinitions()
```

**Trace backwards:**
```
CreateDefinitions ← User clicks upgrade button ← UI event
```

---

## Workflow 3: Understanding ECS Queries

### Example: "How are all roads queried?"

#### Step 1: Find Query Definition

**In a SystemBase:**
```csharp
public partial class RoadMaintenanceSystem : SystemBase
{
    private EntityQuery m_RoadQuery;

    protected override void OnCreate()
    {
        // This is what we're looking for!
        m_RoadQuery = GetEntityQuery(new EntityQueryDesc {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Road>(),
                ComponentType.ReadOnly<Edge>(),
                ComponentType.ReadOnly<Curve>()
            },
            None = new ComponentType[] {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            }
        });
    }
}
```

**Interpretation:**
```
Query = All roads that have:
  ✓ Road component
  ✓ Edge component
  ✓ Curve component
  ✗ NOT deleted
  ✗ NOT temporary (preview)
```

#### Step 2: Find How It's Used

```csharp
protected override void OnUpdate()
{
    var roads = m_RoadQuery.ToEntityArray(Allocator.Temp);

    foreach (var road in roads) {
        // Process each road
    }

    roads.Dispose();  // Important! Free memory
}
```

#### Step 3: Replicate in Your Mod

```csharp
public partial class MyRoadToolSystem : SystemBase
{
    private EntityQuery m_AllRoadsQuery;

    protected override void OnCreate()
    {
        // Copy the query pattern
        m_AllRoadsQuery = GetEntityQuery(
            typeof(Road),
            typeof(Edge),
            typeof(Curve)
        );
    }

    public NativeArray<Entity> GetAllRoads()
    {
        return m_AllRoadsQuery.ToEntityArray(Allocator.Temp);
    }
}
```

---

## Workflow 4: Extracting Math Formulas

### Example: "How are parallel roads offset?"

#### Step 1: Find the Method

```
Search: "parallel" or "offset"
Result: NetToolSystem.CreateParallelCourses()
```

#### Step 2: Export and Read

**Decompiled code:**
```csharp
private void CreateParallelCourses(
    Bezier4x3 baseCurve,
    int count,
    float offset)
{
    for (int i = 1; i <= count; i++)
    {
        float distance = offset * i;

        // Left offset
        Bezier4x3 leftCurve = NetUtils.OffsetCurveLeftSmooth(
            baseCurve,
            distance
        );
        CreateCourse(leftCurve);

        // Right offset
        Bezier4x3 rightCurve = NetUtils.OffsetCurveLeftSmooth(
            baseCurve,
            -distance  // Negative = right
        );
        CreateCourse(rightCurve);
    }
}
```

#### Step 3: Find the Utility Method

```
Navigate: NetUtils.OffsetCurveLeftSmooth()
Location: Colossal.Mathematics or Game.Net
```

**Math implementation:**
```csharp
public static Bezier4x3 OffsetCurveLeftSmooth(
    Bezier4x3 curve,
    float offset)
{
    // Get normals at each control point
    float3 normal0 = GetNormal(curve.a, curve.b);
    float3 normal1 = GetNormal(curve.b, curve.c);
    float3 normal2 = GetNormal(curve.c, curve.d);
    float3 normal3 = GetNormal(curve.d, curve.c);

    // Offset each point
    return new Bezier4x3(
        curve.a + normal0 * offset,
        curve.b + normal1 * offset,
        curve.c + normal2 * offset,
        curve.d + normal3 * offset
    );
}

private static float3 GetNormal(float3 from, float3 to)
{
    float3 forward = math.normalize(to - from);
    float3 up = new float3(0, 1, 0);
    float3 right = math.cross(up, forward);
    return math.normalize(right);
}
```

#### Step 4: Extract to Your Code

```csharp
// Copy the algorithm, simplify if possible
public static Bezier4x3 OffsetCurve(Bezier4x3 curve, float distance)
{
    // Your simplified version
    float3 direction = math.normalize(curve.d - curve.a);
    float3 normal = new float3(-direction.z, 0, direction.x); // Perpendicular

    return new Bezier4x3(
        curve.a + normal * distance,
        curve.b + normal * distance,
        curve.c + normal * distance,
        curve.d + normal * distance
    );
}
```

---

## Workflow 5: UI System Integration

### Example: "How does the road tool UI work?"

#### Step 1: Find UI System

```
Search: "NetToolUISystem" or "*UISystem"
Location: Game.UI.InGame namespace
```

#### Step 2: Understand Binding Pattern

**Decompiled UI System:**
```csharp
public partial class NetToolUISystem : UISystemBase
{
    private NetToolSystem m_NetToolSystem;
    private GetterValueBinding<int> m_ParallelCountBinding;
    private GetterValueBinding<float> m_ElevationBinding;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();

        // Bindings expose C# properties to UI
        AddBinding(m_ParallelCountBinding = new GetterValueBinding<int>(
            "netTool",           // UI group name
            "parallelCount",     // Property name
            () => m_NetToolSystem.parallelCount,  // Getter
            (value) => { m_NetToolSystem.parallelCount = value; }  // Setter
        ));

        AddBinding(m_ElevationBinding = new GetterValueBinding<float>(
            "netTool",
            "elevation",
            () => m_NetToolSystem.elevation,
            (value) => { m_NetToolSystem.elevation = value; }
        ));
    }

    protected override void OnUpdate()
    {
        // Update bindings each frame
        m_ParallelCountBinding?.Update();
        m_ElevationBinding?.Update();
    }
}
```

#### Step 3: JavaScript Side (Coherent UI)

**Location (harder to find):**
```
<GameRoot>\Cities2_Data\StreamingAssets\~UI~\
```

**Typical pattern:**
```javascript
// In road-tool-panel.js
const parallelCount = engine.createBinding("netTool.parallelCount");

parallelCount.subscribe((value) => {
    // Update slider
    document.getElementById("parallel-slider").value = value;
});

function onParallelSliderChange(newValue) {
    // Update game
    parallelCount.value = newValue;
}
```

#### Step 4: Add Your Own Binding

**In your mod's UI system:**
```csharp
public partial class MyToolUISystem : UISystemBase
{
    private MyToolSystem m_ToolSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_ToolSystem = World.GetOrCreateSystemManaged<MyToolSystem>();

        // Add your custom parameter
        AddBinding(new GetterValueBinding<int>(
            "myTool",
            "arterialSpacing",
            () => m_ToolSystem.arterialSpacing,
            (value) => { m_ToolSystem.arterialSpacing = value; }
        ));
    }
}
```

---

## Workflow 6: Tracing Game Events

### Example: "What happens when user clicks 'Build'?"

#### Method 1: Search for UI Actions

```
Search: "OnApply" or "Apply" or "Confirm"
Results: Multiple *Tool.OnApply() methods
```

#### Method 2: Follow the Call Chain

**Start point: Button click in UI**
```
UI JavaScript → engine.call("tool.apply")
```

**UI binding receives:**
```csharp
public partial class ToolUISystem : UISystemBase
{
    private EventBinding m_ApplyBinding;

    protected override void OnCreate()
    {
        AddBinding(m_ApplyBinding = new EventBinding(
            "tool",
            "apply",
            () => OnApply()  // ← This gets called
        ));
    }

    private void OnApply()
    {
        m_ActiveTool.Apply();  // ← Calls tool's Apply method
    }
}
```

**Tool system handles:**
```csharp
public partial class NetToolSystem : ToolBaseSystem
{
    public override void Apply()
    {
        // Create entities from definitions
        foreach (var definition in m_Definitions)
        {
            CreateRoad(definition);
        }

        // Clear preview
        ClearPreview();
    }
}
```

#### Method 3: Set Breakpoint (dnSpy)

1. Attach dnSpy to game process
2. Find `NetToolSystem.Apply()`
3. Set breakpoint
4. Click build in game
5. Step through execution

---

## Common Decompilation Patterns

### Pattern 1: System with Job

**Common structure:**
```csharp
public partial class ExampleSystem : SystemBase
{
    // 1. Query definition
    private EntityQuery m_Query;

    // 2. Job struct
    [BurstCompile]
    private struct ProcessJob : IJobChunk
    {
        public void Execute(/*...*/) {
            // Parallel work
        }
    }

    // 3. System lifecycle
    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(/*...*/);
    }

    protected override void OnUpdate()
    {
        // 4. Schedule job
        var job = new ProcessJob { /*...*/ };
        Dependency = job.ScheduleParallel(m_Query, Dependency);
    }
}
```

### Pattern 2: Tool System

```csharp
public partial class ExampleToolSystem : ToolBaseSystem
{
    // State
    private NativeList<ControlPoint> m_ControlPoints;
    private NativeList<Definition> m_Definitions;

    // Properties (UI-bound)
    public int parameter { get; set; }

    // Lifecycle
    protected override void OnStartRunning() {
        // Tool activated
    }

    protected override void OnUpdate() {
        // Update preview
    }

    public override void Apply() {
        // User confirmed
    }

    protected override void OnStopRunning() {
        // Tool deactivated
    }
}
```

### Pattern 3: Prefab System

```csharp
public class ExamplePrefab : PrefabBase
{
    // Properties
    public float someValue;

    // Component setup
    public override void GetPrefabComponents(HashSet<ComponentType> components)
    {
        components.Add(typeof(SomeComponent));
    }

    // Entity initialization
    public override void Initialize(EntityManager entityManager, Entity entity)
    {
        entityManager.AddComponentData(entity, new SomeComponent {
            value = someValue
        });
    }
}
```

---

## Automation Scripts

### PowerShell: Batch Export Namespaces

```powershell
# Export specific systems
$ilspyPath = "C:\Tools\ILSpy\ILSpy.exe"
$gameDll = "C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll"
$outputDir = "C:\CS2_Decompiled"

$namespaces = @(
    "Game.Tools",
    "Game.Prefabs",
    "Game.Net",
    "Game.Zones",
    "Game.Buildings"
)

foreach ($namespace in $namespaces) {
    Write-Host "Exporting $namespace..."

    & $ilspyPath $gameDll `
        /outputDirectory:"$outputDir\$namespace" `
        /namespace:$namespace `
        /project
}
```

### Python: Extract Method Signatures

```python
import re
import os

def extract_method_signatures(cs_file):
    """Extract all public method signatures from C# file"""
    with open(cs_file, 'r') as f:
        content = f.read()

    # Regex for public methods
    pattern = r'public\s+(?:static\s+)?[\w<>]+\s+(\w+)\s*\([^)]*\)'

    methods = re.findall(pattern, content)
    return methods

# Usage
methods = extract_method_signatures("NetToolSystem.cs")
print("Found methods:", methods)
```

### Bash: Find All Tool Systems

```bash
#!/bin/bash
# Find all tool systems in decompiled code

find . -name "*.cs" -type f -exec grep -l "class.*ToolSystem" {} \; | sort
```

---

## Tips & Tricks

### Finding Hidden Features

**Look for debug code:**
```csharp
#if DEBUG
private void DebugDrawGrid() {
    // This reveals internal structure!
}
#endif
```

**Check for unused enums:**
```csharp
public enum RoadType {
    Small,
    Medium,
    Large,
    Highway,
    Gravel,        // Documented
    Dirt,          // Maybe cut feature?
    Future_Maglev  // Upcoming?
}
```

### Understanding Obfuscated Names

**Pattern recognition:**
```csharp
// Decompiled might have weird names
private void method_0(int int_0, float float_0)

// Context helps:
// - Takes int + float
// - Called in grid creation
// - Returns void
// → Likely "CreateGrid" or "SetGridSize"
```

### Dealing with Generics

**Decompiled generics can be confusing:**
```csharp
public T Get<T>(Entity entity) where T : IComponentData
```

**Read as:**
```
"Get a component of type T from an entity,
where T must be a component type"
```

---

## Troubleshooting

### Issue: ILSpy Crashes on Large DLL

**Solution**: Export in smaller chunks
```
1. Don't export entire Game.dll
2. Export namespaces individually
3. Or export specific types only
```

### Issue: Decompiled Code Won't Compile

**Reasons:**
- Circular references
- Missing dependencies
- Unity-specific attributes

**Solution**: You don't need to compile it!
- Read for understanding
- Copy snippets to your mod
- Rewrite in your own style

### Issue: Can't Find Specific Code

**Try multiple search terms:**
```
"road"          → Too many results
"RoadTool"      → Better
"NetToolSystem" → Specific
```

**Use IL view:**
```
1. Switch to IL view (View → IL)
2. Search IL opcodes
3. More accurate than C# search
```

---

## Next Steps

After decompiling systems:

1. **Document findings** in markdown
2. **Test hypotheses** with debug mod
3. **Build prototype** in your mod
4. **Share knowledge** with community

---

**Happy Reverse Engineering!**
