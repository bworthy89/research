# Cities: Skylines 2 - Code Location Reference

> **Quick reference guide mapping documentation concepts to actual decompiled code locations**

**Decompiled Code Source**: https://github.com/bworthy89/roadmod/tree/main/New%20folder

**Last Updated**: 2025-11-24

---

## How to Use This Guide

1. **Find the system** you're interested in below
2. **Locate the file** in the decompiled code repository
3. **Read our documentation** for explanation
4. **Study the actual code** for implementation details

---

## Core Systems

### NetToolSystem (Road Creation)

**File**: `Game.Tools/NetToolSystem.cs` (7,807 lines, 295KB)

**Line Numbers** (approximate):
- Line 34: Class definition
- CreateGrid method: Search for "CreateGrid"
- CreateParallelCourses: Search for "CreateParallelCourses"
- Control points: Search for "ControlPoint"

**Our Documentation**:
- [GRID_TOOL_ANALYSIS.md](GRID_TOOL_ANALYSIS.md)
- [CreateGrid_Analysis.md](CreateGrid_Analysis.md)
- [CS2_ROAD_API.md](CS2_ROAD_API.md)

**Key Classes/Structs** in same file:
- `NetToolSystem` (main class)
- `ControlPoint`
- `State`
- `Mode` enum
- Various job structs

**Related Files**:
- `Game.Tools/ApplyNetSystem.cs` (40KB) - Applies net changes
- `Game.Tools/NetCourse.cs` (343 bytes) - Course data structure

---

### PrefabSystem (Asset Management)

**File**: `Game.Prefabs/PrefabSystem.cs` (22KB)

**Key Methods** (search for these):
- `AddPrefab()` - Line ~78
- `TryGetPrefab(PrefabID id, out PrefabBase prefab)`
- `GetEntity(PrefabBase prefab)`
- `GetPrefab<T>(Entity entity)`

**Our Documentation**:
- [PREFAB_SYSTEM_DEEP_DIVE.md](PREFAB_SYSTEM_DEEP_DIVE.md)

**Key Properties**:
```csharp
private List<PrefabBase> m_Prefabs;                    // Line ~37
private Dictionary<PrefabBase, Entity> m_Entities;      // Line ~45
private Dictionary<PrefabID, int> m_PrefabIndices;      // Line ~53
```

**Related Files**:
- `Game.Prefabs/PrefabBase.cs` - Base class for all prefabs
- `Game.Prefabs/PrefabID.cs` - Prefab identifier struct
- `Game.Prefabs/PrefabData.cs` - Component data

---

### RoadPrefab (Road Definitions)

**File**: `Game.Prefabs/RoadPrefab.cs` (2.6KB, 92 lines)

**Class Definition**: Line 14
```csharp
public class RoadPrefab : NetGeometryPrefab
```

**Key Properties**:
```csharp
public RoadType m_RoadType;           // Line 16 - Normal or PublicTransport
public float m_SpeedLimit = 100f;     // Line 18
public ZoneBlockPrefab m_ZoneBlock;   // Line 20 - Zoning support
public bool m_TrafficLights;          // Line 22
public bool m_HighwayRules;           // Line 24
```

**IMPORTANT DISCOVERY**:
RoadType is NOT categorized by size (Small/Medium/Large) but by:
```csharp
// File: Game.Prefabs/RoadType.cs
public enum RoadType {
    Normal,            // Regular roads
    PublicTransport    // Bus/tram routes
}
```

**Our Documentation** (needs update):
- Mentions in PREFAB_SYSTEM_DEEP_DIVE.md
- Road API in CS2_ROAD_API.md

**Related Files**:
- `Game.Prefabs/RoadType.cs` - Enum definition (8 lines)
- `Game.Prefabs/RoadData.cs` - Component data (674 bytes)
- `Game.Prefabs/RoadFlags.cs` - Road flags enum (246 bytes)
- `Game.Prefabs/RoadComposition.cs` - Road composition (238 bytes)
- `Game.Prefabs/NetGeometryPrefab.cs` - Parent class

---

### ZoneToolSystem (Zoning)

**File**: `Game.Tools/ZoneToolSystem.cs` (29KB)

**Our Documentation**:
- Brief mention in REVERSE_ENGINEERING_MASTER_GUIDE.md
- Status: 50% complete (see SYSTEM_TRACKING_MATRIX.md)

**Related Files**:
- `Game.Zones/` directory - Zone components and systems
- `Game.Prefabs/ZoneBlockPrefab.cs` - Zone block definitions

---

### Net Components (Roads/Rails/etc)

**Directory**: `Game.Net/`

**Key Files**:
- `Game.Net/Edge.cs` - Edge component
- `Game.Net/Node.cs` - Node component
- `Game.Net/Curve.cs` - Curve component
- `Game.Net/Road.cs` - Road component
- `Game.Net/Composition.cs` - Composition component
- `Game.Net/NetUtils.cs` - Utility methods

**Our Documentation**:
- [CS2_ROAD_API.md](CS2_ROAD_API.md) - Complete component reference

---

### Prefab Components

**Directory**: `Game.Prefabs/`

**Road-Related Prefabs**:
```
RoadPrefab.cs            - Road definitions
NetPrefab.cs             - Base net prefab (roads, rails, paths)
NetGeometryPrefab.cs     - Geometry definition
TrackPrefab.cs           - Train tracks
PathwayPrefab.cs         - Pedestrian paths
```

**Building Prefabs**:
```
BuildingPrefab.cs        - Building definitions
BuildingExtensionPrefab.cs
ResidentialBuildingPrefab.cs
CommercialBuildingPrefab.cs
IndustrialBuildingPrefab.cs
```

**Other Important Prefabs**:
```
VehiclePrefab.cs
TreePrefab.cs
PropPrefab.cs
ZonePrefab.cs
ServicePrefab.cs
```

---

## UI Systems

### UI Binding

**Directory**: `Game.UI.InGame/`

**Key Files**:
- `Game.UI.InGame/NetToolUISystem.cs` - Road tool UI
- `Game.UI.InGame/ZoneToolUISystem.cs` - Zone tool UI
- `Game.UI.InGame/ObjectToolUISystem.cs` - Object placement UI

**Base Classes**:
- `Game.UI/UISystemBase.cs` - Base UI system
- `Colossal.UI.Binding/` - Binding framework

**Our Documentation**:
- UI section in REVERSE_ENGINEERING_MASTER_GUIDE.md
- DECOMPILATION_WORKFLOW.md (UI workflow)

---

## Simulation Systems

**Directory**: `Game.Simulation/`

**Major Systems** (large files):
```
TrafficSimulation.cs     - Traffic flow (needs RE)
EconomySystem.cs         - Economy (needs RE)
CitizenSystem.cs         - Citizen behavior (needs RE)
TransportLineSystem.cs   - Public transit
```

**Status**: Mostly unexplored (see SYSTEM_TRACKING_MATRIX.md)

---

## Utility Systems

### Mathematics

**Files**:
- `Colossal.Mathematics/` - Math utilities
- `Unity.Mathematics/` - Unity math library (standard)
- `Game.Net/NetUtils.cs` - Network-specific math

**Our Documentation**:
- Math formulas in CreateGrid_Analysis.md
- NetUtils in CS2_ROAD_API.md

---

## Quick Lookup Table

| Concept | File Location | Line Count | Our Docs |
|---------|--------------|------------|----------|
| **Road Creation** | Game.Tools/NetToolSystem.cs | 7,807 | GRID_TOOL_ANALYSIS.md |
| **Asset Management** | Game.Prefabs/PrefabSystem.cs | ~500 | PREFAB_SYSTEM_DEEP_DIVE.md |
| **Road Prefabs** | Game.Prefabs/RoadPrefab.cs | 92 | CS2_ROAD_API.md |
| **Road Components** | Game.Net/Road.cs | Small | CS2_ROAD_API.md |
| **Node/Edge/Curve** | Game.Net/*.cs | Small | CS2_ROAD_API.md |
| **Zoning** | Game.Tools/ZoneToolSystem.cs | ~700 | Needs deep dive |
| **Buildings** | Game.Buildings/*.cs | Many | Partially documented |
| **UI Bindings** | Game.UI.InGame/*UISystem.cs | Various | Workflow guide |

---

## Finding Specific Code

### Search Strategies

**1. By Namespace**
```bash
# Find all files in a namespace
ls "/path/to/decompiled/Game.Tools/"
ls "/path/to/decompiled/Game.Prefabs/"
```

**2. By Class Name**
```bash
# Search for class definition
grep -r "class NetToolSystem" .
grep -r "class PrefabSystem" .
```

**3. By Method Name**
```bash
# Find method
grep -rn "CreateGrid" Game.Tools/
grep -rn "TryGetPrefab" Game.Prefabs/
```

**4. By Component Type**
```bash
# Find component usage
grep -rn "ComponentType.ReadWrite<Road>" .
grep -rn "IComponentData" Game.Net/
```

### Common Patterns

**Finding ECS Systems**:
```bash
grep -r "class.*System.*GameSystemBase" . | head -20
grep -r "class.*System.*ToolBaseSystem" . | head -20
```

**Finding Components**:
```bash
grep -r "struct.*:.*IComponentData" . | head -20
grep -r "struct.*:.*IBufferElementData" . | head -20
```

**Finding Prefabs**:
```bash
grep -r "class.*Prefab.*:" Game.Prefabs/ | head -30
```

---

## Important Discoveries from Actual Code

### 1. RoadType Enum

**Corrected Understanding**:

Roads are NOT categorized as "Small/Medium/Large" in the enum. Instead:

```csharp
// Game.Prefabs/RoadType.cs
public enum RoadType {
    Normal,            // Regular roads
    PublicTransport    // Roads with transit
}
```

**Size differentiation** comes from:
- `NetGeometryData.m_DefaultWidth` (width in meters)
- Lane configuration
- Prefab naming (human-readable names like "Small Road")

### 2. Prefab Naming

Prefabs are identified by:
```csharp
PrefabID(string type, string name)
// Example: PrefabID("RoadPrefab", "Small Road")
```

**To find actual road names**, we need runtime enumeration (see below).

### 3. Road Components

From RoadPrefab.cs, roads get these components:
```csharp
// Edge entities
- Road (road-specific)
- Edge (connection)
- Curve (geometry)
- UpdateFrame
- LandValue
- EdgeColor
- NetCondition
- MaintenanceConsumer
- BorderDistrict
- SubBlock (if zonable)
- ConnectedBuilding (if zonable or non-highway)

// Node entities
- Road
- Node
- UpdateFrame
- LandValue
- NodeColor
- NetCondition
- Surface
```

---

## Next Steps

### Immediate Actions

1. **Build Prefab Enumeration Mod**
   - Runtime enumeration of all RoadPrefab instances
   - Log actual prefab names
   - Categorize by width/properties

   **File to create**: `PrefabBrowserMod/`

2. **Verify Our Hypotheses**
   - Test PrefabSystem.TryGetPrefab() with actual names
   - Confirm Entity→Prefab mapping
   - Validate component queries

3. **Update Documentation**
   - Correct RoadType information
   - Add actual code references
   - Include line numbers

---

## Code Reading Tips

### Understanding Decompiled Code

**1. Ignore Compiler-Generated Code**
```csharp
[CompilerGenerated]  // Skip these
private sealed class <>c { ... }
```

**2. Focus on Public APIs**
```csharp
public void CreateGrid(...)  // This is what we care about
private void InternalHelper(...) // Implementation detail
```

**3. Look for Component Patterns**
```csharp
// This is an ECS component
public struct Road : IComponentData { }

// This is a prefab property
public class RoadPrefab : NetGeometryPrefab { }
```

**4. Job Pattern Recognition**
```csharp
[BurstCompile]
private struct SomeJob : IJobChunk {
    // Parallel processing code
}
```

### IDE Tips

**Visual Studio Code**:
```bash
# Open folder
code "/path/to/decompiled/New folder"

# Search shortcuts
Ctrl+Shift+F - Search all files
Ctrl+P - Go to file
F12 - Go to definition
```

**Visual Studio**:
```
File → Open → Folder
Ctrl+, - Search everything
F12 - Go to definition
Shift+F12 - Find all references
```

---

## Contributing

**Found something interesting?**

1. Document in this file (add section)
2. Update SYSTEM_TRACKING_MATRIX.md (mark as analyzed)
3. Create detailed analysis document
4. Submit PR

**Format**:
```markdown
### SystemName

**File**: Path/To/File.cs (line count)
**Key Methods**: Method list
**Our Docs**: Link to documentation
**Notes**: Important findings
```

---

## Resources

**Decompiled Code**: https://github.com/bworthy89/roadmod/tree/main/New%20folder

**Our Documentation**:
- [REVERSE_ENGINEERING_INDEX.md](REVERSE_ENGINEERING_INDEX.md) - Start here
- [SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md) - Progress tracker
- [DECOMPILATION_WORKFLOW.md](DECOMPILATION_WORKFLOW.md) - How to analyze

**Tools**:
- GitHub web interface (easy browsing)
- VS Code (local analysis)
- grep/ripgrep (command-line search)

---

**Last Updated**: 2025-11-24

**Status**: Living document - Add findings as you discover them!
