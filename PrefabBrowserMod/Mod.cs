using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace PrefabBrowser
{
    /// <summary>
    /// Prefab Browser Mod - Enumerates all available prefabs at runtime
    /// Logs all RoadPrefabs, BuildingPrefabs, etc. to help reverse engineering
    /// </summary>
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger("PrefabBrowser")
            .SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info("===================================");
            log.Info("Prefab Browser Mod Loading...");
            log.Info("===================================");

            // Register our enumeration system
            updateSystem.UpdateAt<Systems.PrefabEnumerationSystem>(
                SystemUpdatePhase.ModificationEnd
            );

            log.Info("Prefab Browser loaded successfully!");
            log.Info("Check Player.log for prefab listings");
        }

        public void OnDispose()
        {
            log.Info("Prefab Browser disposed");
        }
    }
}
