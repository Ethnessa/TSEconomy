using Newtonsoft.Json;
using NuGet.Protocol;
using PetaPoco;
using Terraria;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TSEconomy.Logging;
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
        public string JsonBalance { get; private set; }

        /// <summary>
        /// a deserialized copy of JsonBalance.
        /// </summary>
    
        public Dictionary<string, double> ?Balance {
            get 
            {
                return (Dictionary<string, double>)JsonConvert.DeserializeObject(JsonBalance, typeof(Dictionary<string, double>));
            }

        } 
        
        public void Reset()
        {
            JsonBalance = new Dictionary<string, double>().ToJson();
        }
        public double ?GetBalance(Currency curr)
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
                return Api.GetBankAccount(userID);


            var acc = new BankAccount()
            {
                Flags = flags,
                UserID = userID,
                WorldID = Main.worldID,
                JsonBalance = new Dictionary<string, double>().ToJson()
            };

            acc.SetBalance(initializedCurrencyValue, startingCurrency);

            Api.InsertBankAccount(acc);

            if (acc.IsWorldAccount())
                return acc;

            TransactionLogging.Log(transLog.SFormat(Api.GetAccountName(acc.UserID)));

            return acc;

        }

        public bool TryAddBalance(double amount, Currency curr, string transLog = "{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}")
        {
            if(transLog == "{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}")
                transLog = Localization.TryGetString("{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}");

            if (amount < 0)
                return TryRemoveBalance(-amount, curr);

            double? oldBalance = GetBalance(curr);

            var newDict = Balance;
            newDict[curr.InternalName] += amount;

            JsonBalance = newDict.ToJson();

            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, curr.InternalName, amount, transLog.SFormat(Api.GetAccountName(UserID), amount, oldBalance, GetBalance(curr)),
                               TransactionProperties.Add);

            return true;
        }

        public bool TryRemoveBalance(double amount, Currency curr ,string transLog = "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
        {
            if(transLog == "{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}")
                transLog = Localization.TryGetString("{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}");

            if (amount < 0)
                return TryAddBalance(-amount, curr);

            if (Balance[curr.InternalName] < amount && !IsWorldAccount())
                return false;

            double? oldBalance = GetBalance(curr);

            var newDict = Balance;
            newDict[curr.InternalName] -= amount;

            JsonBalance = newDict.ToJson();

            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return true;

            Api.AddTransaction(UserID, curr.InternalName, -amount, transLog.SFormat(Api.GetAccountName(UserID), amount, oldBalance, GetBalance(curr)),
                               TransactionProperties.Add);

            return true;
        }


        public void SetBalance(double amount, Currency curr, string transLog = "{0}'s balance has been set to {1}. Old bal: {2}")
        {
            if(transLog == "{0}'s balance has been set to {1}. Old bal: {2}")
                transLog = Localization.TryGetString("{0}'s balance has been set to {1}. Old bal: {2}");

            double? oldBalance = GetBalance(curr);

            var newDict = Balance;
            newDict[curr.InternalName] = amount;

            JsonBalance = newDict.ToJson();

            Api.UpdateBankAccount(this);

            if (IsWorldAccount())
                return;

            Api.AddTransaction(UserID, curr.InternalName, amount, transLog.SFormat(Api.GetAccountName(UserID), amount, oldBalance), TransactionProperties.Set);
        }
        /// <summary>
        /// Its bettter to use this with the worldAccount for adding and removing money from the account
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="amount"></param>
        /// <returns></returns>

        public bool TryTransferTo(BankAccount receiver, Currency curr, double amount)
        {
            return Api.TryTransferbetween(this, receiver, curr, amount);
        }

        public bool HasPermission(string perm)
        {
            return TShock.Groups.groups.First(i => i.Name == TShock.UserAccounts.GetUserAccountByID(UserID).Group).HasPermission(perm);
        }

        public bool HasEnough(double amount, Currency curr)
        {
            return Api.HasEnough(this, curr, amount);
        }

        public Transaction AddTransaction(double amountChanged, Currency curr,string transLog, TransactionProperties flag)
        {
            return Api.AddTransaction(UserID, curr.InternalName, amountChanged, transLog, flag);
        }

        public bool IsWorldAccount()
        {
            return Flags == BankAccountProperties.WorldAccount;
        }



    }
}
