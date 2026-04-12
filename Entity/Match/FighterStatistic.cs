using Bastocos.Entity.Cards;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;

namespace Bastocos.Entity.Match
{
    public class FighterStatistic
    {
        // à ajouter
        public int AttackCount { get; set; } = 0;

        // à ajouter
        public int LootedCount { get; set; } = 0;

        // à remplacer si supérieur
        public int MaxDamageSingleHitDuel { get; set; } = 0;

        // à remplacer si supérieur
        public int MaxDamageSingleHitAssault { get; set; } = 0;

        // à remplacer si supérieur
        public int MaxDamageDuel { get; set; } = 0;

        // à remplacer si supérieur
        public int MaxDamageAssault { get; set; } = 0;

        // à ajouter
        public int TotalDamage { get; set; } = 0;

        // à ajouter
        public int TotalCommandCount { get; set; } = 0;

        public List<WeaponItem> WeaponItemsObtained = [];
        public List<ArmorItem> ArmorItemsObtained = [];
        public List<CardItem> CardsObtained = [];
        public List<Trashitem> TrashItemsObtained = [];
    }
}