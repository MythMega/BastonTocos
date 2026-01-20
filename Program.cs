using System;
using System.Net;
using System.Threading.Tasks;
using Bastocos.Controller.User;
using Bastocos.Controller.Match;

namespace Bastocos
{
    internal class Program
    {
        private static bool _running = true;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Serveur Bastocos démarré...");

            // --- CONFIG HTTP LISTENER ---
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/"); // écoute sur port 8080
            listener.Start();

            Console.WriteLine("Écoute sur http://localhost:8080/");

            #region Controllers

            var userController = new UserController();
            var assautController = new AssautController();

            #endregion Controllers

            while (_running)
            {
                // Vérifier si une requête arrive sans bloquer
                var contextTask = listener.GetContextAsync();

                // Si la requête n'est pas encore arrivée, on peut faire autre chose
                while (!contextTask.IsCompleted)
                {
                    // --- IDLE ---

                    await Task.Delay(10); // éviter de bloquer le CPU
                }

                // Une requête est arrivée
                var context = contextTask.Result;
                _ = Task.Run(() => HandleRequest(context, userController, assautController));
            }
        }

        private static void HandleRequest(HttpListenerContext context, UserController userCtrl, AssautController matchCtrl)
        {
            string path = context.Request.Url.AbsolutePath.ToLower();
            string method = context.Request.HttpMethod;

            Console.WriteLine($"Requête reçue : {method} {path}");

            // --- ROUTING ---
            if (path.StartsWith("/user"))
            {
                userCtrl.Handle(context);
            }
            else if (path.StartsWith("/match"))
            {
                matchCtrl.Handle(context);
            }
            else
            {
                SendResponse(context, 404, "Route inconnue");
            }
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