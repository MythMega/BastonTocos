using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using BastocosR2.Business.Web;
using System.Net;

namespace Bastocos.Controller.Export
{
    internal class ExportController
    {
        private UserDataExporter _userDataExporter = new UserDataExporter();
        private GitCommitBusiness _gitCommitBusiness = new GitCommitBusiness();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = string.Empty;
            switch (context.Request.Url!.AbsolutePath.ToLower())
            {
                case "/export/userdata":
                    try
                    {
                        _userDataExporter.ExportAllUserDataJson(GlobalEnvironmentItem, GlobalSettingsItems);
                        response = "All Users Data Exporteds";
                    }
                    catch (Exception ex) { response = ex.Source + "\n" + ex.Message + "\n" + ex.Data; }
                    break;

                case "/export/commit":
                    try
                    {
                        _gitCommitBusiness.DoCommit(GlobalEnvironmentItem);
                        response = "Update sent";
                    }
                    catch (Exception ex) { response = ex.Source + "\n" + ex.Message + "\n" + ex.Data; }
                    break;
            }
            return response;
        }
    }
}