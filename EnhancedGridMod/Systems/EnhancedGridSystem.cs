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
            m_Harmony.PatchAll();

            Mod.log.Info("Harmony patches applied successfully");
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
