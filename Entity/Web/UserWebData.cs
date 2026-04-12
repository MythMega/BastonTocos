using Bastocos.Entity.Cards;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Entity.Web.UserWebExportAlbum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Entity.Web
{
    public class UserWebData
    {
        public UserItem User { get; set; } = new UserItem();
        public List<CardAlbumEntrie> Cards { get; set; } = [];
        public List<TrashAlbumEntrie> Trashs { get; set; } = [];
        public List<ArmorAlbumEntrie> Armors { get; set; } = [];
        public List<WeaponAlbumEntrie> Weapons { get; set; } = [];
    }
}