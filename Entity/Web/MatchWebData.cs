using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bastocos.Entity.Web
{
    public class MatchWebData
    {
        public int RemainingTimePercent { get; set; }
        public string PseudoA { get; set; }
        public string PictureA { get; set; }
        public string ArmorA { get; set; }
        public string WeaponA { get; set; }
        public int HPA { get; set; }
        public int HPMax { get; set; }
        public string ImageCenter { get; set; }
        public string PseudoB { get; set; }
        public string PictureB { get; set; }
        public string WeaponB { get; set; }
        public string ArmorB { get; set; }
        public int HPB { get; set; }

        // Méthode qui renvoie le JSON complet
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, options);
        }
    }
}