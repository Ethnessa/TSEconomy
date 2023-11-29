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
        public bool isSystemCurrency()
        {
            // uses a not frequenly used symbol, quite a goofy ahh implementation, we could switch to IDs and add a constructor for Currencies
            return (DisplayName == "System-Cash" || InternalName == "sys" || Symbol == "^");
        }


        public static Currency? GetFirst()
        {
            return TSEconomy.Config.Currencies.ElementAtOrDefault(0);
        }
    }
}
