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
    /// <summary>
    /// TSEconomy's API class, its the main class of the plugin used for managing, 
    /// accessing and manipulating various data.
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// Loads the accounts into a list
        /// </summary>
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
        
        /// <summary>
        /// The System currency, it can be transfered for its exact amount
        /// to any other currency
        /// </summary>
        public static Currency? SystemCurrency

        {
            get
            {
                return Currencies.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns a copy of TSEconomy's currency list, the first currency is the
        /// system currency, the second one is the default currency
        /// </summary>
        public static List<Currency> GetCurrencies()
        {

            Currency[] arr = new Currency[Currencies.Count];

            Currencies.CopyTo(arr);

            return arr.ToList();
        }
        /// <summary>
        /// Adds the specified currency to TSEconomy's currency list
        /// </summary>
        /// <param name="currency"></param>
        /// <returns>returns false if the internal name is already taken</returns>

        public static bool AddCurrency(Currency currency)
        {
            if (Currencies.Any(i => i.InternalName == currency.InternalName))
                return false;

            Currencies.Add(currency);
            return true;
        }

        /// <summary>
        /// Adds the specified currency to TSEconomy's currency list
        /// </summary>
        /// <returns>returns false if the internal name is already taken</returns>
        public static bool AddCurrency(string displayName, string internalName, string symbol, string pluralDisplayName, bool prefixSymbol)
        {
            if (Currencies.Any(i => i.InternalName == internalName))
                return false;

            Currencies.Add(new(displayName, internalName, symbol, pluralDisplayName, prefixSymbol));

            return true;
        }

        /// <summary>
        /// Removes the specified currency from TSEconomy's currency list
        /// </summary>
        /// <param name="currency"></param>
        /// <returns>returns false if theres no specified currency in the list.</returns>
        public static bool RemoveCurrency(Currency currency)
        {
            if (currency.IsSystemCurrency() || !Currencies.Contains(currency))
                return false;

            Currencies.Remove(currency);
            return true;
        }

        /// <summary>
        /// Checks if the specified currency is within TSEconomy's currency list
        /// </summary>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static bool IsCurrencyValid(Currency curr)
        {
            return Currencies.Contains(curr);
        }

        /// <summary>
        /// A reference of the entirety of TSEconomy's bank accounts, it also contains
        /// the world account if it has already been created
        /// </summary>

        public static List<BankAccount> BankAccounts
        {
            get; private set;

        } = new();

        /// <summary>
        /// a reference to the server's worldAccount, it is used to transfer money to
        /// bank accounts for monster kills and other purposes, it is also in some way the Server's BankAccount
        /// it has a UserID of -1, if you try to get its Account via TShock's method, it would return null and create
        /// an error.
        /// </summary>
        public static BankAccount? WorldAccount
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

        /// <summary>
        /// inserts a transaction in TSEconomy's database and logs it
        /// </summary>
        /// <param name="trans"></param>
        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

        /// <summary>
        /// Checks for whether or not the specified player has a bank account
        /// in TSEconomy's database
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasBankAccount(TSPlayer player)
        {
            return HasBankAccount(player.Account.ID);
        }
        /// <summary>
        /// Checks for whether or not the specified player has a bank account
        /// in TSEconomy's database
        /// </summary>
        /// <returns></returns>
        public static bool HasBankAccount(int userId)
        {
            return BankAccounts.Any(i => i.UserID == userId);
        }

        /// <summary>
        /// Gets the Bank account of the specified user, if it is
        /// not found it creates a new one and returns it
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static BankAccount? GetBankAccount(int userId)
        {
            var bankAccount = BankAccounts.FirstOrDefault(i => i.UserID == userId);

            if (bankAccount == null)
            {
                return BankAccount.TryCreateNewAccount(SystemCurrency, 0, userId);
            }
            return bankAccount;
        }
        /// <summary>
        /// Updates the specified bank account in TSEconomy's database
        /// </summary>
        /// <param name="account"></param>
        public static void UpdateBankAccount(BankAccount account)
        {
            DB.Update(account);
        }
        /// <summary>
        /// Inserts the specified bank account in TSEconomy's database 
        /// and listed bank accounts
        /// </summary>
        /// <param name="account"></param>
        public static void InsertBankAccount(BankAccount account)
        {
            DB.Insert(account);
            BankAccounts.Add(account);
        }

        /// <summary>
        /// deletes the bank account from TSEconomy's Database and listed bank accounts
        /// </summary>
        /// <param name="account"></param>
        public static void DeleteBankAccount(BankAccount account)
        {
            TransactionLogging.Log(Localization.TryGetString("{0} had their bank account deleted.").SFormat(GetAccountName(account.ID)));
            DB.Delete(account);
            BankAccounts.Remove(account);
        }

        /// <summary>
        /// Tranfers between the payee and the receiver, returns false if
        /// the payee lacks the money
        /// </summary>
        /// <param name="payee"></param>
        /// <param name="receiver"></param>
        /// <param name="curr"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool TryTransferbetween(BankAccount payee, BankAccount receiver, Currency curr,double amount)
        {
            if (amount < 0)
                TryTransferbetween(receiver, payee, curr, -amount);

            if (payee.GetBalance(curr) >= amount || payee.IsWorldAccount())
            {
                var receiverName = Api.GetAccountName(receiver.UserID);
                var payeeName = Api.GetAccountName(payee.UserID);


                payee.TryModifyBalance(amount, curr, BalanceOperation.Subtract, Localization.TryGetString("{{0}} has transfered {{1}} to {0}. Old bal: {{2}} new bal {{3}}").SFormat(receiverName));

                receiver.TryModifyBalance(amount, curr, BalanceOperation.Add, Localization.TryGetString("{{0}} has received {{1}} from {0}. Old bal: {{2}} new bal {{3}}").SFormat(payeeName));


                return true;
            }
            return false;
        }

        /// <summary>
        /// adds a transaction
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="internalCurrencyName"></param>
        /// <param name="amountChanged"></param>
        /// <param name="transLogMessage"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Database.Models.Transaction AddTransaction(int userID, string internalCurrencyName, double amountChanged, string transLogMessage, TransactionProperties flags)
        {
            var trans = new Database.Models.Transaction(userID, internalCurrencyName, amountChanged, transLogMessage, flags);

            InsertTransaction(trans);
            return trans;
        }

        /// <summary>
        /// Checks for whether or not the specified account has more or equal to the
        /// value specified in the specified currency
        /// </summary>
        /// <param name="account"></param>
        /// <param name="curr"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the name of the user based on their TShock Account ID
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
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
