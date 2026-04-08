using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Tools
{
    public class DefaultElements
    {
        public WeaponItem DefaultWeaponItem { get; set; } = new WeaponItem
        {
            Name = "Main nues",
            Attack = 1,
            BuyValue = 0,
            Defense = 0,
            Description = "Il n'a pas d'arme le bougre.",
            Enabled = true,
            Image = "https://upload.wikimedia.org/wikipedia/commons/5/59/Empty.png",
            ItemType = "WEAPON",
            LootChance = 0,
            SellValue = 0,
            StuffRarity = 0
        };

        public ArmorItem DefaultArmorItem { get; set; } = new ArmorItem
        {
            Name = "Torse nu",
            Attack = 0,
            BuyValue = 0,
            Defense = 0,
            Description = "Il est à oilp le bougre.",
            Enabled = true,
            Image = "https://upload.wikimedia.org/wikipedia/commons/5/59/Empty.png",
            ItemType = "ARMOR",
            LootChance = 0,
            SellValue = 0,
            StuffRarity = 0
        };

        public UserItem DefaultUserItem { get; set; } = new UserItem
        {
            Avatar = "",
            Id = -1,
            Name = "InvalidUser"
        };
    }
}