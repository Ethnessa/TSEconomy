using TSEconomy.Configuration.Models;

namespace Examples
{
    internal class FetchSingular
    {
        public void Example()
        {
            // Fetch the first currency in the list (usually the default currency)
            var currency = Currency.GetFirst();

            // Fetch a currency by display name
            currency = Currency.Get("Gold");

            // Fetch a currency by symbol
            currency = Currency.Get("$");

            // Fetch a currency by it's internal name
            currency = Currency.Get("gold");
        }
    }
}
