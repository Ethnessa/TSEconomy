using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TShockAPI;

namespace TSEconomy
{
    public static class Api
    {
        // TO DO: port a bunch of the methods in their respective class

        /// <summary>
        /// Static instance of our config, can also be access more simply with TSEconomy.Config
        /// </summary>
        public static Configuration.Configuration Config => Configuration.Configuration.Instance;

        /// <summary>
        /// Private reference to our database, can only be accessed from Api class members
        /// </summary>
        private static IDatabase s_DB => TSEconomy.s_DB.DB;
        
        // ? why are we splitting these?
        private static List<Currency> s_currencies = new List<Currency>();
        public static List<Currency> Currencies
        {
            get
            {
                return s_currencies.Concat(Config.Currencies).ToList();
            }
        }

        public static Currency SystemCurrency { get; } = new Currency()
        {
            DisplayName = "System-Cash",
            InternalName = "sys",
            Symbol = "^"
        };
        
        public static List<BankAccount> BankAccounts
        {
            get
            {
                return s_DB.Query<BankAccount>("SELECT * FROM BankAccounts").ToList();
            }
        }

        public static BankAccount WorldAccount
        {
            get
            {
                if (!BankAccounts.Any(i => i.Flags == BankAccountProperties.WorldAccount))
                {
                    var worldAcc = BankAccount.TryCreateNewAccount(0, "sys", -1, BankAccountProperties.WorldAccount,
                                                                Localization.TryGetString("{0} has created a new world account for the server ({1}), initial value was set to {2}"));

                    return worldAcc;
                }

                return BankAccounts.First(i => i.Flags == BankAccountProperties.WorldAccount);
            }
        }

        public static List<BankAccount> GetAllBankAccountsByCurrency(string currencyInternalName)
        {
            return s_DB.Query<BankAccount>("SELECT * FROM BankAccounts Where Currency = @0", currencyInternalName).ToList();
        }

        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            s_DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

        public static bool HasBankAccount(TSPlayer player, Currency curr)
        {
            return HasBankAccount(player.Account.ID, curr);
        }

        public static bool HasBankAccount(int userId, Currency curr)
        {
            return s_DB.ExecuteScalar<int>("SELECT COUNT(*) FROM BankAccounts Where UserID = @0 AND Currency = @1", userId, curr.InternalName) > 0;
        }
        public static BankAccount GetBankAccount(int userId, Currency curr)
        {
            var bankAccount = s_DB.FirstOrDefault<BankAccount>("SELECT * FROM BankAccounts WHERE UserID = @0 AND Currency = @1", userId, curr.InternalName);
            if (bankAccount == null)
            {
                return BankAccount.TryCreateNewAccount(0, curr.InternalName, userId);
            }
            return bankAccount;
        }

        public static List<Currency> GetCurrencies()
        {
            return TSEconomy.Config.Currencies.ToList();
        }

        public static void UpdateBankAccount(BankAccount account)
        {
            s_DB.Update(account);
        }

        public static void InsertBankAccount(BankAccount account)
        {
            s_DB.Insert(account);
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            AddTransaction(account.ID, account.InternalCurrencyName, 0, Localization.TryGetString("{0} had their bank account deleted.").SFormat(Helpers.GetAccountName(account.ID)), TransactionProperties.Set);
            s_DB.Delete(account);
        }

        public static bool TryTransferTo(BankAccount payee, BankAccount receiver, double amount)
        {
            if (amount < 0)
                TryTransferTo(receiver, payee, -amount);

            if (payee.Balance >= amount || payee.IsWorldAccount())
            {
                var receiverName = Helpers.GetAccountName(receiver.UserID);
                var payeeName = Helpers.GetAccountName(payee.UserID);

                payee.TryRemoveBalance(amount, Localization.TryGetString("{{0}} has transfered {{1}} to {0}. Old bal: {{2}} new bal {{3}}").SFormat(receiverName));

                receiver.TryAddBalance(amount, Localization.TryGetString("{{0}} has received {{1}} from {0}. Old bal: {{2}} new bal {{3}}").SFormat(payeeName));

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

        public static bool HasEnough(BankAccount account, double amount)
        {
            if (account.Balance >= amount)
            {
                return true;
            }
            return false;
        }
    }
}
