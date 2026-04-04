using Bastocos.Entity.User;
using Bastocos.Tools;
using Microsoft.Data.Sqlite;
using System;

namespace Bastocos.Business.Database.User
{
    public class UserDatabaseConnection
    {
        private SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(GlobalVar.ConnectionString);
            conn.Open();
            return conn;
        }

        internal void Account_Create(UserItem account)
        {
            using (var conn = GetConnection())
            {
                // 1) Création du User avec ID fourni
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                INSERT INTO User (ID, Username)
                VALUES (@id, @username);
            ";

                    cmd.Parameters.AddWithValue("@id", account.Id);
                    cmd.Parameters.AddWithValue("@username", account.Name);

                    cmd.ExecuteNonQuery();
                }

                // 2) Création des stats associées
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                INSERT INTO UserStats (ID)
                VALUES (@id);
            ";

                    cmd.Parameters.AddWithValue("@id", account.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal bool Account_Exist(int accountId)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM User WHERE ID = @id";
                cmd.Parameters.AddWithValue("@id", accountId);

                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        internal UserItem GetFromUserName(string username)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ID, Username, FirstLogin, LastLogin
                    FROM User
                    WHERE Username = @username
                    LIMIT 1;
                ";

                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new UserItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        LoginStatsItem = new LoginStatsItem
                        {
                            FirstLogin = reader.GetDateTime(2),
                            LastLogin = reader.GetDateTime(3),
                        }
                    };
                }
            }
        }

        internal UserItem GetFullUserData(int userId)
        {
            using (var conn = GetConnection())
            {
                UserItem user = null;

                // --- 1) Récupération des infos User ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT ID, Username, FirstLogin, LastLogin
                FROM User
                WHERE ID = @id
                LIMIT 1;
            ";

                    cmd.Parameters.AddWithValue("@id", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null; // User inexistant

                        user = new UserItem
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            LoginStatsItem = new LoginStatsItem
                            {
                                FirstLogin = reader.GetDateTime(2),
                                LastLogin = reader.GetDateTime(3)
                            }
                        };
                    }
                }

                // --- 2) Récupération des statistiques ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT
                    VictoryCount, DefeatCount, DrawCount, AfkCount,
                    AttackCount, LootedCount, Money,
                    MaxDamageSingleHitDuel, MaxDamageSingleHitAssault,
                    MaxDamageDuel, MaxDamageAssault, TotalDamage,
                    FirstDuelDate, PurchaseCount, MoneySpent,
                    SaleCount, MoneyEarned, CardsLooted, CardsSold,
                    ActiveDays, AssaultsLaunched, AssaultsResurrected,
                    DuelParticipationCount,
                    AssaultVictoryCount, AssaultDefeatCount, AssaultDrawCount, AssaultAfkCount,
                    DuelVictoryCount, DuelDefeatCount, DuelDrawCount, DuelAfkCount,
                    TotalCommandCount
                FROM UserStats
                WHERE ID = @id
                LIMIT 1;
            ";

                    cmd.Parameters.AddWithValue("@id", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            // Si pas de stats, on initialise quand même un objet vide
                            user.StatisticsItem = new StatisticsItem();
                            return user;
                        }

                        user.StatisticsItem = new StatisticsItem
                        {
                            VictoryCount = reader.GetInt32(0),
                            DefeatCount = reader.GetInt32(1),
                            DrawCount = reader.GetInt32(2),
                            AfkCount = reader.GetInt32(3),

                            AttackCount = reader.GetInt32(4),
                            LootedCount = reader.GetInt32(5),
                            Money = reader.GetInt32(6),

                            MaxDamageSingleHitDuel = reader.GetInt32(7),
                            MaxDamageSingleHitAssault = reader.GetInt32(8),

                            MaxDamageDuel = reader.GetInt32(9),
                            MaxDamageAssault = reader.GetInt32(10),
                            TotalDamage = reader.GetInt32(11),

                            FirstDuelDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),

                            PurchaseCount = reader.GetInt32(13),
                            MoneySpent = reader.GetInt32(14),

                            SaleCount = reader.GetInt32(15),
                            MoneyEarned = reader.GetInt32(16),

                            CardsLooted = reader.GetInt32(17),
                            CardsSold = reader.GetInt32(18),

                            ActiveDays = reader.GetInt32(19),
                            AssaultsLaunched = reader.GetInt32(20),
                            AssaultsResurrected = reader.GetInt32(21),

                            DuelParticipationCount = reader.GetInt32(22),

                            AssaultVictoryCount = reader.GetInt32(23),
                            AssaultDefeatCount = reader.GetInt32(24),
                            AssaultDrawCount = reader.GetInt32(25),
                            AssaultAfkCount = reader.GetInt32(26),

                            DuelVictoryCount = reader.GetInt32(27),
                            DuelDefeatCount = reader.GetInt32(28),
                            DuelDrawCount = reader.GetInt32(29),
                            DuelAfkCount = reader.GetInt32(30),

                            TotalCommandCount = reader.GetInt32(31)
                        };
                    }
                }

                return user;
            }
        }

        internal void SetFullUserData(UserItem user)
        {
            using (var conn = GetConnection())
            {
                // --- 1) Update User ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                UPDATE User
                SET Username = @username,
                    FirstLogin = @firstLogin,
                    LastLogin = @lastLogin
                WHERE ID = @id;
            ";

                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@username", user.Name);
                    cmd.Parameters.AddWithValue("@firstLogin", user.LoginStatsItem.FirstLogin);
                    cmd.Parameters.AddWithValue("@lastLogin", user.LoginStatsItem.LastLogin);

                    cmd.ExecuteNonQuery();
                }

                // --- 2) Update UserStats ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                UPDATE UserStats SET
                    VictoryCount = @VictoryCount,
                    DefeatCount = @DefeatCount,
                    DrawCount = @DrawCount,
                    AfkCount = @AfkCount,
                    AttackCount = @AttackCount,
                    LootedCount = @LootedCount,
                    Money = @Money,
                    MaxDamageSingleHitDuel = @MaxDamageSingleHitDuel,
                    MaxDamageSingleHitAssault = @MaxDamageSingleHitAssault,
                    MaxDamageDuel = @MaxDamageDuel,
                    MaxDamageAssault = @MaxDamageAssault,
                    TotalDamage = @TotalDamage,
                    FirstDuelDate = @FirstDuelDate,
                    PurchaseCount = @PurchaseCount,
                    MoneySpent = @MoneySpent,
                    SaleCount = @SaleCount,
                    MoneyEarned = @MoneyEarned,
                    CardsLooted = @CardsLooted,
                    CardsSold = @CardsSold,
                    ActiveDays = @ActiveDays,
                    AssaultsLaunched = @AssaultsLaunched,
                    AssaultsResurrected = @AssaultsResurrected,
                    DuelParticipationCount = @DuelParticipationCount,
                    AssaultVictoryCount = @AssaultVictoryCount,
                    AssaultDefeatCount = @AssaultDefeatCount,
                    AssaultDrawCount = @AssaultDrawCount,
                    AssaultAfkCount = @AssaultAfkCount,
                    DuelVictoryCount = @DuelVictoryCount,
                    DuelDefeatCount = @DuelDefeatCount,
                    DuelDrawCount = @DuelDrawCount,
                    DuelAfkCount = @DuelAfkCount,
                    TotalCommandCount = @TotalCommandCount
                WHERE ID = @id;
            ";

                    var s = user.StatisticsItem;

                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@VictoryCount", s.VictoryCount);
                    cmd.Parameters.AddWithValue("@DefeatCount", s.DefeatCount);
                    cmd.Parameters.AddWithValue("@DrawCount", s.DrawCount);
                    cmd.Parameters.AddWithValue("@AfkCount", s.AfkCount);
                    cmd.Parameters.AddWithValue("@AttackCount", s.AttackCount);
                    cmd.Parameters.AddWithValue("@LootedCount", s.LootedCount);
                    cmd.Parameters.AddWithValue("@Money", s.Money);
                    cmd.Parameters.AddWithValue("@MaxDamageSingleHitDuel", s.MaxDamageSingleHitDuel);
                    cmd.Parameters.AddWithValue("@MaxDamageSingleHitAssault", s.MaxDamageSingleHitAssault);
                    cmd.Parameters.AddWithValue("@MaxDamageDuel", s.MaxDamageDuel);
                    cmd.Parameters.AddWithValue("@MaxDamageAssault", s.MaxDamageAssault);
                    cmd.Parameters.AddWithValue("@TotalDamage", s.TotalDamage);
                    cmd.Parameters.AddWithValue("@FirstDuelDate", (object)s.FirstDuelDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PurchaseCount", s.PurchaseCount);
                    cmd.Parameters.AddWithValue("@MoneySpent", s.MoneySpent);
                    cmd.Parameters.AddWithValue("@SaleCount", s.SaleCount);
                    cmd.Parameters.AddWithValue("@MoneyEarned", s.MoneyEarned);
                    cmd.Parameters.AddWithValue("@CardsLooted", s.CardsLooted);
                    cmd.Parameters.AddWithValue("@CardsSold", s.CardsSold);
                    cmd.Parameters.AddWithValue("@ActiveDays", s.ActiveDays);
                    cmd.Parameters.AddWithValue("@AssaultsLaunched", s.AssaultsLaunched);
                    cmd.Parameters.AddWithValue("@AssaultsResurrected", s.AssaultsResurrected);
                    cmd.Parameters.AddWithValue("@DuelParticipationCount", s.DuelParticipationCount);
                    cmd.Parameters.AddWithValue("@AssaultVictoryCount", s.AssaultVictoryCount);
                    cmd.Parameters.AddWithValue("@AssaultDefeatCount", s.AssaultDefeatCount);
                    cmd.Parameters.AddWithValue("@AssaultDrawCount", s.AssaultDrawCount);
                    cmd.Parameters.AddWithValue("@AssaultAfkCount", s.AssaultAfkCount);
                    cmd.Parameters.AddWithValue("@DuelVictoryCount", s.DuelVictoryCount);
                    cmd.Parameters.AddWithValue("@DuelDefeatCount", s.DuelDefeatCount);
                    cmd.Parameters.AddWithValue("@DuelDrawCount", s.DuelDrawCount);
                    cmd.Parameters.AddWithValue("@DuelAfkCount", s.DuelAfkCount);
                    cmd.Parameters.AddWithValue("@TotalCommandCount", s.TotalCommandCount);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}