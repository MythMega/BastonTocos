using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin.Settings
{
    public class OverlaySettings
    {
        public DefaultImages DefaultImages { get; set; }
        public string HexColorLifebar { get; set; }
        public double RefreshRateSeconds { get; set; }
    }
}