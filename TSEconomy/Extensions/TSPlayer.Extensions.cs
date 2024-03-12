using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TShockAPI;

namespace TSEconomy.Extensions
{
    public static class TSPlayerExtensions
    {
        // these are mostly to avoid writing lots of text just to get bank accounts &
        // to avoid getting null references with manipulating Server Accounts.
        public static int GetUserId(this TSPlayer player)
        {
            if (!player.RealPlayer)
                return -1;

            return player.Account.ID;
        }

        public static BankAccount GetBankAccount(this TSPlayer player, Currency curr)
        {
            if (!player.RealPlayer)
                return Api.WorldAccount;

            return Api.GetBankAccount(player.Account.ID);
        }

        public static bool HasBankAccount(this TSPlayer player, Currency curr)
        {
            if (!player.RealPlayer)
                return true;

            return Api.HasBankAccount(player.Account.ID);
        }


    }
}
