# Cities Skylines 2 - Reverse Engineering & Tool Development

> **A comprehensive project for reverse engineering Cities: Skylines 2 and building custom tools from scratch**

This repository contains:
1. **Comprehensive reverse engineering documentation** for Cities: Skylines 2
2. **Web-based road layout generator** for the ImageOverlay mod
3. **Example mod projects** demonstrating tool development

---

## 📚 Documentation

### Start Here

**New to CS2 modding?**
- **[REVERSE_ENGINEERING_INDEX.md](REVERSE_ENGINEERING_INDEX.md)** - Complete navigation guide to all documentation

**Want to build a tool?**
- **[GETTING_STARTED_TOOL_DEVELOPMENT.md](GETTING_STARTED_TOOL_DEVELOPMENT.md)** - One-day guide from zero to working mod

**Want to understand the game?**
- **[REVERSE_ENGINEERING_MASTER_GUIDE.md](REVERSE_ENGINEERING_MASTER_GUIDE.md)** - Complete RE reference

### Core Documentation

| Document | Purpose |
|----------|---------|
| [REVERSE_ENGINEERING_INDEX.md](REVERSE_ENGINEERING_INDEX.md) | Navigation hub and learning paths |
| [GETTING_STARTED_TOOL_DEVELOPMENT.md](GETTING_STARTED_TOOL_DEVELOPMENT.md) | Beginner's guide to building tools |
| [REVERSE_ENGINEERING_MASTER_GUIDE.md](REVERSE_ENGINEERING_MASTER_GUIDE.md) | Complete RE guide and reference |
| [DECOMPILATION_WORKFLOW.md](DECOMPILATION_WORKFLOW.md) | Practical decompilation workflows |
| [SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md) | Progress tracker (58% complete) |
| [PREFAB_SYSTEM_DEEP_DIVE.md](PREFAB_SYSTEM_DEEP_DIVE.md) | Asset/prefab system analysis |
| [CS2_ROAD_API.md](CS2_ROAD_API.md) | Road creation API reference |

### Example Projects

- **EnhancedGridMod/** - Working mod demonstrating custom grid generation
- **index.html** - Web-based road pattern generator

---

## 🎮 Web-Based Road Layout Generator

A browser tool for generating road network layouts to use with the [ImageOverlay mod](https://github.com/algernon-A/ImageOverlay).

### Features

- **4 Pattern Types:**
  - American Grid (perpendicular streets)
  - Angled Grid (diagonal streets)
  - Organic (curved, natural-looking roads)
  - Radial (hub and spoke design)

- **Customizable Parameters:**
  - Map size (500m - 14,336m)
  - Block dimensions
  - Arterial road spacing
  - Randomization for organic variation
  - Seeded random generation (reproducible results)

- **Road Types:**
  - Arterial roads (red, thick)
  - Collector roads (yellow, medium)
  - Local streets (white, thin)

## How to Use

1. **Open the tool:**
   - Simply open `index.html` in any modern web browser
   - No installation or server required!

2. **Adjust parameters:**
   - Choose a pattern type
   - Set map size and block dimensions
   - Configure arterial road spacing
   - Add randomization for variety (optional)

3. **Generate layout:**
   - Click "Generate Layout" to preview
   - Adjust parameters and regenerate until satisfied

4. **Export PNG:**
   - Click "Export PNG" to download
   - File will be named with pattern, size, and seed

5. **Use in Cities: Skylines 2:**
   - Install the [ImageOverlay mod](https://github.com/algernon-A/ImageOverlay)
   - Place the exported PNG in: `%USERPROFILE%/AppData/LocalLow/Colossal Order/Cities Skylines II/Overlays/`
   - Load the overlay in-game
   - Trace the roads manually using the game's road tools

## Tips

- **Seed value:** Use the same seed to regenerate identical layouts
- **Randomization:** Start at 0% for perfect grids, increase for organic variation
- **Map size:** Match your intended build area in-game
- **Arterial spacing:** Set to 0 to disable arterial roads
- **Color coding:** Follow the red (arterial) → yellow (collector) → white (local) hierarchy

## Technical Details

- Pure HTML/CSS/JavaScript (no dependencies)
- Canvas-based rendering (supports up to 16,384px)
- Exports transparent PNG compatible with ImageOverlay mod
- Seeded random generation for reproducible results

## Browser Compatibility

Works in all modern browsers:
- Chrome/Edge (recommended)
- Firefox
- Safari

---

## 🔬 Reverse Engineering Status

**Overall Progress: 58%**

### ✅ Well Understood
- Entity Component System (95%)
- NetToolSystem road creation (85%)
- PrefabSystem basics (75%)
- Grid generation (95%)
- Road components (90%)

### 🟡 Partial Understanding
- Zone system (50%)
- Building system (40%)
- UI integration (65%)
- Terrain system (45%)

### 🔴 Needs Work
- Simulation systems (15%)
- Advanced building mechanics
- Service systems

See [SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md) for complete status.

---

## 🚀 Quick Start

### For Modders
```bash
# 1. Read the getting started guide
open GETTING_STARTED_TOOL_DEVELOPMENT.md

# 2. Set up your environment (Windows)
$env:CSII_TOOLPATH = "C:\...\Cities Skylines II\Cities2_Data\Managed"

# 3. Build the example mod
cd EnhancedGridMod
dotnet build

# 4. Test in-game!
```

### For Reverse Engineers
```bash
# 1. Download ILSpy
https://github.com/icsharpcode/ILSpy

# 2. Decompile Game.dll
<GameRoot>\Cities2_Data\Managed\Game.dll

# 3. Follow the workflow
open DECOMPILATION_WORKFLOW.md

# 4. Pick an unexplored system
open SYSTEM_TRACKING_MATRIX.md
```

---

## 🤝 Contributing

Contributions welcome! Especially:

- **Reverse engineering** unexplored systems
- **Documentation** improvements
- **Example mods** demonstrating concepts
- **Bug fixes** in existing documentation
- **Community knowledge** sharing

See [REVERSE_ENGINEERING_INDEX.md](REVERSE_ENGINEERING_INDEX.md) for contribution guidelines.

---

## 📞 Community

- **Discord**: Cities: Skylines modding community
- **Reddit**: r/CitiesSkylines
- **Forums**: Paradox modding section

---

## 📜 License

MIT License - Feel free to modify and share!

**Note**: This is reverse-engineered documentation. Do not redistribute game code. Use findings for modding and education only.
