# Cities Skylines 2 - Road Layout Generator

A web-based tool for generating road network layouts to use with the [ImageOverlay mod](https://github.com/algernon-A/ImageOverlay) in Cities: Skylines 2.

## Features

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

## License

MIT License - Feel free to modify and share!
