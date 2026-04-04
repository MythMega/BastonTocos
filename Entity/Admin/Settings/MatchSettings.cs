using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin.Settings
{
    public class MatchSettings
    {
        public int DelayBetweenTwoMatch { get; set; }
        public DefaultsMatchSettings DefaultsMatchSettings { get; set; }
    }
}