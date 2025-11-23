# GridToolPatch Fix Guide - Quick Reference

## The Problem
The GridToolPatch mod was not creating road entities correctly because it was missing access to the EntityCommandBuffer and wasn't using the correct creation pattern.

## The Solution

### 1. Get Access to EntityCommandBuffer

The CreateDefinitionsJob struct has this field:
```csharp
public EntityCommandBuffer m_CommandBuffer;  // NOT ReadOnly!
```

Your patch needs to access this field to create entities.

### 2. The Correct Entity Creation Pattern

Here's the exact pattern used by CreateGrid for EACH road segment:

```csharp
// Step 1: Create the entity
Entity e = m_CommandBuffer.CreateEntity();

// Step 2: Create and add CreationDefinition component
CreationDefinition component = new CreationDefinition
{
    m_Prefab = m_NetPrefab,           // The road prefab
    m_SubPrefab = m_LanePrefab,       // The lane prefab
    m_RandomSeed = random.NextInt()   // Random seed
};
component.m_Flags |= CreationFlags.SubElevation;
m_CommandBuffer.AddComponent(e, component);

// Step 3: Add Updated component
m_CommandBuffer.AddComponent(e, default(Updated));

// Step 4: Create NetCourse with geometry
NetCourse netCourse = default(NetCourse);
netCourse.m_Curve = NetUtils.StraightCurve(startPosition, endPosition);
netCourse.m_StartPosition = GetCoursePos(netCourse.m_Curve, startControlPoint, 0f);
netCourse.m_EndPosition = GetCoursePos(netCourse.m_Curve, endControlPoint, 1f);

// Set required flags
netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsGrid;
netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsGrid;

// Set other flags as needed:
// - CoursePosFlags.IsParallel (for non-first rows)
// - CoursePosFlags.FreeHeight (for intermediate points)
// - CoursePosFlags.IsFirst / IsLast (for grid edges)

netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
netCourse.m_FixedIndex = -1;

// Step 5: Add NetCourse component
m_CommandBuffer.AddComponent(e, netCourse);

// Step 6: Optionally add OwnerDefinition
if (ownerDefinition2.m_Prefab != Entity.Null)
{
    m_CommandBuffer.AddComponent(e, ownerDefinition2);
    if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition2, out var localCurveCache))
    {
        m_CommandBuffer.AddComponent(e, localCurveCache);
    }
}
```

## Key Points

1. **One Entity Per Road Segment** - Each road segment in the grid is a separate entity

2. **Required Components for Each Entity:**
   - CreationDefinition (what to create)
   - Updated (marks as changed)
   - NetCourse (geometry and curve data)
   - OwnerDefinition (optional, if owned by building)

3. **EntityCommandBuffer Methods:**
   - `CreateEntity()` - creates entity, returns Entity handle
   - `AddComponent<T>(Entity, T)` - adds component to entity

4. **Critical NetCourse Properties:**
   - `m_Curve` - use `NetUtils.StraightCurve(start, end)`
   - `m_StartPosition` - use `GetCoursePos(curve, startPoint, 0f)`
   - `m_EndPosition` - use `GetCoursePos(curve, endPoint, 1f)`
   - `m_Length` - use `MathUtils.Length(curve)`
   - `m_FixedIndex` - always set to -1
   - Flags - set IsGrid, IsParallel, FreeHeight, IsFirst/IsLast as appropriate

5. **CreationDefinition Must Have:**
   - `m_Prefab` - the road prefab entity
   - `m_SubPrefab` - the lane prefab entity
   - `m_RandomSeed` - from random.NextInt()
   - `m_Flags` - include CreationFlags.SubElevation

## Grid Creation Loop Structure

```csharp
// Outer loop - Y direction (rows)
for (int y = 0; y <= gridHeight; y++)
{
    // Inner loop - X direction (columns) - HORIZONTAL ROADS
    for (int x = 0; x < gridWidth; x++)
    {
        // Create horizontal road segment using pattern above
        Entity e = m_CommandBuffer.CreateEntity();
        // ... add components ...
    }

    // Create VERTICAL ROADS between rows
    if (y != gridHeight)
    {
        for (int x = 0; x <= gridWidth; x++)
        {
            // Create vertical road segment
            Entity e = m_CommandBuffer.CreateEntity();
            // ... add components ...
        }
    }
}
```

This creates a full rectangular grid with both horizontal and vertical roads.

## Common Mistakes to Avoid

1. ❌ **Don't** try to create one entity for the whole grid
2. ❌ **Don't** forget to set CreationFlags.SubElevation
3. ❌ **Don't** forget to add the Updated component
4. ❌ **Don't** set m_CommandBuffer as ReadOnly in your reflection
5. ❌ **Don't** forget to calculate m_Length for NetCourse
6. ❌ **Don't** forget to set m_FixedIndex to -1

## What Your Patch Needs

Your Harmony patch should:

1. **Prefix/Postfix the Execute method** or **Transpile CreateGrid**
2. **Access m_CommandBuffer field** from CreateDefinitionsJob
3. **Use the exact pattern above** to create entities
4. **Create entities for BOTH directions** (horizontal and vertical roads)
5. **Set appropriate flags** for grid positioning

## Example Harmony Patch Structure

```csharp
[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "Execute")]
class GridToolPatch
{
    static void Prefix(ref NetToolSystem.CreateDefinitionsJob __instance)
    {
        // Access __instance.m_CommandBuffer here
        // Check if it's grid mode
        // Call your custom grid creation logic
    }
}
```

Or use Transpiler to inject your grid creation logic into the CreateGrid method itself.

## Reference Files

- Full analysis: /home/user/research/CreateGrid_Analysis.md
- Source file: /home/user/research/NetToolSystem.cs
- CreateGrid method: Lines 4035-4311
- CreateDefinitionsJob: Lines 2489-2623
