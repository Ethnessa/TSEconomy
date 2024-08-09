using TSEconomy.Api;

namespace TSEconomy.Configuration.Models
{
    public class Currency
    {
        public string DisplayName { get; set; } = "Dollar";
        public string InternalName { get; set; } = "usd";
        public string Symbol { get; set; } = "$";

        public bool PrefixSymbol { get; set; } = true;

        public string PluralDisplayName { get; set; } = "Dollars";

        public Currency(string displayName,  string internalName, string symbol, string pluralDisplayName, bool prefixSymbol)
        {
            DisplayName = displayName;
            InternalName = internalName;
            Symbol = symbol;
            PluralDisplayName = pluralDisplayName;
            PrefixSymbol = prefixSymbol;
        }
        public Currency() { }
        public static Currency? Get(string name)
        {
            string n = name.ToLower();

            return CurrencyApi.Currencies.Values.FirstOrDefault(x => x.InternalName.ToLower().StartsWith(n) || x.DisplayName.ToLower().StartsWith(n) || x.Symbol == n);
        }
        public bool IsSystemCurrency()
        {
            return this == CurrencyApi.SystemCurrency;

        }

        public static Currency? GetDefault()
        {
            return CurrencyApi.Currencies.Values.ElementAtOrDefault(1);
        }

        public string GetName(double amount, bool showSymbol = true, bool showName = false, int DecimalsKept = 2)
        {
            string name;
            string symbol = showSymbol ? Symbol : "";

            if (amount <= 1)
                name = PrefixSymbol ? $"{symbol} {Math.Round(amount, DecimalsKept)} {(showName ? DisplayName : "")}" : $"{(showName ? DisplayName : "")} {Math.Round(amount, DecimalsKept)} {symbol}";

            else
                name = PrefixSymbol ? $"{symbol} {Math.Round(amount, DecimalsKept)} {(showName ? PluralDisplayName : "")}" : $"{(showName ? PluralDisplayName : "")} {Math.Round(amount, DecimalsKept)} {symbol}";

            return name;
        }
    }
}
