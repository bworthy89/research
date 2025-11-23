# Cities Skylines 2 - Grid Tool Analysis & Improvement Plan

## Current Implementation Analysis

### How It Works

The grid tool creates rectangular road networks using a 3-point placement system:

1. **Control Points** (3 required):
   - Point 1: Grid origin
   - Point 2: Defines primary direction
   - Point 3: Defines grid extent

2. **Grid Calculation**:
   ```csharp
   // Calculate grid dimensions
   float2 distances = new float2(
       math.distance(point1, point2),  // Primary direction
       math.distance(point2, point3)   // Perpendicular direction
   );

   // Determine cell count
   float2 cellSize = roadWidth * new float2(16f, 8f);  // Or zone-based
   int2 gridCount = (int2)math.ceil((distances - 0.16f) / cellSize);
   ```

3. **Road Generation**:
   - Creates roads along both axes
   - Uses `OffsetCurveLeftSmooth()` for parallel roads
   - Spacing = distance / grid_count

### Core Methods

**CreateGrid()** (Line 4035):
- Main entry point for grid creation
- Calculates grid dimensions from 3 control points
- Iterates through grid cells creating roads

**CreateParallelCourses()** (Line 3764):
- Creates multiple parallel roads
- Uses `m_ParallelCount` (int2) - roads on each side
- Uses `m_ParallelOffset` (float) - spacing between roads
- Offsets curves using `NetUtils.OffsetCurveLeftSmooth()`

**CreateParallelCourse()** (Line 3818):
- Recursive subdivision for smooth parallel curves
- Splits curves that deviate too much from parent
- Ensures consistent spacing

## Current Limitations

### 1. Rectangular Grid Only
❌ Cannot create angled grids (45°, 30°, etc.)
❌ No support for radial or organic patterns
❌ Limited to orthogonal layouts

### 2. Uniform Spacing
❌ All blocks are the same size
❌ Cannot vary block width/height
❌ No "superblock" support (larger blocks mixed with small)

### 3. Single Road Type
❌ All roads use the same prefab
❌ No automatic arterial/collector/local hierarchy
❌ Cannot mix road types within grid

### 4. Fixed Grid Count
❌ Grid count calculated automatically from distance
❌ Cannot manually specify "10x10 blocks"
❌ Cannot control exact block dimensions

### 5. No Pattern Variations
❌ No built-in neighborhood patterns
❌ Cannot save/load grid presets
❌ No randomization options

### 6. Manual Parallel Count
✅ Can adjust `parallelCount` parameter
⚠️ But it's for offsetting one road, not full grid
❌ Not exposed in UI (?)

## Proposed Improvements

### Phase 1: Enhanced Grid Control

**1. Manual Grid Count Override**
```csharp
public bool useManualGridCount = false;
public int2 manualGridCount = new int2(10, 10);

if (useManualGridCount) {
    gridCount = manualGridCount;
} else {
    // Existing auto-calculation
}
```

**Benefits:**
- "I want exactly a 12x8 block neighborhood"
- Predictable results
- Easier to plan

**2. Block Size Variation**
```csharp
public float blockWidthVariation = 0f;    // 0-50%
public float blockHeightVariation = 0f;   // 0-50%

// Apply variation per block
float actualWidth = baseWidth * random.NextFloat(
    1f - blockWidthVariation,
    1f + blockWidthVariation
);
```

**Benefits:**
- More organic neighborhoods
- Avoid monotonous grids
- Realistic urban variation

**3. Road Type Hierarchy**
```csharp
public int arterialSpacing = 4;  // Every N blocks
public Entity arterialRoadPrefab;
public Entity collectorRoadPrefab;
public Entity localRoadPrefab;

// In grid generation:
bool isArterial = (x % arterialSpacing == 0 || y % arterialSpacing == 0);
Entity prefab = isArterial ? arterialRoadPrefab : localRoadPrefab;
```

**Benefits:**
- Automatic road hierarchy
- Realistic traffic flow
- Follows urban planning principles

### Phase 2: Pattern Support

**4. Grid Patterns**
```csharp
public enum GridPattern {
    Rectangular,      // Current implementation
    Angled45,         // 45° diagonal
    Angled30,         // 30° diagonal
    Radial,           // Hub and spoke
    OrganicCurved,    // Wavy roads
    Superblock        // Large blocks with internal streets
}
```

**Implementation:**
- Rotate grid vectors for angled grids
- Use curve fitting for organic patterns
- Nested grid generation for superblocks

**5. Preset Library**
```csharp
public struct GridPreset {
    public string name;
    public int2 gridCount;
    public float blockWidth;
    public float blockHeight;
    public GridPattern pattern;
    public int arterialSpacing;
}

// Built-in presets:
- "American Suburb" (large blocks, wide roads)
- "European Old Town" (small blocks, narrow roads)
- "Manhattan Grid" (long narrow blocks)
- "Radial District" (hub and spoke)
```

### Phase 3: Advanced Features

**6. Zoning Integration**
```csharp
public bool autoZone = false;
public ZoneType blockZoning = ZoneType.ResidentialLow;

// After creating roads, apply zoning to blocks
if (autoZone) {
    ZoneBlock(blockBounds, blockZoning);
}
```

**7. Service Placement**
```csharp
public bool placeServices = false;
public int serviceSpacing = 8;  // One service per N blocks

// Place schools, parks, etc. automatically
```

**8. Real-World Import**
```csharp
// Import OSM data and extract road network
public void ImportFromOSM(OSMData data, Bounds bounds) {
    // Extract roads, convert to grid
}
```

## Implementation Plan

### Quick Wins (1-2 Days)

**1. Manual Grid Count**
- Add `useManualGridCount` bool parameter
- Add `manualGridCount` int2 parameter
- Modify `CreateGrid()` to use manual count when enabled
- **Impact**: High - gives users direct control

**2. Road Type Hierarchy**
- Add `arterialSpacing`, `arterialRoadPrefab` parameters
- Modify road creation loop to select prefab based on position
- **Impact**: High - instant urban planning benefit

### Medium Effort (1 Week)

**3. Angled Grid Pattern**
- Create `CreateAngledGrid()` method
- Rotate grid vectors by specified angle
- **Impact**: Medium - new capability, but limited use

**4. Block Size Variation**
- Add randomization to block dimensions
- Use seeded random for reproducibility
- **Impact**: Medium - aesthetic improvement

**5. Preset System**
- Define preset structure
- Implement save/load
- Add UI dropdown
- **Impact**: High - huge UX improvement

### Advanced (2-4 Weeks)

**6. Organic Pattern Generator**
- Implement curve-based road generation
- Add noise/variation to grid
- **Impact**: High - unique feature

**7. Superblock Support**
- Nested grid generation
- Arterial perimeter + internal local streets
- **Impact**: High - modern urban planning

**8. Auto-Zoning**
- Integrate with zoning system
- Auto-apply zones to grid blocks
- **Impact**: Very High - massive time saver

## Recommended Approach

### Option A: Mod the Grid Tool
Create a **mod that extends NetToolSystem**:

```csharp
public class EnhancedGridToolSystem : NetToolSystem
{
    // Add new parameters
    public int2 manualGridCount;
    public int arterialSpacing;
    public Entity[] roadPrefabs;

    // Override CreateGrid
    protected override void CreateGrid(...)
    {
        // Enhanced implementation
    }
}
```

**Pros:**
- Clean separation
- Easy to maintain
- Can release incrementally

**Cons:**
- Need to handle tool registration
- UI integration complex

### Option B: Patch Existing Tool
Use **Harmony to patch NetToolSystem.CreateGrid()**:

```csharp
[HarmonyPatch(typeof(NetToolSystem), "CreateGrid")]
public static class GridToolPatch
{
    static bool Prefix(NetToolSystem __instance, ...)
    {
        // Replace with enhanced version
        return false;  // Skip original
    }
}
```

**Pros:**
- Works with existing UI
- No tool registration needed
- Seamless integration

**Cons:**
- Game updates might break it
- More fragile

### Option C: Separate Tool
Create entirely new **"Neighborhood Planner" tool**:

```csharp
public class NeighborhoodPlannerTool : ToolBaseSystem
{
    // Complete custom implementation
    // UI panel with all options
    // Preview before placement
}
```

**Pros:**
- Full control
- Can add preview
- Clean architecture

**Cons:**
- Most work
- Need custom UI
- Duplicate some functionality

## Recommendation

**Start with Option B (Harmony Patch)**:

1. **Week 1**: Patch CreateGrid with manual count + road hierarchy
2. **Week 2**: Add angled grid option
3. **Week 3**: Implement preset system
4. **Week 4**: Add auto-zoning

**Then migrate to Option C** once proven valuable.

## User Interface Mockup

```
┌─ Grid Tool Settings ────────────────────────┐
│                                              │
│ Pattern: [Rectangular ▼]                   │
│   □ Rectangular  □ Angled 45°  □ Radial    │
│                                              │
│ Grid Size:                                   │
│   ⦿ Auto (distance-based)                   │
│   ○ Manual: [10] x [8] blocks              │
│                                              │
│ Block Size:                                  │
│   Width:  [100]m  Variation: [0]%          │
│   Height: [200]m  Variation: [0]%          │
│                                              │
│ Road Types:                                  │
│   ☑ Use road hierarchy                      │
│   Arterial every: [4] blocks               │
│   • Arterial: [Large Road ▼]               │
│   • Local:    [Small Road ▼]               │
│                                              │
│ Options:                                     │
│   ☑ Auto-zone blocks                        │
│     Zone type: [Residential Low ▼]         │
│   □ Place services (experimental)           │
│                                              │
│ Presets: [American Suburb ▼]               │
│   [Save Current]  [Delete]                  │
│                                              │
└──────────────────────────────────────────────┘
```

## Success Metrics

**Quick Wins Impact:**
- Users can create 10x10 grid in 10 seconds (vs 2 minutes manual)
- Automatic arterial roads save 50% planning time
- Presets eliminate decision fatigue

**Advanced Impact:**
- Auto-zoning reduces neighborhood creation time by 80%
- Organic patterns enable new aesthetic styles
- Real-world import opens up recreation possibilities

## Next Steps

**Ready to implement?**

1. Choose approach (A, B, or C)
2. Start with Quick Wins
3. Test with real neighborhoods
4. Iterate based on feedback

**Want me to:**
- Implement the Harmony patch for manual grid count?
- Create the enhanced CreateGrid method?
- Build the UI panel mockup?
- Start with a specific feature?
