# Enhanced Grid Tool - Official Modding Toolchain Implementation

## Official CS2 Modding Approach

### Key Differences from BepInEx
- ✅ Uses `GameSystemBase` instead of `BaseUnityPlugin`
- ✅ Installed via Modding Toolchain (automated setup)
- ✅ Deployed to: `%USERPROFILE%/AppData/LocalLow/Colossal Order/Cities Skylines II/Mods/`
- ✅ **Can still use HarmonyX** for patching (it's built-in!)
- ✅ Visual Studio 2022 (17.8+) or Rider (2021.3.3+) required

## Implementation Options

### Option A: GameSystemBase + HarmonyX (RECOMMENDED)

**Use official mod structure with Harmony patching**

```csharp
using Game;
using Game.Tools;
using HarmonyLib;

public class EnhancedGridMod : GameSystemBase {
    private Harmony m_Harmony;

    protected override void OnCreate() {
        base.OnCreate();

        // Apply Harmony patches
        m_Harmony = new Harmony("com.yourname.enhancedgrid");
        m_Harmony.PatchAll();

        Mod.log.Info("Enhanced Grid Tool loaded!");
    }

    protected override void OnDestroy() {
        // Cleanup
        m_Harmony?.UnpatchAll();
        base.OnDestroy();
    }

    protected override void OnUpdate() {
        // Not needed for Harmony-only approach
    }
}

// Harmony patch (same as before!)
[HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
public class EnhancedGridPatch {
    static bool Prefix(...) {
        // Your custom grid implementation
        return false;
    }
}
```

**Advantages:**
- ✅ Official modding support
- ✅ Same Harmony patching we planned
- ✅ Auto-updates through mod system
- ✅ Best of both worlds

### Option B: Pure GameSystemBase (Complex)

**Create new tool system from scratch**

```csharp
using Game;
using Game.Tools;
using Unity.Entities;

public class EnhancedGridToolSystem : ToolBaseSystem {
    protected override void OnCreate() {
        base.OnCreate();
        // Initialize tool
    }

    protected override void OnStartRunning() {
        base.OnStartRunning();
        // Tool activated
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        // Tool logic each frame
        return inputDeps;
    }
}
```

**Advantages:**
- ✅ Full control
- ✅ No patching needed

**Disadvantages:**
- ❌ Must reimplement entire tool
- ❌ Complex UI integration
- ❌ Duplicate NetToolSystem code

### Option C: System That Modifies NetToolSystem

**Monitor and modify NetToolSystem parameters**

```csharp
public class GridEnhancerSystem : GameSystemBase {
    private NetToolSystem m_NetToolSystem;

    protected override void OnCreate() {
        base.OnCreate();
        m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
    }

    protected override void OnUpdate() {
        // Monitor NetToolSystem
        if (m_NetToolSystem.mode == NetToolSystem.Mode.Grid) {
            // Modify parameters before grid is created
            // Problem: Can't modify grid count calculation directly
        }
    }
}
```

**Disadvantages:**
- ❌ Can't intercept CreateGrid method
- ❌ Limited control
- ❌ Not useful for our needs

---

## RECOMMENDED APPROACH: Option A

**Use official GameSystemBase + HarmonyX patching**

### Project Structure

```
EnhancedGrid/
├── EnhancedGrid.csproj
├── Mod.cs                  # Main mod class (GameSystemBase)
├── Config.cs               # Configuration storage
├── Patches/
│   └── GridToolPatch.cs   # Harmony patches
└── Systems/
    └── PrefabLoader.cs    # Helper for loading road prefabs
```

### Complete Implementation

#### 1. Mod.cs (Entry Point)

```csharp
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;

namespace EnhancedGrid {

    public class Mod : IMod {
        public static IModSettings Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem) {
            Settings = new EnhancedGridSettings(this);
            Settings.RegisterInOptionsUI();
            Settings.RegisterKeyBindings();

            updateSystem.UpdateAt<EnhancedGridSystem>(SystemUpdatePhase.ModificationEnd);
        }

        public void OnDispose() {
        }
    }

    public partial class EnhancedGridSystem : GameSystemBase {
        private Harmony m_Harmony;

        protected override void OnCreate() {
            base.OnCreate();

            // Apply Harmony patches
            m_Harmony = new Harmony("com.yourname.enhancedgrid");
            m_Harmony.PatchAll();

            Mod.log.Info("Enhanced Grid Tool initialized");
        }

        protected override void OnDestroy() {
            m_Harmony?.UnpatchAll();
            base.OnDestroy();
        }

        protected override void OnUpdate() {
            // Not needed - using Harmony only
        }
    }
}
```

#### 2. Config.cs (Settings)

```csharp
using Game.Settings;

namespace EnhancedGrid {

    [FileLocation(nameof(EnhancedGrid))]
    public class EnhancedGridSettings : ModSetting {

        public EnhancedGridSettings(IMod mod) : base(mod) {
        }

        [SettingsUISection("Grid")]
        public bool UseManualGridCount { get; set; } = false;

        [SettingsUISlider(min = 1, max = 50, step = 1, scalarMultiplier = 1)]
        [SettingsUISection("Grid")]
        public int GridX { get; set; } = 10;

        [SettingsUISlider(min = 1, max = 50, step = 1, scalarMultiplier = 1)]
        [SettingsUISection("Grid")]
        public int GridY { get; set; } = 10;

        [SettingsUISlider(min = 0, max = 10, step = 1, scalarMultiplier = 1)]
        [SettingsUISection("Grid")]
        public int ArterialSpacing { get; set; } = 4;

        public override void SetDefaults() {
            UseManualGridCount = false;
            GridX = 10;
            GridY = 10;
            ArterialSpacing = 4;
        }
    }
}
```

#### 3. Patches/GridToolPatch.cs (Harmony)

```csharp
using HarmonyLib;
using Game.Tools;
using Unity.Mathematics;
using Unity.Collections;

namespace EnhancedGrid.Patches {

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
    public class GridToolPatch {

        private static Entity s_ArterialPrefab;
        private static Entity s_LocalPrefab;

        static bool Prefix(
            NetToolSystem.CreateDefinitionsJob __instance,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions) {

            var settings = Mod.Settings as EnhancedGridSettings;

            // Only override if manual mode enabled
            if (!settings.UseManualGridCount) {
                return true; // Run original
            }

            // Get config
            int2 gridCount = new int2(settings.GridX, settings.GridY);
            int arterialSpacing = settings.ArterialSpacing;

            // Load prefabs if needed
            if (s_ArterialPrefab == Entity.Null || s_LocalPrefab == Entity.Null) {
                LoadPrefabs(__instance);
            }

            // Call our enhanced version
            EnhancedCreateGrid(
                __instance,
                ref ownerDefinitions,
                gridCount,
                arterialSpacing
            );

            return false; // Skip original
        }

        static void LoadPrefabs(NetToolSystem.CreateDefinitionsJob job) {
            // Use the same prefab as job for now
            // TODO: Query PrefabSystem for different road types
            s_ArterialPrefab = job.m_NetPrefab;
            s_LocalPrefab = job.m_NetPrefab;
        }

        static void EnhancedCreateGrid(
            NetToolSystem.CreateDefinitionsJob job,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
            int2 gridCount,
            int arterialSpacing) {

            // Copy original CreateGrid implementation
            // Modify to:
            // 1. Use our gridCount instead of auto-calculated
            // 2. Select prefab based on position (arterial vs local)

            // Simplified version:
            for (int2 cell = new int2(0, 0); cell.y < gridCount.y; cell.y++) {
                for (cell.x = 0; cell.x < gridCount.x; cell.x++) {

                    // Determine if this is an arterial road
                    bool isArterial = (arterialSpacing > 0) &&
                                      ((cell.x % arterialSpacing == 0) ||
                                       (cell.y % arterialSpacing == 0));

                    Entity prefab = isArterial ? s_ArterialPrefab : s_LocalPrefab;

                    // Create road with selected prefab
                    // (Copy logic from original CreateGrid)
                }
            }
        }
    }
}
```

#### 4. .csproj (Project File)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>netstandard2.1</TargetFramework>
    <Configurations>Debug;Release</Configurations>
    <LangVersion>latest</LangVersion>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <Optimize>false</Optimize>
    <DebugType>full</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <DefineConstants>DEBUG</DefineConstants>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <Optimize>true</Optimize>
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="Game">
      <Private>false</Private>
    </Reference>
    <Reference Include="Colossal.Core">
      <Private>false</Private>
    </Reference>
    <Reference Include="Colossal.Logging">
      <Private>false</Private>
    </Reference>
    <Reference Include="Colossal.IO.AssetDatabase">
      <Private>false</Private>
    </Reference>
    <Reference Include="Colossal.UI">
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.Entities">
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.Mathematics">
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.Collections">
      <Private>false</Private>
    </Reference>
    <Reference Include="0Harmony">
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <Target Name="DeployToGame" AfterTargets="Build">
    <PropertyGroup>
      <DeployDir>$(USERPROFILE)\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\$(MSBuildProjectName)</DeployDir>
    </PropertyGroup>
    <ItemGroup>
      <FilesToCopy Include="$(TargetDir)**\*.*" />
    </ItemGroup>
    <Copy SourceFiles="@(FilesToCopy)" DestinationFolder="$(DeployDir)\%(RecursiveDir)" />
  </Target>
</Project>
```

---

## Setup Instructions

### 1. Install Modding Toolchain

**In-game:**
1. Open Cities: Skylines II
2. Go to Options → Modding
3. Click "Install" to set up the toolchain
4. Restart game when prompted

### 2. Create Project

**In Visual Studio/Rider:**
1. New Project → "Cities: Skylines II Mod"
2. Name: "EnhancedGrid"
3. Template auto-configures references

### 3. Add Code

Copy the files above into your project

### 4. Build & Test

```
Build → Deploy runs automatically
Launch CS2 → Check Mods menu
Enable "Enhanced Grid"
```

---

## Advantages of Official Modding

✅ **Auto-deployment** to mods folder
✅ **Settings UI** built-in (sliders, checkboxes)
✅ **Mod management** through game UI
✅ **Auto-updates** for users
✅ **HarmonyX included** (same patching power!)
✅ **Official support** from Paradox

---

## Next Steps

1. **Set up toolchain** (5 min)
2. **Create project** from template (2 min)
3. **Copy patch code** (10 min)
4. **Build & test** (5 min)

**Total:** ~22 minutes to working prototype!

**Want me to help set this up?**

---

## Sources

- [Official Modding Toolchain](https://cs2.paradoxwikis.com/Modding_Toolchain)
- [Code Modding Dev Diary](https://www.paradoxinteractive.com/games/cities-skylines-ii/modding/dev-diary-3-code-modding)
- [FPS Limiter Example (GameSystemBase)](https://github.com/krzychu124/FPS_Limiter/blob/main/FPSLimiterSystem.cs)
- [Cities2Modding Guide](https://github.com/optimus-code/Cities2Modding)
