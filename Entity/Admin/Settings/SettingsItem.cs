using System;

namespace Bastocos.Entity.Admin.Settings
{
    public class SettingsItem
    {
        public bool LogEverything { get; set; }
        public bool LogFile { get; set; }
        public int ServerPort { get; set; }
        public MatchSettings MatchSettings { get; set; }
        public LootRatesPercentages LootRatesPercentages { get; set; }
        public ModifierRate ModifierRate { get; set; }
        public OverlaySettings OverlaySettings { get; set; }
        public CommandSettings CommandSettings { get; set; }
    }
}