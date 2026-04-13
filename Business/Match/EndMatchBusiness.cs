using Bastocos.Business.Database.User;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Request;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Entity.Database;

namespace BastocosR2.Business.Match
{
    internal class EndMatchBusiness
    {
        private ItemsDatabaseConnection _ItemsDb = new ItemsDatabaseConnection();
        private CardDatabaseConnection _CardsDb = new CardDatabaseConnection();
        private UserDatabaseConnection userDb = new UserDatabaseConnection();

        public void EndMatch(EnvItem env, SettingsItem settings)
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