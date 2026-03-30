using Bastocos.Entity.Stuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity
{
    public class BasicItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public int LootChance { get; set; }
        public int SellValue { get; set; }
        public int BuyValue { get; set; }
    }
}