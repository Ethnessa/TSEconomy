using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Logging;

namespace TSEconomy
{
    public class Api
    {
        public static Configuration.Configuration Config => Configuration.Configuration.Instance;
        public static IDatabase DB => TSEconomy.DB.DB;

        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

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

            AddTransaction(userID, curr.InternalName, 0, $"{Helpers.GetAccountName(userID)} created a new bank account.");
            DB.Insert(bank);
            return bank;
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            AddTransaction(account.ID, account.InternalCurrencyName, 0, $"{Helpers.GetAccountName(account.ID)} had their bank account deleted.");
            DB.Delete(account);
        }

        public static bool TryTransferTo(BankAccount payee, BankAccount receiver, double amount)
        {
            if (payee.Balance >= amount)
            {
                var receiverName = Helpers.GetAccountName(receiver.UserID);
                var payeeName = Helpers.GetAccountName(payee.UserID);

                payee.Balance -= amount;
                AddTransaction(payee.UserID, payee.InternalCurrencyName, amount, $"{payeeName} sent {amount} to {payeeName}");

                receiver.Balance += amount;
                AddTransaction(receiver.UserID, receiver.InternalCurrencyName, amount, $"{receiverName} received {amount} from {payeeName}");

                return true;
            }
            return false;
        }

        public static bool TryMakePayment(BankAccount payee, double amount)
        {
            if (payee.Balance >= amount)
            {
                payee.Balance -= amount;
                AddTransaction(payee.UserID, payee.InternalCurrencyName, amount, $"{Helpers.GetAccountName(payee.UserID)} made a payment for {amount}");

                return true;
            }
            return false;
        }

        public static Database.Models.Transaction AddTransaction(int userID, string internalCurrencyName, double amountChanged, string transLogMessage)
        {
            var trans = new Database.Models.Transaction()
            {
                UserID = userID,
                InternalCurrencyName = internalCurrencyName,
                Amount = amountChanged,
                TransactionDetails = transLogMessage,
                Timestamp = DateTime.Now
            };
            Api.InsertTransaction(trans);
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
