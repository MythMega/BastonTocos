using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin
{
    public class ItemSet
    {
        public List<Trashitem> Trashitems { get; set; }
        public List<ArmorItem> ArmorItems { get; set; }
        public List<WeaponItem> WeaponItems { get; set; }
    }
}