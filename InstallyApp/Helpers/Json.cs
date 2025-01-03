using System.IO;
using System.Text.Json;
using InstallyAPI.Models;
using InstallyApp.Models;

namespace InstallyApp.Utils.Functions
{
    public class Json
    {
        public static T JsonToClass<T>(string caminhoArquivoJson)
        {
            string json = File.ReadAllText(caminhoArquivoJson);
            return JsonSerializer.Deserialize<T>(json);
        }

         public static List<PackageEntity> JsonToClassPkg(string data)
        {
            if (data == string.Empty) data = "[]";

            List<PackageEntity> dataClasse;
            
            try
            {
                dataClasse = JsonSerializer.Deserialize<List<PackageEntity>>(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return dataClasse;
        }

        public static string ClassToJson<T>(T classe)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(classe, options);
        }
    }
}