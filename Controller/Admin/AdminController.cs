using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match.Request;
using System.Net;

namespace Bastocos.Controller.Admin
{
    internal class AdminController
    {
        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = string.Empty;
            switch (context.Request.Url!.AbsolutePath.ToLower())
            {
                case "/admin/stopmatch":
                    GlobalEnvironmentItem.FightQueue.ForEach(a => a.RequestStatut = a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.Finishing ? RequestStatut.Canceled_By_Admin : a.RequestStatut);
                    GlobalEnvironmentItem.CurrentMatch = null;
                    GlobalEnvironmentItem.LastMatchEnd = DateTime.Now;
                    response = "Le match a été annulé.";
                    break;

                case "/admin/clearqueue":
                    GlobalEnvironmentItem.FightQueue.ForEach(a => a.RequestStatut = a.RequestStatut == RequestStatut.In_Queue ? RequestStatut.Canceled_By_Admin : a.RequestStatut);
                    response = "La queue a été clear.";
                    break;
            }
            return response;
        }
    }
}