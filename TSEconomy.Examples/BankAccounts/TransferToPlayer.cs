using TSEconomy.Database.Models;
using TShockAPI;

namespace Examples
{
    public class TransferToPlayer
    {
        // how do we transfer money from one player to another? john doe -> jane doe
        public void Example(BankAccount johnDoe, BankAccount janeDoe)
        {
            // for this example, since we have already shown how to fetch bank accounts and currencies, we will pass them in through the arguments

            // it's quite simple to make a transaction, for example we will transfer 50 bucks from john doe to jane doe

            johnDoe.TryTransferTo(janeDoe, 50.00); // (-$50) john doe -> jane doe (+$50)

            // we can check to see if the user actually had enough money to make this happen like so:

            bool success = janeDoe.TryTransferTo(johnDoe, 50.00); // (-$50) jane doe -> john doe (+$50)
            if (success)
            {
                TShock.Log.ConsoleInfo("Transaction succeeded!");
            }
            else
            {
                TShock.Log.ConsoleWarn("User did not have enough funds.");
            }
        }
    }
}
