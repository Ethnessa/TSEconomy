using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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
        public static List<string> supportedLanguages = new List<string> { "Lang_en", "Lang_es" };

        private static string LocalizationDirectory = Path.Combine(TSEconomy.PluginDirectory, "Localization");

        private static Dictionary<string, string> LocalizedPluginTexts = new();

        public static void SetupLanguage()
        {
            if (!Directory.Exists(LocalizationDirectory))
                Directory.CreateDirectory(LocalizationDirectory);

            getFileName(out string fileName);

            if (!File.Exists(Path.Combine(LocalizationDirectory, fileName)))
            {
                var assembly = Assembly.GetExecutingAssembly();

                string FileResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

                var FileStream = assembly.GetManifestResourceStream(FileResourceName);

                TextReader FileReader = new StreamReader(FileStream);

                File.WriteAllText(Path.Combine(LocalizationDirectory, fileName), FileReader.ReadToEnd());

            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValue>));

            using(XmlReader xmlReader = XmlReader.Create(Path.Combine(LocalizationDirectory, fileName)))
            {
                List<KeyValue> list= (List<KeyValue>)serializer.Deserialize(xmlReader);

                LocalizedPluginTexts = list.ToDictionary(i => i.Key, i => i.Value);
            }

            TShock.Log.ConsoleInfo(TryGetString("Localization succesfully loaded from file {0}!", "Lang").SFormat(Path.Combine(LocalizationDirectory, fileName)));

        }

        public static string TryGetString(string key, string tag = "")
        {
            if(!LocalizedPluginTexts.TryGetValue(key, out string value))
                value = key;

            if(tag == "")
                return value;

            if (tag.Equals("plugin", StringComparison.CurrentCultureIgnoreCase))
                return "[TSEconomy] " + value;

            return "[TSEconomy {0}] ".SFormat(tag) + value;

        }

        private static void getFileName(out string name)
        {
            if (supportedLanguages.Any(i => i == TSEconomy.Config.Language))
                name = TSEconomy.Config.Language + ".xml";
            
            else
            {
                name = "Lang_en.xml";

                TShock.Log.ConsoleError($"[TSEconomy Lang] Set value of the 'Language' property in configs is not valid, defaulting to 'en-CA'.\n here are the supported languages: {string.Join(", ", supportedLanguages)}");
            }
        }
    }
}
