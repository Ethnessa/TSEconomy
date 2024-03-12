using TSEconomy.Configuration.Models;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Commands
{
    internal class BankAdminCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.Admin };

        public override void Execute(CommandArgs args)
        {
            var player = args.Player;
            var parameters = args.Parameters;

            var subcmd = parameters[0].ToLower();

            switch (subcmd)
            {
                case string s when s == Localization.TryGetString("setbal"):
                    {
                        var affectedUser = Api.GetUser(parameters.ElementAtOrDefault(0), out var affectedPlayer);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That player does not exist!", "SetBalance"));
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]Please enter a currency name!", "SetBalance"));
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That currency does not exist!", "SetBalance"));
                            return;
                        }

                        var amountInput = parameters.ElementAtOrDefault(2);
                        if (amountInput == default)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]Please enter an amount to set!", "SetBalance"));
                            return;
                        }

                        bool couldParse = int.TryParse(amountInput, out int amnt);
                        if (couldParse && amnt > 0)
                        {
                            var bankAccount = Api.GetBankAccount(affectedUser.ID);
                            bankAccount.SetBalance(amnt, currency, Localization.TryGetString("The admin {0} set the user {{0}}'s balance from {{1}} to {{2}}.").SFormat(player.Name));

                            if (affectedPlayer != null)
                            {
                                affectedPlayer.SendInfoMessage(Localization.TryGetString("[i:855]Your balance for {0} was set to {1} by {2}", "plugin").SFormat(currency.DisplayName, currency.GetName(amnt), player.Name));
                            }
                            return;
                        }
                        else
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]The amount you entered was invalid!", "SetBalance"));
                            return;
                        }
                    }
                case string s when s == Localization.TryGetString("resetbal"):
                    {
                        var affectedUser = Api.GetUser(parameters.ElementAtOrDefault(0), out var affectedPlayer);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That player does not exist!", "ResetBalance"));
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]Please enter a currency name!", "ResetBalance"));
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That's not a valid currency!", "ResetBalance"));
                            return;
                        }

                        var bankAccount = Api.GetBankAccount(affectedUser.ID);
                        bankAccount.SetBalance(0, currency, Localization.TryGetString("The admin {0} reset the user {{0}}'s balance from {{2}} to {{1}}.").SFormat(player.Name));
                        if (affectedPlayer != null)
                        {
                            affectedPlayer.SendInfoMessage(Localization.TryGetString("[i:855]Your balance for {0} was reset to 0 by {1}", "plugin").SFormat(currency.DisplayName, player.Name));
                        }
                        return;
                    }
                case string s when s == Localization.TryGetString("checkbal"):
                    {
                        var affectedUser = Api.GetUser(parameters.ElementAtOrDefault(0), out _);

                        if (affectedUser == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That player does not exist!", "CheckBalance"));
                            return;
                        }

                        var currencyInput = parameters.ElementAtOrDefault(1);

                        if (currencyInput == default)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]Please enter a currency name!", "CheckBalance"));
                            return;
                        }

                        Currency? currency = Currency.Get(currencyInput);
                        if (currency == null)
                        {
                            player.SendErrorMessage(Localization.TryGetString("[i:855]That's not a valid currency!", "CheckBalance"));
                            return;
                        }

                        var bankAccount = Api.GetBankAccount(affectedUser.ID);
                        player.SendInfoMessage(Localization.TryGetString("[i:855]The user {0}'s balance for {1} is {2}.", "CheckBalance").SFormat(affectedUser.Name, currency.DisplayName, currency.GetName((double)bankAccount.GetBalance(currency))));
                        return;
                    }
            }
        }
    }
}
