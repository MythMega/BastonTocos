using Bastocos.Business.Account;
using Bastocos.Business.Match;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.User;
using Bastocos.Entity.Web;
using Bastocos.Tools.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Controller.Match
{
    internal class MatchController
    {
        private WebRequestTreatment _webRequestTreatment = new WebRequestTreatment();
        private MatchBusiness _match = new MatchBusiness();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = String.Empty;
            switch (context.Request.Url.AbsolutePath.ToLower())
            {
                case "/match/startassault":
                    FightAssaultRequest assaultRequest = await _webRequestTreatment.ParseRequestBody<FightAssaultRequest>(context.Request);
                    _match.InitializeAssault(GlobalEnvironmentItem, assaultRequest);
                    response = "Match added in queue";
                    break;

                case "/match/getmatchinfo":
                    response = new MatchWebData
                    {
                        // global data
                        HPMax = GlobalEnvironmentItem.CurrentMatch.FighterA.HP_Max,
                        RemainingTimePercent = GlobalEnvironmentItem.CurrentMatch.GetRemainingWebTime(GlobalSettingsItems),

                        // player A
                        PseudoA = GlobalEnvironmentItem.CurrentMatch.FighterA.User.Name,
                        PictureA = "https://images2.minutemediacdn.com/image/upload/c_crop,x_904,y_171,w_2195,h_1234/c_fill,w_1440,ar_1440:810,f_auto,q_auto,g_auto/images/ImagnImages/mmsport/si/01jab1dnay1j8zhecp44.jpg",
                        HPA = GlobalEnvironmentItem.CurrentMatch.FighterA.HP_Current,
                        WeaponA = GlobalEnvironmentItem.CurrentMatch.FighterA.WeaponItem.Image,
                        ArmorA = GlobalEnvironmentItem.CurrentMatch.FighterA.ArmorItem.Image,

                        // player b
                        PseudoB = GlobalEnvironmentItem.CurrentMatch.FighterB.User.Name,
                        PictureB = "https://tubbz.com/cdn/shop/files/Steve_Minecraft_FETUBBZ_PL_1.jpg",
                        HPB = GlobalEnvironmentItem.CurrentMatch.FighterB.HP_Current,
                        WeaponB = GlobalEnvironmentItem.CurrentMatch.FighterB.WeaponItem.Image,
                        ArmorB = GlobalEnvironmentItem.CurrentMatch.FighterB.ArmorItem.Image,
                    }.ToJson();

                    break;
            }

            return response;
        }
    }
}