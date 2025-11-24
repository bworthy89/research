# Cities: Skylines 2 - Complete Reverse Engineering Documentation

> **Comprehensive documentation for reverse engineering Cities: Skylines 2 and building custom tools from scratch**

**Last Updated**: 2025-11-24
**Status**: Active Development
**Completion**: ~58% of core systems

---

## 📚 Documentation Structure

### 🚀 Getting Started

**New to CS2 modding? Start here:**

1. **[GETTING_STARTED_TOOL_DEVELOPMENT.md](GETTING_STARTED_TOOL_DEVELOPMENT.md)**
   *One-day guide from zero to your first working tool*
   - Environment setup
   - Hello World mod
   - Simple road counter tool
   - Best practices

### 📖 Core Guides

**Essential reading for all developers:**

2. **[REVERSE_ENGINEERING_MASTER_GUIDE.md](REVERSE_ENGINEERING_MASTER_GUIDE.md)**
   *Complete reference for reverse engineering the entire game*
   - Tools & setup
   - Game architecture overview
   - System-by-system breakdown
   - Building custom tools
   - Advanced topics

3. **[DECOMPILATION_WORKFLOW.md](DECOMPILATION_WORKFLOW.md)**
   *Practical workflows for decompiling and analyzing specific systems*
   - Step-by-step decompilation
   - Finding functionality
   - Understanding ECS queries
   - Extracting math formulas
   - UI system integration

4. **[SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md)**
   *Progress tracker showing what's been reverse engineered*
   - Complete system inventory
   - Status and completion percentages
   - Priority matrix
   - Knowledge gaps
   - Next steps

### 🔍 Deep Dives

**Detailed analysis of specific systems:**

5. **[PREFAB_SYSTEM_DEEP_DIVE.md](PREFAB_SYSTEM_DEEP_DIVE.md)**
   *Everything about the prefab/asset system*
   - PrefabID structure
   - Query methods
   - Road prefab usage
   - Code examples

6. **[CS2_ROAD_API.md](CS2_ROAD_API.md)**
   *Road creation API documentation*
   - Core components (Node, Edge, Curve)
   - NetUtils methods
   - Road creation workflow
   - Grid generation examples

7. **[GRID_TOOL_ANALYSIS.md](GRID_TOOL_ANALYSIS.md)**
   *NetToolSystem grid mode analysis*
   - CreateGrid method breakdown
   - Parallel road creation
   - Parameters and controls

8. **[CreateGrid_Analysis.md](CreateGrid_Analysis.md)**
   *Deep dive into grid generation algorithm*
   - Algorithm walkthrough
   - Math formulas
   - Step-by-step execution

### 🛠️ Implementation Guides

**For building actual mods:**

9. **[CORRECT_MOD_IMPLEMENTATION.md](CORRECT_MOD_IMPLEMENTATION.md)**
   *Best practices for mod development*

10. **[OFFICIAL_MOD_IMPLEMENTATION.md](OFFICIAL_MOD_IMPLEMENTATION.md)**
    *Official mod structure guidelines*

11. **[IMPLEMENTATION_READY.md](IMPLEMENTATION_READY.md)**
    *Pre-implementation checklist*

12. **[IMPLEMENTATION_COMPLETE.md](IMPLEMENTATION_COMPLETE.md)**
    *Post-implementation validation*

### 🔧 Specific Features

**Focused guides for specific features:**

13. **[GridToolPatch_FixGuide.md](GridToolPatch_FixGuide.md)**
    *Harmony patching for grid tool*

14. **[BLOCKER_SOLUTIONS.md](EnhancedGridMod/BLOCKER_SOLUTIONS.md)**
    *Solutions to common blocking issues*

---

## 🎯 Quick Navigation

### By Experience Level

**Beginner (Just Starting)**
1. GETTING_STARTED_TOOL_DEVELOPMENT.md
2. CS2_ROAD_API.md (basics section)
3. Build simple mods

**Intermediate (Have basic mod working)**
1. REVERSE_ENGINEERING_MASTER_GUIDE.md
2. DECOMPILATION_WORKFLOW.md
3. PREFAB_SYSTEM_DEEP_DIVE.md
4. Study NetToolSystem

**Advanced (Building complex tools)**
1. SYSTEM_TRACKING_MATRIX.md (find gaps)
2. Decompile specific systems
3. Deep algorithm analysis
4. Contribute findings

### By Goal

**"I want to build a road tool"**
```
1. GETTING_STARTED_TOOL_DEVELOPMENT.md
2. CS2_ROAD_API.md
3. GRID_TOOL_ANALYSIS.md
4. NetToolSystem.cs (decompile)
5. EnhancedGridMod/ (example project)
```

**"I want to understand the prefab system"**
```
1. REVERSE_ENGINEERING_MASTER_GUIDE.md (Prefab section)
2. PREFAB_SYSTEM_DEEP_DIVE.md
3. PrefabSystem.cs (decompile)
4. Test with debug mod
```

**"I want to understand game architecture"**
```
1. REVERSE_ENGINEERING_MASTER_GUIDE.md (Architecture section)
2. DECOMPILATION_WORKFLOW.md (Patterns section)
3. SYSTEM_TRACKING_MATRIX.md (Overview)
4. Explore Game.dll namespaces
```

**"I want to build a zoning tool"**
```
1. CS2_ROAD_API.md (understand roads first)
2. REVERSE_ENGINEERING_MASTER_GUIDE.md (Zone section)
3. Decompile ZoneSystem
4. Study zone components
⚠️ Status: Partial RE (50% complete)
```

**"I want to contribute to reverse engineering"**
```
1. SYSTEM_TRACKING_MATRIX.md (find gaps)
2. DECOMPILATION_WORKFLOW.md (process)
3. Pick an un-analyzed system
4. Document your findings
5. Submit PR
```

---

## 📊 Current Status

### Overall Progress: 58%

**Core Systems (Well Understood)**
- ✅ Entity Component System (95%)
- ✅ NetToolSystem road creation (85%)
- ✅ PrefabSystem basics (75%)
- ✅ Grid generation (95%)
- ✅ Road components (90%)
- ✅ Basic UI bindings (70%)

**Partial Understanding**
- 🟡 Zone system (50%)
- 🟡 Building system (40%)
- 🟡 Lane system (45%)
- 🟡 Terrain system (45%)
- 🟡 UI integration (65%)

**Not Yet Analyzed**
- 🔴 Simulation systems (15%)
- 🔴 Advanced building mechanics
- 🔴 Service systems
- 🔴 Multiplayer (if exists)

**See [SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md) for details**

---

## 🏗️ Example Projects

### EnhancedGridMod

**Location**: `EnhancedGridMod/`

**Purpose**: Reference implementation of a grid generation tool

**Features**:
- Manual grid count control
- Arterial road spacing
- Road hierarchy
- Harmony patching examples

**Status**: Phase 1 complete, compilable

**Key Files**:
- `Mod.cs` - Entry point
- `Systems/EnhancedGridSystem.cs` - Main system
- `Patches/GridToolPatch.cs` - Harmony patches
- `Setting.cs` - Configuration

### Web-Based Road Layout Generator

**Location**: Root directory (`index.html`, `app.js`, `style.css`)

**Purpose**: Generate road patterns for ImageOverlay mod

**Features**:
- American grid
- Angled grid
- Organic patterns
- Radial layouts
- PNG export

**Usage**: Open `index.html` in browser

---

## 🔬 Reverse Engineering Achievements

### What We've Accomplished

**NetToolSystem Analysis**
- ✅ Decompiled complete NetToolSystem.cs (301KB)
- ✅ Mapped CreateGrid algorithm
- ✅ Documented CreateParallelCourses
- ✅ Understood control point system
- ✅ Identified key data structures

**PrefabSystem Analysis**
- ✅ PrefabID structure documented
- ✅ Query methods mapped
- ✅ Entity←→Prefab relationship clear
- 🟡 Road prefab names (hypothesis)
- 🔴 Runtime enumeration (needed)

**Component System**
- ✅ Node, Edge, Curve documented
- ✅ Road, Composition understood
- ✅ CreationDefinition mapped
- 🟡 Lane components (partial)

**Math & Utilities**
- ✅ NetUtils methods documented
- ✅ Bezier curve operations
- ✅ Offset calculations
- ✅ Rotation/tangent math

**UI System**
- ✅ Binding pattern understood
- ✅ GetterValueBinding documented
- ✅ EventBinding mapped
- 🟡 Coherent UI integration (partial)
- 🔴 Custom UI creation (needs work)

### What's Left to Do

**High Priority**
1. Verify exact road prefab names
2. Complete lane system analysis
3. Document zone spawn system
4. Map building placement rules
5. UI integration deep dive

**Medium Priority**
6. Terrain system details
7. Save/load format
8. Advanced NetGeometry
9. Path-finding basics
10. Service building systems

**Low Priority**
11. Simulation deep dive
12. Economy system
13. Citizen AI
14. Vehicle behavior
15. Audio system

**See [SYSTEM_TRACKING_MATRIX.md](SYSTEM_TRACKING_MATRIX.md) for complete list**

---

## 🧰 Tools & Resources

### Decompilation Tools

**ILSpy** - Primary tool
- https://github.com/icsharpcode/ILSpy
- Best for exporting full projects

**dnSpy** - Debugger
- https://github.com/dnSpy/dnSpy
- Runtime debugging support
- No longer maintained

**dotPeek** - JetBrains
- https://www.jetbrains.com/decompiler/
- Commercial quality output

### Development Tools

**Visual Studio 2022**
- https://visualstudio.microsoft.com/

**JetBrains Rider**
- https://www.jetbrains.com/rider/
- Best C# IDE (paid)

**.NET 7.0 SDK**
- https://dotnet.microsoft.com/download/dotnet/7.0

### Libraries

**Harmony** - Code patching
- https://harmony.pardeike.net/
- Version: 2.3.3
- Essential for modding

**Unity DOTS**
- https://docs.unity3d.com/Packages/com.unity.entities@latest
- ECS framework documentation

**Unity Mathematics**
- https://docs.unity3d.com/Packages/com.unity.mathematics@latest
- Math library reference

### Game Files

**Windows (Steam)**
```
C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\
```

**Key DLLs**
```
Cities2_Data\Managed\Game.dll
Cities2_Data\Managed\Colossal.*.dll
Cities2_Data\Managed\Unity.*.dll
```

**Mod Folder**
```
%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\
```

**Logs**
```
%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

---

## 📝 Contributing

### How to Contribute

**Found new information?**
1. Document your findings in markdown
2. Follow existing format
3. Update SYSTEM_TRACKING_MATRIX.md
4. Submit PR or share in Discord

**Reverse engineered a new system?**
1. Use DECOMPILATION_WORKFLOW.md as guide
2. Create `<SystemName>_Analysis.md`
3. Include:
   - Purpose and overview
   - Key methods
   - Data structures
   - Code examples
   - Unknown/questions
4. Update tracking matrix

**Built a cool tool?**
1. Share source code
2. Document architecture
3. Write usage guide
4. Add to examples section

### Documentation Standards

**File naming:**
```
UPPERCASE_WITH_UNDERSCORES.md     # Guides/references
SystemName_Analysis.md            # System analyses
feature-guide.md                  # Specific features
```

**Section structure:**
```markdown
# Title

> Brief description

## Table of Contents

## Overview

## Detailed Sections

## Examples

## Questions/Unknowns

## Next Steps
```

**Code examples:**
```csharp
// Always include context comments
public void ExampleMethod() {
    // Explain what this does
}
```

---

## 🗺️ Learning Paths

### Path 1: Road Tool Developer (2-3 weeks)

**Week 1: Basics**
- [ ] Set up environment
- [ ] Read GETTING_STARTED
- [ ] Build Hello World mod
- [ ] Build Road Counter tool
- [ ] Study CS2_ROAD_API.md

**Week 2: NetToolSystem**
- [ ] Decompile NetToolSystem.cs
- [ ] Read GRID_TOOL_ANALYSIS.md
- [ ] Read CreateGrid_Analysis.md
- [ ] Study EnhancedGridMod code
- [ ] Build simple grid modifier

**Week 3: Advanced**
- [ ] Read PREFAB_SYSTEM_DEEP_DIVE.md
- [ ] Implement prefab switching
- [ ] Add custom parameters
- [ ] Build complete tool
- [ ] Polish and release

### Path 2: General Modder (4-6 weeks)

**Weeks 1-2: Foundation** (same as Path 1)

**Week 3: Architecture**
- [ ] Read REVERSE_ENGINEERING_MASTER_GUIDE
- [ ] Understand ECS deeply
- [ ] Study job system
- [ ] Learn query patterns

**Week 4: Systems**
- [ ] Choose a system (Zone/Building/Terrain)
- [ ] Decompile and analyze
- [ ] Document findings
- [ ] Build proof of concept

**Weeks 5-6: Integration**
- [ ] Combine multiple systems
- [ ] Build complex tool
- [ ] Add UI
- [ ] Release and iterate

### Path 3: Reverse Engineer (Ongoing)

**Phase 1: Setup**
- [ ] Install all tools
- [ ] Decompile Game.dll
- [ ] Read all existing docs
- [ ] Understand current state

**Phase 2: Pick Target**
- [ ] Check SYSTEM_TRACKING_MATRIX
- [ ] Choose unexplored system
- [ ] Study dependencies
- [ ] Plan approach

**Phase 3: Analyze**
- [ ] Use DECOMPILATION_WORKFLOW
- [ ] Export system code
- [ ] Map structure
- [ ] Identify patterns
- [ ] Test hypotheses

**Phase 4: Document**
- [ ] Write analysis document
- [ ] Create code examples
- [ ] Note unknowns
- [ ] Update tracking matrix
- [ ] Share findings

---

## 🎓 Advanced Topics

### Unity DOTS/ECS Mastery

**Required for advanced modding**

**Resources:**
- Unity DOTS manual
- ECS samples on GitHub
- Burst compiler documentation
- Job system guide

**Key concepts:**
- Archetypes
- Chunk iteration
- Burst compilation
- Job dependencies
- Shared components

### Harmony Patching Techniques

**Essential for modifying game behavior**

**Patch types:**
- Prefix (run before)
- Postfix (run after)
- Transpiler (modify IL)
- Finalizer (error handling)

**Best practices:**
- Minimal changes
- Preserve original behavior option
- Handle edge cases
- Test compatibility

### Performance Optimization

**Making mods fast**

**Techniques:**
- Burst compilation
- Job parallelization
- Native collections
- Query optimization
- Caching strategies

### UI Development

**Creating custom interfaces**

**Approaches:**
- Use existing UI bindings (easiest)
- Mod UI frameworks (medium)
- Coherent UI (advanced)
- Custom rendering (expert)

---

## 📞 Community & Support

### Official Resources

**Paradox Forums**
- Modding section
- Official announcements
- Developer interaction

**Cities: Skylines 2 Website**
- Game updates
- Official mod support info

### Community

**Discord Servers**
- Cities: Skylines modding
- Game dev communities
- Unity DOTS developers

**Reddit**
- r/CitiesSkylines
- r/gamedev
- r/Unity3D

**GitHub**
- Example mods
- Tool repositories
- Documentation projects

### Getting Help

**Before asking:**
1. Read relevant documentation
2. Check SYSTEM_TRACKING_MATRIX for status
3. Search existing issues/forums
4. Try debugging yourself

**When asking:**
1. Describe what you're trying to do
2. Show what you've tried
3. Include error messages
4. Provide code snippets
5. Mention game version

---

## 📜 License & Legal

### Reverse Engineering

**Legal status**: Reverse engineering for interoperability (modding) is generally legal in most jurisdictions.

**Do NOT:**
- Redistribute game code
- Claim ownership of game code
- Use findings for piracy
- Violate EULA

**DO:**
- Use findings for modding
- Share knowledge
- Build tools
- Contribute to community

### Documentation License

This documentation: **MIT License**
- Free to use
- Free to modify
- Free to share
- Attribution appreciated

### Mod Code

Check Paradox's modding terms for specifics.

Generally:
- ✅ Free mods
- ✅ Donation-supported
- ❌ Paid mods (usually prohibited)
- ⚠️ Check official policy

---

## 🔄 Version History

### 2025-11-24 - Initial Complete Documentation
- Created comprehensive documentation set
- Analyzed NetToolSystem, PrefabSystem
- Documented road creation API
- Built EnhancedGridMod example
- Created learning guides
- Established tracking matrix

### Future Updates

**Planned:**
- Zone system complete analysis
- Building system documentation
- UI system deep dive
- Prefab enumeration tool
- Advanced examples

**Check git history for detailed changes**

---

## 🎯 Project Goals

### Short Term (1 month)

- [ ] Verify all road prefab names
- [ ] Complete lane system RE
- [ ] Document zone spawn logic
- [ ] Build prefab browser tool
- [ ] Create video tutorials

### Medium Term (3 months)

- [ ] Complete zone system RE
- [ ] Complete building system RE
- [ ] Advanced UI integration
- [ ] Terrain tool RE
- [ ] Community mod library

### Long Term (6+ months)

- [ ] Comprehensive API documentation
- [ ] Full simulation system RE
- [ ] Advanced tool framework
- [ ] Mod development SDK
- [ ] Video course series

---

## 📚 Glossary

**ECS** - Entity Component System. Unity's data-oriented architecture.

**Entity** - A unique ID referencing a game object.

**Component** - Data attached to entities (pure structs).

**System** - Logic that processes components.

**Prefab** - Template/blueprint for entities.

**Harmony** - Library for runtime code patching.

**Burst** - Unity's optimizing compiler.

**Job** - Parallel work unit in Unity's job system.

**Query** - Request for entities matching criteria.

**Archetype** - Set of component types.

**Chunk** - Memory block storing entities.

**DOTS** - Data-Oriented Technology Stack.

---

## 🚀 Quick Start Commands

```powershell
# Set up environment
$env:CSII_TOOLPATH = "C:\...\Cities Skylines II\Cities2_Data\Managed"

# Create new mod project
dotnet new classlib -n MyMod

# Build mod
dotnet build

# Install mod
Copy-Item "bin\Debug\net7.0\MyMod.dll" `
    "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\"

# View logs (real-time)
Get-Content "...\Player.log" -Wait -Tail 50
```

---

## 📖 Recommended Reading Order

**For absolute beginners:**
1. GETTING_STARTED_TOOL_DEVELOPMENT.md
2. Build the examples
3. CS2_ROAD_API.md (basics)
4. Experiment!

**For developers:**
1. REVERSE_ENGINEERING_MASTER_GUIDE.md
2. DECOMPILATION_WORKFLOW.md
3. SYSTEM_TRACKING_MATRIX.md
4. Pick a system to study
5. Build something

**For reverse engineers:**
1. All guides
2. SYSTEM_TRACKING_MATRIX.md (find gaps)
3. Decompile Game.dll
4. Choose unexplored system
5. Document findings

---

**Happy Modding! 🎮🏙️**

---

*This is a living document. Contributions welcome!*

*Last major update: 2025-11-24*
