using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy
{
    public class Helpers
    {
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
                onlinePlayer = null;
                return account;
            }

            onlinePlayer = null;
            return null;
        }

        public static string? GetAccountName(int UserID)
        {
            var user = TShock.UserAccounts.GetUserAccountByID(UserID);
            if(user == null)
            {
                return null;
            }

            return user.Name;
        }
    }
}
