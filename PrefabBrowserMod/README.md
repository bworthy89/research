# Prefab Browser Mod

> **Debug tool for reverse engineering - Enumerates all game prefabs at runtime**

## Purpose

This mod helps reverse engineering by logging ALL available prefabs when the game starts. This answers critical questions like:

- What are the exact names of road prefabs?
- How many building types exist?
- What properties do roads have?
- Which roads are zonable?

## What It Logs

### Road Prefabs
- Name
- Road type (Normal/PublicTransport)
- Speed limit
- Zonable status
- Highway rules
- Traffic lights
- Width (from geometry data)

### Building Prefabs
- All building names
- Categorization (Residential/Commercial/Industrial)
- Total counts

### Vehicle Prefabs
- All vehicle names
- Sample listings

### Zone Prefabs
- All zone types

### Summary Statistics
- Top 20 prefab types by count

## Installation

### Build
```bash
dotnet build
```

### Install
```powershell
# Windows
Copy-Item "bin\Debug\net7.0\PrefabBrowserMod.dll" `
    "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\"
```

### Run
1. Launch Cities: Skylines 2
2. Wait for main menu to load
3. Check Player.log

## Output Location

**Windows**:
```
%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

## Example Output

```
===================================
STARTING PREFAB ENUMERATION
===================================
Total prefabs found: 3247

--- ROAD PREFABS ---
Found 47 road prefabs:

  Normal Roads:
    - Small Road                                 Speed: 50km/h [ZONABLE] [LIGHTS]
      Width: 8.0m
    - Medium Road                                Speed: 60km/h [ZONABLE] [LIGHTS]
      Width: 12.0m
    - Large Road                                 Speed: 80km/h [ZONABLE]
      Width: 20.0m
    - Highway                                    Speed: 120km/h [HIGHWAY]
      Width: 32.0m
    - Gravel Road                                Speed: 40km/h [ZONABLE]
      Width: 8.0m

  PublicTransport Roads:
    - Tram Track                                 Speed: 50km/h
      Width: 4.0m
```

## How It Works

1. **OnLoad**: Mod registers the enumeration system
2. **OnUpdate**: System runs once at startup
3. **GetAllPrefabs**: Uses reflection to access PrefabSystem.prefabs
4. **Enumerate**: Categorizes and logs all prefabs

## Code Structure

```
PrefabBrowserMod/
├── PrefabBrowserMod.csproj   # Project file
├── Mod.cs                    # Entry point
└── Systems/
    └── PrefabEnumerationSystem.cs  # Main enumeration logic
```

## Usage for Reverse Engineering

### Find Road Names
Search Player.log for "--- ROAD PREFABS ---" to see all roads.

### Find Specific Prefab
```bash
# Search log for specific prefab
grep "Highway" Player.log
grep "Residential" Player.log
```

### Update Documentation
Use findings to update:
- PREFAB_SYSTEM_DEEP_DIVE.md
- CODE_LOCATION_REFERENCE.md
- SYSTEM_TRACKING_MATRIX.md

### Build Tools
Now you know exact prefab names for:
```csharp
var roadID = new PrefabID("RoadPrefab", "Small Road");  // Verified!
if (prefabSystem.TryGetPrefab(roadID, out var prefab)) {
    // Use it
}
```

## Troubleshooting

### No Output
- Mod may not be loaded - check for "Prefab Browser Mod Loading" in log
- PrefabSystem not initialized - try loading a city first

### Empty Prefab List
- Reflection may have failed
- Check error messages in log
- PrefabSystem structure may have changed in game update

### Partial Output
- Normal - system only runs once
- Reload game to re-enumerate

## Future Enhancements

Potential additions:
- [ ] Export to JSON file
- [ ] Filter by prefab type
- [ ] Search functionality
- [ ] GUI browser (advanced)
- [ ] Property deep inspection
- [ ] Component listing

## License

MIT License - Use freely for reverse engineering and modding education

---

**Created**: 2025-11-24

**Purpose**: Reverse engineering tool - not for end users

**Use with**: Our comprehensive CS2 reverse engineering documentation
