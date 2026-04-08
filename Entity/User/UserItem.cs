using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.User
{
    public class UserItem
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = "Unset Name";
        public string Avatar { get; set; } = "https://upload.wikimedia.org/wikipedia/commons/5/59/Empty.png";
        public StatisticsItem StatisticsItem { get; set; } = new StatisticsItem();
        public LoginStatsItem LoginStatsItem { get; set; } = new LoginStatsItem();
    }
}