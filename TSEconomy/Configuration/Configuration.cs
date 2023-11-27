using Newtonsoft.Json;
using TSEconomy.Configuration.Models;
using TShockAPI;

namespace TSEconomy.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration
    {
        private static string path = Path.Combine(TShock.SavePath, "TSEconomy.json");
        public static Configuration Instance { get; set; }

        [JsonProperty("UseMySQL", Order = 0)]
        public bool UseMySQL { get; set; } = false;

        [JsonProperty("Currencies")]
        public List<Currency> Currencies { get; set; } = new() { new() };

        public static void Load()
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                try
                {
                    Instance = JsonConvert.DeserializeObject<Configuration>(json);
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError($"TSEconomy.json could not be loaded: \n {ex.ToString()}");
                }
            }
            else
            {
                Save();
            }
        }

        public static void Save()
        {
            if (Instance == null)
            {
                Instance = new Configuration();
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }
}
