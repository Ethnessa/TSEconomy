using Newtonsoft.Json;
using TSEconomy.Configuration.Models;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration
    {
        private static string configPath = Path.Combine(TSEconomy.PluginDirectory, "TSEconomy.json");
        public static Configuration Instance { get; set; }

        [JsonProperty("UseMySQL", Order = 0)]
        public bool UseMySQL { get; set; } = false;

        [JsonProperty("TransactionLogPath", Order = 1)]
        public string TransactionLogPath { get; set; } = Path.Combine(TSEconomy.PluginDirectory, "TSEconomyLogs");
        [JsonProperty("LocalizedTextsPath", Order = 2)]
        public string LocalizationDirectory { get; set; } = Path.Combine(TSEconomy.PluginDirectory, "Localization");

        [JsonProperty("Language", Order = 3)]
        public string Language { get; set; } = Localization.GetCurrentlyUsedLanguage();

        [JsonProperty("Currencies", Order = 4)]
        public Currency[] Currencies { get; set; } = { new() };
        [JsonProperty("ResetBalancesOnNewWorld", Order = 5)]
        public bool ResetBalancesOnNewWorld { get; set; } = false;

        [JsonProperty("MaxLogFilesAllowed", Order = 6)]
        public int MaxLogFilesAllowed { get; set; } = 15;

        // try and make this last always if possible
        [JsonProperty("CommandAliases", Order = 7)]
        public Aliases Aliases { get; set; } = new();

        public static void Load()
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
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
            Instance ??= new Configuration();

            if(!Directory.Exists(TSEconomy.PluginDirectory))
                Directory.CreateDirectory(TSEconomy.PluginDirectory);

            File.WriteAllText(configPath, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }
}
