using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Business.Web
{
    internal class WebExporterBusiness
    {
        public void ExportFile(string content, string relativeFolderLocation, string filename)
        {
            // Combine le chemin du dossier et le nom du fichier
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), relativeFolderLocation);

            // Crée le dossier s'il n'existe pas
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Chemin complet du fichier
            string filePath = Path.Combine(folderPath, filename);

            // Écrit le contenu dans le fichier
            File.WriteAllText(filePath, content);
        }

        public void ExportFileAbsolute(string content, string FolderLocation, string filename)
        {
            // Combine le chemin du dossier et le nom du fichier
            string folderPath = Path.Combine(FolderLocation);

            // Crée le dossier s'il n'existe pas
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Chemin complet du fichier
            string filePath = Path.Combine(folderPath, filename);

            // Écrit le contenu dans le fichier
            File.WriteAllText(filePath, content);
        }
    }
}