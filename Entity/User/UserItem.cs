using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.User
{
    public class UserItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StatisticsItem StatisticsItem { get; set; }
        public LoginStatsItem LoginStatsItem { get; set; }
    }
}