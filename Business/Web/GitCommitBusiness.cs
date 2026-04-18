using Bastocos.Entity.Admin;
using System.Diagnostics;

namespace BastocosR2.Business.Web
{
    internal class GitCommitBusiness
    {
        internal void DoCommit(EnvItem env)
        {
            string commitName = $"Autocommit {env.SessionStartDate:dd/MM/yyyy} - {env.SessionCommitCount}";
            string webFolder = Path.Combine(AppContext.BaseDirectory, "web");

            if (!Directory.Exists(webFolder))
                throw new DirectoryNotFoundException($"Dossier introuvable : {webFolder}");

            // Exécute une commande git dans le dossier ./web/
            void RunGit(string args)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    WorkingDirectory = webFolder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Console.WriteLine(output);
                if (!string.IsNullOrWhiteSpace(error))
                    Console.WriteLine("⚠️ GIT ERROR: " + error);
            }

            // Séquence des commandes
            RunGit("pull origin master");
            RunGit("add ./UsersData/*");
            RunGit("add ./users.json");
            RunGit("add ./globaldata.json");
            RunGit($"commit -m \"{commitName}\"");
            RunGit("push origin master");

            env.SessionCommitCount++;
        }
    }
}