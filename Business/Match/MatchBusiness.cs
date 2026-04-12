using Bastocos.Business.Account;
using Bastocos.Business.Database.User;
using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Cards;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Fight;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.Stuffs.Equipments;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Entity.Database;
using BastocosR2.Tools;
using System.Text;

namespace Bastocos.Business.Match
{
    internal class MatchBusiness
    {
        private AccountManagement AccountManagement = new AccountManagement();
        private UserDatabaseConnection userDb = new UserDatabaseConnection();
        private DefaultElements _defaultElements = new DefaultElements();
        private ItemsDatabaseConnection _ItemsDb = new ItemsDatabaseConnection();
        private CardDatabaseConnection _CardsDb = new CardDatabaseConnection();

        public void InitializeDuel()
        {
        }

        public string InitializeAssault(EnvItem envItem, FightAssaultRequest request)
        {
            envItem.RefreshLastActivity(request.UserItemA);
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
            if (!envItem.IsStillActive(request.UserItemB))
            {
                return $"L'utilisateur {request.UserItemB.Name} ne semble pas être actif.";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemA.Id == request.UserItemA.Id))
            {
                return request.UserItemA.Name + " est déjà en attente d'un combat / en combat";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemB!.Name.Equals(request.UserToDuel, StringComparison.CurrentCultureIgnoreCase)))
            {
                return request.UserItemB.Name + " est déjà en attente d'un combat / en combat";
            }
            request.UserItemB = userDb.GetFromUserName(request.UserToDuel);
            request.RequestStatut = RequestStatut.In_Queue;
            FightRequest fightRequest = request;
            fightRequest.FightType = FightType.Assault;
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

        internal string Attack(EnvItem env, SettingsItem settings, UserItem attacker)
        {
            var match = env.CurrentMatch;
            if (match is null)
                return "y a pas de match gros tocos";
            if (env.FightQueue.Any(a => a.RequestStatut == RequestStatut.Finishing))
                return "Le match est terminé, un suivant va démarrer prochainement si il y en a en queue";
            if (match.Finished)
                return "C'est bon le combat est finit stop les damages";

            match.FightStats.LastAction = DateTime.Now;

            // Détermine si l'utilisateur est FighterA ou FighterB
            bool isA = match.FighterA.User.Id == attacker.Id;
            bool isB = match.FighterB.User.Id == attacker.Id;

            if (!isA && !isB)
                return "t'es pas dans le match gros tocos";

            // Sélectionne automatiquement l'attaquant et la cible
            var attackerFighter = isA ? match.FighterA : match.FighterB;
            var defenderFighter = isA ? match.FighterB : match.FighterA;

            attackerFighter.FighterStatistic.AttackCount++;
            attackerFighter.FighterStatistic.TotalCommandCount++;

            // Calcul des dégâts
            int fulldamage = attackerFighter.WeaponItem.Attack - defenderFighter.ArmorItem.Defense;
            int blockedDamages = 0;
            if (defenderFighter.ArmorItem.Defense > attackerFighter.WeaponItem.Attack)
            {
                blockedDamages = attackerFighter.WeaponItem.Attack;
            }
            else
            {
                blockedDamages = defenderFighter.ArmorItem.Defense;
            }
            decimal blockedDamageValues = blockedDamages / 2;
            if (blockedDamages > 0)
            {
                fulldamage += (int)Math.Round(blockedDamageValues, 0);
            }
            int damageDone = Math.Max(fulldamage, 0);

            // Applique les dégâts
            defenderFighter.HP_Current -= damageDone;

            // applique les statistiques
            if (env.CurrentMatch.MatchType == FightType.Assault)
            {
                attackerFighter.FighterStatistic.MaxDamageSingleHitAssault = damageDone > attackerFighter.FighterStatistic.MaxDamageSingleHitAssault ? damageDone : attackerFighter.FighterStatistic.MaxDamageSingleHitAssault;
            }
            else
            {
                attackerFighter.FighterStatistic.MaxDamageSingleHitDuel = damageDone > attackerFighter.FighterStatistic.MaxDamageSingleHitDuel ? damageDone : attackerFighter.FighterStatistic.MaxDamageSingleHitDuel;
            }
            attackerFighter.FighterStatistic.TotalDamage += damageDone;

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
                    EndMatch(env, settings);
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

        private void EndMatch(EnvItem env, SettingsItem settings)
        {
            updateStatistics(env, settings);
            updateInventory(env, settings);
            // une fois ce match terminé, on le met en statut fini et set le LastMatchEnd à ce temps, comme ça, une fois le délai LastMatchEnd + le temps entre chaque combat, le runner central dans program.cs set le env.CurrentMatch a null et on reboucle
            env.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.Running).RequestStatut = RequestStatut.Finnished;
            env.CurrentMatch.Finished = true;
            env.LastMatchEnd = DateTime.Now;
        }

        private void updateInventory(EnvItem env, SettingsItem settings)
        {
            if (env.CurrentMatch is null) { return; }

            List<Fighter> players = [env.CurrentMatch.FighterA, env.CurrentMatch.FighterB];

            List<ItemEntrie> itemEntries = [];
            List<CardEntrie> cardEntries = [];

            // On collecte toutes les entrées brutes
            players.ForEach(player =>
            {
                int userId = player.User.Id;

                player.FighterStatistic.ArmorItemsObtained
                    .ForEach(item => itemEntries.Add(new ItemEntrie { Count = 1, Id = userId, Item = item.Name }));

                player.FighterStatistic.WeaponItemsObtained
                    .ForEach(item => itemEntries.Add(new ItemEntrie { Count = 1, Id = userId, Item = item.Name }));

                player.FighterStatistic.TrashItemsObtained
                    .ForEach(item => itemEntries.Add(new ItemEntrie { Count = 1, Id = userId, Item = item.Name }));

                player.FighterStatistic.CardsObtained
                    .ForEach(card => cardEntries.Add(new CardEntrie { Count = 1, Id = userId, Item = card.Name }));
            });

            // AGRÉGATION : on regroupe par (Id, Item) et on additionne les Count
            var aggregatedItems = itemEntries
                .GroupBy(e => new { e.Id, e.Item })
                .Select(g => new ItemEntrie
                {
                    Id = g.Key.Id,
                    Item = g.Key.Item,
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            var aggregatedCards = cardEntries
                .GroupBy(e => new { e.Id, e.Item })
                .Select(g => new CardEntrie
                {
                    Id = g.Key.Id,
                    Item = g.Key.Item,
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            // On envoie les données agrégées à la DB
            _ItemsDb.AddOrCreate(aggregatedItems);
            _CardsDb.AddOrCreate(aggregatedCards);
        }

        private void updateStatistics(EnvItem env, SettingsItem settings)
        {
            var userADB = userDb.GetFullUserData(env.CurrentMatch.FighterA.User.Id);
            var userBDB = userDb.GetFullUserData(env.CurrentMatch.FighterB.User.Id);

            // USER A

            // Statistic additives
            userADB.StatisticsItem.AttackCount += env.CurrentMatch.FighterA.FighterStatistic.AttackCount;
            userADB.StatisticsItem.LootedCount += env.CurrentMatch.FighterA.FighterStatistic.LootedCount;
            userADB.StatisticsItem.TotalCommandCount += env.CurrentMatch.FighterA.FighterStatistic.TotalCommandCount;
            userADB.StatisticsItem.TotalDamage += env.CurrentMatch.FighterA.FighterStatistic.TotalDamage;

            // ajout de money, 25 (RewardWinner) si seul en vie, sinon 10 (RewardLooser)
            userADB.StatisticsItem.Money += env.CurrentMatch.FighterA.HP_Current > 0 && env.CurrentMatch.FighterB.HP_Current == 0 ? settings.MatchSettings.MoneyMatchSetting.RewardWinner : settings.MatchSettings.MoneyMatchSetting.RewardLooser;

            // USER B

            // Statistic additives
            userBDB.StatisticsItem.AttackCount += env.CurrentMatch.FighterB.FighterStatistic.AttackCount;
            userBDB.StatisticsItem.LootedCount += env.CurrentMatch.FighterB.FighterStatistic.LootedCount;
            userBDB.StatisticsItem.TotalCommandCount += env.CurrentMatch.FighterB.FighterStatistic.TotalCommandCount;
            userBDB.StatisticsItem.TotalDamage += env.CurrentMatch.FighterB.FighterStatistic.TotalDamage;

            // ajout de money, 25 (RewardWinner) si seul en vie, sinon 10 (RewardLooser)
            userBDB.StatisticsItem.Money += env.CurrentMatch.FighterB.HP_Current > 0 && env.CurrentMatch.FighterA.HP_Current == 0
                ? settings.MatchSettings.MoneyMatchSetting.RewardWinner
                : settings.MatchSettings.MoneyMatchSetting.RewardLooser;

            // compte victoire défaite draw en fonction du mode de combat
            if (env.CurrentMatch.MatchType == FightType.Assault)
            {
                if (env.CurrentMatch.FighterA.HP_Current > 0 && env.CurrentMatch.FighterB.HP_Current == 0)
                {
                    userADB.StatisticsItem.AssaultVictoryCount++;
                    userBDB.StatisticsItem.AssaultDefeatCount++;
                    userADB.StatisticsItem.VictoryCount++;
                    userBDB.StatisticsItem.DefeatCount++;
                }
                else if (env.CurrentMatch.FighterB.HP_Current > 0 && env.CurrentMatch.FighterA.HP_Current == 0)
                {
                    userADB.StatisticsItem.AssaultDefeatCount++;
                    userBDB.StatisticsItem.AssaultVictoryCount++;
                    userADB.StatisticsItem.DefeatCount++;
                    userBDB.StatisticsItem.VictoryCount++;
                }
                else
                {
                    userADB.StatisticsItem.AssaultDrawCount++;
                    userBDB.StatisticsItem.AssaultDrawCount++;
                    userADB.StatisticsItem.DrawCount++;
                    userBDB.StatisticsItem.DrawCount++;
                }
                userBDB.StatisticsItem.MaxDamageSingleHitAssault = userBDB.StatisticsItem.MaxDamageSingleHitAssault > env.CurrentMatch.FighterB.FighterStatistic.MaxDamageSingleHitAssault ? userBDB.StatisticsItem.MaxDamageSingleHitAssault : env.CurrentMatch.FighterB.FighterStatistic.MaxDamageSingleHitAssault;

                userADB.StatisticsItem.MaxDamageSingleHitAssault = userADB.StatisticsItem.MaxDamageSingleHitAssault > env.CurrentMatch.FighterA.FighterStatistic.MaxDamageSingleHitAssault ? userADB.StatisticsItem.MaxDamageSingleHitAssault : env.CurrentMatch.FighterA.FighterStatistic.MaxDamageSingleHitAssault;

                userBDB.StatisticsItem.MaxDamageDuel = userBDB.StatisticsItem.MaxDamageDuel > env.CurrentMatch.FighterB.FighterStatistic.MaxDamageDuel ? userBDB.StatisticsItem.MaxDamageDuel : env.CurrentMatch.FighterB.FighterStatistic.MaxDamageDuel;

                userADB.StatisticsItem.MaxDamageDuel = userADB.StatisticsItem.MaxDamageDuel > env.CurrentMatch.FighterA.FighterStatistic.MaxDamageDuel ? userADB.StatisticsItem.MaxDamageDuel : env.CurrentMatch.FighterA.FighterStatistic.MaxDamageDuel;
            }
            if (env.CurrentMatch.MatchType == FightType.Duel)
            {
                // Victoire / Défaite / Égalité
                if (env.CurrentMatch.FighterA.HP_Current > 0 && env.CurrentMatch.FighterB.HP_Current == 0)
                {
                    userADB.StatisticsItem.DuelVictoryCount++;
                    userBDB.StatisticsItem.DuelDefeatCount++;
                    userADB.StatisticsItem.VictoryCount++;
                    userBDB.StatisticsItem.DefeatCount++;
                }
                else if (env.CurrentMatch.FighterB.HP_Current > 0 && env.CurrentMatch.FighterA.HP_Current == 0)
                {
                    userADB.StatisticsItem.DuelDefeatCount++;
                    userBDB.StatisticsItem.DuelVictoryCount++;
                    userADB.StatisticsItem.DefeatCount++;
                    userBDB.StatisticsItem.VictoryCount++;
                }
                else
                {
                    userADB.StatisticsItem.DuelDrawCount++;
                    userBDB.StatisticsItem.DuelDrawCount++;
                    userADB.StatisticsItem.DrawCount++;
                    userBDB.StatisticsItem.DrawCount++;
                }

                // Max damage single hit (Duel)
                userBDB.StatisticsItem.MaxDamageSingleHitDuel =
                    userBDB.StatisticsItem.MaxDamageSingleHitDuel > env.CurrentMatch.FighterB.FighterStatistic.MaxDamageSingleHitDuel
                    ? userBDB.StatisticsItem.MaxDamageSingleHitDuel
                    : env.CurrentMatch.FighterB.FighterStatistic.MaxDamageSingleHitDuel;

                userADB.StatisticsItem.MaxDamageSingleHitDuel =
                    userADB.StatisticsItem.MaxDamageSingleHitDuel > env.CurrentMatch.FighterA.FighterStatistic.MaxDamageSingleHitDuel
                    ? userADB.StatisticsItem.MaxDamageSingleHitDuel
                    : env.CurrentMatch.FighterA.FighterStatistic.MaxDamageSingleHitDuel;

                // Max damage total duel
                userBDB.StatisticsItem.MaxDamageDuel =
                    userBDB.StatisticsItem.MaxDamageDuel > env.CurrentMatch.FighterB.FighterStatistic.MaxDamageDuel
                    ? userBDB.StatisticsItem.MaxDamageDuel
                    : env.CurrentMatch.FighterB.FighterStatistic.MaxDamageDuel;

                userADB.StatisticsItem.MaxDamageDuel =
                    userADB.StatisticsItem.MaxDamageDuel > env.CurrentMatch.FighterA.FighterStatistic.MaxDamageDuel
                    ? userADB.StatisticsItem.MaxDamageDuel
                    : env.CurrentMatch.FighterA.FighterStatistic.MaxDamageDuel;
            }

            userDb.SetFullUserData(userADB);
            userDb.SetFullUserData(userBDB);
        }
    }
}