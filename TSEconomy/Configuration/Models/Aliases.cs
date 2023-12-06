using Newtonsoft.Json;
using TSEconomy.Commands;

namespace TSEconomy.Configuration.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Aliases
    {
        internal string[] GetAliases(CommandBase command) 
        {
            switch (command)
            {
                case BalanceCommand:
                    return BalanceAliases;
                case BankAdminCommand:
                    return BankAdmin;
                case ListCurrenciesCommand:
                    return ListCurrencies;
                case SendCommand:
                    return Send;
                default:
                    return new string[0];
            }
        }

        [JsonProperty("Balance")]
        internal string[] BalanceAliases { get; set; } = { "balance", "bal", "money" };

        [JsonProperty("BankAdmin")]
        internal string[] BankAdmin { get; set; } = { "bankadmin", "banka", "ecoadmin", "ba" };

        [JsonProperty("ListCurrencies")]
        internal string[] ListCurrencies { get; set; } = { "listcurrencies", "listcur", "listc", "lc", "currencies" };

        [JsonProperty("Send")]
        internal string[] Send { get; set; } = { "send", "pay", "give" };

    }
}
