using PetaPoco;
using Terraria;
using TSEconomy.Configuration.Models;
using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy.Database.Models
{
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
        public double Balance { get { return _balance; } }
        
        public static BankAccount? TryCreateNewAccount(double initialbalance, string internalCurrencyName, int userID, BankAccountProperties flags = BankAccountProperties.Default,
                                                    string transLog = "{0} has created a new bank account ({1}), with the initial value of {2}.")
        {

            var curr = Currency.Get(internalCurrencyName);

            if (curr == null)
            {
                TShock.Log.Error("[TSEconomy CreateNewAccount] Error: tried to create a new bank account with an invalid currency.");
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

            Api.DB.Insert(acc);

            if (acc.IsWorldAccount())
                return acc;

            Api.AddTransaction(userID, internalCurrencyName, initialbalance, transLog.SFormat(Helpers.GetAccountName(userID), internalCurrencyName, initialbalance), 
                               TransactionProperties.set);

            return acc;

        }

        // we have two variants as we might not want the logs to show -amount 
        public bool TryAddBalance(double amount, string transLog = "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
        {

            if (amount < 0)
                return TryAddBalance(-amount, transLog);

            _balance += amount;

            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance - amount, Balance), 
                               TransactionProperties.add);

            return true;
        }

        public bool TryRemoveBalance(double amount, string transLog = "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
        {
            if (_balance < amount && !IsWorldAccount())
                return false;

            _balance -= amount;
            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance + amount, Balance),
                               TransactionProperties.add);

            return true;
        }

        /// <summary>
        /// once implemented we'll want tryremovebalance and tryaddbalance to use it to with the world account. 
        /// </summary>
        public void TryTransferTo(BankAccount receiver)
        {
            throw new NotImplementedException();
        }

        public void SetBalance(double amount, string transLog = "{0}'s balance has been set to {1}. Old bal: {2}")
        {
            double oldBalance = _balance;

            _balance = amount;
            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, oldBalance), TransactionProperties.set);
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
            return (Flags & BankAccountProperties.WorldAccount) == BankAccountProperties.WorldAccount;
        }



    }
}
