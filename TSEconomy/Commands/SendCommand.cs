﻿using Microsoft.Xna.Framework;
using TSEconomy.Configuration.Models;
using TSEconomy.Extensions;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Commands
{
    internal class SendCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.Send };
        // send <player> <currency> <amount>
        public override void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            if (param.ElementAtOrDefault(0) == default)
            {
                player.SendInfoMessage(Localization.TryGetString("[i:855]Please use the command as follows: /{0} <player> <currency> <amount>", "Send").SFormat(ShortestAlias));
                return;
            }

            var sendingTo = Api.GetUser(param.ElementAtOrDefault(0), out var receiverPlayer);

            if (sendingTo == null)
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]That player does not exist!", "Send"));
                return;
            }

            if (sendingTo.ID == player.GetUserId())
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]You can't send money to yourself!", "Send"));
                return;
            }

            var curr = param.ElementAtOrDefault(1);
            Currency? currency = curr == default ? Currency.GetDefault() : Currency.Get(curr);

            if (currency == null)
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]That's not a valid currency!", "Send"));
                return;
            }

            var amntInput = param.ElementAtOrDefault(2);
            if (!int.TryParse(amntInput, out var amnt))
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]The amount you entered was invalid!", "Send"));
                return;
            }

            if (amnt <= 0)
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]You can't send 0 or less money!", "Send"));
                return;
            }

            var bankAccount = Api.GetBankAccount(player.GetUserId());
            var receiverAccount = Api.GetBankAccount(sendingTo.ID);
            var success = bankAccount.TryTransferTo(receiverAccount, currency, amnt);

            if (!success)
            {
                player.SendErrorMessage(Localization.TryGetString("[i:855]You don't have enough money!", "Send"));
                return;
            }

            player.SendMessage(Localization.TryGetString("[i:855]You sent {0} to {1}", "Send").SFormat(currency.GetName(amnt, showName: true), sendingTo.Name), Color.LightGreen);

            receiverPlayer?.SendMessage(Localization.TryGetString("[i:855]You have been sent {0}{1}.", "plugin")
                              .SFormat(currency.GetName(amnt, showName: true), args.Silent ? "" : Localization.TryGetString(" by {0}").SFormat(player.Name)), Color.LightGreen);
        }
    }
}
