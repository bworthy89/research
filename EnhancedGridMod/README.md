# Enhanced Grid Tool for Cities: Skylines II

An official mod that enhances the built-in grid tool with additional control and features for creating better neighborhoods.

## Features

### ✅ Phase 1 - Current Features

- **Manual Grid Count**: Override automatic grid calculation and specify exact dimensions (e.g., 10x10 blocks)
- **Arterial Road Spacing**: Automatically place arterial roads every N blocks for proper road hierarchy
- **Block Size Variation**: Add randomization to block sizes for more organic neighborhoods (coming soon)
- **In-Game Settings**: All features accessible through Options → Enhanced Grid Tool

### 🚧 Planned Features (Phase 2)

- **Multiple Road Types**: Automatically select different road prefabs (arterial vs local roads)
- **Advanced Patterns**: Angled grids, radial patterns, organic layouts
- **Preset System**: Save and load common grid configurations
- **Auto-Zoning**: Automatically zone blocks based on type

## Installation

### Requirements

- Cities: Skylines II (v1.1.0 or later)
- Official Modding Toolchain installed (Options → Modding → Install Toolchain)

### For Users

1. Download the mod from Paradox Mods
2. Enable in-game: Main Menu → Mods → Enable "Enhanced Grid Tool"
3. Configure: Options → Enhanced Grid Tool

### For Developers

1. Install Cities: Skylines II Modding Toolchain:
   - In-game: Options → Modding → Install Toolchain
   - This sets up the `CSII_TOOLPATH` environment variable

2. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/EnhancedGrid.git
   cd EnhancedGrid/EnhancedGridMod
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. The mod will auto-deploy to:
   ```
   %USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\EnhancedGrid\
   ```

5. Launch Cities: Skylines II and enable the mod

## Usage

### Basic Grid Creation

1. Enable manual grid count in mod settings
2. Set desired grid dimensions (e.g., Grid X = 12, Grid Y = 8)
3. Use the grid tool as normal (bulldozer mode → roads → grid)
4. Place your 3 control points - the grid will use your specified dimensions

### Road Hierarchy

1. Set "Arterial Road Spacing" to desired interval (e.g., 4)
2. Create a grid - every 4th road will be an arterial
3. Creates realistic neighborhood structure with proper traffic flow

### Settings Reference

| Setting | Description | Default |
|---------|-------------|---------|
| Use Manual Grid Count | Override auto-calculation | Off |
| Grid Width (blocks) | Number of blocks horizontally | 10 |
| Grid Height (blocks) | Number of blocks vertically | 10 |
| Arterial Road Spacing | Place arterial every N blocks (0 = none) | 4 |
| Enable Block Variation | Randomize block sizes | Off |
| Variation Amount (%) | How much to vary sizes | 0% |

## Technical Details

### Architecture

- **Mod Entry**: `Mod.cs` - IMod interface implementation
- **Settings**: `Setting.cs` - ModSetting with UI attributes
- **System**: `EnhancedGridSystem.cs` - GameSystemBase that applies Harmony patches
- **Patches**: `GridToolPatch.cs` - Harmony patch for NetToolSystem.CreateGrid

### How It Works

The mod uses HarmonyX to patch the `NetToolSystem.CreateDefinitionsJob.CreateGrid()` method:

1. Intercepts grid creation before original method runs
2. Checks if manual mode is enabled
3. If enabled, uses custom grid count instead of auto-calculated
4. Selects appropriate road prefab based on position (arterial vs local)
5. Generates road definitions with proper spacing

### Key Technologies

- **Unity DOTS/ECS**: Entity-component system architecture
- **HarmonyX**: Runtime method patching
- **Official Mod SDK**: Uses official CS2 modding toolchain

## Development

### Project Structure

```
EnhancedGridMod/
├── EnhancedGridMod.csproj    # Project file
├── Mod.cs                     # Main mod entry (IMod)
├── Setting.cs                 # Settings UI and localization
├── Systems/
│   └── EnhancedGridSystem.cs # GameSystemBase for Harmony
├── Patches/
│   └── GridToolPatch.cs      # Grid tool Harmony patch
└── Properties/
    ├── PublishConfiguration.xml
    └── Thumbnail.png
```

### Building

```bash
# Debug build
dotnet build -c Debug

# Release build
dotnet build -c Release
```

### Testing

1. Build the mod
2. Launch Cities: Skylines II
3. Check log file for mod initialization:
   ```
   %USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\
   ```
4. Look for: "Enhanced Grid Tool loaded successfully"
5. Test in-game with bulldozer → roads → grid tool

## Troubleshooting

### Mod doesn't load
- Verify modding toolchain is installed (Options → Modding)
- Check CSII_TOOLPATH environment variable is set
- Check mod is enabled in Mods menu
- Review log file for errors

### Settings don't appear
- Mod must be enabled first
- Restart game after enabling mod
- Check Options → Enhanced Grid Tool

### Grid still auto-calculates
- Ensure "Use Manual Grid Count" is enabled in settings
- Changes take effect immediately (no restart needed)

## Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

MIT License - see LICENSE file for details

## Credits

- **HarmonyX**: Harmony patching library
- **Cities: Skylines II**: Colossal Order / Paradox Interactive
- **Community**: Thanks to the CS2 modding community for resources and support

## Links

- [Official CS2 Modding Docs](https://cs2.paradoxwikis.com/Modding)
- [Cities2Modding GitHub](https://github.com/optimus-code/Cities2Modding)
- [HarmonyX Documentation](https://github.com/BepInEx/HarmonyX/wiki)

## Changelog

### v1.0.0 (Current)
- Initial release
- Manual grid count override
- Arterial road spacing
- In-game settings UI
- Basic Harmony patch implementation

### Planned Updates
- v1.1.0: Multiple road type support (different prefabs for arterial/local)
- v1.2.0: Advanced grid patterns (angled, radial, organic)
- v1.3.0: Preset system and auto-zoning
