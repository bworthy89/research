using Game;
using HarmonyLib;
using Unity.Entities;

namespace EnhancedGrid.Systems
{
    public partial class EnhancedGridSystem : GameSystemBase
    {
        private Harmony m_Harmony;

        protected override void OnCreate()
        {
            base.OnCreate();

            // Apply Harmony patches
            m_Harmony = new Harmony("com.enhancedgrid.mod");

            try
            {
                m_Harmony.PatchAll();
                Mod.log.Info("Harmony patches applied successfully");

                // Diagnostic: List all patches
                var patches = m_Harmony.GetPatchedMethods();
                int count = 0;
                foreach (var method in patches)
                {
                    count++;
                    Mod.log.Info($"Patched method: {method.DeclaringType?.Name}.{method.Name}");
                }
                Mod.log.Info($"Total patches applied: {count}");

                if (count == 0)
                {
                    Mod.log.Error("WARNING: No Harmony patches were applied! The mod will not function.");
                }
            }
            catch (System.Exception ex)
            {
                Mod.log.Error($"Failed to apply Harmony patches: {ex.Message}");
                Mod.log.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnDestroy()
        {
            if (m_Harmony != null)
            {
                m_Harmony.UnpatchAll(m_Harmony.Id);
                m_Harmony = null;
            }

            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            // Not needed for Harmony-only approach
        }
    }
}
