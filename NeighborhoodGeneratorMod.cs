using Game.Tools;
using Game.Net;
using Game.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Colossal.Mathematics;

namespace NeighborhoodGenerator
{
    /// <summary>
    /// CS2 Mod: Neighborhood Generator
    /// Generates road layouts programmatically
    /// </summary>
    public partial class NeighborhoodGeneratorSystem : ToolBaseSystem
    {
        private EntityQuery m_RoadPrefabQuery;
        private PrefabSystem m_PrefabSystem;

        // Configuration
        public struct GeneratorConfig
        {
            public float3 Origin;
            public int BlocksX;
            public int BlocksZ;
            public float BlockWidth;
            public float BlockDepth;
            public float ArterialSpacing;  // 0 = no arterials
            public PatternType Pattern;
        }

        public enum PatternType
        {
            Grid,
            AngledGrid,
            Radial,
            Organic
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            // Get prefab system reference
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            // Don't override default tool
            Enabled = false;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        /// <summary>
        /// Main entry point - generates neighborhood from config
        /// </summary>
        public void GenerateNeighborhood(GeneratorConfig config)
        {
            // Get appropriate road prefabs
            Entity smallRoadPrefab = GetRoadPrefab("Small Road");
            Entity mediumRoadPrefab = GetRoadPrefab("Medium Road");
            Entity largeRoadPrefab = GetRoadPrefab("Large Road");

            // Create entity command buffer for deferred entity creation
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            switch (config.Pattern)
            {
                case PatternType.Grid:
                    GenerateGridPattern(config, ecb, smallRoadPrefab, largeRoadPrefab);
                    break;
                case PatternType.AngledGrid:
                    GenerateAngledGridPattern(config, ecb, smallRoadPrefab, largeRoadPrefab);
                    break;
                case PatternType.Radial:
                    GenerateRadialPattern(config, ecb, smallRoadPrefab, mediumRoadPrefab, largeRoadPrefab);
                    break;
                case PatternType.Organic:
                    GenerateOrganicPattern(config, ecb, smallRoadPrefab, mediumRoadPrefab);
                    break;
            }

            // Execute all entity creation commands
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Generates classic American grid pattern
        /// </summary>
        private void GenerateGridPattern(
            GeneratorConfig config,
            EntityCommandBuffer ecb,
            Entity localRoadPrefab,
            Entity arterialRoadPrefab)
        {
            float3 origin = config.Origin;
            float arterialSpacing = config.ArterialSpacing;

            // Generate vertical roads
            for (int x = 0; x <= config.BlocksX; x++)
            {
                float xPos = x * config.BlockWidth;
                bool isArterial = arterialSpacing > 0 &&
                                 math.abs(xPos % arterialSpacing) < 0.1f &&
                                 xPos > 0;

                Entity prefab = isArterial ? arterialRoadPrefab : localRoadPrefab;

                float3 start = origin + new float3(xPos, 0, 0);
                float3 end = start + new float3(0, 0, config.BlocksZ * config.BlockDepth);

                CreateStraightRoad(ecb, start, end, prefab);
            }

            // Generate horizontal roads
            for (int z = 0; z <= config.BlocksZ; z++)
            {
                float zPos = z * config.BlockDepth;
                bool isArterial = arterialSpacing > 0 &&
                                 math.abs(zPos % arterialSpacing) < 0.1f &&
                                 zPos > 0;

                Entity prefab = isArterial ? arterialRoadPrefab : localRoadPrefab;

                float3 start = origin + new float3(0, 0, zPos);
                float3 end = start + new float3(config.BlocksX * config.BlockWidth, 0, 0);

                CreateStraightRoad(ecb, start, end, prefab);
            }
        }

        /// <summary>
        /// Generates 45-degree angled grid
        /// </summary>
        private void GenerateAngledGridPattern(
            GeneratorConfig config,
            EntityCommandBuffer ecb,
            Entity localRoadPrefab,
            Entity arterialRoadPrefab)
        {
            float3 origin = config.Origin;
            float diagonal = config.BlockWidth * 1.414f; // sqrt(2)

            int numRoads = config.BlocksX + config.BlocksZ;

            // Diagonal roads (one direction)
            for (int i = -numRoads; i <= numRoads; i++)
            {
                float offset = i * diagonal;

                float3 start = origin + new float3(offset, 0, 0);
                float3 end = origin + new float3(
                    offset + config.BlocksX * config.BlockWidth,
                    0,
                    config.BlocksZ * config.BlockDepth
                );

                CreateStraightRoad(ecb, start, end, localRoadPrefab);
            }

            // Diagonal roads (perpendicular direction)
            for (int i = -numRoads; i <= numRoads; i++)
            {
                float offset = i * diagonal;

                float3 start = origin + new float3(offset, 0, config.BlocksZ * config.BlockDepth);
                float3 end = origin + new float3(
                    offset + config.BlocksX * config.BlockWidth,
                    0,
                    0
                );

                CreateStraightRoad(ecb, start, end, localRoadPrefab);
            }
        }

        /// <summary>
        /// Generates radial (hub and spoke) pattern
        /// </summary>
        private void GenerateRadialPattern(
            GeneratorConfig config,
            EntityCommandBuffer ecb,
            Entity localRoadPrefab,
            Entity collectorRoadPrefab,
            Entity arterialRoadPrefab)
        {
            float3 center = config.Origin + new float3(
                config.BlocksX * config.BlockWidth * 0.5f,
                0,
                config.BlocksZ * config.BlockDepth * 0.5f
            );

            float maxRadius = math.min(
                config.BlocksX * config.BlockWidth,
                config.BlocksZ * config.BlockDepth
            ) * 0.5f;

            // Radial roads (spokes)
            int numRadials = 8;
            for (int i = 0; i < numRadials; i++)
            {
                float angle = (i / (float)numRadials) * math.PI * 2f;
                float3 direction = new float3(math.cos(angle), 0, math.sin(angle));
                float3 end = center + direction * maxRadius;

                CreateStraightRoad(ecb, center, end, arterialRoadPrefab);
            }

            // Concentric rings
            int numRings = 4;
            for (int ring = 1; ring <= numRings; ring++)
            {
                float radius = (maxRadius / numRings) * ring;
                Entity prefab = ring % 2 == 0 ? collectorRoadPrefab : localRoadPrefab;

                CreateCircularRoad(ecb, center, radius, prefab, 32);
            }
        }

        /// <summary>
        /// Generates organic/curved pattern (simplified version)
        /// </summary>
        private void GenerateOrganicPattern(
            GeneratorConfig config,
            EntityCommandBuffer ecb,
            Entity localRoadPrefab,
            Entity collectorRoadPrefab)
        {
            // Simplified organic - creates wavy horizontal roads
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(12345);

            for (int z = 0; z <= config.BlocksZ; z++)
            {
                float zBase = z * config.BlockDepth;
                NativeList<float3> points = new NativeList<float3>(Allocator.Temp);

                // Create wavy path with variation
                for (int x = 0; x <= config.BlocksX * 4; x++)
                {
                    float xPos = (x / 4f) * config.BlockWidth;
                    float zVariation = random.NextFloat(-10f, 10f);

                    points.Add(config.Origin + new float3(xPos, 0, zBase + zVariation));
                }

                // Connect points with curves
                for (int i = 0; i < points.Length - 1; i++)
                {
                    CreateStraightRoad(ecb, points[i], points[i + 1], localRoadPrefab);
                }

                points.Dispose();
            }
        }

        /// <summary>
        /// Creates a straight road between two points
        /// </summary>
        private void CreateStraightRoad(
            EntityCommandBuffer ecb,
            float3 startPos,
            float3 endPos,
            Entity roadPrefab)
        {
            // Calculate direction for rotation
            float3 direction = math.normalize(endPos - startPos);

            // 1. Create start node
            Entity startNode = ecb.CreateEntity();
            ecb.AddComponent(startNode, new Node
            {
                m_Position = startPos,
                m_Rotation = quaternion.LookRotationSafe(direction, new float3(0, 1, 0))
            });

            // 2. Create end node
            Entity endNode = ecb.CreateEntity();
            ecb.AddComponent(endNode, new Node
            {
                m_Position = endPos,
                m_Rotation = quaternion.LookRotationSafe(-direction, new float3(0, 1, 0))
            });

            // 3. Create edge
            Entity edge = ecb.CreateEntity();
            ecb.AddComponent(edge, new Edge
            {
                m_Start = startNode,
                m_End = endNode
            });

            // 4. Create Bezier curve
            Bezier4x3 bezier = NetUtils.StraightCurve(startPos, endPos);
            ecb.AddComponent(edge, new Curve
            {
                m_Bezier = bezier,
                m_Length = MathUtils.Length(bezier)
            });

            // 5. Add composition (links to prefab)
            ecb.AddComponent(edge, new Composition
            {
                m_Edge = edge,
                m_StartNode = startNode,
                m_EndNode = endNode
            });

            // 6. Add road component
            ecb.AddComponent(edge, new Road
            {
                m_Flags = RoadFlags.None
            });

            // TODO: Copy components from prefab
            // This is needed for proper road rendering
            // CopyPrefabComponents(ecb, roadPrefab, edge);
        }

        /// <summary>
        /// Creates a circular road (ring)
        /// </summary>
        private void CreateCircularRoad(
            EntityCommandBuffer ecb,
            float3 center,
            float radius,
            Entity roadPrefab,
            int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * math.PI * 2f;
                float angle2 = ((i + 1) / (float)segments) * math.PI * 2f;

                float3 pos1 = center + new float3(
                    math.cos(angle1) * radius,
                    0,
                    math.sin(angle1) * radius
                );

                float3 pos2 = center + new float3(
                    math.cos(angle2) * radius,
                    0,
                    math.sin(angle2) * radius
                );

                CreateStraightRoad(ecb, pos1, pos2, roadPrefab);
            }
        }

        /// <summary>
        /// Gets a road prefab by name
        /// </summary>
        private Entity GetRoadPrefab(string name)
        {
            // TODO: Implement prefab lookup
            // This would query the PrefabSystem for roads by name
            // For now, return Entity.Null (needs implementation)
            return Entity.Null;
        }

        protected override void InitializeRaycast()
        {
            base.InitializeRaycast();
            // Configure what the tool can raycast to
            // m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // This would be triggered by UI button or keybind
            // For now, just return
            return inputDeps;
        }
    }
}
