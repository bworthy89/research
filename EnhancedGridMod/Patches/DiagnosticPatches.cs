using HarmonyLib;
using Game.Tools;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Game.Net;
using Game.Common;
using Colossal.Mathematics;
using System.Reflection;

namespace EnhancedGrid.Patches
{
    /// <summary>
    /// Diagnostic patches to identify which method is actually called for grid creation
    /// </summary>

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateStraightLine")]
    public static class CreateStraightLinePatch
    {
        static bool Prefix(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, int2 gridSize)
        {
            Mod.log.Info($"🟢 CreateStraightLine called! gridSize: {gridSize.x}x{gridSize.y}");
            return true; // Let original run for now
        }
    }

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateCourseObject")]
    public static class CreateCourseObjectPatch
    {
        static void Prefix()
        {
            Mod.log.Info("🟡 CreateCourseObject called!");
        }
    }

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateParallelCourse")]
    public static class CreateParallelCoursePatch
    {
        static void Prefix()
        {
            Mod.log.Info("🟠 CreateParallelCourse called!");
        }
    }

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateComplexCurve")]
    public static class CreateComplexCurvePatch
    {
        static void Prefix()
        {
            Mod.log.Info("🔵 CreateComplexCurve called!");
        }
    }

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "CreateSimpleCurve")]
    public static class CreateSimpleCurvePatch
    {
        static void Prefix()
        {
            Mod.log.Info("🟣 CreateSimpleCurve called!");
        }
    }

    [HarmonyPatch(typeof(NetToolSystem.CreateDefinitionsJob), "Execute")]
    public static class ExecutePatch
    {
        static void Prefix()
        {
            Mod.log.Info("⚫ CreateDefinitionsJob.Execute called!");
        }
    }
}
