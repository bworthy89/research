# Phase 1 - Critical Issues Found

## 🔴 BLOCKER: CreationDefinition Not Added to Job

### Issue
The `CreateRoad` method in `GridToolPatch.cs` creates a `CreationDefinition` but never adds it to any collection that NetToolSystem will process.

**Current Code (Lines 134-171):**
```csharp
static void CreateRoad(...)
{
    // Creates definition
    CreationDefinition definition = new CreationDefinition { ... };

    // Adds to ownerDefinitions (wrong collection?)
    if (!ownerDefinitions.ContainsKey(topLevelEntity)) {
        ownerDefinitions.Add(...);
    }

    // ❌ PROBLEM: definition is never used!
}
```

### Root Cause
We don't know the internal structure of `NetToolSystem.CreateDefinitionsJob`. The original `CreateGrid` method likely:
1. Creates `CreationDefinition` instances
2. Adds them to a field on the job (e.g., `m_CreationDefinitions`)
3. NetToolSystem processes these after job completes

We need to know:
- What field stores the definitions?
- What type is it? (List? NativeList? NativeArray?)
- How does the job expose it?

### Research Needed

#### Option 1: Decompile CreateDefinitionsJob
Find the actual structure:
```csharp
public struct CreateDefinitionsJob {
    public NativeList<CreationDefinition> m_CreationDefinitions; // ?
    public NativeParallelHashMap<Entity, OwnerDefinition> m_OwnerDefinitions; // ?
    // ... other fields
}
```

**Where to look:**
- Decompiled `Game.Tools.NetToolSystem.CreateDefinitionsJob`
- Look for fields that store definitions
- Check what CreateGrid adds to

#### Option 2: Use Harmony Transpiler
Instead of replacing entire method, inject our logic:
```csharp
[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    // Modify IL to:
    // 1. Replace grid count calculation with our value
    // 2. Keep rest of original logic
}
```

**Pros:**
- Don't need to know job structure
- Original method handles definition storage
- Less likely to break

**Cons:**
- More complex to implement
- Harder to debug

#### Option 3: Reflection to Find Field
At runtime, use reflection to find the collection:
```csharp
static void CreateRoad(...)
{
    CreationDefinition def = new CreationDefinition { ... };

    // Use reflection to add to job's definition list
    var field = typeof(NetToolSystem.CreateDefinitionsJob)
        .GetField("m_CreationDefinitions", BindingFlags.NonPublic | BindingFlags.Instance);

    var list = (NativeList<CreationDefinition>)field.GetValue(job);
    list.Add(def);
}
```

**Cons:**
- Slower (reflection)
- Fragile (field name may change)
- May not work with struct jobs

### Recommendation

**Short term**: Find decompiled `CreateDefinitionsJob` structure
- Check existing decompiled code
- Or decompile from `Game.dll` if needed

**Long term**: Consider Transpiler approach for robustness

---

## ⚠️ Warning: Harmony Patch Signature

### Issue
The Prefix patch signature may not match the actual `CreateGrid` method.

**Current signature:**
```csharp
static bool Prefix(
    NetToolSystem.CreateDefinitionsJob __instance,
    ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
    Bezier4x3 centerCurve,
    Bezier4x3 sideCurve,
    bool isStraight,
    Entity topLevelEntity)
```

**Need to verify:**
- Parameter names (Harmony matches by type, but good to know)
- Parameter types (must match exactly)
- Parameter order (critical)
- Is CreateGrid private or public?

### How to Verify

1. Decompile `NetToolSystem.CreateDefinitionsJob.CreateGrid`
2. Check exact signature
3. Update patch if needed

**Expected signature (approximate):**
```csharp
private void CreateGrid(
    NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
    Bezier4x3 centerCurve,
    Bezier4x3 sideCurve,
    bool isStraight,
    Entity topLevelEntity)
```

Note: It's a method on a struct, so `__instance` is passed by value (copy).

---

## 🟡 Minor: OwnerDefinition Usage

### Issue
We only add one `OwnerDefinition` per `topLevelEntity`, but create multiple roads.

**Current code:**
```csharp
if (!ownerDefinitions.ContainsKey(topLevelEntity)) {
    ownerDefinitions.Add(topLevelEntity, new OwnerDefinition { ... });
}
```

This means:
- First road creates the entry
- All subsequent roads skip adding

**Questions:**
- Should each road have its own OwnerDefinition?
- Is OwnerDefinition per-grid or per-road?
- What is topLevelEntity? (probably the grid entity itself)

### Need to Check
Original CreateGrid implementation to see how it uses `ownerDefinitions`.

---

## 🟢 Looks Good

### Settings System
- ✅ ModSetting structure looks correct
- ✅ Attributes for UI generation proper
- ✅ LocaleEN implementation matches template

### Mod Entry
- ✅ IMod interface correct
- ✅ OnLoad/OnDispose proper
- ✅ System registration looks right

### Harmony System
- ✅ GameSystemBase for patch application correct
- ✅ PatchAll() in OnCreate is right approach

---

## Next Steps

### Immediate (Before Building)
1. ✅ Identify this blocker
2. 🔄 Find decompiled CreateDefinitionsJob structure
3. 🔄 Verify CreateGrid signature
4. 🔄 Fix CreateRoad to actually add definitions

### After Fix (Build & Test)
5. Build mod with CS2 toolchain
6. Check logs for load errors
7. Test Harmony patch applies
8. Test in-game functionality

---

## Questions for Research

1. **Where is CreateDefinitionsJob defined?**
   - Likely in `Game.Tools.NetToolSystem`
   - Need to see full struct definition

2. **What does original CreateGrid do?**
   - How does it add road definitions?
   - What collections does it use?
   - Can we call it with modified parameters?

3. **Alternative approaches?**
   - Could we patch at a higher level?
   - Could we modify parameters before CreateGrid is called?
   - Could we use Postfix to fix up definitions after?

---

## Decompilation Targets

Need to decompile and examine:

1. `Game.Tools.NetToolSystem.CreateDefinitionsJob` (struct)
   - All fields
   - CreateGrid method implementation
   - How definitions are stored

2. `Game.Tools.NetToolSystem`
   - How it uses CreateDefinitionsJob
   - How definitions are processed after job

3. `Game.Tools.CreationDefinition`
   - Full structure
   - How it's used by system

4. `Game.Tools.OwnerDefinition`
   - What it represents
   - How it's used

---

## Status

**Phase 1 Testing: BLOCKED**

Cannot proceed to build/test until CreateRoad implementation is fixed.

**Priority**: HIGH - This is core functionality
**Effort**: 1-2 hours to research and fix
**Risk**: MEDIUM - May need to change approach entirely
