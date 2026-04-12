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
            string a = GlobalVar.ConnectionString;
            var conn = new SqliteConnection(a);
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
                INSERT INTO User (ID, Username, Picture, FirstLogin, LastLogin)
                VALUES (@id, @username, @avatar, @first, @first);
            ";

                    cmd.Parameters.AddWithValue("@id", account.Id);
                    cmd.Parameters.AddWithValue("@username", account.Name);
                    cmd.Parameters.AddWithValue("@avatar", account.Avatar);
                    cmd.Parameters.AddWithValue("@first", DateTime.Now);

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
                    SELECT ID, Username, Picture, FirstLogin, LastLogin
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
                        Avatar = reader.GetString(2),
                        LoginStatsItem = new LoginStatsItem
                        {
                            FirstLogin = reader.GetDateTime(3),
                            LastLogin = reader.GetDateTime(4),
                        }
                    };
                }
            }
        }

        internal UserItem GetFullUserData(int userId)
        {
            using (var conn = GetConnection())
            {
                UserItem? user = null;

                // --- 1) Récupération des infos User ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT ID, Username, Picture, FirstLogin, LastLogin
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
                            Avatar = reader.GetString(2),
                            LoginStatsItem = new LoginStatsItem
                            {
                                FirstLogin = reader.GetDateTime(3),
                                LastLogin = reader.GetDateTime(4)
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
                    cmd.Parameters.AddWithValue("@FirstDuelDate", s.FirstDuelDate as object ?? DBNull.Value);
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

        internal string UpdateAccount(UserItem user)
        {
            using (var conn = GetConnection())
            {
                // --- 1) Update User ---
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                UPDATE User
                SET Username = @username,
                    LastLogin = @lastLogin,
                    Picture = @avatar
                WHERE ID = @id;
            ";

                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@username", user.Name);
                    cmd.Parameters.AddWithValue("@avatar", user.Avatar);
                    cmd.Parameters.AddWithValue("@lastLogin", user.LoginStatsItem.LastLogin);

                    cmd.ExecuteNonQuery();
                }
            }

            return @$"compte ""{user.Name}"" mis a jour";
        }

        internal List<UserItem> GetAllFullUserData()
        {
            var users = new List<UserItem>();

            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT
                U.ID, U.Username, U.Picture, U.FirstLogin, U.LastLogin,
                S.VictoryCount, S.DefeatCount, S.DrawCount, S.AfkCount,
                S.AttackCount, S.LootedCount, S.Money,
                S.MaxDamageSingleHitDuel, S.MaxDamageSingleHitAssault,
                S.MaxDamageDuel, S.MaxDamageAssault, S.TotalDamage,
                S.FirstDuelDate, S.PurchaseCount, S.MoneySpent,
                S.SaleCount, S.MoneyEarned, S.CardsLooted, S.CardsSold,
                S.ActiveDays, S.AssaultsLaunched, S.AssaultsResurrected,
                S.DuelParticipationCount,
                S.AssaultVictoryCount, S.AssaultDefeatCount, S.AssaultDrawCount, S.AssaultAfkCount,
                S.DuelVictoryCount, S.DuelDefeatCount, S.DuelDrawCount, S.DuelAfkCount,
                S.TotalCommandCount
            FROM User U
            LEFT JOIN UserStats S ON U.ID = S.ID;
        ";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new UserItem
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Avatar = reader.GetString(2),
                            LoginStatsItem = new LoginStatsItem
                            {
                                FirstLogin = reader.GetDateTime(3),
                                LastLogin = reader.GetDateTime(4)
                            }
                        };

                        // Si la colonne VictoryCount (index 5) est NULL => pas de stats
                        if (reader.IsDBNull(5))
                        {
                            user.StatisticsItem = new StatisticsItem();
                        }
                        else
                        {
                            user.StatisticsItem = new StatisticsItem
                            {
                                VictoryCount = reader.GetInt32(5),
                                DefeatCount = reader.GetInt32(6),
                                DrawCount = reader.GetInt32(7),
                                AfkCount = reader.GetInt32(8),

                                AttackCount = reader.GetInt32(9),
                                LootedCount = reader.GetInt32(10),
                                Money = reader.GetInt32(11),

                                MaxDamageSingleHitDuel = reader.GetInt32(12),
                                MaxDamageSingleHitAssault = reader.GetInt32(13),

                                MaxDamageDuel = reader.GetInt32(14),
                                MaxDamageAssault = reader.GetInt32(15),
                                TotalDamage = reader.GetInt32(16),

                                FirstDuelDate = reader.IsDBNull(17) ? (DateTime?)null : reader.GetDateTime(17),

                                PurchaseCount = reader.GetInt32(18),
                                MoneySpent = reader.GetInt32(19),

                                SaleCount = reader.GetInt32(20),
                                MoneyEarned = reader.GetInt32(21),

                                CardsLooted = reader.GetInt32(22),
                                CardsSold = reader.GetInt32(23),

                                ActiveDays = reader.GetInt32(24),
                                AssaultsLaunched = reader.GetInt32(25),
                                AssaultsResurrected = reader.GetInt32(26),

                                DuelParticipationCount = reader.GetInt32(27),

                                AssaultVictoryCount = reader.GetInt32(28),
                                AssaultDefeatCount = reader.GetInt32(29),
                                AssaultDrawCount = reader.GetInt32(30),
                                AssaultAfkCount = reader.GetInt32(31),

                                DuelVictoryCount = reader.GetInt32(32),
                                DuelDefeatCount = reader.GetInt32(33),
                                DuelDrawCount = reader.GetInt32(34),
                                DuelAfkCount = reader.GetInt32(35),

                                TotalCommandCount = reader.GetInt32(36)
                            };
                        }

                        users.Add(user);
                    }
                }
            }

            return users;
        }
    }
}