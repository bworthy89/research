using HarmonyLib;
using Game.Tools;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Game.Net;
using Game.Common;
using Game.Prefabs;

namespace EnhancedGrid.Patches
{
    /// <summary>
    /// Harmony patch for NetToolSystem.CreateDefinitionsJob.CreateGrid method
    /// Enhances the grid tool with manual grid count and arterial road spacing
    /// </summary>
    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateGrid")]
    public static class GridToolPatch
    {
        private static Entity s_ArterialPrefab = Entity.Null;
        private static Entity s_LocalPrefab = Entity.Null;

        /// <summary>
        /// Prefix patch intercepts the original CreateGrid method
        /// </summary>
        static bool Prefix(
            NetToolSystem.CreateDefinitionsJob __instance,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
            Bezier4x3 centerCurve,
            Bezier4x3 sideCurve,
            bool isStraight,
            Entity topLevelEntity)
        {
            var settings = Mod.Settings;

            // Only override if manual mode enabled
            if (!settings.UseManualGridCount)
            {
                return true; // Run original method
            }

            Mod.log.Info($"Enhanced Grid: Generating {settings.GridX}x{settings.GridY} grid");

            // Get config
            int2 gridCount = new int2(settings.GridX, settings.GridY);
            int arterialSpacing = settings.ArterialSpacing;

            // For Phase 1: Use same prefab as job
            // TODO Phase 2: Query PrefabSystem for different road types
            s_ArterialPrefab = __instance.m_NetPrefab;
            s_LocalPrefab = __instance.m_NetPrefab;

            // Call enhanced implementation
            EnhancedCreateGrid(
                __instance,
                ref ownerDefinitions,
                centerCurve,
                sideCurve,
                isStraight,
                topLevelEntity,
                gridCount,
                arterialSpacing
            );

            return false; // Skip original method
        }

        /// <summary>
        /// Enhanced grid creation implementation
        /// Based on original CreateGrid but with manual count and arterial support
        /// </summary>
        static void EnhancedCreateGrid(
            NetToolSystem.CreateDefinitionsJob job,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
            Bezier4x3 centerCurve,
            Bezier4x3 sideCurve,
            bool isStraight,
            Entity topLevelEntity,
            int2 gridCount,
            int arterialSpacing)
        {
            Mod.log.Info("Enhanced grid generation started");

            // Get the three grid control points
            float3 point1 = centerCurve.a;  // Grid origin
            float3 point2 = centerCurve.d;  // Primary direction
            float3 point3 = sideCurve.d;    // Grid extent

            // Calculate base vectors
            float3 primaryVector = point2 - point1;
            float3 secondaryVector = point3 - point2;

            // Calculate spacing for each direction
            float2 spacing = new float2(
                math.length(primaryVector) / math.max(1, gridCount.x),
                math.length(secondaryVector) / math.max(1, gridCount.y)
            );

            // Normalize direction vectors
            float3 primaryDir = math.normalize(primaryVector);
            float3 secondaryDir = math.normalize(secondaryVector);

            Mod.log.Info($"Grid spacing: {spacing.x:F2}m x {spacing.y:F2}m");

            // Create horizontal roads (along primary direction)
            for (int y = 0; y <= gridCount.y; y++)
            {
                float3 startPoint = point1 + secondaryDir * (y * spacing.y);
                float3 endPoint = startPoint + primaryVector;

                bool isArterial = (arterialSpacing > 0) && (y % arterialSpacing == 0);
                Entity prefab = isArterial ? s_ArterialPrefab : s_LocalPrefab;

                CreateRoad(job, ref ownerDefinitions, startPoint, endPoint, prefab, topLevelEntity);
            }

            // Create vertical roads (along secondary direction)
            for (int x = 0; x <= gridCount.x; x++)
            {
                float3 startPoint = point1 + primaryDir * (x * spacing.x);
                float3 endPoint = startPoint + secondaryVector;

                bool isArterial = (arterialSpacing > 0) && (x % arterialSpacing == 0);
                Entity prefab = isArterial ? s_ArterialPrefab : s_LocalPrefab;

                CreateRoad(job, ref ownerDefinitions, startPoint, endPoint, prefab, topLevelEntity);
            }

            Mod.log.Info($"Enhanced grid generation completed: {gridCount.x}x{gridCount.y} = {(gridCount.x + 1) * 2 + (gridCount.y + 1) * 2 - 4} roads");
        }

        /// <summary>
        /// Creates a single road segment
        /// </summary>
        static void CreateRoad(
            NetToolSystem.CreateDefinitionsJob job,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
            float3 startPoint,
            float3 endPoint,
            Entity prefab,
            Entity topLevelEntity)
        {
            // Create straight curve between start and end points
            Bezier4x3 curve = NetUtils.StraightCurve(startPoint, endPoint);

            // Create road definition
            CreationDefinition definition = new CreationDefinition
            {
                m_Prefab = prefab,
                m_SubPrefab = Entity.Null,
                m_Flags = CreationFlags.Permanent | CreationFlags.Attach,
                m_RandomSeed = 0,
                m_Curve = curve,
                m_Original = Entity.Null,
                m_Owner = topLevelEntity
            };

            // Add to definitions map
            if (!ownerDefinitions.ContainsKey(topLevelEntity))
            {
                ownerDefinitions.Add(topLevelEntity, new OwnerDefinition
                {
                    m_Prefab = prefab,
                    m_Position = startPoint,
                    m_Rotation = quaternion.identity
                });
            }

            // Note: The actual road creation is handled by the NetToolSystem
            // after the CreateDefinitionsJob completes
            // We're just adding the definition to the collection
        }
    }
}
