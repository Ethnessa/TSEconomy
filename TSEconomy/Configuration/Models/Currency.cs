namespace TSEconomy.Configuration.Models
{
    public class Currency
    {
        public string DisplayName { get; set; } = "Dollar";
        public string InternalName { get; set; } = "usd";
        public string Symbol { get; set; } = "$";

        public static Currency? Get(string name)
        {
            return TSEconomy.Config.Currencies.FirstOrDefault(x => x.InternalName == name || x.DisplayName == name || x.Symbol == name);
        }

        public static Currency? GetFirst()
        {
            return TSEconomy.Config.Currencies.ElementAtOrDefault(0);
        }
    }
}
