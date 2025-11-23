using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using System.Collections.Generic;

namespace EnhancedGrid
{
    [FileLocation(nameof(EnhancedGrid))]
    [SettingsUIGroupOrder(kGridGroup, kRoadGroup)]
    [SettingsUIShowGroupName(kGridGroup, kRoadGroup)]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kGridGroup = "Grid Settings";
        public const string kRoadGroup = "Road Types";

        public Setting(IMod mod) : base(mod)
        {
        }

        [SettingsUISection(kSection, kGridGroup)]
        public bool UseManualGridCount { get; set; } = false;

        [SettingsUISlider(min = 1, max = 50, step = 1, scalarMultiplier = 1)]
        [SettingsUISection(kSection, kGridGroup)]
        public int GridX { get; set; } = 10;

        [SettingsUISlider(min = 1, max = 50, step = 1, scalarMultiplier = 1)]
        [SettingsUISection(kSection, kGridGroup)]
        public int GridY { get; set; } = 10;

        [SettingsUISlider(min = 0, max = 10, step = 1, scalarMultiplier = 1)]
        [SettingsUISection(kSection, kRoadGroup)]
        public int ArterialSpacing { get; set; } = 4;

        [SettingsUISection(kSection, kGridGroup)]
        public bool EnableBlockSizeVariation { get; set; } = false;

        [SettingsUISlider(min = 0, max = 50, step = 5, scalarMultiplier = 1)]
        [SettingsUISection(kSection, kGridGroup)]
        public int BlockVariationPercent { get; set; } = 0;

        public override void SetDefaults()
        {
            UseManualGridCount = false;
            GridX = 10;
            GridY = 10;
            ArterialSpacing = 4;
            EnableBlockSizeVariation = false;
            BlockVariationPercent = 0;
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Enhanced Grid Tool" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kGridGroup), "Grid Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kRoadGroup), "Road Types" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.UseManualGridCount)), "Use Manual Grid Count" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.UseManualGridCount)), "Override automatic grid calculation with manual values" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.GridX)), "Grid Width (blocks)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.GridX)), "Number of blocks horizontally" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.GridY)), "Grid Height (blocks)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.GridY)), "Number of blocks vertically" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ArterialSpacing)), "Arterial Road Spacing" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ArterialSpacing)), "Place arterial road every N blocks (0 = no arterials)" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableBlockSizeVariation)), "Enable Block Variation" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableBlockSizeVariation)), "Add randomization to block sizes for organic feel" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BlockVariationPercent)), "Variation Amount (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.BlockVariationPercent)), "How much blocks can vary in size" },
            };
        }

        public void Unload()
        {
        }
    }
}
