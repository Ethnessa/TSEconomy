using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TShockAPI;

namespace Examples
{
    internal class GetBalanceOfUser
    {
        public void Example()
        {
            // fetch user john doe
            var user = TSEconomy.Helpers.GetUser("johndoe", out var onlinePlayer); // if the user account has an online player, we can utilize it as a TSPlayer, otherwise it is null

            if (user == null) return; // our user account doesn't exist

            // fetch USD
            Currency? usd = Currency.Get("usd"); // we are fetching our currency from it's internal name

            if (usd == null) // log in console if we cant find the currency
            {
                if (onlinePlayer != null)
                {
                    onlinePlayer.SendErrorMessage("'usd' doesn't exist!");
                }

                TShock.Log.ConsoleError("We could not find that currency!");
                return;
            }

            // Fetch our user's bank account
            BankAccount bankAccount = TSEconomy.Api.GetBankAccount(user.ID, usd);

            // display the amount in the user's account
            TShock.Log.ConsoleInfo($"{user.Name} has {bankAccount.Balance} {usd.DisplayName}");
        }
    }
}
