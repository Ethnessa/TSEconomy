using PetaPoco;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy.Api
{
    /// <summary>
    /// Manages bank account operations.
    /// </summary>
    public static class AccountApi
    {
        private static readonly IDatabase DB = TSEconomy.DB.DB;

        /// <summary>
        /// A dictionary of bank accounts with the player's name as the key.
        /// </summary>
        public static Dictionary<string, BankAccount> BankAccounts { get; private set; } = new();

        internal static void LoadAccounts()
        {
            var accounts = DB.Query<BankAccount>("SELECT * FROM BankAccounts").ToList();
            foreach (var account in accounts)
            {
                var playerName = GetAccountName(account.UserID);
                if (playerName != null)
                {
                    BankAccounts.Add(playerName, account);
                }
            }
        }


        internal static async Task LoadAccountsAsync()
        {
            await Task.Run(() => LoadAccounts());
        }

        public static bool HasBankAccount(TSPlayer player) => BankAccounts.ContainsKey(player.Name);

        public static bool HasBankAccount(int userId) => BankAccounts.ContainsKey(GetAccountName(userId));

        public static BankAccount? GetBankAccount(int userId)
        {
            BankAccounts.TryGetValue(GetAccountName(userId), out var bankAccount);

            if (bankAccount == null)
            {
                return BankAccount.TryCreateNewAccount(CurrencyApi.SystemCurrency, 0, userId);
            }
            return bankAccount;
        }

        public static void UpdateBankAccount(BankAccount account)
        {
            DB.Update(account);
            var playerName = GetAccountName(account.UserID);
            if (playerName != null)
            {
                BankAccounts[playerName] = account;
            }
        }

        public static async Task UpdateBankAccountAsync(BankAccount account)
        {
            await Task.Run(() => UpdateBankAccount(account));
        }

        public static void InsertBankAccount(BankAccount account)
        {
            DB.Insert(account);
            var playerName = GetAccountName(account.UserID);
            if (playerName != null)
            {
                BankAccounts.Add(playerName, account);
            }
        }

        public static async Task InsertBankAccountAsync(BankAccount account)
        {
            await Task.Run(() => InsertBankAccount(account));
        }

        public static void DeleteBankAccount(BankAccount account)
        {
            TransactionLogging.Log(Localization.TryGetString("{0} had their bank account deleted.").SFormat(GetAccountName(account.ID)));
            DB.Delete(account);
            var playerName = GetAccountName(account.UserID);
            if (playerName != null)
            {
                BankAccounts.Remove(playerName);
            }
        }

        public static async Task DeleteBankAccountAsync(BankAccount account)
        {
            await Task.Run(() => DeleteBankAccount(account));
        }

        public static bool HasEnough(BankAccount account, Currency curr, double amount)
        {
            return account.GetBalance(curr) >= amount;
            
        }

        public static string? GetAccountName(int userId)
        {
            if (userId == -1)
            {
                return TSPlayer.Server.Name;
            }

            var user = TShock.UserAccounts.GetUserAccountByID(userId);
            if (user == null)
            {
                return null;
            }

            return user.Name;
        }

        public static BankAccount? WorldAccount
        {
            get
            {
                if (!BankAccounts.ContainsKey(TSPlayer.Server.Name))
                {
                    var worldAcc = BankAccount.TryCreateNewAccount(CurrencyApi.SystemCurrency, 0, -1, BankAccountProperties.WorldAccount,
                                                                    Localization.TryGetString("{0} has created a new world account for the server ({1}), initial value was set to {2}"));

                    return worldAcc;
                }

                return BankAccounts.Values.First(i => i.Flags == BankAccountProperties.WorldAccount);
            }
        }

        /// <summary>
        /// Retrieves a TShock user account and player from a string, which can be either a username or a player name.
        /// </summary>
        /// <param name="userInput">The string used to identify the player or account. It can be either a username or an in-game player name.</param>
        /// <param name="onlinePlayer">
        /// When this method returns, contains the TSPlayer instance associated with the specified user input if the player is online;
        /// otherwise, null if the player is not online or does not exist. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// The UserAccount associated with the specified user input if found; otherwise, null if the account does not exist.
        /// </returns>

        public static UserAccount? GetUser(string userInput, out TSPlayer? onlinePlayer)
        {
            var player = TSPlayer.FindByNameOrID(userInput);
            if (player.Any())
            {
                onlinePlayer = player.FirstOrDefault();

                return onlinePlayer.Account;
            }

            var account = TShock.UserAccounts.GetUserAccounts().FirstOrDefault(i => i.Name.StartsWith(userInput));

            if (account != null)
            {
                onlinePlayer = TShock.Players.FirstOrDefault(x => x?.Account?.Name == account.Name);
                return account;
            }

            onlinePlayer = null;
            return null;
        }
    }
}