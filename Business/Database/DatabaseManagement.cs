using Bastocos.Entity.Admin.Database;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Business.Database
{
    public static class DatabaseManagement
    {
        public static int DatabaseVersion = 1;

        public static bool DatabaseExist(DatabaseConnection dbconnect)
        {
            return File.Exists(dbconnect.ConnectionString);
        }

        public static void CreateDatabaseFile(DatabaseConnection dbconnect)
        {
            // Crée le fichier si inexistant
            if (!File.Exists(dbconnect.ConnectionString))
                File.Create(dbconnect.ConnectionString).Close();

            using (var connection = new SqliteConnection($"Data Source={dbconnect.ConnectionString};"))
            {
                connection.Open();

                string sql = File.ReadAllText("SQL/Update/0001.sqlite");

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateDatabaseFile(DatabaseConnection databaseconnect)
        {
        }
    }
}