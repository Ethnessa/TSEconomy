using Microsoft.Xna.Framework;
using TSEconomy.Configuration.Models;
using TShockAPI;

namespace TSEconomy.Commands
{
    public class BalanceCommand : CommandBase
    {
        public override string[] Aliases { get; set; } = { "balance", "bal", "money" };
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.Balance };

        public override void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            var curr = param.ElementAtOrDefault(0);
            Currency? currency = curr == default ? Currency.GetFirst() : Currency.Get(curr);

            if (currency == null)
            {
                player.SendErrorMessage("That's not a valid currency!");
                return;
            }

            var bankAccount = Api.GetBankAccount(player.Account.ID, currency);
            player.SendMessage($"You have {currency.Symbol}{bankAccount.Balance} {currency.DisplayName.ToLower()}{(bankAccount.Balance == 1 ? "" : "s")}", Color.LightGreen);
        }
    }
}
