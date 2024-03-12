using IL.Terraria.Graphics;
using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy
{
    public static class Api
    {
        // TO DO: port a bunch of the methods in their respective class


        internal static void LoadAccounts()
        {
            BankAccounts.AddRange(DB.Query<BankAccount>("SELECT * FROM BankAccounts").ToList());
        }

        /// <summary>
        /// Static instance of our config, can also be access more simply with TSEconomy.Config
        /// </summary>
        public static Configuration.Configuration Config => Configuration.Configuration.Instance;

        /// <summary>
        /// Private reference to our database, can only be accessed from Api class members
        /// </summary>
        private static IDatabase DB => TSEconomy.DB.DB;

        /// <summary>
        /// Represents a list of valid currencies
        /// </summary>
        internal static List<Currency> Currencies { get; set; } = new();

        public static Currency SystemCurrency
        {
            get
            {
                return Currencies.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns a copy of TSEconomy's currency list
        /// </summary>
        public static List<Currency> GetCurrencies()
        {
            Currency[] arr = new Currency[Currencies.Count];

            Currencies.CopyTo(arr);

            return arr.ToList();
        }

        public static bool AddCurrency(Currency currency)
        {
            if (Currencies.Any(i => i.InternalName == currency.InternalName))
                return false;

            Currencies.Add(currency);
            return true;
        }

        public static bool RemoveCurrency(Currency currency)
        {
            if (currency.IsSystemCurrency() || !Currencies.Contains(currency))
                return false;

            Currencies.Remove(currency);
            return true;
        }

        public static bool AddCurrency(string displayName, string internalName, string symbol, string pluralDisplayName, bool prefixSymbol)
        {
            if (Currencies.Any(i => i.InternalName == internalName))
                return false;

            Currencies.Add(new(displayName, internalName, symbol, pluralDisplayName, prefixSymbol));

            return true;
        }

        public static bool IsCurrencyValid(Currency curr)
        {
            return Currencies.Contains(curr);
        }

        public static List<BankAccount> BankAccounts
        {
            get; private set;

        } = new();

        public static BankAccount WorldAccount
        {
            get
            {
                if (!BankAccounts.Any(i => i.Flags == BankAccountProperties.WorldAccount))
                {
                    var worldAcc = BankAccount.TryCreateNewAccount(SystemCurrency, 0, -1, BankAccountProperties.WorldAccount,
                                                                Localization.TryGetString("{0} has created a new world account for the server ({1}), initial value was set to {2}"));

                    return worldAcc;
                }

                return BankAccounts.First(i => i.Flags == BankAccountProperties.WorldAccount);
            }
        }
        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

        public static bool HasBankAccount(TSPlayer player)
        {
            return HasBankAccount(player.Account.ID);
        }

        public static bool HasBankAccount(int userId)
        {
            return BankAccounts.Any(i => i.UserID == userId);
        }
        public static BankAccount GetBankAccount(int userId)
        {
            var bankAccount = BankAccounts.FirstOrDefault(i => i.UserID == userId);

            if (bankAccount == null)
            {
                return BankAccount.TryCreateNewAccount(SystemCurrency, 0, userId);
            }
            return bankAccount;
        }

        public static void UpdateBankAccount(BankAccount account)
        {
            DB.Update(account);
        }

        public static void InsertBankAccount(BankAccount account)
        {
            DB.Insert(account);
            BankAccounts.Add(account);
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            TransactionLogging.Log(Localization.TryGetString("{0} had their bank account deleted.").SFormat(GetAccountName(account.ID)));
            DB.Delete(account);
            BankAccounts.Remove(account);
        }

        public static bool TryTransferbetween(BankAccount payee, BankAccount receiver, Currency curr,double amount)
        {
            if (amount < 0)
                TryTransferbetween(receiver, payee, curr, -amount);

            if (payee.GetBalance(curr) >= amount || payee.IsWorldAccount())
            {
                var receiverName = Api.GetAccountName(receiver.UserID);
                var payeeName = Api.GetAccountName(payee.UserID);

                payee.TryRemoveBalance(amount, curr, Localization.TryGetString("{{0}} has transfered {{1}} to {0}. Old bal: {{2}} new bal {{3}}").SFormat(receiverName));

                receiver.TryAddBalance(amount, curr, Localization.TryGetString("{{0}} has received {{1}} from {0}. Old bal: {{2}} new bal {{3}}").SFormat(payeeName));

                return true;
            }
            return false;
        }


        public static Database.Models.Transaction AddTransaction(int userID, string internalCurrencyName, double amountChanged, string transLogMessage, TransactionProperties flags)
        {
            var trans = new Database.Models.Transaction(userID, internalCurrencyName, amountChanged, transLogMessage, flags);

            InsertTransaction(trans);
            return trans;
        }

        public static bool HasEnough(BankAccount account, Currency curr, double amount)
        {
            if (account.GetBalance(curr) >= amount)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a TShock user account and player from a string, which can be either a username or a player name.
        /// </summary>
        /// <param name="userInput">The string used to identify the player or account. It can be either a username or an in-game player name.</param>
        /// <param name="onlinePlayer">
        /// When this method returns, contains the TSPlayer instance associated with the specified user input if the player is online;
        /// otherwise, null if the player is not online or does not exist. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// The UserAccount associated with the specified user input if found; otherwise, null if the account does not exist.
        /// </returns>
        public static UserAccount? GetUser(string userInput, out TSPlayer? onlinePlayer)
        {
            var player = TSPlayer.FindByNameOrID(userInput);
            if (player.Any())
            {
                onlinePlayer = player.FirstOrDefault();
                return onlinePlayer.Account;
            }

            var account = TShock.UserAccounts.GetUserAccountByName(userInput);
            if (account != null)
            {
                onlinePlayer = TShock.Players.FirstOrDefault(x => x?.Account?.Name == account.Name);
                return account;
            }

            onlinePlayer = null;
            return null;
        }


        public static string? GetAccountName(int UserID)
        {
            if (UserID == -1)
            {
                return TSPlayer.Server.Name;
            }

            var user = TShock.UserAccounts.GetUserAccountByID(UserID);
            if (user == null)
            {
                return null;
            }

            return user.Name;
        }
    }
}
