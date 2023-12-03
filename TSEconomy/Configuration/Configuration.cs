using Newtonsoft.Json;
using TSEconomy.Configuration.Models;
using TShockAPI;

namespace TSEconomy.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration
    {
        private static string path = Path.Combine(TSEconomy.PluginDirectory, "TSEconomy.json");
        public static Configuration Instance { get; set; }

        [JsonProperty("TransactionLogPath")]
        public string TransactionLogPath { get; set; } = Path.Combine(TSEconomy.PluginDirectory, "TSEconomyLogs");

        [JsonProperty("UseMySQL", Order = 0)]
        public bool UseMySQL { get; set; } = false;

        [JsonProperty("Currencies")]
        public Currency[] Currencies { get; set; } = { new() };

        [JsonProperty("Language")]
        public string Language { get; set; } = "Lang_en";

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
                    TShock.Log.ConsoleError("TSEconomy.json could not be loaded: \n {0}".SFormat(ex.ToString()));
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
            if(!Directory.Exists(TSEconomy.PluginDirectory))
                Directory.CreateDirectory(TSEconomy.PluginDirectory);

            File.WriteAllText(path, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }
}
