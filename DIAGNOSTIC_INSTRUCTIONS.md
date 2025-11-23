# Diagnostic Instructions - Debug Settings Not Affecting Grid Behavior

## Problem
Settings UI appears and can be changed, but the grid tool behavior doesn't change.

## Root Cause Hypothesis
The Harmony patch is either:
1. Not applying at all (method signature mismatch)
2. Applying but not being called (wrong method targeted)
3. Being called but logic has a bug

## Changes Made

I've added comprehensive diagnostic logging to identify the exact issue:

### 1. EnhancedGridSystem.cs
- Try-catch around Harmony patching
- Lists all successfully patched methods
- Warns if zero patches applied
- Logs any exceptions

### 2. GridToolPatch.cs
- Custom `TargetMethod()` that lists ALL methods in `CreateDefinitionsJob`
- Shows exact method signatures with parameter types
- Logs when Prefix is called
- Logs current settings values

## What To Do Next

### Step 1: Build the Mod
```bash
cd EnhancedGridMod
dotnet build
```

### Step 2: Test In-Game
1. Launch Cities: Skylines II
2. Load a city or start a new game
3. Try using the grid tool (with or without changing settings)
4. Exit the game

### Step 3: Find the Logs
**Location:**
```
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log
```

**Or use Windows search:** `%appdata%` → go up one level to `AppData` → `LocalLow` → `Colossal Order` → `Cities Skylines II` → `Logs`

### Step 4: Search the Log

Open Player.log and search for these strings:

#### A. Check if mod loaded:
Search for: `Enhanced Grid Tool loaded successfully`

**If NOT found:** Mod isn't loading at all. Check:
- Is mod installed in correct location?
- Is mod enabled in game's Mods menu?
- Any compile errors during build?

#### B. Check if Harmony patches applied:
Search for: `Harmony patches applied successfully`

Then look for: `Total patches applied:`

**If says 0 patches:** The patch signature doesn't match. Continue to step C.

#### C. Check method discovery:
Search for: `Looking for CreateGrid in type:`

This will show all available methods and their signatures.

**What to look for:**
```
Found X methods in CreateDefinitionsJob
  Method: CreateGrid(paramType1, paramType2, ...)
  Method: OtherMethod(...)
```

**Send me:**
1. The complete parameter list for `CreateGrid`
2. Any error messages

#### D. Check if patch is being called:
Search for: `🎯 GridToolPatch.Prefix called!`

**If found:** Patch is working! Check the next line for settings values.

**If NOT found:** Patch applied but isn't being called. The method might be named differently or not used anymore.

## Expected Log Output

If everything works correctly, you should see:

```
Enhanced Grid Tool loaded successfully
Harmony patches applied successfully
Looking for CreateGrid in type: Game.Tools.NetToolSystem+CreateDefinitionsJob
Found 15 methods in CreateDefinitionsJob
  Method: CreateGrid(NativeParallelHashMap`2, Bezier4x3, ...)
  [other methods...]
Found CreateGrid: CreateGrid
  Parameters: NativeParallelHashMap`2 ownerDefinitions, Bezier4x3 centerCurve, ...
Patched method: CreateDefinitionsJob.CreateGrid
Total patches applied: 1
```

Then when you use the grid tool:
```
🎯 GridToolPatch.Prefix called!
UseManualGridCount: True, GridX: 10, GridY: 10
Enhanced Grid: Generating 10x10 grid
Enhanced grid generation started
Grid spacing: 15.00m x 15.00m
Enhanced grid generation completed: 10x10 = 210 road segments
```

## Most Likely Issues

### Issue 1: Method Signature Changed
**Symptom:** `Total patches applied: 0`

**Cause:** Game updated and CreateGrid signature changed

**Fix:** We need to update the Prefix parameter list to match what the log shows

### Issue 2: Method Renamed
**Symptom:** `Could not find CreateGrid method!`

**Cause:** Method was renamed in a game update

**Fix:** We need to find the new name from the method list

### Issue 3: Settings Not Persisting
**Symptom:** Patch called but `UseManualGridCount: False`

**Cause:** Settings aren't saving or loading correctly

**Fix:** Check settings file location and permissions

## Send Me These Log Excerpts

Please search Player.log for these sections and send them to me:

1. **Mod loading:**
   ```
   [Search for: "Enhanced Grid"]
   [Copy 10-20 lines around it]
   ```

2. **Harmony patching:**
   ```
   [Search for: "Harmony patches"]
   [Copy entire section including method list]
   ```

3. **Grid tool usage:**
   ```
   [Search for: "GridToolPatch" or "Enhanced Grid: Generating"]
   [Copy any lines found]
   ```

4. **Any errors:**
   ```
   [Search for: "EnhancedGrid" and "Error" or "Exception"]
   [Copy any error messages]
   ```

With this information, I can tell you exactly what the issue is and how to fix it!
