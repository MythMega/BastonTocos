using Bastocos.Business.Account;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.User;
using Bastocos.Tools.Json;
using System.Net;
using System.Threading.Tasks;

namespace Bastocos.Controller.User
{
    internal class UserController
    {
        private readonly WebRequestTreatment _webRequestTreatment = new();
        private readonly AccountManagement _accountManagement = new();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = string.Empty;
            switch (context.Request.Url!.AbsolutePath.ToLower())
            {
                case "/user/register":
                    UserItem userItem = await _webRequestTreatment.ParseRequestBody<UserItem>(context.Request);
                    response = _accountManagement.CreateAccount(userItem, GlobalEnvironmentItem);
                    GlobalEnvironmentItem.RefreshLastActivity(userItem);
                    break;

                case "/user/ready":
                    UserItem userToBeReady = await _webRequestTreatment.ParseRequestBody<UserItem>(context.Request);
                    GlobalEnvironmentItem.RefreshLastActivity(userToBeReady);
                    response = "Tu as été ajouté comme actif pendant 45min";
                    break;

                case "/user/sellgoods/":
                    break;
            }
            return response;
        }
    }
}