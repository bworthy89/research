# Cities Skylines 2 - Road Creation API Documentation

> Reverse-engineered from decompiled Game.dll

## Overview

Roads in CS2 are built using Unity's Entity Component System (ECS). A road consists of:
- **Nodes**: Intersection points or endpoints
- **Edges**: Segments connecting two nodes
- **Curves**: Bezier curve geometry defining the road shape
- **Lanes**: Traffic lanes, pedestrian paths, utilities

## Core Components

### Node Component
```csharp
public struct Node : IComponentData
{
    public float3 m_Position;      // World position (x, y, z)
    public quaternion m_Rotation;  // Orientation
}
```

### Edge Component
```csharp
public struct Edge : IComponentData
{
    public Entity m_Start;  // Reference to start Node entity
    public Entity m_End;    // Reference to end Node entity
}
```

### Curve Component
```csharp
public struct Curve : IComponentData
{
    public Bezier4x3 m_Bezier;  // Bezier curve (4 control points)
    public float m_Length;       // Curve length in meters
}
```

### Road Component
```csharp
public struct Road : IComponentData
{
    public RoadFlags m_Flags;  // StartHalfAligned, EndHalfAligned, IsLit, etc.
}
```

### Composition Component
Links the edge with its road type (lanes, width, surface type):
```csharp
public struct Composition : IComponentData
{
    public Entity m_Edge;
    public Entity m_StartNode;
    public Entity m_EndNode;
}
```

## Utility Methods (NetUtils)

### Creating Straight Roads
```csharp
// Creates a straight Bezier curve between two points
public static Bezier4x3 StraightCurve(float3 startPos, float3 endPos)
{
    return new Bezier4x3(
        startPos,
        math.lerp(startPos, endPos, 0.333f),
        math.lerp(startPos, endPos, 0.667f),
        endPos
    );
}
```

### Creating Curved Roads
```csharp
// Fits a smooth curve with specified tangents
public static Bezier4x3 FitCurve(
    float3 startPos,
    float3 startTangent,
    float3 endPos,
    float3 endTangent
)
```

### Calculating Node Rotation
```csharp
// Converts a tangent vector to a rotation quaternion
public static quaternion GetNodeRotation(float3 tangent)
{
    return quaternion.LookRotationSafe(tangent, new float3(0, 1, 0));
}
```

### Offsetting Curves
```csharp
// Offsets curve to the left/right by specified distance
public static Bezier4x3 OffsetCurveLeftSmooth(
    Bezier4x3 curve,
    float offset
)
```

## Road Creation Workflow

### High-Level Approach

1. **Create Start Node**
   - Instantiate entity
   - Add Node component with position and rotation

2. **Create End Node**
   - Instantiate entity
   - Add Node component

3. **Create Edge**
   - Instantiate entity
   - Add Edge component (references to start/end nodes)
   - Add Curve component (generated Bezier)
   - Add Composition component (road type)
   - Add Road component (flags)

4. **Add Lanes**
   - Create SubLane entities
   - Add CarLane, PedestrianLane, etc.

### Using NetToolSystem

The `NetToolSystem` handles road creation through control points:

```csharp
public class NetToolSystem : ToolBaseSystem
{
    // Control points define the path
    private NativeList<ControlPoint> m_ControlPoints;

    // Mode determines creation type
    public enum Mode
    {
        Straight,      // Direct line between points
        SimpleCurve,   // Single curve
        ComplexCurve,  // Multi-segment curve
        Grid,          // Parallel roads
        Replace        // Upgrade existing
    }
}
```

**Control Point Structure:**
```csharp
public struct ControlPoint
{
    public float3 m_Position;         // World position
    public float3 m_HitPosition;      // Cursor hit position
    public Entity m_OriginalEntity;   // Snapped entity (if any)
}
```

## Creating a Custom Road Tool

### Basic Structure

```csharp
using Game.Tools;
using Unity.Entities;
using Unity.Mathematics;

public partial class MyRoadGeneratorSystem : ToolBaseSystem
{
    private EntityCommandBuffer m_CommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        Enabled = false;  // Don't override default tool
    }

    protected override void OnUpdate()
    {
        // Your road generation logic
    }

    private void CreateStraightRoad(float3 start, float3 end, Entity roadPrefab)
    {
        // 1. Create start node
        Entity startNode = m_CommandBuffer.CreateEntity();
        m_CommandBuffer.AddComponent(startNode, new Node
        {
            m_Position = start,
            m_Rotation = NetUtils.GetNodeRotation(math.normalize(end - start))
        });

        // 2. Create end node
        Entity endNode = m_CommandBuffer.CreateEntity();
        m_CommandBuffer.AddComponent(endNode, new Node
        {
            m_Position = end,
            m_Rotation = NetUtils.GetNodeRotation(math.normalize(start - end))
        });

        // 3. Create edge
        Entity edge = m_CommandBuffer.CreateEntity();
        m_CommandBuffer.AddComponent(edge, new Edge
        {
            m_Start = startNode,
            m_End = endNode
        });

        // 4. Add curve geometry
        Bezier4x3 curve = NetUtils.StraightCurve(start, end);
        m_CommandBuffer.AddComponent(edge, new Curve
        {
            m_Bezier = curve,
            m_Length = MathUtils.Length(curve)
        });

        // 5. Add composition (road type)
        m_CommandBuffer.AddComponent(edge, new Composition
        {
            m_Edge = edge,
            m_StartNode = startNode,
            m_EndNode = endNode
        });

        // 6. Add road component
        m_CommandBuffer.AddComponent(edge, new Road
        {
            m_Flags = RoadFlags.None
        });
    }
}
```

## Grid Generation Example

```csharp
public void GenerateGridLayout(
    float3 origin,
    int blocksX,
    int blocksZ,
    float blockWidth,
    float blockDepth,
    Entity roadPrefab)
{
    // Generate vertical roads
    for (int x = 0; x <= blocksX; x++)
    {
        float3 start = origin + new float3(x * blockWidth, 0, 0);
        float3 end = start + new float3(0, 0, blocksZ * blockDepth);
        CreateStraightRoad(start, end, roadPrefab);
    }

    // Generate horizontal roads
    for (int z = 0; z <= blocksZ; z++)
    {
        float3 start = origin + new float3(0, 0, z * blockDepth);
        float3 end = start + new float3(blocksX * blockWidth, 0, 0);
        CreateStraightRoad(start, end, roadPrefab);
    }
}
```

## Important Notes

### Prefab Requirements
- You need a valid road prefab Entity reference
- Get prefabs via `PrefabSystem`
- Example: Small road, medium road, highway, etc.

### Coordinate System
- Y-axis is up (elevation)
- Roads snap to terrain by default
- Use `AdjustPosition()` for terrain following

### Entity Command Buffer
- Don't create entities directly in jobs
- Use `EntityCommandBuffer` for deferred creation
- Playback buffer after job completion

### Snapping System
Roads automatically snap to:
- Existing nodes (within threshold)
- Building lot edges
- Zone boundaries
- Elevation points

### Lane Generation
- Lanes are auto-generated based on Composition
- LaneSystem handles lane creation/updates
- SubLane entities created per lane

## Next Steps

1. **Get Road Prefabs**: Query `PrefabSystem` for available road types
2. **Handle Terrain**: Use `TerrainHeightData` for elevation
3. **Add UI**: Create parameter panel using Game.UI
4. **Test Incrementally**: Start with single straight road
5. **Expand Patterns**: Add curved, radial, organic layouts

## Resources

- NetToolSystem.cs - Reference implementation
- NetUtils.cs - Utility methods
- LaneSystem.cs - Lane creation logic
- Game.Prefabs - Road prefab definitions

---

**Status**: This is reverse-engineered documentation. Some details may need verification through testing.
