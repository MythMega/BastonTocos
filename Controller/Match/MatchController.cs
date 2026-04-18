using Bastocos.Business.Match;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.User;
using Bastocos.Entity.Web;
using Bastocos.Tools.Json;
using BastocosR2.Entity.Web;
using BastocosR2.Entity.Web.UserWebExportAlbum;
using System.Net;
using System.Text.Json;

namespace Bastocos.Controller.Match
{
    internal class MatchController
    {
        private readonly WebRequestTreatment _webRequestTreatment = new();
        private readonly MatchBusiness _match = new();

        internal async Task<string> HandleAsync(HttpListenerContext context, EnvItem GlobalEnvironmentItem, SettingsItem GlobalSettingsItems)
        {
            string response = String.Empty;
            switch (context.Request.Url.AbsolutePath.ToLower())
            {
                case "/match/startassault":
                    FightAssaultRequest assaultRequest = await _webRequestTreatment.ParseRequestBody<FightAssaultRequest>(context.Request);
                    response = _match.InitializeAssault(GlobalEnvironmentItem, assaultRequest);
                    break;

                case "/match/requestDuel":
                    FightDuelRequest duelRequest = await _webRequestTreatment.ParseRequestBody<FightDuelRequest>(context.Request);
                    response = _match.InitializeDuelRequest(GlobalEnvironmentItem, duelRequest);
                    break;

                case "/match/loot":
                    UserItem userWhoLoot = await _webRequestTreatment.ParseRequestBody<UserItem>(context.Request);
                    response = _match.SearchForLoot(GlobalEnvironmentItem, GlobalSettingsItems, userWhoLoot);
                    break;

                case "/match/attack":
                    UserItem userWhoAttack = await _webRequestTreatment.ParseRequestBody<UserItem>(context.Request);
                    response = _match.Attack(GlobalEnvironmentItem, GlobalSettingsItems, userWhoAttack);
                    break;

                case "/match/getqueuepending":
                    response = _match.GetQueue(GlobalEnvironmentItem);
                    break;

                case "/match/removequeueitem":
                    WebQueueWebData queuedMatchCanceller = await _webRequestTreatment.ParseRequestBody<WebQueueWebData>(context.Request);
                    response = _match.RemoveQueueItem(GlobalEnvironmentItem, queuedMatchCanceller);
                    break;

                case "/match/getcurrentmatch":
                    response = GlobalEnvironmentItem.CurrentMatch is null ? "{}" : JsonSerializer.Serialize(new MatchWebReducedData
                    {
                        UsernameA = GlobalEnvironmentItem.CurrentMatch.FighterA.User.Name,
                        UsernameB = GlobalEnvironmentItem.CurrentMatch.FighterB.User.Name,
                        HPA = GlobalEnvironmentItem.CurrentMatch.FighterA.HP_Current,
                        HPB = GlobalEnvironmentItem.CurrentMatch.FighterB.HP_Current,
                        Statut = GlobalEnvironmentItem.CurrentMatch.Finished ? "Terminé" : "En cours"
                    });
                    break;

                case "/match/healplayer":

                    break;

                case "/match/getmatchinfo":
                    response = GlobalEnvironmentItem.CurrentMatch is null ? "{}" : new MatchWebData
                    {
                        // global data
                        HPMax = GlobalEnvironmentItem.CurrentMatch.FighterA.HP_Max,
                        RemainingTimePercent = GlobalEnvironmentItem.CurrentMatch.GetRemainingWebTime(GlobalSettingsItems),
                        ImageCenter = "https://upload.wikimedia.org/wikipedia/commons/7/70/Street_Fighter_VS_logo.png",
                        WebHitDatas = GlobalEnvironmentItem.CurrentMatch.GetRemainingWebActions(),

                        // player A
                        PseudoA = GlobalEnvironmentItem.CurrentMatch.FighterA.User.Name,
                        PictureA = GlobalEnvironmentItem.CurrentMatch.FighterA.User.Avatar,
                        HPA = GlobalEnvironmentItem.CurrentMatch.FighterA.HP_Current,
                        WeaponA = GlobalEnvironmentItem.CurrentMatch.FighterA.WeaponItem.Image,
                        ArmorA = GlobalEnvironmentItem.CurrentMatch.FighterA.ArmorItem.Image,
                        ArmorAValue = GlobalEnvironmentItem.CurrentMatch.FighterA.ArmorItem.Defense,
                        WeaponAValue = GlobalEnvironmentItem.CurrentMatch.FighterA.WeaponItem.Attack,

                        // player B
                        PseudoB = GlobalEnvironmentItem.CurrentMatch.FighterB.User.Name,
                        PictureB = GlobalEnvironmentItem.CurrentMatch.FighterB.User.Avatar,
                        HPB = GlobalEnvironmentItem.CurrentMatch.FighterB.HP_Current,
                        WeaponB = GlobalEnvironmentItem.CurrentMatch.FighterB.WeaponItem.Image,
                        ArmorB = GlobalEnvironmentItem.CurrentMatch.FighterB.ArmorItem.Image,
                        ArmorBValue = GlobalEnvironmentItem.CurrentMatch.FighterB.ArmorItem.Defense,
                        WeaponBValue = GlobalEnvironmentItem.CurrentMatch.FighterB.WeaponItem.Attack,
                    }.ToJson();
                    Console.WriteLine(response);

                    break;
            }
            return response;
        }
    }
}