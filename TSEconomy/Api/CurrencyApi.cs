using TSEconomy.Configuration.Models;

namespace TSEconomy.Api
{
    /// <summary>
    /// Manages currency operations.
    /// </summary>
    public static class CurrencyApi
    {
        public static Dictionary<string,Currency> Currencies { get; set; } = new();

        public static Currency? SystemCurrency => Currencies.Values.FirstOrDefault();

        public static bool AddCurrency(Currency currency)
        {
            if (Currencies.Values.Any(i => i.InternalName == currency.InternalName))
                return false;

            Currencies.Add(currency.InternalName, currency);
            return true;
        }

        public static bool AddCurrency(string displayName, string internalName, string symbol, string pluralDisplayName, bool prefixSymbol)
        {
            if (Currencies.Values.Any(i => i.InternalName == internalName))
                return false;

            Currencies.Add(internalName,new(displayName, internalName, symbol, pluralDisplayName, prefixSymbol));
            return true;
        }

        public static bool RemoveCurrency(Currency currency)
        {
            if (currency.IsSystemCurrency() || !Currencies.Values.Contains(currency))
                return false;

            Currencies.Remove(currency.InternalName);
            return true;
        }

        public static bool IsCurrencyValid(Currency curr) => Currencies.Values.Contains(curr);
    }
}