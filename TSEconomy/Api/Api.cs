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
        private static IDatabase DB => TSEconomy.DB.DB;
        
        /// <summary>
        /// Represents a list of valid currencies
        /// </summary>
        internal static List<Currency> Currencies { get; set; } = new();

        public static Currency SystemCurrency { 
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

        public static void AddCurrency(string displayName, string internalName, string symbol, string pluralDisplayName, bool prefixSymbol)
        {
            Currencies.Add(new(displayName, internalName, symbol, pluralDisplayName, prefixSymbol));
        }

        public static bool IsCurrencyValid(Currency curr)
        {
            return Currencies.Contains(curr);
        }

        public static List<BankAccount> BankAccounts
        {
            get
            {
                return DB.Query<BankAccount>("SELECT * FROM BankAccounts").ToList();
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
            return DB.Query<BankAccount>("SELECT * FROM BankAccounts Where Currency = @0", currencyInternalName).ToList();
        }

        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

        public static bool HasBankAccount(TSPlayer player, Currency curr)
        {
            return HasBankAccount(player.Account.ID, curr);
        }

        public static bool HasBankAccount(int userId, Currency curr)
        {
            return DB.ExecuteScalar<int>("SELECT COUNT(*) FROM BankAccounts Where UserID = @0 AND Currency = @1", userId, curr.InternalName) > 0;
        }
        public static BankAccount GetBankAccount(int userId, Currency curr)
        {
            var bankAccount = DB.FirstOrDefault<BankAccount>("SELECT * FROM BankAccounts WHERE UserID = @0 AND Currency = @1", userId, curr.InternalName);
            if (bankAccount == null)
            {
                return BankAccount.TryCreateNewAccount(0, curr.InternalName, userId);
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
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            AddTransaction(account.ID, account.InternalCurrencyName, 0, Localization.TryGetString("{0} had their bank account deleted.").SFormat(Helpers.GetAccountName(account.ID)), TransactionProperties.Set);
            DB.Delete(account);
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
