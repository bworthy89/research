using Colossal.Logging;
using Game;
using Game.Prefabs;
using Unity.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PrefabBrowser.Systems
{
    /// <summary>
    /// Enumerates all prefabs in the game at startup
    /// Logs categorized lists to help reverse engineering efforts
    /// </summary>
    public partial class PrefabEnumerationSystem : GameSystemBase
    {
        private ILog log = LogManager.GetLogger("PrefabBrowser");
        private PrefabSystem m_PrefabSystem;
        private bool m_HasEnumerated = false;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            log.Info("PrefabEnumerationSystem created");
        }

        protected override void OnUpdate()
        {
            // Only enumerate once at startup
            if (!m_HasEnumerated && m_PrefabSystem != null)
            {
                m_HasEnumerated = true;
                EnumeratePrefabs();
            }
        }

        private void EnumeratePrefabs()
        {
            log.Info("===================================");
            log.Info("STARTING PREFAB ENUMERATION");
            log.Info("===================================");

            try
            {
                // Access internal prefabs list via reflection or public API
                var prefabs = GetAllPrefabs();

                if (prefabs == null || prefabs.Count == 0)
                {
                    log.Warn("No prefabs found! PrefabSystem may not be initialized yet.");
                    return;
                }

                log.Info($"Total prefabs found: {prefabs.Count}");
                log.Info("");

                // Categorize and log
                EnumerateRoadPrefabs(prefabs);
                EnumerateBuildingPrefabs(prefabs);
                EnumerateVehiclePrefabs(prefabs);
                EnumerateZonePrefabs(prefabs);
                EnumerateOtherPrefabs(prefabs);

                log.Info("===================================");
                log.Info("PREFAB ENUMERATION COMPLETE");
                log.Info("===================================");
            }
            catch (System.Exception e)
            {
                log.Error($"Error during prefab enumeration: {e.Message}");
                log.Error($"Stack trace: {e.StackTrace}");
            }
        }

        private List<PrefabBase> GetAllPrefabs()
        {
            // Try to access prefabs via internal property
            var prefabsProperty = typeof(PrefabSystem).GetProperty("prefabs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (prefabsProperty != null)
            {
                var prefabs = prefabsProperty.GetValue(m_PrefabSystem) as System.Collections.IEnumerable;
                return prefabs?.Cast<PrefabBase>().ToList();
            }

            log.Warn("Could not access PrefabSystem.prefabs property");
            return new List<PrefabBase>();
        }

        private void EnumerateRoadPrefabs(List<PrefabBase> allPrefabs)
        {
            log.Info("--- ROAD PREFABS ---");

            var roadPrefabs = allPrefabs.OfType<RoadPrefab>().ToList();
            log.Info($"Found {roadPrefabs.Count} road prefabs:");

            var groupedByType = roadPrefabs
                .GroupBy(r => r.m_RoadType)
                .OrderBy(g => g.Key);

            foreach (var group in groupedByType)
            {
                log.Info($"\n  {group.Key} Roads:");
                foreach (var road in group.OrderBy(r => r.name))
                {
                    var zoneInfo = road.m_ZoneBlock != null ? "[ZONABLE]" : "";
                    var highwayInfo = road.m_HighwayRules ? "[HIGHWAY]" : "";
                    var trafficLightInfo = road.m_TrafficLights ? "[LIGHTS]" : "";

                    log.Info($"    - {road.name,-40} Speed: {road.m_SpeedLimit,3}km/h {zoneInfo} {highwayInfo} {trafficLightInfo}");

                    // Try to get width info
                    var prefabEntity = m_PrefabSystem.GetEntity(road);
                    if (EntityManager.TryGetComponent<NetGeometryData>(prefabEntity, out var geomData))
                    {
                        log.Info($"      Width: {geomData.m_DefaultWidth:F1}m");
                    }
                }
            }

            log.Info("");
        }

        private void EnumerateBuildingPrefabs(List<PrefabBase> allPrefabs)
        {
            log.Info("--- BUILDING PREFABS ---");

            var buildingPrefabs = allPrefabs.OfType<BuildingPrefab>().ToList();
            log.Info($"Found {buildingPrefabs.Count} building prefabs");

            // Sample first 50 for brevity
            log.Info("Sample (first 50):");
            foreach (var building in buildingPrefabs.Take(50))
            {
                log.Info($"  - {building.name}");
            }

            // Group by type
            var residential = allPrefabs.OfType<ResidentialBuildingPrefab>().Count();
            var commercial = allPrefabs.OfType<CommercialBuildingPrefab>().Count();
            var industrial = allPrefabs.OfType<IndustrialBuildingPrefab>().Count();

            log.Info($"\nBy Type:");
            log.Info($"  Residential: {residential}");
            log.Info($"  Commercial: {commercial}");
            log.Info($"  Industrial: {industrial}");
            log.Info("");
        }

        private void EnumerateVehiclePrefabs(List<PrefabBase> allPrefabs)
        {
            log.Info("--- VEHICLE PREFABS ---");

            var vehiclePrefabs = allPrefabs.OfType<VehiclePrefab>().ToList();
            log.Info($"Found {vehiclePrefabs.Count} vehicle prefabs");

            // Sample first 30
            log.Info("Sample (first 30):");
            foreach (var vehicle in vehiclePrefabs.Take(30))
            {
                log.Info($"  - {vehicle.name}");
            }
            log.Info("");
        }

        private void EnumerateZonePrefabs(List<PrefabBase> allPrefabs)
        {
            log.Info("--- ZONE PREFABS ---");

            var zonePrefabs = allPrefabs.OfType<ZonePrefab>().ToList();
            log.Info($"Found {zonePrefabs.Count} zone prefabs:");

            foreach (var zone in zonePrefabs)
            {
                log.Info($"  - {zone.name}");
            }
            log.Info("");
        }

        private void EnumerateOtherPrefabs(List<PrefabBase> allPrefabs)
        {
            log.Info("--- PREFAB TYPE SUMMARY ---");

            var typeGroups = allPrefabs
                .GroupBy(p => p.GetType().Name)
                .OrderByDescending(g => g.Count());

            foreach (var group in typeGroups.Take(20))
            {
                log.Info($"  {group.Key,-40} : {group.Count(),5} prefabs");
            }
            log.Info("");
        }
    }
}
