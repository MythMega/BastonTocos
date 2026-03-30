using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin.Database
{
    public class VersionsItem
    {
        public int DatabaseVersion { get; set; }
        public DateTime LastDatabaseUpdate { get; set; }
        public int AppVersion { get; set; }
    }
}