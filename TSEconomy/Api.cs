using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;

namespace TSEconomy
{
    public class Api
    {
        public static Configuration.Configuration Config => Configuration.Configuration.Instance;
        public static IDatabase DB => TSEconomy.DB.DB;

        public static bool HasBankAccount(int userId, Currency curr)
        {
            return DB.ExecuteScalar<int>("SELECT COUNT(*) FROM BankAccounts Where UserID = @0 AND Currency = @1", userId, curr.InternalName) > 0;
        }
        public static BankAccount GetBankAccount(int userId, Currency curr)
        {
            if (!HasBankAccount(userId, curr))
            {
                return InsertBankAccount(userId, curr);
            }
            return DB.FirstOrDefault<BankAccount>("SELECT * FROM BankAccounts WHERE UserID = @0 AND Currency = @1", userId, curr.InternalName);
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

        public static BankAccount InsertBankAccount(int userID, Currency curr)
        {
            var bank = new BankAccount()
            {
                Balance = 0,
                UserID = userID,
                InternalCurrencyName = curr.InternalName
            };
            DB.Insert(bank);
            return bank;
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            DB.Delete(account);
        }

        public static bool TryTransferTo(BankAccount payee, BankAccount receiver, double amount)
        {
            if (payee.Balance >= amount)
            {
                receiver.Balance += amount;
                payee.Balance -= amount;
                return true;
            }
            return false;
        }

        public static bool TryMakePayment(BankAccount payee, double amount)
        {
            if (payee.Balance >= amount)
            {
                payee.Balance -= amount;
                return true;
            }
            return false;
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
