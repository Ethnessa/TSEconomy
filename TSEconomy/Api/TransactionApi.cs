using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Database.Models;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TShockAPI;

namespace TSEconomy.Api
{
    /// <summary>
    /// Manages transaction operations.
    /// </summary>
    public static class TransactionApi
    {
        private static readonly IDatabase DB = TSEconomy.DB.DB;

        public static void InsertTransaction(Database.Models.Transaction trans)
        {
            DB.Insert(trans);
            TransactionLogging.Log(trans);
        }

        public static async Task InsertTransactionAsync(Database.Models.Transaction trans)
        {
            await Task.Run(() => InsertTransaction(trans));
        }

        public static Database.Models.Transaction AddTransaction(int userId, string internalCurrencyName, double amountChanged, string transLogMessage, TransactionProperties flags)
        {
            var trans = new Database.Models.Transaction(userId, internalCurrencyName, amountChanged, transLogMessage, flags);
            InsertTransaction(trans);
            return trans;
        }

        public static async Task<Database.Models.Transaction> AddTransactionAsync(int userId, string internalCurrencyName, double amountChanged, string transLogMessage, TransactionProperties flags)
        {
            return await Task.Run(() => AddTransaction(userId, internalCurrencyName, amountChanged, transLogMessage, flags));
        }

        public static bool TryTransferBetween(BankAccount payee, BankAccount receiver, Currency curr, double amount)
        {
            if (amount < 0)
                TryTransferBetween(receiver, payee, curr, -amount);

            if (payee.GetBalance(curr) >= amount || payee.IsWorldAccount())
            {
                var receiverName = AccountApi.GetAccountName(receiver.UserID);
                var payeeName = AccountApi.GetAccountName(payee.UserID);

                payee.TryModifyBalance(amount, curr, BalanceOperation.Subtract, Localization.TryGetString("{{0}} has transferred {{1}} to {0}. Old bal: {{2}} new bal {{3}}").SFormat(receiverName));
                receiver.TryModifyBalance(amount, curr, BalanceOperation.Add, Localization.TryGetString("{{0}} has received {{1}} from {0}. Old bal: {{2}} new bal {{3}}").SFormat(payeeName));

                return true;
            }
            return false;
        }

        public static async Task<bool> TryTransferBetweenAsync(BankAccount payee, BankAccount receiver, Currency curr, double amount)
        {
            return await Task.Run(() => TryTransferBetween(payee, receiver, curr, amount));
        }
    }
}