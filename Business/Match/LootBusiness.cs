using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Cards;
using Bastocos.Entity.Match;
using Bastocos.Entity.Stuffs.Equipments;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Business.Match
{
    public class LootBusiness
    {
        internal string SearchForLoot(EnvItem globalEnvironmentItem, SettingsItem globalSettingsItems, UserItem userWhoLoot)
        {
            string result = string.Empty;
            int total = globalSettingsItems.LootRatesPercentages.Equipments
          + globalSettingsItems.LootRatesPercentages.Trash
          + globalSettingsItems.LootRatesPercentages.Cards;

            if (globalEnvironmentItem.CurrentMatch == null)
            {
                return "Y a pas de combat là bolosse";
            }
            if (globalEnvironmentItem.CurrentMatch.FighterA.User.Id != userWhoLoot.Id && globalEnvironmentItem.CurrentMatch.FighterB.User.Id != userWhoLoot.Id)
                return "Tu n'es pas en combat tocos";
            if (globalEnvironmentItem.CurrentMatch.Finished)
                return "C'est bon le combat est finit stop le loot";
            Fighter looter = globalEnvironmentItem.CurrentMatch.FighterA.User.Id == userWhoLoot.Id ? globalEnvironmentItem.CurrentMatch.FighterA : globalEnvironmentItem.CurrentMatch.FighterB;
            looter.FighterStatistic.LootedCount++;
            int roll = globalEnvironmentItem.RandomGenerator.Next(total);
            bool determined = false;
            // Détermine la tranche
            int threshold = globalSettingsItems.LootRatesPercentages.Equipments;

            if (roll < threshold)
            {
                result = LootEquipments(globalEnvironmentItem, looter);
                determined = true;
            }

            threshold += globalSettingsItems.LootRatesPercentages.Trash;

            if (roll < threshold && !determined)
            {
                result = LootTrash(globalEnvironmentItem, looter);
                determined = true;
            }
            // Sinon, c’est forcément Cards
            if (!determined)
                result = LootCards(globalEnvironmentItem, looter);

            return result;
        }

        private string LootCards(EnvItem env, Fighter userWhoLoot)
        {
            // On calcule la somme totale des pondérations
            int totalPonderation = env.CardItems.Sum(c => c.LootChance);

            // On tire un nombre entre 0 et totalPonderation - 1
            int roll = env.RandomGenerator.Next(totalPonderation);

            // On parcourt les cartes jusqu'à trouver celle correspondant au tirage
            int cumulative = 0;
            CardItem selected = null;

            foreach (var card in env.CardItems)
            {
                cumulative += card.LootChance;
                if (roll < cumulative)
                {
                    selected = card;
                    break;
                }
            }

            // Sécurité (ne devrait jamais arriver)
            if (selected == null)
                selected = env.CardItems.Last();

            // On ajoute la carte au joueur
            userWhoLoot.FighterStatistic.CardsObtained.Add(selected);

            return $"Card obtenue : {selected.Name}";
        }

        private string LootTrash(EnvItem env, Fighter userWhoLoot)
        {
            var list = env.Items.Trashitems;

            if (list.Count == 0)
                return "Rien n'a été récupéré car aucun déchet n'a été chargé";

            // 1) Somme totale des pondérations
            int totalPonderation = list.Sum(t => t.LootChance);

            // 2) Tirage aléatoire entre 0 et totalPonderation - 1
            int roll = env.RandomGenerator.Next(totalPonderation);

            // 3) Recherche de l'item correspondant au tirage
            int cumulative = 0;
            Trashitem selected = null;

            foreach (var item in list)
            {
                cumulative += item.LootChance;
                if (roll < cumulative)
                {
                    selected = item;
                    break;
                }
            }

            // Sécurité (ne devrait jamais arriver)
            selected ??= list.Last();

            // 4) Ajout dans les stats du joueur
            userWhoLoot.FighterStatistic.TrashItemsObtained.Add(selected);

            return $"Tu as trouvé : {selected.Name} (valeur : {selected.SellValue})";
        }

        private string LootEquipments(EnvItem env, Fighter userWhoLoot)
        {
            bool armor = env.RandomGenerator.Next(2) == 1;
            return armor ? LootArmor(env, userWhoLoot) : LootWeapon(env, userWhoLoot);
        }

        private string LootWeapon(EnvItem env, Fighter looter)
        {
            var list = env.Items.WeaponItems;

            if (list.Count == 0)
                return "Rien n'a été récupéré car aucune arme n'a été chargée";

            // 1) Somme des pondérations
            int totalPonderation = list.Sum(w => w.LootChance);

            // 2) Tirage aléatoire
            int roll = env.RandomGenerator.Next(totalPonderation);

            // 3) Sélection pondérée
            int cumulative = 0;
            WeaponItem selected = null;

            foreach (var item in list)
            {
                cumulative += item.LootChance;
                if (roll < cumulative)
                {
                    selected = item;
                    break;
                }
            }

            selected ??= list.Last();

            // 4) Ajout dans les stats
            looter.FighterStatistic.WeaponItemsObtained.Add(selected);

            // 5) Equipement éventuel
            string result = $"Tu as trouvé : {selected.Name}";
            if (CanEquip(env, looter, selected as EquipmentItem))
            {
                int boost = selected.Attack - looter.WeaponItem.Attack;
                looter.WeaponItem = selected;
                result += $" Tu l'équipe. +{boost} ATK";
            }

            return result;
        }

        private string LootArmor(EnvItem env, Fighter looter)
        {
            var list = env.Items.ArmorItems;

            if (list.Count == 0)
                return "Rien n'a été récupéré car aucune armure n'a été chargée";

            // 1) Somme des pondérations
            int totalPonderation = list.Sum(a => a.LootChance);

            // 2) Tirage aléatoire
            int roll = env.RandomGenerator.Next(totalPonderation);

            // 3) Sélection pondérée
            int cumulative = 0;
            ArmorItem selected = null;

            foreach (var item in list)
            {
                cumulative += item.LootChance;
                if (roll < cumulative)
                {
                    selected = item;
                    break;
                }
            }

            selected ??= list.Last();

            // 4) Ajout dans les stats
            looter.FighterStatistic.ArmorItemsObtained.Add(selected);

            // 5) Equipement éventuel
            string result = $"Tu as trouvé : {selected.Name}";
            if (CanEquip(env, looter, selected as EquipmentItem))
            {
                int boost = selected.Defense - looter.ArmorItem.Defense;
                looter.ArmorItem = selected;
                result += $" Tu l'équipe. +{boost} DEF";
            }

            return result;
        }

        private bool CanEquip(EnvItem env, Fighter looter, EquipmentItem equipment)
        {
            if (equipment is WeaponItem weaponItem)
            {
                return looter.WeaponItem.Attack < equipment.Attack;
            }
            else if (equipment is ArmorItem armorItem)
            {
                return looter.ArmorItem.Defense < equipment.Defense;
            }
            return false;
        }
    }
}