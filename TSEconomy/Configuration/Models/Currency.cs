namespace TSEconomy.Configuration.Models
{
    public class Currency
    {
        public string DisplayName { get; set; } = "Dollar";
        public string InternalName { get; set; } = "usd";
        public string Symbol { get; set; } = "$";

        public bool PrefixSymbol { get; set; } = true;

        public string PluralDisplayName { get; set; } = "Dollars";

        public static Currency? Get(string name)
        {
            return Api.Currencies.FirstOrDefault(x => x.InternalName == name || x.DisplayName == name || x.Symbol == name);
        }
        public bool isSystemCurrency()
        {
            // uses a not frequenly used symbol, quite a goofy ahh implementation, we could switch to IDs and add a constructor for Currencies
            return (DisplayName == "System-Cash" || InternalName == "sys" || Symbol == "^");
        }

        public static Currency? GetFirst()
        {
            return Api.Currencies.ElementAtOrDefault(0);
        }

        public string GetName(double amount, bool showSymbol = true, bool showName = false, int DecimalsKept = 2)
        {
            string name;
            string symbol = (showSymbol ? Symbol : "");
            
            if (amount <= 1)
                name = PrefixSymbol ? $"{symbol} {Math.Round(amount, DecimalsKept)} {(showName ? DisplayName : "")}" : $"{(showName ? DisplayName : "")} {Math.Round(amount, DecimalsKept)} {symbol}";

            else
                name = PrefixSymbol ? $"{symbol} {Math.Round(amount, DecimalsKept)} {(showName ? PluralDisplayName : "")}" : $"{(showName ? PluralDisplayName : "")} {Math.Round(amount, DecimalsKept)} {symbol}";

            return name;
        }
    }
}
