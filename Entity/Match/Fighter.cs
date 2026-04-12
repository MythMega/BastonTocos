using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.User;
using BastocosR2.Tools;

namespace Bastocos.Entity.Match
{
    public class Fighter
    {
        public UserItem User { get; set; } = new UserItem();
        public int HP_Current { get; set; }
        public int HP_Max { get; set; }
        public ArmorItem ArmorItem { get; set; } = new ArmorItem();
        public WeaponItem WeaponItem { get; set; } = new WeaponItem();
        public FighterStatistic FighterStatistic { get; set; } = new FighterStatistic();

        public void Damage(int damage)
        {
            HP_Current -= damage;
        }
    }
}