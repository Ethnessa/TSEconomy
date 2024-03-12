using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Terraria.Localization;
using TShockAPI;

namespace TSEconomy.Lang
{
    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public KeyValue() 
        {
            Key = "";
            Value = "";
        }
        public KeyValue(string key, string value) 
        {
            Key = key;
            Value = value;
        }
    }
    public static class Localization
    {
        public static List<string> SupportedLanguages = new List<string> { "Lang_en", "Lang_es", "Lang_ru" };

        private static Dictionary<string, string> _localizedPluginTexts = new();

        public static void SetupLanguage()
        {
            string localizationDirectory = TSEconomy.Config.LocalizationDirectory;

            if (!Directory.Exists(localizationDirectory))
                Directory.CreateDirectory(localizationDirectory);

            getFileName(out string fileName);

            if (!File.Exists(Path.Combine(localizationDirectory, fileName)))
            {
                var assembly = Assembly.GetExecutingAssembly();

                string fileResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

                var fileStream = assembly.GetManifestResourceStream(fileResourceName);

                TextReader fileReader = new StreamReader(fileStream);

                File.WriteAllText(Path.Combine(localizationDirectory, fileName), fileReader.ReadToEnd());

            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValue>));

            using(XmlReader xmlReader = XmlReader.Create(Path.Combine(localizationDirectory, fileName)))
            {
                List<KeyValue> list= (List<KeyValue>)serializer.Deserialize(xmlReader);

                _localizedPluginTexts = list.ToDictionary(i => i.Key, i => i.Value);
            }

            TShock.Log.ConsoleInfo(TryGetString("Localization succesfully loaded from file {0}!", "Lang").SFormat(Path.Combine(localizationDirectory, fileName)));

        }

        public static string TryGetString(string key, string tag = "")
        {
            if(!_localizedPluginTexts.TryGetValue(key, out string value))
                value = key;

            if(tag == "")
                return value;

            if (tag.Equals("plugin", StringComparison.CurrentCultureIgnoreCase))
                return "[TSEconomy] " + value;

            return "[TSEconomy {0}] ".SFormat(tag) + value;

        }

        private static void getFileName(out string name)
        {
            if (SupportedLanguages.Any(i => i == TSEconomy.Config.Language))
                name = TSEconomy.Config.Language + ".xml";
            
            else
            {
                name = "Lang_en.xml";

                TShock.Log.ConsoleError($"[TSEconomy Lang] Set value of the 'Language' property in configs is not valid, defaulting to 'en-CA'.\n here are the supported languages: {string.Join(", ", SupportedLanguages)}");
            }
        }
        /// <summary>
        /// We are getting the cultureInfo of the server the same way TShock's I18n TranslationCultureInfo
        /// does, this is because TShockAPI.I18n is internal so we cant access it
        /// </summary>
        /// <returns>a "Lang_la" style language code gotten the same way TShock does</returns>
        public static string GetCurrentlyUsedLanguage()
        {
            CultureInfo cultureInfo = null;

            string environmentVariable = Environment.GetEnvironmentVariable("TSHOCK_LANGUAGE");

            if (environmentVariable != null)
            {
                cultureInfo = new CultureInfo(environmentVariable);
            }

            else if (Terraria.Program.LaunchParameters.TryGetValue("-lang", out var value) && int.TryParse(value, out var result) && GameCulture._legacyCultures.TryGetValue(result, out var value2))
            {
                cultureInfo = Redirect(value2.CultureInfo);
            }

            else if (Terraria.Program.LaunchParameters.TryGetValue("-language", out var languageArg))
            {
                GameCulture gameCulture = GameCulture._legacyCultures.Values.SingleOrDefault((GameCulture c) => c.Name == languageArg);
                if (gameCulture != null)
                {
                    cultureInfo = Redirect(gameCulture.CultureInfo);
                }
            }

            cultureInfo ??= CultureInfo.CurrentUICulture;

            static CultureInfo Redirect(CultureInfo cultureInfo)
            {
                if (!(cultureInfo.Name == "zh-Hans"))
                {
                    return cultureInfo;
                }

                return new CultureInfo("zh-CN");
            }

            return cultureInfo.Name switch
            {
                string s when s.StartsWith("es") => "Lang_es",
                string s when s.StartsWith("ru") => "Lang_ru",
                _ => "Lang_en",
            };
        }
    }
}
