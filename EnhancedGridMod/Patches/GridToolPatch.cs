using HarmonyLib;
using Game.Tools;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Game.Net;
using Game.Common;
using Game.Prefabs;
using Colossal.Mathematics;
using System.Reflection;
using System.Linq;

namespace EnhancedGrid.Patches
{
    /// <summary>
    /// Harmony patch for NetToolSystem.CreateDefinitionsJob.CreateGrid method
    /// Enhances the grid tool with manual grid count and arterial road spacing
    /// </summary>
    [HarmonyPatch]
    public static class GridToolPatch
    {
        private static Entity s_ArterialPrefab = Entity.Null;
        private static Entity s_LocalPrefab = Entity.Null;

        /// <summary>
        /// Manually specify target method for better diagnostics
        /// </summary>
        static MethodBase TargetMethod()
        {
            var jobType = typeof(NetToolSystem.CreateDefinitionsJob);
            Mod.log.Info($"Looking for CreateGrid in type: {jobType.FullName}");

            var methods = jobType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Mod.log.Info($"Found {methods.Length} methods in CreateDefinitionsJob");

            foreach (var method in methods)
            {
                Mod.log.Info($"  Method: {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
            }

            var createGridMethod = jobType.GetMethod("CreateGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (createGridMethod == null)
            {
                Mod.log.Error("Could not find CreateGrid method! Available methods listed above.");
                return null;
            }

            Mod.log.Info($"Found CreateGrid: {createGridMethod.Name}");
            var parameters = createGridMethod.GetParameters();
            Mod.log.Info($"  Parameters: {string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}");

            return createGridMethod;
        }

        /// <summary>
        /// Prefix patch intercepts the original CreateGrid method
        /// </summary>
        static bool Prefix(
            ref NetToolSystem.CreateDefinitionsJob __instance,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions)
        {
            // DIAGNOSTIC: Log that patch is being called
            Mod.log.Info("🎯 GridToolPatch.Prefix called!");

            var settings = Mod.Settings;

            if (settings == null)
            {
                Mod.log.Error("Settings is null!");
                return true;
            }

            Mod.log.Info($"UseManualGridCount: {settings.UseManualGridCount}, GridX: {settings.GridX}, GridY: {settings.GridY}");

            // Only override if manual mode enabled
            if (!settings.UseManualGridCount)
            {
                Mod.log.Info("Manual mode disabled, using original grid generation");
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
                ref __instance,
                ref ownerDefinitions,
                gridCount,
                arterialSpacing
            );

            return false; // Skip original method
        }

        /// <summary>
        /// Enhanced grid creation implementation using EntityCommandBuffer
        /// Based on original CreateGrid but with manual count and arterial support
        /// </summary>
        static void EnhancedCreateGrid(
            ref NetToolSystem.CreateDefinitionsJob job,
            ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions,
            int2 gridCount,
            int arterialSpacing)
        {
            Mod.log.Info("Enhanced grid generation started");

            // Get control points
            var controlPoints = job.m_ControlPoints;
            if (controlPoints.Length < 2)
            {
                Mod.log.Warn("Not enough control points");
                return;
            }

            ControlPoint point1 = controlPoints[0];  // Grid origin
            ControlPoint point2 = controlPoints[1];  // Primary direction
            ControlPoint point3 = controlPoints[controlPoints.Length - 1];  // Grid extent

            // Calculate grid dimensions
            float3 primaryVector = point2.m_Position - point1.m_Position;
            float3 secondaryVector = point3.m_Position - point2.m_Position;

            float primaryLength = math.length(primaryVector);
            float secondaryLength = math.length(secondaryVector);

            float3 primaryDir = math.normalize(primaryVector);
            float3 secondaryDir = math.normalize(secondaryVector);

            // Calculate spacing
            float2 spacing = new float2(
                primaryLength / math.max(1, gridCount.x),
                secondaryLength / math.max(1, gridCount.y)
            );

            Mod.log.Info($"Grid spacing: {spacing.x:F2}m x {spacing.y:F2}m");

            // Initialize random
            Unity.Mathematics.Random random = job.m_RandomSeed.GetRandom(0);

            // Create horizontal roads (along primary direction)
            for (int y = 0; y <= gridCount.y; y++)
            {
                float yOffset = y * spacing.y;

                for (int x = 0; x < gridCount.x; x++)
                {
                    float xStart = x * spacing.x;
                    float xEnd = (x + 1) * spacing.x;

                    float3 startPos = point1.m_Position + secondaryDir * yOffset + primaryDir * xStart;
                    float3 endPos = point1.m_Position + secondaryDir * yOffset + primaryDir * xEnd;

                    bool isArterial = (arterialSpacing > 0) && (y % arterialSpacing == 0);
                    Entity prefab = isArterial ? s_ArterialPrefab : s_LocalPrefab;

                    CreateRoadEntity(
                        ref job,
                        startPos,
                        endPos,
                        prefab,
                        random.NextInt(),
                        isFirst: (x == 0),
                        isLast: (x == gridCount.x - 1),
                        isParallel: (y != 0)
                    );
                }
            }

            // Create vertical roads (along secondary direction)
            for (int x = 0; x <= gridCount.x; x++)
            {
                float xOffset = x * spacing.x;

                for (int y = 0; y < gridCount.y; y++)
                {
                    float yStart = y * spacing.y;
                    float yEnd = (y + 1) * spacing.y;

                    float3 startPos = point1.m_Position + primaryDir * xOffset + secondaryDir * yStart;
                    float3 endPos = point1.m_Position + primaryDir * xOffset + secondaryDir * yEnd;

                    bool isArterial = (arterialSpacing > 0) && (x % arterialSpacing == 0);
                    Entity prefab = isArterial ? s_ArterialPrefab : s_LocalPrefab;

                    CreateRoadEntity(
                        ref job,
                        startPos,
                        endPos,
                        prefab,
                        random.NextInt(),
                        isFirst: (y == 0),
                        isLast: (y == gridCount.y - 1),
                        isParallel: (x != 0)
                    );
                }
            }

            int totalRoads = (gridCount.x) * (gridCount.y + 1) + (gridCount.y) * (gridCount.x + 1);
            Mod.log.Info($"Enhanced grid generation completed: {gridCount.x}x{gridCount.y} = {totalRoads} road segments");
        }

        /// <summary>
        /// Creates a single road entity using the EntityCommandBuffer pattern
        /// Based on the decompiled CreateGrid implementation
        /// </summary>
        static void CreateRoadEntity(
            ref NetToolSystem.CreateDefinitionsJob job,
            float3 startPos,
            float3 endPos,
            Entity prefab,
            int randomSeed,
            bool isFirst,
            bool isLast,
            bool isParallel)
        {
            // 1. CREATE ENTITY
            Entity e = job.m_CommandBuffer.CreateEntity();

            // 2. CREATE AND ADD CreationDefinition COMPONENT
            CreationDefinition creationDef = new CreationDefinition
            {
                m_Prefab = prefab,
                m_SubPrefab = job.m_LanePrefab,
                m_RandomSeed = randomSeed
            };
            creationDef.m_Flags |= CreationFlags.SubElevation;
            job.m_CommandBuffer.AddComponent(e, creationDef);

            // 3. ADD Updated COMPONENT
            job.m_CommandBuffer.AddComponent(e, default(Updated));

            // 4. CREATE NetCourse COMPONENT
            NetCourse netCourse = default(NetCourse);

            // Create curve
            netCourse.m_Curve = NetUtils.StraightCurve(startPos, endPos);

            // Create course positions (simplified - using basic setup)
            netCourse.m_StartPosition = new CoursePos
            {
                m_Entity = Entity.Null,
                m_SplitPosition = 0f,
                m_Position = startPos,
                m_Rotation = quaternion.LookRotationSafe(math.normalize(endPos - startPos), math.up()),
                m_Elevation = startPos.y,
                m_Flags = CoursePosFlags.IsGrid,
                m_ParentMesh = -1
            };

            netCourse.m_EndPosition = new CoursePos
            {
                m_Entity = Entity.Null,
                m_SplitPosition = 1f,
                m_Position = endPos,
                m_Rotation = quaternion.LookRotationSafe(math.normalize(endPos - startPos), math.up()),
                m_Elevation = endPos.y,
                m_Flags = CoursePosFlags.IsGrid,
                m_ParentMesh = -1
            };

            // Set additional flags
            if (isParallel)
            {
                netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsParallel;
                netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsParallel;
            }

            if (isFirst)
            {
                netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
            }

            if (isLast)
            {
                netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
            }

            // Set length and fixed index
            netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
            netCourse.m_FixedIndex = -1;

            // 5. ADD NetCourse COMPONENT TO ENTITY
            job.m_CommandBuffer.AddComponent(e, netCourse);

            // Note: OwnerDefinition is optional and typically only added for buildings
            // Skipping it for now as grids are usually standalone
        }
    }
}
