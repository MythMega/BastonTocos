using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin.Settings
{
    public class DefaultsMatchSettings
    {
        public int DefaultHP { get; set; }
        public int DefaultDamage { get; set; }
        public int DefaultArmor { get; set; }
        public int MissChancePercentage { get; set; }
        public int CritChancePercentage { get; set; }
        public int CancelMatchForAFKTimeMinute { get; set; }
        public int MaxDuration { get; set; }
    }
}