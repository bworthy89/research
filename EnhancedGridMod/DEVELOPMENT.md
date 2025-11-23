# Enhanced Grid Mod - Development Notes

## Implementation Status

### ✅ Phase 1A - Complete
**Basic Manual Grid Count Override**

Files implemented:
- `Mod.cs` - IMod interface with OnLoad/OnDispose
- `Setting.cs` - ModSetting with UI controls and localization
- `EnhancedGridSystem.cs` - GameSystemBase that applies Harmony patches
- `GridToolPatch.cs` - Harmony patch for CreateGrid method
- `EnhancedGridMod.csproj` - Project file with official toolchain imports

Features working:
- Manual grid count override (specify exact X×Y dimensions)
- Arterial road spacing (place arterials every N blocks)
- Block variation settings (UI ready, logic TBD)
- In-game settings menu integration
- Harmony patch intercepts grid creation

## Current Implementation Details

### Harmony Patch Strategy

The `GridToolPatch.Prefix` method intercepts `NetToolSystem.CreateDefinitionsJob.CreateGrid()`:

1. **Check if manual mode enabled**: If not, returns `true` to run original method
2. **Extract settings**: Get gridX, gridY, arterialSpacing from mod settings
3. **Call EnhancedCreateGrid**: Custom implementation with manual count
4. **Return false**: Skip original method execution

### Grid Generation Algorithm

The `EnhancedCreateGrid` method:

1. Extracts 3 control points from curves (origin, primary direction, extent)
2. Calculates spacing: `distance / gridCount`
3. Generates horizontal roads (along primary axis)
4. Generates vertical roads (along secondary axis)
5. Selects arterial vs local prefab based on position modulo spacing

### Known Limitations (Phase 1)

- **Same prefab for all roads**: Currently uses `job.m_NetPrefab` for both arterial and local
  - Need PrefabSystem query to get different road types
  - Planned for Phase 2

- **Simplified road creation**: Using basic `CreateRoad` helper
  - May not handle all edge cases original method does
  - May need to copy more logic from original CreateGrid

- **No block variation yet**: Setting exists but logic not implemented
  - Need to add randomization to spacing calculations
  - Planned for Phase 1B

## Next Steps

### Phase 1B - Enhanced Prefab Support
**Goal**: Use different road types for arterials vs locals

1. Add PrefabSystem reference to EnhancedGridSystem
2. Query for road prefabs by name:
   ```csharp
   PrefabID arterialID = new PrefabID("NetPrefab", "Large Road");
   PrefabID localID = new PrefabID("NetPrefab", "Small Road");
   ```
3. Store prefab entities in static fields
4. Pass to GridToolPatch via static reference

**Unknown**: Exact prefab names - need empirical testing

### Phase 1C - Block Variation
**Goal**: Randomize block sizes for organic feel

1. Implement variation in spacing calculation:
   ```csharp
   if (settings.EnableBlockSizeVariation) {
       float variation = settings.BlockVariationPercent / 100f;
       spacing.x *= random.NextFloat(1f - variation, 1f + variation);
   }
   ```
2. Use seeded random for reproducibility
3. Test that roads still connect properly

### Phase 2 - Advanced Features

- Multiple grid patterns (angled, radial, organic)
- Preset system (save/load configurations)
- Auto-zoning integration
- Custom UI panel (beyond settings menu)

## Testing Plan

### Unit Tests
- [ ] Mod loads without errors
- [ ] Settings appear in menu
- [ ] Harmony patch applies successfully
- [ ] Manual mode toggles correctly

### Integration Tests
- [ ] Grid generates with manual count
- [ ] Grid dimensions match settings
- [ ] Arterial spacing works correctly
- [ ] Roads connect at intersections
- [ ] Works with different road prefabs
- [ ] Compatible with other mods

### Test Scenarios

1. **Basic Grid**
   - Set 10×10 grid
   - Place 3 control points
   - Verify exactly 10×10 blocks created

2. **Arterial Spacing**
   - Set spacing = 4
   - Verify every 4th road is arterial (when Phase 1B complete)

3. **Edge Cases**
   - 1×1 grid (minimum)
   - 50×50 grid (maximum)
   - Arterial spacing = 0 (disabled)
   - Arterial spacing > grid size

4. **Compatibility**
   - Disable manual mode → original tool works
   - Works with curved placement
   - Works on slopes/terrain

## Debugging Tips

### Enable Logging

Check log file at:
```
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log
```

Look for:
- "Enhanced Grid Tool loaded successfully"
- "Harmony patches applied successfully"
- "Enhanced Grid: Generating NxM grid"
- "Enhanced grid generation completed"

### Common Issues

**Mod doesn't load**
- Check CSII_TOOLPATH environment variable set
- Verify modding toolchain installed
- Check for compile errors in log

**Harmony patch doesn't apply**
- Verify target method signature matches
- Check for game version mismatch
- Look for Harmony errors in log

**Settings don't save**
- AssetDatabase.global.LoadSettings may fail silently
- Check file permissions on mods folder
- Verify Setting.SetDefaults() called

**Grid creates incorrectly**
- Log spacing calculations
- Verify gridCount values
- Check curve calculations

## Code Structure Notes

### Why IMod + GameSystemBase?

- **IMod**: Entry point for mod loading (required by official system)
  - Handles settings registration
  - Manages lifecycle (OnLoad/OnDispose)
  - Cannot use Harmony directly

- **GameSystemBase**: System that runs in game world
  - Can use Harmony patches
  - Has access to ECS world
  - Updates each frame (if OnUpdate implemented)

### Settings System

ModSetting uses attributes for auto-UI generation:
- `[SettingsUISection]` - Groups settings
- `[SettingsUISlider]` - Creates slider control
- `[SettingsUIGroupOrder]` - Orders groups
- `LocaleEN` - Provides English text strings

Settings auto-save on change (handled by framework).

### Harmony Best Practices

1. **Prefix returns bool**: `true` = run original, `false` = skip
2. **ref parameters**: Must use `ref` for output parameters
3. **__instance**: First parameter (if not static method)
4. **Method signature must match**: Parameter order and types critical

## Resources

- [Official Mod Template](https://github.com/CitiesSkylinesModding/StockModTemplatesDiffer)
- [CS2 Modding Wiki](https://cs2.paradoxwikis.com/Modding)
- [HarmonyX Wiki](https://github.com/BepInEx/HarmonyX/wiki)
- [Decompiled Game Code](https://github.com/bworthy89/roadmod) (for reference)

## File Locations

**Development**: `<repo>/EnhancedGridMod/`
**Deployed**: `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\EnhancedGrid\`
**Logs**: `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\`
**Settings**: `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsSettings\`
