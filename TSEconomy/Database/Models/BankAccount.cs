using PetaPoco;
using Terraria;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Database.Models
{
    /// <summary>
    /// Represents a user's bank account, tied to a currency in database
    /// </summary>
    [TableName("BankAccounts")]
    [PrimaryKey("ID")]
    public class BankAccount
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("Currency")]
        public string InternalCurrencyName { get; set; }

        [Column("WorldID")]
        public int WorldID { get; set; }

        [Column("Flags")]
        public BankAccountProperties Flags { get; set; }

        private double _balance;

        [Column("Balance")]

        public double Balance { get { return _balance; } private set { } }

        /// <summary>
        /// Attempts to create a new bank account for the specified user, with the specified currency.
        /// </summary>
        /// <param name="initialbalance">Starting balance for bank account</param>
        /// <param name="internalCurrencyName">The internal currency ID</param>
        /// <param name="userID">TShock UserAccount's ID</param>
        /// <param name="flags">BankAccount properties</param>
        /// <param name="transLog">Transaction message for logging</param>
        /// <returns></returns>
        public static BankAccount? TryCreateNewAccount(double initialbalance, string internalCurrencyName, int userID, BankAccountProperties flags = BankAccountProperties.Default,
                                                       string transLog = "{0} has created a new bank account ({1}), with the initial value of {2}.")
        {
            if (transLog == "{0} has created a new bank account ({1}), with the initial value of {2}.")
                transLog = Localization.TryGetString("{0} has created a new bank account ({1}), with the initial value of {2}.");

            var curr = Currency.Get(internalCurrencyName);

            if (curr == null)
            {
                TShock.Log.Error(Localization.TryGetString("Error: tried to create a new bank account with an invalid currency.", "CreateNewAccount"));
                return null;
            }

            if (Api.HasBankAccount(userID, curr))
                return Api.GetBankAccount(userID, curr);


            var acc = new BankAccount()
            {
                Flags = flags,
                InternalCurrencyName = internalCurrencyName,
                UserID = userID,
                WorldID = Main.worldID
            };

            acc.SetBalance(initialbalance);

            Api.InsertBankAccount(acc);

            if (acc.IsWorldAccount())
                return acc;

            Api.AddTransaction(userID, internalCurrencyName, initialbalance, transLog.SFormat(Helpers.GetAccountName(userID), internalCurrencyName, initialbalance),
                               TransactionProperties.Set);

            return acc;

        }

        /// <summary>
        /// Attempts to modify the balance of the bank account.
        /// </summary>
        /// <param name="amount">The amount to increment/reduce by</param>
        /// <param name="operationType">Whether the option is an adding or subtraction operation</param>
        /// <param name="transLog">Overrides default transaction log message</param>
        /// <returns>Whether the transaction can be completed</returns>
        public bool TryModifyBalance(double amount, BalanceOperation operationType, string transLog = null)
        {
            if (operationType == BalanceOperation.Add)
            {
                transLog ??= Localization.TryGetString("{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}");

                if (amount < 0)
                    return false;

                _balance += amount;
            }
            else if(operationType == BalanceOperation.Subtract)
            {
                transLog ??= Localization.TryGetString("{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}");

                if (_balance < amount && !IsWorldAccount())
                    return false;

                _balance -= amount;
            }
            else
            {
                return false;
            }

            Api.UpdateBankAccount(this);

            if (!IsWorldAccount())
            {
                if (operationType == BalanceOperation.Add)
                {
                    Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance - amount, Balance), TransactionProperties.Add);
                }
                else
                {
                    Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance + amount, Balance), TransactionProperties.Add);
                }
            }

            return true;
        }


        // UNDONE
        /// <summary>
        /// once implemented we'll want tryremovebalance and tryaddbalance to use it to with the world account. 
        /// </summary>
        public void SetBalance(double amount, string transLog = "{0}'s balance has been set to {1}. Old bal: {2}")
        {
            if (transLog == "{0}'s balance has been set to {1}. Old bal: {2}")
                transLog = Localization.TryGetString("{0}'s balance has been set to {1}. Old bal: {2}");

            double oldBalance = _balance;

            _balance = amount;
            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, oldBalance), TransactionProperties.Set);
        }

        public bool TryTransferTo(BankAccount receiver, double amount)
        {
            return Api.TryTransferTo(this, receiver, amount);
        }

        public bool HasPermission(string perm)
        {
            return TShock.Groups.groups.First(i => i.Name == TShock.UserAccounts.GetUserAccountByID(UserID).Group).HasPermission(perm);
        }

        public bool HasEnough(double amount)
        {
            return Api.HasEnough(this, amount);
        }

        public Transaction AddTransaction(double amountChanged, string transLog, TransactionProperties flag)
        {
            return Api.AddTransaction(UserID, InternalCurrencyName, amountChanged, transLog, flag);
        }

        public bool IsWorldAccount()
        {
            return Flags == BankAccountProperties.WorldAccount;
        }



    }
}
