using Microsoft.Xna.Framework;
using TSEconomy.Configuration.Models;
using TSEconomy.Extensions;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Commands
{
    internal class BalanceCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.Balance };

        public override void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            var curr = param.ElementAtOrDefault(0);
            Currency? currency = curr == default ? Currency.GetDefault() : Currency.Get(curr);

            if (currency == null)
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]That's not a valid currency!", "Balance"));
                return;
            }

            var bankAccount = player.GetBankAccount(currency);
            player.SendMessage(Localization.TryGetString("[i:855]You have {0}!", "Balance").SFormat(currency.GetName(bankAccount.Balance, showName: true)), Color.LightGreen);
        }
    }
}
