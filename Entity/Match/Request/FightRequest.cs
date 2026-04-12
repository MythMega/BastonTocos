using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Match.Request
{
    public class FightRequest
    {
        public DateTime DateRequest { get; set; } = DateTime.Now;
        public RequestStatut RequestStatut { get; set; } = RequestStatut.Treatment_Request;
        public FightType FightType { get; set; } = FightType.Assault;
        public UserItem UserItemA { get; set; } = new UserItem();
        public UserItem? UserItemB { get; set; }

        // l'utilisateur invité a duel
        public string UserToDuel { get; set; } = "Unrecognized String for usertoduel";
    }
}