using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy
{
    /// <summary>
    /// Simple utilities class, used for general purpose methods intending to reduce boilerplate code.
    /// </summary>
    public static class Helpers
    {
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

            var account = TShock.UserAccounts.GetUserAccountByName(userInput);
            if (account != null)
            {
                onlinePlayer = TShock.Players.FirstOrDefault(x => x?.Account?.Name == account.Name);
                return account;
            }

            onlinePlayer = null;
            return null;
        }

        // ?? is this necessary when TShock.UserAccounts.GetUserAccountByID exists
        public static string? GetAccountName(int UserID)
        {
            if (UserID == -1)
            {
                return TSPlayer.Server.Name;
            }

            var user = TShock.UserAccounts.GetUserAccountByID(UserID);
            if (user == null)
            {
                return null;
            }

            return user.Name;
        }
    }
}
