using PetaPoco;

using Terraria;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Logging;
using TShockAPI;

namespace TSEconomy
{
    public static class Api
    {
        // TO DO: port a bunch of the methods in their respective class

        public static Configuration.Configuration Config => Configuration.Configuration.Instance;
        public static IDatabase DB => TSEconomy.DB.DB;

        private static List<Currency> _currencies = new List<Currency>();
        public static List<Currency> Currencies
        {
            get
            {
                return _currencies.Concat(Config.Currencies).ToList();
            }   
        }

        public static Currency SystemCurrency 
        {
            get
            {
                if (!Currencies.Any(i => i.isSystemCurrency()))
                {
                    var systemCurr = new Currency()
                    {
                        DisplayName = "System-Cash",
                        InternalName = "sys",
                        Symbol = "^"
                    };

                    _currencies.Add(systemCurr);
                    return systemCurr;
                }

                return Currencies.First(i => i.isSystemCurrency());
            }
        }
        public static List<BankAccount> BankAccounts {
            get
            {
                return DB.Query<BankAccount>("SELECT * FROM BankAccounts").ToList();
            }
        }

        public static BankAccount WorldAccount
        {
            get
            {
                if (!BankAccounts.Any(i => (i.Flags & BankAccountProperties.WorldAccount) == BankAccountProperties.WorldAccount))
                {
                    var worldAcc = BankAccount.TryCreateNewAccount(0, "sys", -1, BankAccountProperties.WorldAccount,
                                                                "{0} has created a new world account for the server ({1}), initial value was set to {2}");

                    return worldAcc;
                }

                return BankAccounts.First(i => (i.Flags & BankAccountProperties.WorldAccount) == BankAccountProperties.WorldAccount);
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

        public static List<Currency> GetCurrencies()
        {
            return TSEconomy.Config.Currencies;
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
            AddTransaction(account.ID, account.InternalCurrencyName, 0, $"{Helpers.GetAccountName(account.ID)} had their bank account deleted.", TransactionProperties.set);
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

                payee.TryRemoveBalance(amount, "{{0}} has transfered {{1}} to {0}. Old bal: {{2}} new bal {{3}}".SFormat(receiverName));

                receiver.TryAddBalance(amount, "{{0}} has received {{1}} from {0}. Old bal: {{2}} new bal {{3}}".SFormat(payeeName));

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
