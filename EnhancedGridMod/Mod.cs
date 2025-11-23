using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;

namespace EnhancedGrid
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EnhancedGrid)}.{nameof(Mod)}")
            .SetShowsErrorsInUI(false);

        public static Setting Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            // Load settings
            Settings = new Setting(this);
            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            AssetDatabase.global.LoadSettings(nameof(EnhancedGrid), Settings, new Setting(this));

            // Register our system
            updateSystem.UpdateAt<Systems.EnhancedGridSystem>(SystemUpdatePhase.ModificationEnd);

            log.Info("Enhanced Grid Tool loaded successfully");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
