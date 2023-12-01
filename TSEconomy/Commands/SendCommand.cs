using Microsoft.Xna.Framework;
using TSEconomy.Configuration.Models;
using TSEconomy.Extentions;
using TShockAPI;

namespace TSEconomy.Commands
{
    public class SendCommand : CommandBase
    {
        public override string[] Aliases { get; set; } = { "send", "transferto", "pay" };
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.Send };
        // send <player> <currency> <amount>
        public override void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            if (param.ElementAtOrDefault(0) == default)
            {
                player.SendInfoMessage($"Please use the command as follows: /{ShortestAlias} <player> <currency> <amount>");
                return;
            }

            var sendingTo = Helpers.GetUser(param.ElementAtOrDefault(0), out var receiverPlayer);

            if (sendingTo == null)
            {
                player.SendErrorMessage("That player doesn't exist!");
                return;
            }

            if(sendingTo.ID == player.GetUserId())
            {
                player.SendErrorMessage("You can't send money to yourself!");
                return;
            }

            var curr = param.ElementAtOrDefault(1);
            Currency? currency = curr == default ? Currency.GetFirst() : Currency.Get(curr);

            if (currency == null)
            {
                player.SendErrorMessage("That's not a valid currency!");
                return;
            }

            var amntInput = param.ElementAtOrDefault(2);
            if (!int.TryParse(amntInput, out var amnt))
            {
                player.SendErrorMessage("That's not a valid amount!");
                return;
            }

            if (amnt <= 0)
            {
                player.SendErrorMessage("You can't send 0 or less money!");
                return;
            }

            var bankAccount = Api.GetBankAccount(player.GetUserId(), currency);
            var receiverAccount = Api.GetBankAccount(sendingTo.ID, currency);
            var success = bankAccount.TryTransferTo(receiverAccount, amnt);

            if (!success)
            {
                player.SendErrorMessage("You don't have enough money!");
                return;
            }

            player.SendMessage($"You sent {currency.Symbol}{amnt} {currency.DisplayName.ToLower()}{(amnt == 1 ? "" : "s")} to {sendingTo.Name}", Color.LightGreen);
            if(receiverPlayer != null)
            {
                if (receiverPlayer != null)
                {
                    receiverPlayer.SendMessage($"You have been sent {currency.Symbol}{amnt} {currency.DisplayName.ToLower()}{(amnt == 1 ? "" : "s")}", Color.LightGreen);
                }
            }
        }
    }
}
