# Harmony Struct Patching Issue - Resolution

## Problem Discovery

### Symptoms
- Harmony reported patches "applied successfully"
- Diagnostic patches showed in patch list
- **BUT: No patch methods were ever called when using grid tool**
- No log output from any Prefix/Postfix patches

### Root Cause
```csharp
// NetToolSystem.cs:2489
public struct CreateDefinitionsJob : IJob  // <-- STRUCT, not class!
{
    public void Execute() { ... }
    private void CreateGrid(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions) { ... }
}
```

**Harmony cannot reliably patch struct methods** because:
1. Structs are value types (copied, not referenced)
2. When a struct method is called, it operates on a copy
3. Harmony's method interception works by replacing method pointers
4. This doesn't work for struct methods because of copying behavior
5. Unity Jobs system (IJob) makes this even worse - jobs run on worker threads

## Solution

### Strategy: Patch at Class Level

Instead of patching the struct method directly, patch the **class method that creates the struct**:

```csharp
// OLD APPROACH (doesn't work):
[HarmonyPatch]
public static class GridToolPatch
{
    static MethodBase TargetMethod()
    {
        return typeof(NetToolSystem.CreateDefinitionsJob).GetMethod("CreateGrid");
    }

    static bool Prefix(ref NetToolSystem.CreateDefinitionsJob __instance)
    {
        // ❌ This never gets called!
    }
}

// NEW APPROACH (works):
[HarmonyPatch(typeof(NetToolSystem), "UpdateCourse")]
public static class GridToolPatch
{
    static void Prefix(NetToolSystem __instance, ref JobHandle inputDeps)
    {
        // ✅ This DOES get called!
        // We can now intercept BEFORE the job is created
    }
}
```

### Implementation Details

**NetToolSystem.UpdateCourse** (line 7050):
- Creates the CreateDefinitionsJob struct
- Populates all its fields
- Schedules it with `IJobExtensions.Schedule(jobData, inputDeps)`

**Our patch intercepts BEFORE job creation:**
1. Check if mode is Grid using reflection
2. Access private `m_ControlPoints` field
3. Can modify control points before job runs
4. Let original method create and schedule job with our modified data

### Code Structure

```csharp
[HarmonyPatch(typeof(NetToolSystem), "UpdateCourse")]
public static class GridToolPatch
{
    private static FieldInfo m_ControlPointsField;
    private static FieldInfo m_ModeField;

    static void Prefix(NetToolSystem __instance)
    {
        // 1. Get mode via reflection
        var mode = (NetToolSystem.Mode)m_ModeField.GetValue(__instance);
        if (mode != NetToolSystem.Mode.Grid) return;

        // 2. Get control points
        var controlPoints = m_ControlPointsField.GetValue(__instance)
            as NativeList<ControlPoint>;

        // 3. Modify control points to create our custom grid
        // (to be implemented in next phase)
    }
}
```

## Verification

When working correctly, you should see in Player.log:
```
[INFO] GridToolPatch reflection initialized
[INFO] 🎯 GridToolPatch intercepting Grid mode: 5x5
[INFO] Original control points: 3
```

If you DON'T see these messages when using grid tool, the patch isn't working.

## Key Takeaways

1. **Always check if target is struct or class** before creating Harmony patches
2. **Structs cannot be reliably patched** - patch the calling class method instead
3. **Use reflection** to access private fields when patching class methods
4. **Patch upstream** - intercept before the struct is created rather than during execution
5. **Diagnostic logging** is essential for debugging patch issues

## References

- NetToolSystem.cs:2489 - CreateDefinitionsJob struct definition
- NetToolSystem.cs:2625 - Execute() method with Mode.Grid case
- NetToolSystem.cs:7050 - UpdateCourse() method (our new patch target)
- NetToolSystem.cs:7056-7109 - Job creation and scheduling
