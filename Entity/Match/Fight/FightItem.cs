using Bastocos.Entity.Match.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Match.Assault
{
    public class FightItem
    {
        public FightStats FightStats { get; set; }
        public Fighter FighterA { get; set; }
        public Fighter FighterB { get; set; }

        internal int GetRemainingWebTime(Admin.Settings.SettingsItem globalSettingsItems)
        {
            int maxDuration = globalSettingsItems.MatchSettings.DefaultsMatchSettings.MaxDuration;

            if (maxDuration <= 0)
                return 100;

            DateTime start = FightStats.StartTime;
            DateTime end = start.AddMinutes(maxDuration);

            DateTime now = DateTime.Now;

            double total = (end - start).TotalSeconds;
            double elapsed = (now - start).TotalSeconds;

            double percent = (elapsed / total) * 100.0;

            // Clamp manuel
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            return (int)percent;
        }
    }
}