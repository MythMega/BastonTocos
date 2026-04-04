using Bastocos.Entity.Admin.Database;
using Bastocos.Tools;
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
            return File.Exists(GlobalVar.ConnectionString);
        }

        public static void CreateDatabaseFile(DatabaseConnection dbconnect)
        {
            // Crée le fichier si inexistant
            if (!File.Exists(GlobalVar.ConnectionString))
                File.Create(GlobalVar.ConnectionString).Close();

            using (var connection = new SqliteConnection($"Data Source={GlobalVar.ConnectionString};"))
            {
                connection.Open();

                string sql = File.ReadAllText("SQL/Update/0001.sqlite");

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static VersionsItem GetVersions(DatabaseConnection dbconnect)
        {
            var item = new VersionsItem();

            using (var connection = new SqliteConnection($"Data Source={GlobalVar.ConnectionString};"))
            {
                connection.Open();

                string query = "SELECT DatabaseVersion, DatabaseLastUpdate, AppVersion FROM DataSettings LIMIT 1;";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.DatabaseVersion = reader.GetInt32(0);
                        item.LastDatabaseUpdate = reader.GetDateTime(1);
                        item.AppVersion = reader.GetInt32(2);
                    }
                }
            }

            return item;
        }

        public static void UpdateDatabaseFile(DatabaseConnection dbconnect)
        {
            // 1. Lire la version actuelle
            VersionsItem versions = GetVersions(dbconnect);
            int currentVersion = versions.DatabaseVersion;

            Console.WriteLine($"Current DB version: {currentVersion}");

            // 2. Récupérer tous les fichiers de migration
            string updateFolder = "SQL/Update/";
            var files = Directory.GetFiles(updateFolder, "*.sqlite")
                                 .OrderBy(f => f) // tri naturel : 0001, 0002, 0003...
                                 .ToList();

            foreach (var file in files)
            {
                // Extraire le numéro du fichier (ex: 0002 → 2)
                string fileName = Path.GetFileNameWithoutExtension(file); // "0002"
                if (!int.TryParse(fileName, out int scriptVersion))
                    continue;

                // Si le script est déjà appliqué → on skip
                if (scriptVersion <= currentVersion)
                    continue;

                Console.WriteLine($"Applying migration {scriptVersion} from {file}");

                // 3. Lire le script SQL
                string sql = File.ReadAllText(file);

                // 4. Exécuter le script
                using (var connection = new SqliteConnection($"Data Source={GlobalVar.ConnectionString};"))
                {
                    connection.Open();

                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 5. Mettre à jour la version dans DataSettings
                    string updateVersionSql = @"
                UPDATE DataSettings
                SET DatabaseVersion = $version,
                    DatabaseLastUpdate = CURRENT_TIMESTAMP;
            ";

                    using (var updateCmd = new SqliteCommand(updateVersionSql, connection))
                    {
                        updateCmd.Parameters.AddWithValue("$version", scriptVersion);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Database updated to version {scriptVersion}");

                // Mettre à jour la version locale pour continuer la boucle
                currentVersion = scriptVersion;
            }

            Console.WriteLine("Database is up to date.");
        }
    }
}