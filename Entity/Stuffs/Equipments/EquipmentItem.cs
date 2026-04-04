using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Stuffs.Equipments
{
    public class EquipmentItem : StuffItem
    {
        public int Attack { get; set; } = 0;
        public int Defense { get; set; } = 0;
    }
}