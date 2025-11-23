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
using Unity.Jobs;

namespace EnhancedGrid.Patches
{
    /// <summary>
    /// Harmony patch for NetToolSystem.UpdateCourse method
    /// Enhances the grid tool by injecting custom control points before job execution
    ///
    /// NOTE: We patch UpdateCourse (class method) instead of CreateGrid (struct method)
    /// because Harmony cannot reliably patch struct methods.
    /// </summary>
    [HarmonyPatch(typeof(NetToolSystem), "UpdateCourse")]
    public static class GridToolPatch
    {
        private static FieldInfo m_ControlPointsField;
        private static FieldInfo m_ModeField;
        private static bool s_ReflectionInitialized = false;

        /// <summary>
        /// Initialize reflection to access private fields
        /// </summary>
        static void InitializeReflection()
        {
            if (s_ReflectionInitialized) return;

            var netToolSystemType = typeof(NetToolSystem);
            m_ControlPointsField = netToolSystemType.GetField("m_ControlPoints",
                BindingFlags.NonPublic | BindingFlags.Instance);
            m_ModeField = netToolSystemType.GetField("m_Mode",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (m_ControlPointsField == null)
                Mod.log.Error("Could not find m_ControlPoints field!");
            if (m_ModeField == null)
                Mod.log.Error("Could not find m_Mode field!");

            s_ReflectionInitialized = true;
            Mod.log.Info("GridToolPatch reflection initialized");
        }

        /// <summary>
        /// Prefix patch intercepts UpdateCourse before grid creation
        /// Signature: private JobHandle UpdateCourse(JobHandle inputDeps, bool removeUpgrade)
        /// </summary>
        static void Prefix(
            NetToolSystem __instance,
            ref JobHandle inputDeps,
            bool removeUpgrade)
        {
            InitializeReflection();

            var settings = Mod.Settings;
            if (settings == null || !settings.UseManualGridCount)
                return; // Let original run

            // Get current mode using reflection
            if (m_ModeField == null) return;
            var mode = (NetToolSystem.Mode)m_ModeField.GetValue(__instance);

            if (mode != NetToolSystem.Mode.Grid)
                return; // Only intercept Grid mode

            // Get actual mode property to handle upgradeOnly case
            var actualMode = __instance.actualMode;
            if (actualMode != NetToolSystem.Mode.Grid)
                return;

            Mod.log.Info($"🎯 GridToolPatch intercepting Grid mode: {settings.GridX}x{settings.GridY}");

            // Get control points
            if (m_ControlPointsField == null) return;

            // NativeList is a struct (value type), so we need to unbox it directly
            var controlPointsObj = m_ControlPointsField.GetValue(__instance);
            if (controlPointsObj == null)
            {
                Mod.log.Warn("Control points field returned null");
                return;
            }

            var controlPoints = (NativeList<ControlPoint>)controlPointsObj;
            if (!controlPoints.IsCreated)
            {
                Mod.log.Warn("Control points not initialized");
                return;
            }

            if (controlPoints.Length < 3)
            {
                Mod.log.Info($"Not enough control points for grid ({controlPoints.Length}), letting original handle it");
                return; // Need 3 points for grid mode
            }

            Mod.log.Info($"Original control points: {controlPoints.Length}");
            // The grid generation in the job will use the existing control points
            // We're just logging here - actual grid customization will come in next phase
        }

    }
}
