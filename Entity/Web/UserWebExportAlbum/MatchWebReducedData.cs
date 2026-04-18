using Bastocos.Entity.Match.Request;
namespace BastocosR2.Entity.Web.UserWebExportAlbum
{
    internal class MatchWebReducedData
    {
        public string? UsernameA { get; set; }
        public string? UsernameB { get; set; }
        public int HPA { get; set; }
        public int HPB { get; set; }
        public string Statut { get; set; } = String.Empty;
    }
}
