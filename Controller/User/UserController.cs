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
        private WebRequestTreatment _webRequestTreatment = new WebRequestTreatment();
        private AccountManagement _accountManagement = new AccountManagement();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = string.Empty;
            switch (context.Request.Url.AbsolutePath.ToLower())
            {
                case "/user/register":
                    UserItem userItem = await _webRequestTreatment.ParseRequestBody<UserItem>(context.Request);
                    response = _accountManagement.CreateAccount(userItem, GlobalEnvironmentItem);
                    break;
            }

            return response;
        }
    }
}