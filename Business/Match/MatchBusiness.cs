using Bastocos.Business.Account;
using Bastocos.Business.Database.User;
using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Fight;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.Stuffs.Equipments;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Tools;
using System.Text;

namespace Bastocos.Business.Match
{
    internal class MatchBusiness
    {
        private AccountManagement AccountManagement = new AccountManagement();
        private UserDatabaseConnection userDb = new UserDatabaseConnection();
        private DefaultElements _defaultElements = new DefaultElements();

        public void InitializeDuel()
        {
        }

        public string InitializeAssault(EnvItem envItem, FightAssaultRequest request)
        {
            request.UserToDuel = request.UserToDuel.Replace("@", "");
            if (request.UserToDuel == request.UserItemA.Name)
            {
                return "Tu ne peux pas te duel toi même gros con";
            }
            request.UserItemB = AccountManagement.GetFromUsername(request.UserToDuel, envItem.Dbconnect);
            if (request.UserItemB == null)
            {
                return "Utilisateur non trouvé";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemA.Id == request.UserItemA.Id))
            {
                return request.UserItemA.Name + " est déjà en attente d'un combat / en combat";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemB!.Name == request.UserToDuel))
            {
                return request.UserItemB.Name + " est déjà en attente d'un combat / en combat";
            }
            request.UserItemB = userDb.GetFromUserName(request.UserToDuel);
            request.RequestStatut = RequestStatut.In_Queue;
            envItem.FightQueue.Add(request);
            return $"Combat ajouté a la liste d'attente {envItem.FightQueue.Where(f => f.RequestStatut == RequestStatut.In_Queue).Count()}";
        }

        public void Start(EnvItem globalEnvironmentItem, MatchSettings matchSettings)
        {
            FightRequest futurematch = globalEnvironmentItem.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.In_Queue);
            globalEnvironmentItem.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.In_Queue).RequestStatut = RequestStatut.Running;
            if (futurematch == null)
            {
                return;
            }
            int HP = matchSettings.DefaultsMatchSettings.DefaultHP;
            if (futurematch.UserItemB is null)
                throw new Exception(ErrorItem.ERROR_ACCOUNT_ALREADY_EXIST);
            globalEnvironmentItem.CurrentMatch = new FightItem
            {
                FighterA = new Fighter
                {
                    User = futurematch.UserItemA,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FighterB = new Fighter
                {
                    User = futurematch.UserItemB,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FightStats = new FightStats
                {
                    LastAction = DateTime.Now,
                    StartTime = DateTime.Now,
                }
            };
        }

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

            Fighter looter = globalEnvironmentItem.CurrentMatch.FighterA.User.Id == userWhoLoot.Id ? globalEnvironmentItem.CurrentMatch.FighterA : globalEnvironmentItem.CurrentMatch.FighterB;
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

        private string LootCards(EnvItem globalEnvironmentItem, Fighter userWhoLoot)
        {
            return $"Card obtenue : {globalEnvironmentItem.CardItems[globalEnvironmentItem.RandomGenerator.Next(globalEnvironmentItem.CardItems.Count)].Name}";
        }

        private string LootTrash(EnvItem env, Fighter userWhoLoot)
        {
            var list = env.Items.Trashitems;

            if (list.Count == 0)
                return "Rien n'a été récupéré car aucun déchet n'a été chargé";

            int index = env.RandomGenerator.Next(list.Count);
            Trashitem selected = list[index];

            // Ici tu fais ce que tu veux avec l'item sélectionné
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
                return "Rien n'a été récupéré car aucune armure n'a été chargée";

            int index = env.RandomGenerator.Next(list.Count);
            WeaponItem selected = list[index];
            string result = $"Tu as trouvé : {selected.Name}";
            if (CanEquip(env, looter, selected as EquipmentItem))
            {
                int boost = selected.Attack - looter.WeaponItem.Attack;
                looter.WeaponItem = selected;
                result += $"Tu l'équipe. +{boost} ATK";
            }
            // Ici tu fais ce que tu veux avec l'item sélectionné
            return result;
        }

        private string LootArmor(EnvItem env, Fighter looter)
        {
            var list = env.Items.ArmorItems;

            if (list.Count == 0)
                return "Rien n'a été récupéré car aucune armure n'a été chargée";

            int index = env.RandomGenerator.Next(list.Count);
            ArmorItem selected = list[index];
            string result = $"Tu as trouvé : {selected.Name}";
            if (CanEquip(env, looter, selected as EquipmentItem))
            {
                int boost = selected.Defense - looter.ArmorItem.Defense;
                looter.ArmorItem = selected;
                result += $"Tu l'équipe. +{boost} DEF";
            }
            // Ici tu fais ce que tu veux avec l'item sélectionné
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

        internal string Attack(EnvItem env, SettingsItem settings, UserItem attacker)
        {
            var match = env.CurrentMatch;
            if (match is null)
                return "y a pas de match gros tocos";
            if (env.FightQueue.Any(a => a.RequestStatut == RequestStatut.Finishing))
                return "Le match est terminé, un suivant va démarrer prochainement si il y en a en queue";

            match.FightStats.LastAction = DateTime.Now;

            // Détermine si l'utilisateur est FighterA ou FighterB
            bool isA = match.FighterA.User.Id == attacker.Id;
            bool isB = match.FighterB.User.Id == attacker.Id;

            if (!isA && !isB)
                return "t'es pas dans le match gros tocos";

            // Sélectionne automatiquement l'attaquant et la cible
            var attackerFighter = isA ? match.FighterA : match.FighterB;
            var defenderFighter = isA ? match.FighterB : match.FighterA;

            // Calcul des dégâts
            int rawDamage = attackerFighter.WeaponItem.Attack - defenderFighter.ArmorItem.Defense;
            int damageDone = Math.Max(rawDamage, 0);

            // Applique les dégâts
            defenderFighter.HP_Current -= damageDone;
            bool matchfinito = attackerFighter.HP_Current <= 0 || defenderFighter.HP_Current <= 0;
            // minimise les PV a 0
            env.CurrentMatch!.FighterB.HP_Current = env.CurrentMatch.FighterB.HP_Current < 0 ? 0 : env.CurrentMatch.FighterB.HP_Current;
            env.CurrentMatch.FighterA.HP_Current = env.CurrentMatch.FighterA.HP_Current < 0 ? 0 : env.CurrentMatch.FighterA.HP_Current;
            // Message final
            if (matchfinito)
            {
                StringBuilder res = new();
                res.Append("Match Terminé ! ");
                if (attackerFighter.HP_Current <= 0 && defenderFighter.HP_Current <= 0)
                {
                    res.Append("Match nul ! Les deux sont dead, kaput!");
                }
                else
                {
                    List<Fighter> fighters = [attackerFighter, defenderFighter];
                    Fighter winner = fighters.OrderByDescending(f => f.HP_Current).First();
                    res.Append($"Gagnant : {winner.User.Name}, avec {winner.HP_Current} PV restant.");
                    EndMatch(env);
                }
                return res.ToString();
            }
            else
            {
                env.CurrentMatch!.WebHitDatas.Add(new BastocosR2.Tools.Json.WebHitData
                {
                    // on envoie sur le perso qui réçoit les dégats, donc pas celui qui attaque
                    Perso = attacker.Id == env.CurrentMatch.FighterA.User.Id ? "B" : "A",

                    // on affiche les dégats inversé (pour 6 damages, on affiche -6)
                    PVEdit = (damageDone * -1).ToString(),
                    Displayed = false,
                });
                return damageDone > 0
                    ? $"{attackerFighter.User.Name} a infligé {damageDone} à {defenderFighter.User.Name}."
                    : $"Dégâts de {attackerFighter.User.Name} totalement bloqués par {defenderFighter.User.Name}.";
            }
        }

        private void EndMatch(EnvItem env)
        {
            // une fois ce match terminé, on le met en statut fini et set le LastMatchEnd à ce temps, comme ça, une fois le délai LastMatchEnd + le temps entre chaque combat, le runner central dans program.cs set le env.CurrentMatch a null et on reboucle
            env.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.Running).RequestStatut = RequestStatut.Finnished;
            env.LastMatchEnd = DateTime.Now;
        }
    }
}