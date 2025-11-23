# NetToolSystem.CreateGrid Analysis Report

## Overview
This document contains extracted code from the decompiled Cities: Skylines 2 `NetToolSystem.cs` file, focusing on how the `CreateGrid` method creates road entities using the EntityCommandBuffer.

---

## 1. CreateDefinitionsJob Struct

**Location:** Line 2489

The `CreateDefinitionsJob` is an IJob struct that contains the EntityCommandBuffer used for creating entities.

### Key Fields:

```csharp
public struct CreateDefinitionsJob : IJob
{
    [ReadOnly]
    public bool m_EditorMode;

    [ReadOnly]
    public Mode m_Mode;

    [ReadOnly]
    public int2 m_ParallelCount;

    [ReadOnly]
    public float m_ParallelOffset;

    [ReadOnly]
    public RandomSeed m_RandomSeed;

    [ReadOnly]
    public NativeList<ControlPoint> m_ControlPoints;

    [ReadOnly]
    public ComponentLookup<NetGeometryData> m_NetGeometryData;

    [ReadOnly]
    public Entity m_NetPrefab;

    [ReadOnly]
    public Entity m_LanePrefab;

    // THE KEY FIELD - EntityCommandBuffer for creating entities
    public EntityCommandBuffer m_CommandBuffer;

    public void Execute()
    {
        // ... execution logic calls CreateGrid, CreateStraightLine, etc.
    }
}
```

**Important:** The `m_CommandBuffer` field is NOT ReadOnly - it's the writable buffer used to create entities.

---

## 2. Complete CreateGrid Method

**Location:** Lines 4035-4311

```csharp
private void CreateGrid(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions)
{
    ControlPoint controlPoint = m_ControlPoints[0];
    ControlPoint controlPoint2 = m_ControlPoints[1];
    ControlPoint controlPoint3 = m_ControlPoints[m_ControlPoints.Length - 1];
    FixElevation(ref controlPoint);
    FixElevation(ref controlPoint2);
    NetGeometryData netGeometryData = default(NetGeometryData);
    if (m_NetGeometryData.HasComponent(m_NetPrefab))
    {
        netGeometryData = m_NetGeometryData[m_NetPrefab];
        if (netGeometryData.m_MaxSlopeSteepness == 0f)
        {
            SetHeight(controlPoint, ref controlPoint2);
            SetHeight(controlPoint, ref controlPoint3);
        }
    }
    bool flag = math.dot(controlPoint3.m_Position.xz - controlPoint2.m_Position.xz, MathUtils.Right(controlPoint2.m_Direction)) > 0f;
    flag ^= math.dot(controlPoint3.m_Position.xz - controlPoint.m_Position.xz, controlPoint2.m_Direction) < 0f;
    float3 @float = new float3(controlPoint2.m_Direction.x, 0f, controlPoint2.m_Direction.y);
    controlPoint2.m_Position = controlPoint.m_Position + @float * math.dot(controlPoint3.m_Position - controlPoint.m_Position, @float);
    float2 float2 = new float2(math.distance(controlPoint.m_Position.xz, controlPoint2.m_Position.xz), math.distance(controlPoint2.m_Position.xz, controlPoint3.m_Position.xz));
    float2 float3 = (((netGeometryData.m_Flags & Game.Net.GeometryFlags.SnapCellSize) == 0) ? (netGeometryData.m_DefaultWidth * new float2(16f, 8f)) : ((float)ZoneUtils.GetCellWidth(netGeometryData.m_DefaultWidth) * 8f + new float2(192f, 96f)));
    float2 float4 = math.max(1f, math.ceil((float2 - 0.16f) / float3));
    float3 = float2 / float4;
    float4 -= math.select(0f, 1f, float3 < netGeometryData.m_DefaultWidth + 3f);
    int2 @int = new int2(Mathf.RoundToInt(float4.x), Mathf.RoundToInt(float4.y));

    // Fall back to straight line if grid is too small
    if (@int.y == 0)
    {
        CreateStraightLine(ref ownerDefinitions, new int2(0, 1));
        return;
    }
    if (@int.x == 0)
    {
        CreateStraightLine(ref ownerDefinitions, new int2(1, m_ControlPoints.Length - 1));
        return;
    }

    Unity.Mathematics.Random random = m_RandomSeed.GetRandom(0);
    CoursePos coursePos = GetCoursePos(new Bezier4x3(controlPoint.m_Position, controlPoint.m_Position, controlPoint.m_Position, controlPoint.m_Position), controlPoint, 0f);
    CoursePos coursePos2 = GetCoursePos(new Bezier4x3(controlPoint3.m_Position, controlPoint3.m_Position, controlPoint3.m_Position, controlPoint3.m_Position), controlPoint3, 1f);
    coursePos.m_Flags |= CoursePosFlags.IsFirst;
    coursePos2.m_Flags |= CoursePosFlags.IsLast;
    OwnerDefinition ownerDefinition2;
    bool ownerDefinition = GetOwnerDefinition(ref ownerDefinitions, Entity.Null, checkControlPoints: true, coursePos, coursePos2, out ownerDefinition2);

    // Calculate grid dimensions and setup iteration
    float length = math.distance(controlPoint.m_Position.xz, controlPoint2.m_Position.xz);
    float length2 = math.distance(controlPoint2.m_Position.xz, controlPoint3.m_Position.xz);
    Line3.Segment line = new Line3.Segment(controlPoint.m_Position, controlPoint.m_Position + controlPoint3.m_Position - controlPoint2.m_Position);
    Line3.Segment line2 = new Line3.Segment(controlPoint2.m_Position, controlPoint3.m_Position);
    Line1.Segment line3 = new Line1.Segment(controlPoint.m_Elevation, controlPoint2.m_Elevation);
    Line1.Segment line4 = new Line1.Segment(controlPoint2.m_Elevation, controlPoint3.m_Elevation);

    int2 int2 = default(int2);
    int2.y = 0;
    Line3.Segment line5 = default(Line3.Segment);
    Line3.Segment line6 = default(Line3.Segment);
    Line1.Segment line7 = default(Line1.Segment);
    Line1.Segment line8 = default(Line1.Segment);

    // OUTER LOOP - Y direction (perpendicular to first edge)
    while (int2.y <= @int.y)
    {
        // Calculate positions for this horizontal row
        float cutPosition = GetCutPosition(netGeometryData, length2, (float)int2.y / (float)@int.y);
        float cutPosition2 = GetCutPosition(netGeometryData, length2, (float)(int2.y + 1) / (float)@int.y);
        line5.a = MathUtils.Position(line, cutPosition);
        line5.b = MathUtils.Position(line2, cutPosition);
        line6.a = MathUtils.Position(line, cutPosition2);
        line6.b = MathUtils.Position(line2, cutPosition2);
        line7.a = MathUtils.Position(line3, cutPosition);
        line7.b = MathUtils.Position(line4, cutPosition);
        line8.a = MathUtils.Position(line3, cutPosition2);
        line8.b = MathUtils.Position(line4, cutPosition2);

        // INNER LOOP - X direction (parallel to first edge)
        int2.x = 0;
        while (int2.x < @int.x)
        {
            // === KEY ENTITY CREATION PATTERN ===

            // 1. CREATE ENTITY
            Entity e = m_CommandBuffer.CreateEntity();

            // 2. CREATE AND ADD CreationDefinition COMPONENT
            CreationDefinition component = new CreationDefinition
            {
                m_Prefab = m_NetPrefab,
                m_SubPrefab = m_LanePrefab,
                m_RandomSeed = random.NextInt()
            };
            component.m_Flags |= CreationFlags.SubElevation;
            m_CommandBuffer.AddComponent(e, component);

            // 3. ADD Updated COMPONENT
            m_CommandBuffer.AddComponent(e, default(Updated));

            // 4. CALCULATE CONTROL POINTS
            bool num = math.all(int2 == 0);
            bool flag2 = math.all(new int2(int2.x + 1, int2.y) == @int);
            bool flag3 = (int2.y & 1) == 1 || int2.y == @int.y;

            ControlPoint controlPoint4;
            if (num)
            {
                controlPoint4 = controlPoint;
            }
            else
            {
                float cutPosition3 = GetCutPosition(netGeometryData, length, (float)int2.x / (float)@int.x);
                controlPoint4 = new ControlPoint
                {
                    m_Rotation = controlPoint.m_Rotation,
                    m_Position = MathUtils.Position(line5, cutPosition3),
                    m_Elevation = MathUtils.Position(line7, cutPosition3)
                };
            }

            ControlPoint controlPoint5;
            if (flag2)
            {
                controlPoint5 = controlPoint3;
            }
            else
            {
                float cutPosition4 = GetCutPosition(netGeometryData, length, (float)(int2.x + 1) / (float)@int.x);
                controlPoint5 = new ControlPoint
                {
                    m_Rotation = controlPoint.m_Rotation,
                    m_Position = MathUtils.Position(line5, cutPosition4),
                    m_Elevation = MathUtils.Position(line7, cutPosition4)
                };
            }

            // 5. CREATE NetCourse COMPONENT
            NetCourse netCourse = default(NetCourse);
            netCourse.m_Curve = NetUtils.StraightCurve(controlPoint4.m_Position, controlPoint5.m_Position);
            netCourse.m_StartPosition = GetCoursePos(netCourse.m_Curve, controlPoint4, 0f);
            netCourse.m_EndPosition = GetCoursePos(netCourse.m_Curve, controlPoint5, 1f);

            if (!ownerDefinition)
            {
                netCourse.m_StartPosition.m_ParentMesh = -1;
                netCourse.m_EndPosition.m_ParentMesh = -1;
            }

            // Set flags
            netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsGrid;
            netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsGrid;

            if (int2.y != 0)
            {
                netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsParallel;
                netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsParallel;
            }

            if (!num)
            {
                netCourse.m_StartPosition.m_Flags |= CoursePosFlags.FreeHeight;
            }

            if (!flag2)
            {
                netCourse.m_EndPosition.m_Flags |= CoursePosFlags.FreeHeight;
            }

            if (int2.y == 0 || int2.y == @int.y)
            {
                if (int2.x == 0)
                {
                    netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
                }
                if (int2.x + 1 == @int.x)
                {
                    netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
                }
                netCourse.m_StartPosition.m_Flags |= (CoursePosFlags)(flag ? 32 : 16);
                netCourse.m_EndPosition.m_Flags |= (CoursePosFlags)(flag ? 32 : 16);
            }

            // Handle curve inversion for alternating rows
            if (flag3)
            {
                netCourse.m_Curve = MathUtils.Invert(netCourse.m_Curve);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_Entity, ref netCourse.m_EndPosition.m_Entity);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_SplitPosition, ref netCourse.m_EndPosition.m_SplitPosition);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_Position, ref netCourse.m_EndPosition.m_Position);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_Rotation, ref netCourse.m_EndPosition.m_Rotation);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_Elevation, ref netCourse.m_EndPosition.m_Elevation);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_Flags, ref netCourse.m_EndPosition.m_Flags);
                CommonUtils.Swap(ref netCourse.m_StartPosition.m_ParentMesh, ref netCourse.m_EndPosition.m_ParentMesh);
                quaternion a = quaternion.RotateY(MathF.PI);
                netCourse.m_StartPosition.m_Rotation = math.mul(a, netCourse.m_StartPosition.m_Rotation);
                netCourse.m_EndPosition.m_Rotation = math.mul(a, netCourse.m_EndPosition.m_Rotation);
            }

            netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
            netCourse.m_FixedIndex = -1;

            // 6. ADD NetCourse COMPONENT TO ENTITY
            m_CommandBuffer.AddComponent(e, netCourse);

            // 7. OPTIONALLY ADD OwnerDefinition AND LocalCurveCache
            if (ownerDefinition2.m_Prefab != Entity.Null)
            {
                m_CommandBuffer.AddComponent(e, ownerDefinition2);
                if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition2, out var localCurveCache))
                {
                    m_CommandBuffer.AddComponent(e, localCurveCache);
                }
            }

            int2.x++;
        }

        // CREATE PERPENDICULAR ROADS (between rows)
        if (int2.y != @int.y)
        {
            int2.x = 0;
            while (int2.x <= @int.x)
            {
                // Same pattern as above, but for perpendicular roads
                Entity e2 = m_CommandBuffer.CreateEntity();
                CreationDefinition component2 = new CreationDefinition
                {
                    m_Prefab = m_NetPrefab,
                    m_SubPrefab = m_LanePrefab,
                    m_RandomSeed = random.NextInt()
                };
                component2.m_Flags |= CreationFlags.SubElevation;
                m_CommandBuffer.AddComponent(e2, component2);
                m_CommandBuffer.AddComponent(e2, default(Updated));

                // ... (similar setup for perpendicular roads)
                // Lines 4228-4306 contain similar logic for perpendicular direction

                int2.x++;
            }
        }

        int2.y++;
    }
}
```

---

## 3. Entity Creation Pattern - Key Takeaways

### The Standard Pattern for Creating Road Entities:

```csharp
// 1. Create the entity
Entity e = m_CommandBuffer.CreateEntity();

// 2. Add CreationDefinition component (defines what to create)
CreationDefinition component = new CreationDefinition
{
    m_Prefab = m_NetPrefab,        // The road prefab
    m_SubPrefab = m_LanePrefab,    // The lane prefab
    m_RandomSeed = random.NextInt()
};
component.m_Flags |= CreationFlags.SubElevation;
m_CommandBuffer.AddComponent(e, component);

// 3. Add Updated component (marks as changed)
m_CommandBuffer.AddComponent(e, default(Updated));

// 4. Create and add NetCourse component (defines the geometry)
NetCourse netCourse = default(NetCourse);
netCourse.m_Curve = NetUtils.StraightCurve(startPos, endPos);
netCourse.m_StartPosition = GetCoursePos(netCourse.m_Curve, startPoint, 0f);
netCourse.m_EndPosition = GetCoursePos(netCourse.m_Curve, endPoint, 1f);
// ... set flags and properties ...
m_CommandBuffer.AddComponent(e, netCourse);

// 5. Optionally add OwnerDefinition (for buildings, etc.)
if (ownerDefinition.m_Prefab != Entity.Null)
{
    m_CommandBuffer.AddComponent(e, ownerDefinition);
}
```

### EntityCommandBuffer Methods Used:
- **`CreateEntity()`** - Creates a new entity and returns its handle
- **`AddComponent<T>(Entity, T)`** - Adds a component to an entity

### Components Added to Each Road Entity:
1. **CreationDefinition** - Tells the system what prefab to instantiate
2. **Updated** - Marks the entity as modified
3. **NetCourse** - Contains the geometry, curve, positions, and flags
4. **OwnerDefinition** (optional) - If the road is owned by a building/area
5. **LocalCurveCache** (optional) - If in editor mode and has owner

---

## 4. Helper Methods

### GetCutPosition
**Location:** Lines 4026-4033

```csharp
private float GetCutPosition(NetGeometryData netGeometryData, float length, float t)
{
    if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.SnapCellSize) != 0)
    {
        return math.saturate(MathUtils.Snap(length * t + 0.16f, 8f) / length);
    }
    return t;
}
```

**Purpose:** Adjusts the interpolation parameter `t` to snap to cell boundaries for zoning grids.

### GetCoursePos
**Location:** Lines 4682-4731

```csharp
private CoursePos GetCoursePos(Bezier4x3 curve, ControlPoint controlPoint, float courseDelta)
{
    CoursePos result = default(CoursePos);
    if (controlPoint.m_OriginalEntity != Entity.Null)
    {
        if (m_EdgeData.HasComponent(controlPoint.m_OriginalEntity))
        {
            if (controlPoint.m_CurvePosition <= 0f)
            {
                result.m_Entity = m_EdgeData[controlPoint.m_OriginalEntity].m_Start;
                result.m_SplitPosition = 0f;
            }
            else if (controlPoint.m_CurvePosition >= 1f)
            {
                result.m_Entity = m_EdgeData[controlPoint.m_OriginalEntity].m_End;
                result.m_SplitPosition = 1f;
            }
            else
            {
                result.m_Entity = controlPoint.m_OriginalEntity;
                result.m_SplitPosition = controlPoint.m_CurvePosition;
            }
        }
        else if (m_NodeData.HasComponent(controlPoint.m_OriginalEntity))
        {
            result.m_Entity = controlPoint.m_OriginalEntity;
            result.m_SplitPosition = controlPoint.m_CurvePosition;
        }
    }
    result.m_Position = controlPoint.m_Position;
    result.m_Elevation = controlPoint.m_Elevation;
    result.m_Rotation = NetUtils.GetNodeRotation(MathUtils.Tangent(curve, courseDelta));
    result.m_CourseDelta = courseDelta;
    result.m_ParentMesh = controlPoint.m_ElementIndex.x;

    // ... (parent mesh handling logic)

    return result;
}
```

**Purpose:** Creates a CoursePos struct from a control point, handling entity references and mesh hierarchies.

### CreateStraightLine (For Comparison)
**Location:** Lines 3611-3683

Key pattern (similar to CreateGrid):

```csharp
private void CreateStraightLine(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, int2 index)
{
    // ... setup control points ...

    Unity.Mathematics.Random random = m_RandomSeed.GetRandom(0);
    CreationDefinition creationDefinition = new CreationDefinition
    {
        m_Prefab = m_NetPrefab,
        m_SubPrefab = m_LanePrefab,
        m_RandomSeed = random.NextInt()
    };
    creationDefinition.m_Flags |= CreationFlags.SubElevation;

    NetCourse course = default(NetCourse);
    course.m_Curve = NetUtils.StraightCurve(controlPoint.m_Position, controlPoint2.m_Position);
    course.m_StartPosition = GetCoursePos(course.m_Curve, controlPoint, 0f);
    course.m_EndPosition = GetCoursePos(course.m_Curve, controlPoint2, 1f);
    // ... set flags ...
    course.m_Length = MathUtils.Length(course.m_Curve);
    course.m_FixedIndex = -1;

    // THE CREATION PATTERN
    Entity e = m_CommandBuffer.CreateEntity();
    m_CommandBuffer.AddComponent(e, creationDefinition);
    m_CommandBuffer.AddComponent(e, default(Updated));

    if (GetOwnerDefinition(..., out var ownerDefinition))
    {
        m_CommandBuffer.AddComponent(e, ownerDefinition);
        if (m_EditorMode && GetLocalCurve(course, ownerDefinition, out var localCurveCache))
        {
            m_CommandBuffer.AddComponent(e, localCurveCache);
        }
    }
    else
    {
        course.m_StartPosition.m_ParentMesh = -1;
        course.m_EndPosition.m_ParentMesh = -1;
    }

    m_CommandBuffer.AddComponent(e, course);

    // ... parallel courses logic ...
}
```

---

## 5. Key Findings for GridToolPatch

### What was missing in the mod's GridToolPatch:

1. **EntityCommandBuffer Access:**
   - The mod needs to access `m_CommandBuffer` from the CreateDefinitionsJob
   - This field is **NOT ReadOnly** in the original code

2. **Entity Creation Sequence:**
   ```csharp
   Entity entity = commandBuffer.CreateEntity();
   commandBuffer.AddComponent(entity, creationDefinition);
   commandBuffer.AddComponent(entity, default(Updated));
   commandBuffer.AddComponent(entity, netCourse);
   ```

3. **Required Components:**
   - **CreationDefinition** - Must include m_Prefab, m_SubPrefab, m_RandomSeed, and CreationFlags.SubElevation
   - **Updated** - Simple marker component
   - **NetCourse** - The geometry definition with curve, positions, flags, and length
   - **OwnerDefinition** (conditional) - If there's an owner building/area

4. **NetCourse Properties:**
   - `m_Curve` - Created with NetUtils.StraightCurve()
   - `m_StartPosition` and `m_EndPosition` - Created with GetCoursePos()
   - `m_Length` - Calculated with MathUtils.Length()
   - `m_FixedIndex` - Always set to -1
   - Flags properly set (IsGrid, IsParallel, FreeHeight, IsFirst, IsLast, etc.)

5. **Grid Pattern:**
   - Creates roads in two directions (parallel and perpendicular)
   - Nested loops: outer loop for Y (rows), inner loop for X (columns)
   - Creates perpendicular roads between rows (when int2.y != @int.y)
   - Each road segment is a separate entity

---

## 6. Source File Information

- **Downloaded from:** https://raw.githubusercontent.com/bworthy89/roadmod/main/New%20folder/Game.Tools/NetToolSystem.cs
- **File size:** 294 KB
- **Download date:** 2025-11-23
- **Local path:** /home/user/research/NetToolSystem.cs

---

## Summary

The CreateGrid method creates a rectangular grid of roads by:
1. Calculating grid dimensions based on distances and net geometry
2. Using nested loops to iterate through grid positions
3. For each position, creating an entity via `m_CommandBuffer.CreateEntity()`
4. Adding required components: CreationDefinition, Updated, NetCourse, and optionally OwnerDefinition
5. Setting appropriate flags on the NetCourse to indicate grid position (IsGrid, IsParallel, IsFirst, IsLast, etc.)
6. Creating both horizontal and vertical roads to form the complete grid

The key insight is that **each road segment is a separate entity**, and the EntityCommandBuffer is used to batch-create all these entities with their components, which are then processed by other systems to actually instantiate the road geometry.
