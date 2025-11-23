# Cities Skylines 2 - Neighborhood Generator Mod

An in-game mod that generates road layouts programmatically using CS2's modding API.

## Project Status

**⚠️ PROOF OF CONCEPT** - This is a code skeleton based on reverse-engineered CS2 internals. Not yet a working mod.

## What This Mod Will Do

Generate complete neighborhood road layouts with one click:

- **American Grid**: Classic perpendicular streets with optional arterial roads
- **Angled Grid**: 45° diagonal road network
- **Radial**: Hub-and-spoke design with concentric rings
- **Organic**: Curved, natural-looking roads

## Files

### Core Code
- **NeighborhoodGeneratorMod.cs**: Main mod system with generation algorithms
- **CS2_ROAD_API.md**: Complete documentation of CS2's road creation API

### Web Tool (Alternative)
- **index.html, app.js, style.css**: Web-based generator that exports PNG overlays
- Works with [ImageOverlay mod](https://github.com/algernon-A/ImageOverlay)

## How It Works

### Road Creation Process

1. **Create Nodes** (intersection points)
   ```csharp
   Entity node = ecb.CreateEntity();
   ecb.AddComponent(node, new Node {
       m_Position = new float3(x, y, z),
       m_Rotation = quaternion.identity
   });
   ```

2. **Create Edge** (road segment)
   ```csharp
   Entity edge = ecb.CreateEntity();
   ecb.AddComponent(edge, new Edge {
       m_Start = startNode,
       m_End = endNode
   });
   ```

3. **Add Curve Geometry**
   ```csharp
   Bezier4x3 curve = NetUtils.StraightCurve(startPos, endPos);
   ecb.AddComponent(edge, new Curve {
       m_Bezier = curve,
       m_Length = MathUtils.Length(curve)
   });
   ```

4. **Link Composition** (road type/prefab)
   ```csharp
   ecb.AddComponent(edge, new Composition {
       m_Edge = edge,
       m_StartNode = startNode,
       m_EndNode = endNode
   });
   ```

### Pattern Algorithms

**Grid Pattern:**
- Generates X vertical and Z horizontal roads
- Every N meters uses arterial road type
- Perfectly aligned perpendicular intersections

**Radial Pattern:**
- Creates radial spokes from center point
- Adds concentric circular roads at intervals
- Alternates between road types for hierarchy

**Organic Pattern:**
- Uses random variation for natural curves
- Connects waypoints with smooth curves
- More realistic suburban appearance

## What's Missing (TODOs)

### Critical
- [ ] **Prefab Lookup**: Get road Entity references from PrefabSystem
- [ ] **Component Copying**: Copy all components from prefab to edge
- [ ] **Terrain Snapping**: Adjust road elevation to terrain height
- [ ] **UI Panel**: In-game interface for parameters
- [ ] **Tool Activation**: Keybind or toolbar button

### Important
- [ ] Lane generation (auto-handled by LaneSystem?)
- [ ] Intersection nodes proper setup
- [ ] Zoning alongside roads
- [ ] Building placement
- [ ] Service coverage validation

### Nice to Have
- [ ] Save/load presets
- [ ] Real-world map import (OSM)
- [ ] Undo/redo support
- [ ] Preview before placement
- [ ] Randomization seeds

## Building the Mod

### Requirements
- Visual Studio 2022 (17.8+) or JetBrains Rider (2021.3.3+)
- Cities: Skylines II installed
- .NET SDK compatible with Unity 2022.3.7f1

### Setup

1. **Create BepInEx mod project**
   ```bash
   # Use the Cities2Modding template
   git clone https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template
   ```

2. **Add game references**
   ```xml
   <Reference Include="Game">
     <HintPath>C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll</HintPath>
   </Reference>
   <Reference Include="Colossal.Core">
     <HintPath>...\Colossal.Core.dll</HintPath>
   </Reference>
   ```

3. **Copy NeighborhoodGeneratorMod.cs** to your project

4. **Implement missing methods**:
   - `GetRoadPrefab()` - Query PrefabSystem
   - `CopyPrefabComponents()` - Clone components from prefab
   - `OnUpdate()` - Trigger generation from UI

5. **Build and test**
   ```bash
   dotnet build
   # Copy DLL to BepInEx/plugins/
   ```

## Testing Strategy

### Phase 1: Single Road
Test with one straight road:
```csharp
float3 start = new float3(0, 0, 0);
float3 end = new float3(100, 0, 0);
CreateStraightRoad(ecb, start, end, smallRoadPrefab);
```

### Phase 2: Simple Grid
Generate 3x3 grid, verify intersections

### Phase 3: Full Patterns
Test all pattern types with various parameters

### Phase 4: Integration
- Test with existing city roads
- Verify snapping behavior
- Check performance with large grids

## Known Challenges

1. **Prefab System**: Need to properly query and reference road prefabs
2. **Component Cloning**: Must copy all components from prefab to instances
3. **Terrain Elevation**: Roads need to follow ground height
4. **Lane Generation**: May need manual SubLane creation
5. **Performance**: Large grids (100x100) could lag

## Alternative: Web Tool Approach

If the mod proves too complex, use the web tool instead:

1. Open `index.html` in browser
2. Configure parameters (size, pattern, spacing)
3. Click "Generate Layout"
4. Export PNG
5. Place in `%USERPROFILE%/AppData/LocalLow/Colossal Order/Cities Skylines II/Overlays/`
6. Load with ImageOverlay mod in-game
7. Trace roads manually

**Pros**: Works now, no C# needed
**Cons**: Manual tracing required

## Resources

- [CS2 Modding Toolchain](https://cs2.paradoxwikis.com/Modding_Toolchain)
- [Cities2Modding GitHub](https://github.com/optimus-code/Cities2Modding)
- [Mod Template](https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template)
- [ImageOverlay Mod](https://github.com/algernon-A/ImageOverlay)

## Contributing

This is a proof-of-concept. To make it a working mod:

1. Fork this repo
2. Implement the TODO items
3. Test with CS2
4. Submit PR with working features

## License

MIT License - Reverse engineering for modding purposes is generally accepted in the gaming community.

---

**Questions?** Open an issue or check the [CS2 Modding Discord](https://discord.gg/citiesskylines2modding).
