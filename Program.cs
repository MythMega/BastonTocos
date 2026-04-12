using Bastocos.Business.Database;
using Bastocos.Business.Match;
using Bastocos.Controller.Admin;
using Bastocos.Controller.Export;
using Bastocos.Controller.Match;
using Bastocos.Controller.User;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bastocos
{
    internal class Program
    {
        private static bool _running = true;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Chargement des données.");
            EnvItem GlobalEnvironmentItem = new();
            GlobalEnvironmentItem.LoadAllData();
            GlobalEnvironmentItem.ExportWebData();
            Console.WriteLine("Données initialisées.");

            Console.WriteLine("Chargement des paramètres.");
            SettingsItem GlobalSettingsItems = new();
            GlobalSettingsItems = JsonSerializer.Deserialize<SettingsItem>(File.ReadAllText($"./Data/Settings.json"))!;
            Console.WriteLine("Paramètres initialisés.");

            // chargement du systeme de bdd
            SQLitePCL.Batteries.Init();

            if (!DatabaseManagement.DatabaseExist())
            {
                DatabaseManagement.CreateDatabaseFile();
            }

            DatabaseManagement.UpdateDatabaseFile();

            JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

            // --- CONFIG HTTP LISTENER ---
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{GlobalSettingsItems.ServerPort}/"); // écoute sur port configuré
            listener.Start();

            Console.WriteLine($"Écoute sur http://localhost:{GlobalSettingsItems.ServerPort}/");

            #region Controllers

            var userController = new UserController();
            var assautController = new MatchController();
            var adminController = new AdminController();
            var exportController = new ExportController();

            #endregion Controllers

            while (_running)
            {
                var context = await listener.GetContextAsync();

                var request = context.Request;
                var response = context.Response;
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.OutputStream.Close();
                    continue;
                }

                // CORS
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                if (GlobalEnvironmentItem.LastMatchEnd.AddMinutes(GlobalSettingsItems.MatchSettings.DelayBetweenTwoMatch) <= DateTime.Now
                    && GlobalEnvironmentItem.CurrentMatch is not null
                    && !GlobalEnvironmentItem.FightQueue.Any(fq => fq.RequestStatut == Entity.Match.Request.RequestStatut.Finishing)
                    && !GlobalEnvironmentItem.FightQueue.Any(fq => fq.RequestStatut == Entity.Match.Request.RequestStatut.Running))
                {
                    // rajoute x minutes de vide
                    //GlobalEnvironmentItem.LastMatchEnd = DateTime.Now;
                    GlobalEnvironmentItem.CurrentMatch = null;
                }
                if (GlobalEnvironmentItem.CurrentMatch == null &&
                    GlobalEnvironmentItem.LastMatchEnd.AddMinutes(GlobalSettingsItems.MatchSettings.DelayBetweenTwoMatch) <= DateTime.Now)
                {
                    if (GlobalEnvironmentItem.FightQueue.Any(fq => fq.RequestStatut == Entity.Match.Request.RequestStatut.In_Queue))
                    {
                        new MatchBusiness().Start(GlobalEnvironmentItem, GlobalSettingsItems.MatchSettings);
                    }
                    else
                    {
                        GlobalEnvironmentItem.LastMatchEnd = DateTime.Now;
                    }
                }
                _ = Task.Run(() => HandleRequest(context,

                    // controlleurs
                    userController,
                    assautController,
                    adminController,
                    exportController,

                    // data env & settings
                    GlobalEnvironmentItem, GlobalSettingsItems));
            }
        }

        private static async Task HandleRequest(
            // Request and content
            HttpListenerContext context,

            // Controllers
            UserController userCtrl,
            MatchController matchCtrl,
            AdminController adminCtrl,
            ExportController exportController,

            // Data
            EnvItem GlobalEnvironmentItem,
            SettingsItem GlobalSettingsItems
            )
        {
            string path = context.Request.Url.AbsolutePath.ToLower();
            string method = context.Request.HttpMethod;
            if (method == "POST")
                Console.WriteLine($"Requête reçue : {method} {path}");

            string response = String.Empty;
            // --- ROUTING ---
            if (path.StartsWith("/user"))
            {
                response = await userCtrl.HandleAsync(context, GlobalEnvironmentItem, GlobalSettingsItems);
            }
            else if (path.StartsWith("/match"))
            {
                response = await matchCtrl.HandleAsync(context, GlobalEnvironmentItem, GlobalSettingsItems);
            }
            else if (path.StartsWith("/admin"))
            {
                response = await adminCtrl.HandleAsync(context, GlobalEnvironmentItem, GlobalSettingsItems);
            }
            else if (path.StartsWith("/export"))
            {
                response = await exportController.HandleAsync(context, GlobalEnvironmentItem, GlobalSettingsItems);
            }
            else
            {
                SendResponse(context, 404, "Route inconnue");
                return;
            }

            SendResponse(context, 200, response);
        }

        public static void SendResponse(HttpListenerContext ctx, int status, string message)
        {
            ctx.Response.StatusCode = status;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
            ctx.Response.Close();
        }
    }
}