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
    /// <summary>

    /// The main BankAccount class, contains manipulation methods to its fields 
    /// and other helper methods, manual manipulation of fields should be followed
    /// by APi.UpdateBankAccount([bankAccount])

    /// </summary>
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
        
        /// <summary>
        /// Resets the entirety of the account's balance
        /// </summary>
        public void Reset()
        {
            JsonBalance = new Dictionary<string, double>().ToJson();
            TransactionLogging.Log("Reseted {0} balance for every currency!".SFormat(Api.GetAccountName(UserID)));
            Api.UpdateBankAccount(this);
        }
        /// <summary>
        /// Gets the balance for the specified currency of this account
        /// </summary>
        /// <param name="curr"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add a currency to the currency/balance dictionary of this account
        /// </summary>
        /// <param name="curr"></param>
        /// <returns></returns>
        public bool AddCurrency(Currency curr)
        {
            if (!Api.IsCurrencyValid(curr) || Balance.ContainsKey(curr.InternalName))
                return false;

            var newDict = Balance;
            newDict.Add(curr.InternalName, 0f);

            JsonBalance = newDict.ToJson();

            return true;
        }

        /// <summary>
        /// Attempts to create a new bank account for the specified user, with the specified currency.
        /// </summary>
        /// <param name="initialbalance">Starting balance for bank account</param>
        /// <param name="internalCurrencyName">The internal currency ID</param>
        /// <param name="userID">TShock UserAccount's ID</param>
        /// <param name="flags">BankAccount properties</param>
        /// <param name="transLog">Transaction message for logging</param>
        /// <returns></returns>
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

        /// <summary>
        /// Attempts to modify the balance of the bank account.
        /// </summary>
        /// <param name="amount">The amount to increment/reduce by</param>
        /// <param name="curr">The currency to increment/reduce the balance</param>
        /// <param name="operationType">Whether the option is an adding or subtraction operation</param>
        /// <param name="transLog">Overrides default transaction log message</param>
        /// <returns>Whether the transaction can be completed</returns>
        public bool TryModifyBalance(double amount, Currency curr,BalanceOperation operationType, string transLog = null)
        {

            double? oldBalance;

            if (operationType == BalanceOperation.Add)
            {
                transLog ??= Localization.TryGetString("{0}'s balance has been increased by {1}. Old bal: {2} new bal: {3}");

                if (amount < 0)
                    return TryModifyBalance(-amount, curr, BalanceOperation.Subtract);

                oldBalance = GetBalance(curr);

                var newDict = Balance;
                newDict[curr.InternalName] += amount;

                JsonBalance = newDict.ToJson();
            }
            else if (operationType == BalanceOperation.Subtract)
            {
                transLog ??= Localization.TryGetString("{0}'s balance has been decreased by {1}. Old bal: {2} new bal: {3}");

                if (amount < 0)
                    return TryModifyBalance(-amount, curr, BalanceOperation.Add);

                if (GetBalance(curr) < amount && !IsWorldAccount())
                    return false;

                oldBalance = GetBalance(curr);

                var newDict = Balance;
                newDict[curr.InternalName] -= amount;

                JsonBalance = newDict.ToJson();
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
                    Api.AddTransaction(UserID, curr.InternalName, amount, transLog.SFormat(Api.GetAccountName(UserID), amount, oldBalance, GetBalance(curr)), TransactionProperties.Add);
                }
                else
                {
                    Api.AddTransaction(UserID, curr.InternalName, -amount, transLog.SFormat(Api.GetAccountName(UserID), amount, oldBalance, GetBalance(curr)), TransactionProperties.Add);
                }
            }

            return true;
        }

        /// <summary>
        /// sets the account's balance for the specified currency to the specified amount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="curr"></param>
        /// <param name="transLog"></param>

        public void SetBalance(double amount, Currency curr, string transLog = "{0}'s balance has been set to {1}. Old bal: {2}")
        {
            if (transLog == "{0}'s balance has been set to {1}. Old bal: {2}")
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
        /// Transfers the specified amount to the specified account for the stated currency,
        /// it is used to transfer with the worldAccount for adding and removing money from the account from monster gains for instance
        /// </summary>
        /// <returns></returns>
        public bool TryTransferTo(BankAccount receiver, Currency curr, double amount)
        {
            return Api.TryTransferbetween(this, receiver, curr, amount);
        }

        /// <summary>
        /// Checks whether or not the account's player has the specified perm
        /// </summary>
        /// <param name="perm"></param>
        /// <returns></returns>
        public bool HasPermission(string perm)
        {
            return TShock.Groups.groups.First(i => i.Name == TShock.UserAccounts.GetUserAccountByID(UserID).Group).HasPermission(perm);
        }

        /// <summary>
        /// Checks whether or not the account's balance is more or equal to the amount
        /// for the specified currency
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public bool HasEnough(double amount, Currency curr)
        {
            return Api.HasEnough(this, curr, amount);
        }

        /// <summary>
        /// adds a transaction in the account's name
        /// </summary>
        /// <param name="amountChanged"></param>
        /// <param name="curr"></param>
        /// <param name="transLog"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public Transaction AddTransaction(double amountChanged, Currency curr,string transLog, TransactionProperties flag)
        {
            return Api.AddTransaction(UserID, curr.InternalName, amountChanged, transLog, flag);
        }

        /// <summary>
        /// checks for whether or not the account is the worldAccount
        /// </summary>
        /// <returns></returns>
        public bool IsWorldAccount()
        {
            return Flags == BankAccountProperties.WorldAccount;
        }



    }
}
