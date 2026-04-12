using Bastocos.Entity.User;
using Bastocos.Tools;
using BastocosR2.Entity.Database;
using Microsoft.Data.Sqlite;

namespace BastocosR2.Business.Database.Entities
{
    internal class ItemsDatabaseConnection
    {
        private SqliteConnection GetConnection()
        {
            string a = GlobalVar.ConnectionString;
            var conn = new SqliteConnection(a);
            conn.Open();
            return conn;
        }

        internal List<ItemEntrie> GetAllItemsFromUser(UserItem userItem)
        {
            var result = new List<ItemEntrie>();

            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT ID, Item, Count
            FROM ItemsStorage
            WHERE ID = @id;
        ";
                cmd.Parameters.AddWithValue("@id", userItem.Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entry = new ItemEntrie
                        {
                            Id = reader.GetInt32(0),
                            Item = reader.GetString(1),
                            Count = reader.GetInt32(2)
                        };
                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        internal List<ItemEntrie> GetAllItems()
        {
            var result = new List<ItemEntrie>();

            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT ID, Item, Count
            FROM ItemsStorage;
        ";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entry = new ItemEntrie
                        {
                            Id = reader.GetInt32(0),
                            Item = reader.GetString(1),
                            Count = reader.GetInt32(2)
                        };
                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        internal bool HasItem(UserItem userItem, string itemName)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT COUNT(1)
            FROM ItemsStorage
            WHERE ID = @id AND Item = @item;
        ";
                cmd.Parameters.AddWithValue("@id", userItem.Id);
                cmd.Parameters.AddWithValue("@item", itemName);

                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        internal int GetItemCount(UserItem userItem, string itemName)
        {
            using (var conn = GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT Count
            FROM ItemsStorage
            WHERE ID = @id AND Item = @item;
        ";
                cmd.Parameters.AddWithValue("@id", userItem.Id);
                cmd.Parameters.AddWithValue("@item", itemName);

                var obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value)
                    return 0;

                return Convert.ToInt32(obj);
            }
        }

        internal void AddOrCreate(List<ItemEntrie> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            using (var conn = GetConnection())
            using (var tran = conn.BeginTransaction())
            {
                using (var cmdUpdate = conn.CreateCommand())
                using (var cmdInsert = conn.CreateCommand())
                {
                    cmdUpdate.CommandText = @"
                UPDATE ItemsStorage
                SET Count = Count + @count
                WHERE ID = @id AND Item = @item;
            ";
                    cmdUpdate.Parameters.Add(new SqliteParameter("@count", System.Data.DbType.Int32));
                    cmdUpdate.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32));
                    cmdUpdate.Parameters.Add(new SqliteParameter("@item", System.Data.DbType.String));

                    cmdInsert.CommandText = @"
                INSERT INTO ItemsStorage (ID, Item, Count)
                VALUES (@id, @item, @count);
            ";
                    cmdInsert.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32));
                    cmdInsert.Parameters.Add(new SqliteParameter("@item", System.Data.DbType.String));
                    cmdInsert.Parameters.Add(new SqliteParameter("@count", System.Data.DbType.Int32));

                    foreach (var e in entries)
                    {
                        cmdUpdate.Parameters["@count"].Value = e.Count;
                        cmdUpdate.Parameters["@id"].Value = e.Id;
                        cmdUpdate.Parameters["@item"].Value = e.Item;
                        var rows = cmdUpdate.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            cmdInsert.Parameters["@id"].Value = e.Id;
                            cmdInsert.Parameters["@item"].Value = e.Item;
                            cmdInsert.Parameters["@count"].Value = e.Count;
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }

                tran.Commit();
            }
        }

        internal void UpdateOrCreate(List<ItemEntrie> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            using (var conn = GetConnection())
            using (var tran = conn.BeginTransaction())
            {
                // Préparer deux commandes réutilisables : update puis insert
                using (var cmdUpdate = conn.CreateCommand())
                using (var cmdInsert = conn.CreateCommand())
                {
                    cmdUpdate.CommandText = @"
                UPDATE ItemsStorage
                SET Count = @count
                WHERE ID = @id AND Item = @item;
            ";
                    cmdUpdate.Parameters.Add(new SqliteParameter("@count", System.Data.DbType.Int32));
                    cmdUpdate.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32));
                    cmdUpdate.Parameters.Add(new SqliteParameter("@item", System.Data.DbType.String));

                    cmdInsert.CommandText = @"
                INSERT INTO ItemsStorage (ID, Item, Count)
                VALUES (@id, @item, @count);
            ";
                    cmdInsert.Parameters.Add(new SqliteParameter("@id", System.Data.DbType.Int32));
                    cmdInsert.Parameters.Add(new SqliteParameter("@item", System.Data.DbType.String));
                    cmdInsert.Parameters.Add(new SqliteParameter("@count", System.Data.DbType.Int32));

                    foreach (var e in entries)
                    {
                        // 1) tenter l'update
                        cmdUpdate.Parameters["@count"].Value = e.Count;
                        cmdUpdate.Parameters["@id"].Value = e.Id;
                        cmdUpdate.Parameters["@item"].Value = e.Item;
                        var rows = cmdUpdate.ExecuteNonQuery();

                        // 2) si aucune ligne mise à jour, on insert
                        if (rows == 0)
                        {
                            cmdInsert.Parameters["@id"].Value = e.Id;
                            cmdInsert.Parameters["@item"].Value = e.Item;
                            cmdInsert.Parameters["@count"].Value = e.Count;
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }

                tran.Commit();
            }
        }
    }
}