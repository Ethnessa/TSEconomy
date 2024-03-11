using Newtonsoft.Json;
using NuGet.Protocol;
using Org.BouncyCastle.Crypto.Generators;
using PetaPoco;
using Terraria;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TShockAPI;

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

        // [Column("Currency")]
        // public string InternalCurrencyName { get; set; }

        [Column("WorldID")]
        public int WorldID { get; set; }

        [Column("Flags")]
        public BankAccountProperties Flags { get; set; }

        [Column("Balance")]
        public string JsonBalance { get; private set; } = new Dictionary<string, double>().ToJson();

        /// <summary>
        /// a deserialized copy of JsonBalance.
        /// </summary>
    
        public Dictionary<string, double> Balance {
            get 
            {
                return (Dictionary<string, double>)JsonConvert.DeserializeObject(JsonBalance, typeof(Dictionary<string, double>));
            }
            private set { } 
        }  
        public double? GetBalance(Currency curr)
        {
            if (!Api.IsCurrencyValid(curr))
            {
                TShock.Log.ConsoleError(Localization.TryGetString("Error: tried to get an account's balance for a currency that does not exist.", "GetBalance"));
                return null;
            }

            if (!Balance.ContainsKey(curr.InternalName))
                AddCurrency(curr);

            return Balance[curr.InternalName];

        }

        public bool AddCurrency(Currency curr)
        {
            if (!Api.IsCurrencyValid(curr) || Balance.ContainsKey(curr.InternalName))
                return false;

            var newDict = Balance;
            newDict.Add(curr.InternalName, 0f);

            JsonBalance = newDict.ToJson();

            return true;
        }

        public static BankAccount? TryCreateNewAccount(Currency startingCurrency, double initializedCurrencyValue, int userID, BankAccountProperties flags = BankAccountProperties.Default,
                                                       string transLog = "{0} has created a new bank")
        {
            if(transLog == "{0} has created a new bank account.")
                transLog = Localization.TryGetString("{0} has created a new bank account.");

            if (!Api.IsCurrencyValid(startingCurrency))
            {
                // error
                return null;
            }

            if(initializedCurrencyValue < 0)
            {
                // error
                return null;
            }

            if (Api.HasBankAccount(userID))
                return Api.GetBankAccount(userID, curr);


            var acc = new BankAccount()
            {
                Flags = flags,
                UserID = userID,
                WorldID = Main.worldID
            };

            acc.SetBalance(initializedCurrencyValue, startingCurrency);

            Api.InsertBankAccount(acc);

            if (acc.IsWorldAccount())
                return acc;

            Api.AddTransaction(userID, startingCurrency.InternalName, initialbalance, transLog.SFormat(Helpers.GetAccountName(userID), internalCurrencyName, initialbalance),
                               TransactionProperties.Set);

            return acc;

        }

        // we have two variants as we might not want the logs to show -amount 
        public bool TryAddBalance(double amount, string transLog = "{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}")
        {
            if(transLog == "{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}")
                transLog = Localization.TryGetString("{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}");

            if (amount < 0)
                return TryAddBalance(-amount, transLog);

            _balance += amount;

            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance - amount, Balance),
                               TransactionProperties.Add);

            return true;
        }

        public bool TryRemoveBalance(double amount, string transLog = "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
        {
            if(transLog == "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
                transLog = Localization.TryGetString("{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}");

            if (_balance < amount && !IsWorldAccount())
                return false;

            _balance -= amount;
            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, InternalCurrencyName, amount, transLog.SFormat(Helpers.GetAccountName(UserID), amount, Balance + amount, Balance),
                               TransactionProperties.Add);

            return true;
        }

        // UNDONE
        /// <summary>
        /// once implemented we'll want tryremovebalance and tryaddbalance to use it to with the world account. 
        /// </summary>
        public void SetBalance(double amount, string transLog = "{0}'s balance has been set to {1}. Old bal: {2}")
        {
            if(transLog == "{0}'s balance has been set to {1}. Old bal: {2}")
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
