using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSEconomy.Configuration.Models;
using TShockAPI;

namespace TSEconomy.Commands
{
    public class BankAdminCommand : CommandBase
    {
        public override string[] Aliases { get; set; } = { "bankadmin", "ecoadmin", "ba" };
        public override string[] PermissionNodes { get; set; } = { Permissions.Admin };

        public override void Execute(CommandArgs args)
        {
            var player = args.Player;
            var parameters = args.Parameters;

            var subcmd = parameters[0].ToLower();

            switch (subcmd)
            {
                case "setbal":
                    {
                        var affectedUser = Helpers.GetUser(parameters.ElementAtOrDefault(0), out var affectedPlayer);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage("That player does not exist!");
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage("Please enter a currency name!");
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage("That currency does not exist!");
                            return;
                        }

                        var amountInput = parameters.ElementAtOrDefault(2);
                        if (amountInput == default)
                        {
                            player.SendErrorMessage("Please enter an amount to set!");
                            return;
                        }

                        bool couldParse = int.TryParse(amountInput, out int amnt);
                        if (couldParse && amnt > 0)
                        {
                            var bankAccount = Api.GetBankAccount(affectedUser.ID, currency);
                            bankAccount.SetBalance(amnt, "The admin {0} set the user {{0}}'s balance from {{1}} to {{2}}.".SFormat(player.Name));

                            if (affectedPlayer != null)
                            {
                                affectedPlayer.SendInfoMessage($"Your balance for {currency.DisplayName} was set to {amnt} by {player.Name}");
                            }
                            return;
                        }
                        else
                        {
                            player.SendErrorMessage("The amount you entered was invalid!");
                            return;
                        }
                    }
                case "resetbal":
                    {
                        var affectedUser = Helpers.GetUser(parameters.ElementAtOrDefault(0), out var affectedPlayer);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage("That player does not exist!");
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage("Please enter a currency name!");
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage("That currency does not exist!");
                            return;
                        }

                        var bankAccount = Api.GetBankAccount(affectedUser.ID, currency);
                        bankAccount.SetBalance(0, "The admin {0} reset the user {{0}}'s balance from {bankAccount.Balance} to {{1}}. Old Balance: {{2}}".SFormat(player.Name));
                        if (affectedPlayer != null)
                        {
                            affectedPlayer.SendInfoMessage($"Your balance for {currency.DisplayName} was reset to 0 by {player.Name}");
                        }
                        return;
                    }
                case "checkbal":
                    {
                        var affectedUser = Helpers.GetUser(parameters.ElementAtOrDefault(0), out var affectedPlayer);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage("That player does not exist!");
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage("Please enter a currency name!");
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage("That currency does not exist!");
                            return;
                        }

                        var bankAccount = Api.GetBankAccount(affectedUser.ID, currency);
                        player.SendInfoMessage($"The user {affectedUser.Name}'s balance for {currency.DisplayName} is {bankAccount.Balance}.");
                        return;
                    }
            }
        }
    }
}
