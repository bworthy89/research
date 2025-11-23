# Blocker Solutions - CreateDefinitionsJob Unknown Structure

## The Problem

We need to know how to add `CreationDefinition` instances to the job so NetToolSystem processes them. Current implementation creates definitions but doesn't store them anywhere.

## Solution Options (Ranked by Feasibility)

### ⭐ Option 1: Harmony Transpiler (RECOMMENDED)

**Modify the IL code of CreateGrid instead of replacing it entirely.**

#### Advantages
- ✅ Don't need to know internal job structure
- ✅ Original method handles all road creation logic
- ✅ Only change what we need (grid count calculation)
- ✅ More robust against game updates

#### Implementation
```csharp
[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);

    // Find the grid count calculation:
    // int2 gridCount = new int2(
    //     Mathf.RoundToInt(gridCountFloat.x),
    //     Mathf.RoundToInt(gridCountFloat.y)
    // );

    // Replace with call to our method:
    // int2 gridCount = GetGridCount(gridCountFloat);

    for (int i = 0; i < codes.Count; i++)
    {
        // Look for the pattern that calculates gridCount
        if (codes[i].opcode == OpCodes.Call &&
            codes[i].operand.ToString().Contains("RoundToInt"))
        {
            // Replace with our calculation
            codes[i] = new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(GridToolPatch), nameof(GetManualOrAutoGridCount)));
        }
    }

    return codes;
}

static int2 GetManualOrAutoGridCount(float2 autoCalculated)
{
    if (Mod.Settings.UseManualGridCount)
    {
        return new int2(Mod.Settings.GridX, Mod.Settings.GridY);
    }
    return new int2(
        Mathf.RoundToInt(autoCalculated.x),
        Mathf.RoundToInt(autoCalculated.y)
    );
}
```

#### Challenges
- Need to understand IL patterns
- Requires testing to ensure injection point is correct
- Debugging IL modifications is harder

#### Effort: 2-3 hours

---

### Option 2: Access Decompiled Source

**Get the actual CreateDefinitionsJob structure and CreateGrid implementation.**

#### Where to Get It

1. **Decompile Game.dll yourself:**
   ```bash
   # Using ILSpy or dnSpy
   # Decompile: Game.dll → Game.Tools.NetToolSystem
   # Find: CreateDefinitionsJob struct and CreateGrid method
   ```

2. **Check existing decompilation:**
   - User mentioned `github.com/bworthy89/roadmod` in context
   - May have decompiled code already

3. **Use reflection at runtime:**
   ```csharp
   // In EnhancedGridSystem.OnCreate()
   var jobType = typeof(NetToolSystem.CreateDefinitionsJob);
   var fields = jobType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

   foreach (var field in fields)
   {
       Mod.log.Info($"Field: {field.Name}, Type: {field.FieldType}");
   }
   ```

#### Once We Have It

If we find `m_CreationDefinitions` or similar:

```csharp
static void CreateRoad(
    NetToolSystem.CreateDefinitionsJob job,
    ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
    float3 startPoint,
    float3 endPoint,
    Entity prefab,
    Entity topLevelEntity)
{
    CreationDefinition definition = new CreationDefinition { ... };

    // Use reflection to access the field
    var field = typeof(NetToolSystem.CreateDefinitionsJob)
        .GetField("m_CreationDefinitions", BindingFlags.NonPublic | BindingFlags.Instance);

    if (field != null)
    {
        var list = (NativeList<CreationDefinition>)field.GetValue(job);
        list.Add(definition);
        field.SetValue(job, list); // Update if needed
    }
}
```

#### Effort: 1-2 hours if source available, 4-6 hours if need to decompile

---

### Option 3: Different Patching Strategy

**Patch a different method that's easier to work with.**

#### Alternative Patch Points

1. **Patch the method that CALLS CreateGrid:**
   - Intercept before CreateGrid is called
   - Modify parameters (grid count calculation inputs)
   - Let original CreateGrid run with modified inputs

2. **Patch CreateParallelCourses:**
   - This is called by CreateGrid for each road
   - Might be easier to modify road creation at this level

3. **Patch at ToolBaseSystem level:**
   - Higher-level interception
   - Modify tool state before grid creation

#### Example: Patch GetGridCount (if such method exists)

```csharp
[HarmonyPatch(typeof(NetToolSystem), "CalculateGridDimensions")] // hypothetical
[HarmonyPostfix]
static void ModifyGridDimensions(ref int2 __result)
{
    if (Mod.Settings.UseManualGridCount)
    {
        __result = new int2(Mod.Settings.GridX, Mod.Settings.GridY);
    }
}
```

#### Effort: Depends on finding right method - 2-4 hours research

---

### Option 4: Simplified Prefix (Hacky)

**Just return true and log - test if patch applies correctly first.**

```csharp
static bool Prefix()
{
    if (Mod.Settings.UseManualGridCount)
    {
        Mod.log.Info($"Would create {Mod.Settings.GridX}x{Mod.Settings.GridY} grid");
        Mod.log.Warn("Not implemented yet - using original");
    }
    return true; // Always run original for now
}
```

**Purpose:** Test that Harmony patching works before solving the storage problem.

#### Effort: 5 minutes

---

## Recommended Path Forward

### Phase 1A: Verify Patching Works (30 min)
1. Implement simplified Prefix that just logs and runs original
2. Build and test in-game
3. Verify patch applies and logs appear
4. Confirms our Harmony setup is correct

### Phase 1B: Implement Transpiler (2-3 hours)
1. Learn IL patterns for grid count calculation
2. Implement transpiler to replace calculation
3. Test with manual grid count
4. Arterial spacing will still not work (need prefab selection)

### Phase 1C: Prefab Selection (2-3 hours)
1. For arterial spacing, need different approach
2. Might need to use transpiler to inject prefab selection logic too
3. Or wait until we have decompiled source

### Alternative: Get Decompiled Source (1-2 hours)
1. Ask user if they have access to decompiled NetToolSystem
2. Or use reflection to discover job structure
3. Implement proper Prefix with all features working

---

## Decision Matrix

| Approach | Effort | Robustness | Features | Risk |
|----------|--------|------------|----------|------|
| Transpiler | Medium | High | Partial | Medium |
| Decompiled Source | Low-High | Medium | Full | Low |
| Alternative Patch | Medium | Medium | Partial | Medium |
| Simplified Test | Low | High | None | Low |

---

## Questions for User

1. **Do you have access to decompiled NetToolSystem code?**
   - Can provide the CreateDefinitionsJob structure?
   - Can show the actual CreateGrid implementation?

2. **What's your preference?**
   - Quick win with Transpiler (partial features)?
   - Wait for decompiled source (full features)?
   - Try reflection approach (experimental)?

3. **Priority for Phase 1?**
   - Just manual grid count (doable with Transpiler)?
   - Must have arterial spacing too (harder without source)?

---

## Next Action

**Recommended:** Implement Phase 1A (simplified test) to verify our setup works, then decide on full implementation based on what resources are available.

```csharp
// Quick test implementation
static bool Prefix(
    NetToolSystem.CreateDefinitionsJob __instance,
    ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
    Bezier4x3 centerCurve,
    Bezier4x3 sideCurve,
    bool isStraight,
    Entity topLevelEntity)
{
    if (Mod.Settings.UseManualGridCount)
    {
        Mod.log.Info($"🎯 Enhanced Grid: Would create {Mod.Settings.GridX}x{Mod.Settings.GridY} grid");
        Mod.log.Info("   (Not yet implemented - using original calculation)");
    }

    return true; // Run original for now
}
```

This lets us:
- ✅ Test mod loads
- ✅ Test settings work
- ✅ Test Harmony applies
- ✅ See it working (with logs)
- ✅ Plan next step with confidence
