using Bastocos.Entity.Cards;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Tools.Json
{
    internal class WebFullElementData
    {
        public List<CardItem> Cards { get; set; }
        public List<ArmorItem> Armors { get; set; }
        public List<WeaponItem> Weapons { get; set; }
        public List<Trashitem> Trashs { get; set; }
    }
}