using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match.Request;
using Bastocos.Tools.Json;
using BastocosR2.Business.User;
using BastocosR2.Entity.Match;
using BastocosR2.Entity.User;
using System.Net;

namespace Bastocos.Controller.Admin
{
    internal class AdminController
    {
        private readonly WebRequestTreatment _webRequestTreatment = new();
        private readonly UserBusiness _userBusiness = new UserBusiness();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = string.Empty;
            switch (context.Request.Url!.AbsolutePath.ToLower())
            {
                case "/admin/stopmatch":
                    CurrentMatchCancellationItem currentMatchCancellationItem = await _webRequestTreatment.ParseRequestBody<CurrentMatchCancellationItem>(context.Request);
                    GlobalEnvironmentItem.FightQueue.ForEach(a => a.RequestStatut = (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.Finishing) ?
                    RequestStatut.Canceled_By_Admin :
                    a.RequestStatut);
                    GlobalEnvironmentItem.CurrentMatch = null;
                    GlobalEnvironmentItem.LastMatchEnd = DateTime.Now;
                    response = "Le match a été annulé.";
                    break;

                case "/admin/clearqueuepending":
                    GlobalEnvironmentItem.FightQueue.ForEach(a => a.RequestStatut = a.RequestStatut == RequestStatut.In_Queue ? RequestStatut.Canceled_By_Admin : a.RequestStatut);
                    response = "La queue a été clear.";
                    break;

                case "/admin/setinactive":
                    IdContainerItem idToSetInactive = await _webRequestTreatment.ParseRequestBody<IdContainerItem>(context.Request);
                    _userBusiness.LeaveActiveId(idToSetInactive, GlobalEnvironmentItem);
                    response = "c'est tout good";
                    break;
            }
            return response;
        }
    }
}