# Cities: Skylines 2 - Getting Started with Tool Development

> **Goal**: Go from zero to your first working custom tool in one day

**Prerequisites**: Basic C# knowledge, Cities: Skylines 2 installed

---

## Table of Contents

1. [Environment Setup (30 minutes)](#1-environment-setup)
2. [Understanding the Basics (1 hour)](#2-understanding-the-basics)
3. [Your First Mod (1 hour)](#3-your-first-mod)
4. [Building a Simple Tool (2-3 hours)](#4-building-a-simple-tool)
5. [Testing & Debugging (30 minutes)](#5-testing--debugging)
6. [Next Steps](#6-next-steps)

---

## 1. Environment Setup

### Install Required Software

#### A. Development Tools

**Visual Studio 2022 Community** (Free)
```
Download: https://visualstudio.microsoft.com/
Workloads:
  ☑ .NET desktop development
  ☑ Game development with Unity
```

**OR JetBrains Rider** (Paid, but excellent)
```
Download: https://www.jetbrains.com/rider/
```

#### B. .NET 7.0 SDK

```
Download: https://dotnet.microsoft.com/download/dotnet/7.0
Install: x64 version
```

Verify installation:
```bash
dotnet --version
# Should show: 7.0.x
```

#### C. Decompilation Tool

**ILSpy (Recommended)**
```
Download: https://github.com/icsharpcode/ILSpy/releases
Choose: ILSpy_binaries_*.zip
Extract and run ILSpy.exe
```

### Set Up Cities: Skylines 2 Mod SDK

#### Find Your Game Installation

**Windows (Steam):**
```
C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II
```

**Windows (Epic):**
```
C:\Program Files\Epic Games\CitiesSkylines2
```

**Set Environment Variable:**

1. Open PowerShell as Administrator:
```powershell
[System.Environment]::SetEnvironmentVariable(
    'CSII_TOOLPATH',
    'C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\Managed',
    'User'
)
```

2. Verify:
```powershell
[System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'User')
```

#### Create Mod Development Folder

```powershell
# Create your workspace
New-Item -ItemType Directory -Path "C:\CS2Mods\MyFirstTool"
cd "C:\CS2Mods\MyFirstTool"
```

---

## 2. Understanding the Basics

### Core Concepts (5-Minute Version)

#### Entity Component System (ECS)

**Think of it like a spreadsheet:**

| Entity (ID) | Node (Position) | Edge (Start, End) | Road (Flags) |
|-------------|-----------------|-------------------|--------------|
| 1 | (0,0,0) | - | - |
| 2 | (10,0,0) | - | - |
| 3 | - | Start:1, End:2 | IsLit |

- **Entities**: Just IDs (like row numbers)
- **Components**: Data columns
- **Systems**: Code that processes rows

#### Prefabs vs Entities

**Prefab** = Template (like a cookie cutter)
```
RoadPrefab "Small Road"
- Width: 8m
- Lanes: 2
- Speed: 50 km/h
```

**Entity** = Instance (like a cookie)
```
Road Entity #12345
- Position: (100, 0, 50)
- Uses prefab: "Small Road"
```

#### Systems

**Systems** = Where your code runs

```csharp
public partial class MySystem : SystemBase {
    protected override void OnUpdate() {
        // This runs every frame
    }
}
```

### Key Game DLLs

Located in: `<GameRoot>\Cities2_Data\Managed\`

| DLL | Contains | Importance |
|-----|----------|------------|
| **Game.dll** | All game logic | ⭐⭐⭐⭐⭐ |
| Colossal.Core.dll | Core framework | ⭐⭐⭐⭐ |
| Unity.Entities.dll | ECS system | ⭐⭐⭐⭐ |
| Unity.Mathematics.dll | Math library | ⭐⭐⭐ |

---

## 3. Your First Mod

### Project Structure

```
MyFirstTool/
├── MyFirstTool.csproj       # Project file
├── Mod.cs                   # Entry point
└── Systems/
    └── HelloWorldSystem.cs  # Your code
```

### Step 1: Create Project File

**MyFirstTool.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net7.0</TargetFramework>
        <LangVersion>latest</LangVersion>
        <Configurations>Debug;Release</Configurations>
    </PropertyGroup>

    <!-- Import official mod build system -->
    <Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.props" />
    <Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.targets" />

    <!-- Game references -->
    <ItemGroup>
        <Reference Include="Game"><Private>false</Private></Reference>
        <Reference Include="Colossal.Core"><Private>false</Private></Reference>
        <Reference Include="Colossal.Logging"><Private>false</Private></Reference>
        <Reference Include="Unity.Entities"><Private>false</Private></Reference>
        <Reference Include="Unity.Mathematics"><Private>false</Private></Reference>
        <Reference Include="UnityEngine.CoreModule"><Private>false</Private></Reference>

        <!-- Harmony for code patching -->
        <PackageReference Include="Lib.Harmony" Version="2.3.3" />
    </ItemGroup>
</Project>
```

### Step 2: Create Entry Point

**Mod.cs:**
```csharp
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace MyFirstTool
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger("MyFirstTool")
            .SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info("MyFirstTool loading...");

            // Register our system
            updateSystem.UpdateAt<Systems.HelloWorldSystem>(
                SystemUpdatePhase.ModificationEnd
            );

            log.Info("MyFirstTool loaded successfully!");
        }

        public void OnDispose()
        {
            log.Info("MyFirstTool disposed");
        }
    }
}
```

### Step 3: Create Your First System

**Systems/HelloWorldSystem.cs:**
```csharp
using Colossal.Logging;
using Game;
using Unity.Entities;

namespace MyFirstTool.Systems
{
    public partial class HelloWorldSystem : GameSystemBase
    {
        private ILog log = LogManager.GetLogger("MyFirstTool");
        private bool hasRun = false;

        protected override void OnCreate()
        {
            base.OnCreate();
            log.Info("HelloWorldSystem created!");
        }

        protected override void OnUpdate()
        {
            // Only log once to avoid spam
            if (!hasRun)
            {
                log.Info("Hello, Cities: Skylines 2!");
                hasRun = true;
            }
        }
    }
}
```

### Step 4: Build and Test

**Build:**
```bash
cd C:\CS2Mods\MyFirstTool
dotnet build
```

**Output location:**
```
bin\Debug\net7.0\MyFirstTool.dll
```

**Install mod:**
```powershell
# Copy to game's mod folder
Copy-Item "bin\Debug\net7.0\MyFirstTool.dll" `
    "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\"
```

**Test:**
1. Launch Cities: Skylines 2
2. Check main menu (mod should load)
3. Check logs:
```
%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

**Expected output:**
```
MyFirstTool loading...
HelloWorldSystem created!
MyFirstTool loaded successfully!
Hello, Cities: Skylines 2!
```

✅ **Success!** You've created your first mod!

---

## 4. Building a Simple Tool

### Project: "Road Counter Tool"

**Goal**: Create a tool that counts all roads in the city when activated

### Step 1: Create Tool System

**Systems/RoadCounterSystem.cs:**
```csharp
using Colossal.Logging;
using Game.Tools;
using Game.Net;
using Unity.Entities;
using Unity.Collections;

namespace MyFirstTool.Systems
{
    public partial class RoadCounterSystem : ToolBaseSystem
    {
        private ILog log = LogManager.GetLogger("RoadCounter");
        private EntityQuery m_RoadQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            // Create query for all roads
            m_RoadQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Road>(),
                    ComponentType.ReadOnly<Edge>(),
                    ComponentType.ReadOnly<Curve>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });

            log.Info("RoadCounterSystem created");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            log.Info("Road Counter tool activated!");
            CountRoads();
        }

        protected override void OnUpdate()
        {
            // Tool doesn't need per-frame updates for this simple example
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            log.Info("Road Counter tool deactivated");
        }

        private void CountRoads()
        {
            // Get all road entities
            NativeArray<Entity> roads = m_RoadQuery.ToEntityArray(Allocator.Temp);

            log.Info($"Total roads in city: {roads.Length}");

            // Count road types (if we had that data)
            int straightCount = 0;
            int curvedCount = 0;

            foreach (var roadEntity in roads)
            {
                if (EntityManager.TryGetComponent<Curve>(roadEntity, out var curve))
                {
                    // Simple heuristic: if curve length ~= straight distance, it's straight
                    float straightDist = Unity.Mathematics.math.distance(
                        curve.m_Bezier.a,
                        curve.m_Bezier.d
                    );

                    if (curve.m_Length / straightDist < 1.1f)
                        straightCount++;
                    else
                        curvedCount++;
                }
            }

            log.Info($"  Straight roads: {straightCount}");
            log.Info($"  Curved roads: {curvedCount}");

            // Clean up
            roads.Dispose();
        }
    }
}
```

### Step 2: Update Mod Entry Point

**Mod.cs** (update OnLoad):
```csharp
public void OnLoad(UpdateSystem updateSystem)
{
    log.Info("MyFirstTool loading...");

    // Register systems
    updateSystem.UpdateAt<Systems.HelloWorldSystem>(
        SystemUpdatePhase.ModificationEnd
    );

    updateSystem.UpdateAt<Systems.RoadCounterSystem>(
        SystemUpdatePhase.ToolUpdate
    );

    log.Info("MyFirstTool loaded successfully!");
}
```

### Step 3: Build and Test

```bash
dotnet build
Copy-Item "bin\Debug\net7.0\MyFirstTool.dll" `
    "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\" -Force
```

**Test in-game:**
1. Load a city with roads
2. Activate any tool (to trigger tool systems)
3. Check Player.log

**Expected output:**
```
Road Counter tool activated!
Total roads in city: 127
  Straight roads: 89
  Curved roads: 38
```

---

## 5. Testing & Debugging

### Logging Best Practices

**Different log levels:**
```csharp
log.Debug("Detailed debug info");    // Only in debug builds
log.Info("Normal information");      // General info
log.Warn("Something unusual");       // Warnings
log.Error("Something failed");       // Errors
```

**Structured logging:**
```csharp
log.Info($"Processing {count} entities in {time:F2}ms");
```

**Conditional logging:**
```csharp
if (someCondition)
{
    log.Info("Condition met!");
}
```

### Common Issues & Solutions

#### Issue: Mod Not Loading

**Symptoms**: No log messages appear

**Checklist:**
- [ ] DLL copied to correct folder?
- [ ] `CSII_TOOLPATH` environment variable set?
- [ ] Game restarted after copying DLL?
- [ ] Check for compilation errors

**Debug:**
```csharp
// Add to Mod.OnLoad()
try {
    log.Info("Starting load...");
    // Your code
    log.Info("Load complete!");
} catch (Exception e) {
    log.Error($"Load failed: {e}");
}
```

#### Issue: NullReferenceException

**Symptoms**: Crash or system doesn't work

**Common causes:**
```csharp
// WRONG: Entity might not have component
var curve = EntityManager.GetComponentData<Curve>(entity);

// RIGHT: Check first
if (EntityManager.TryGetComponent<Curve>(entity, out var curve)) {
    // Use curve safely
}
```

#### Issue: "Component type not registered"

**Solution**: Make sure component exists in query:
```csharp
// If you query for it, entities MUST have it
m_Query = GetEntityQuery(
    ComponentType.ReadOnly<RequiredComponent>()
);
```

### Viewing Logs

**Windows:**
```
%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

**Tail logs in real-time (PowerShell):**
```powershell
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Player.log" -Wait -Tail 50
```

**Filter for your mod:**
```powershell
Get-Content "Player.log" | Select-String "MyFirstTool"
```

---

## 6. Next Steps

### Level Up Your Mod

Now that you have the basics, try these progressively harder challenges:

#### Beginner Challenges

1. **Road Length Calculator**
   - Sum total length of all roads
   - Calculate average road length
   - Find longest/shortest road

2. **Building Counter**
   - Count total buildings
   - Categorize by type (residential/commercial/industrial)
   - Find tallest building

3. **City Statistics**
   - Count zones
   - Count intersections (nodes with >2 connections)
   - Calculate grid regularity

#### Intermediate Challenges

4. **Simple Road Tool**
   - Extend NetToolSystem using Harmony
   - Add a custom parameter (e.g., road spacing)
   - Log when roads are created

5. **Pattern Detector**
   - Find grid patterns in existing cities
   - Detect radial layouts
   - Identify disconnected road networks

6. **Road Upgrader**
   - Find all "Small Road" instances
   - Patch to upgrade to "Medium Road"
   - Add UI trigger

#### Advanced Challenges

7. **Grid Generator**
   - Create full grid from scratch
   - Use PrefabSystem to select road types
   - Implement road hierarchy (arterial/local)

8. **Neighborhood Generator**
   - Generate grid
   - Auto-zone blocks
   - Spawn initial buildings

9. **Custom UI Panel**
   - Create parameter controls
   - Add preview rendering
   - Integrate with game UI

### Study Existing Code

**Recommended reverse engineering targets:**

**Week 1-2: Foundation**
- NetToolSystem (road creation)
- PrefabSystem (asset management)
- Basic components (Node, Edge, Curve)

**Week 3-4: Tools**
- ZoneToolSystem (zoning)
- TerrainToolSystem (landscaping)
- Tool UI bindings

**Week 5-6: Simulation**
- ZoneSpawnSystem (building spawning)
- TransportLineSystem (public transit)
- EconomySystem (if interested)

### Join the Community

**Resources:**
- Cities: Skylines 2 Modding Discord
- Reddit: r/CitiesSkylines
- Paradox Forums: Modding section

**Share your work:**
- GitHub repositories
- Steam Workshop (when available)
- YouTube tutorials

### Recommended Reading

**Official Documentation:**
- [Unity DOTS Manual](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Unity Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@latest)
- [Harmony Documentation](https://harmony.pardeike.net/)

**Community Resources:**
- Reverse engineered API docs (this repository)
- Example mods on GitHub
- Video tutorials

---

## Complete Example: Grid Road Tool

### Full Project Structure

```
SimpleGridTool/
├── SimpleGridTool.csproj
├── Mod.cs
└── Systems/
    └── SimpleGridToolSystem.cs
```

### SimpleGridToolSystem.cs

```csharp
using Colossal.Logging;
using Game.Tools;
using Game.Net;
using Game.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
using HarmonyLib;

namespace SimpleGridTool.Systems
{
    // Harmony patch to modify grid creation
    [HarmonyPatch(typeof(NetToolSystem), "CreateGrid")]
    public class GridPatch
    {
        private static ILog log = LogManager.GetLogger("SimpleGridTool");

        static void Prefix(NetToolSystem __instance)
        {
            log.Info("Creating grid with custom logic!");
            // Your custom grid logic here
        }
    }

    public partial class SimpleGridToolSystem : GameSystemBase
    {
        private ILog log = LogManager.GetLogger("SimpleGridTool");

        protected override void OnCreate()
        {
            base.OnCreate();

            // Apply Harmony patches
            var harmony = new Harmony("com.example.simplegridtool");
            harmony.PatchAll();

            log.Info("SimpleGridToolSystem created and patched!");
        }

        protected override void OnUpdate()
        {
            // System update logic if needed
        }

        // Helper method for custom grid creation
        public void CreateCustomGrid(
            float3 origin,
            int gridX,
            int gridZ,
            float spacing)
        {
            log.Info($"Custom grid: {gridX}x{gridZ}, spacing: {spacing}m");
            // Implementation here
        }
    }
}
```

### Build Script

**build.ps1:**
```powershell
# Build and install script
$ModName = "MyFirstTool"
$GameModPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods"

Write-Host "Building $ModName..." -ForegroundColor Green
dotnet build -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Installing mod..." -ForegroundColor Green

    Copy-Item "bin\Debug\net7.0\$ModName.dll" $GameModPath -Force

    Write-Host "Mod installed! Launch the game to test." -ForegroundColor Cyan
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
```

**Usage:**
```powershell
.\build.ps1
```

---

## Troubleshooting Guide

### Build Errors

#### "CSII_TOOLPATH not found"

```powershell
# Set environment variable
[System.Environment]::SetEnvironmentVariable(
    'CSII_TOOLPATH',
    'C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\Managed',
    'User'
)

# Restart Visual Studio/Rider
```

#### "Reference 'Game' not found"

**Check:**
1. Environment variable points to correct path
2. `Game.dll` exists in that path
3. Path has no typos

#### "Mod.props not found"

**Cause**: Official mod SDK not set up correctly

**Solution**: Reinstall game or manually create Mod.props/Mod.targets

### Runtime Errors

#### Mod loads but doesn't work

**Check Player.log for:**
```
Exception: System.TypeLoadException
```

**Common causes:**
- Wrong .NET version (need 7.0)
- Missing dependencies
- Conflicting mods

#### Game crashes on startup

**Debug process:**
1. Remove mod DLL
2. Verify game works
3. Add mod back
4. Check logs for exception before crash

---

## Success Checklist

By the end of this guide, you should have:

- [x] Development environment set up
- [x] First "Hello World" mod working
- [x] Understanding of ECS basics
- [x] Road counter tool working
- [x] Logs showing your mod's output
- [x] Confidence to build more!

---

## What's Next?

**You're now ready to:**

1. Read the **REVERSE_ENGINEERING_MASTER_GUIDE.md** for deeper knowledge
2. Study **NetToolSystem** in ILSpy
3. Use **DECOMPILATION_WORKFLOW.md** to analyze specific systems
4. Check **SYSTEM_TRACKING_MATRIX.md** for RE progress
5. Build your own custom tool!

---

**Welcome to Cities: Skylines 2 modding! 🎉**

**Questions?** Check the community Discord or forums.

**Built something cool?** Share it with the community!

---

**Last Updated**: 2025-11-24
