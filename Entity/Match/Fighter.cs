using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.User;

namespace Bastocos.Entity.Match
{
    public class Fighter
    {
        public UserItem User { get; set; }
        public int HP_Current { get; set; }
        public int HP_Max { get; set; }
        public ArmorItem ArmorItem { get; set; }
        public WeaponItem WeaponItem { get; set; }

        public void Damage(int damage)
        {
            HP_Current -= damage;
        }
    }
}