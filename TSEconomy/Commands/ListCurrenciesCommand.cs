using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSEconomy.Configuration.Models;
using TShockAPI;

namespace TSEconomy.Commands
{
    public class ListCurrenciesCommand : CommandBase
    {
        public override string[] Aliases { get; set; } = { "listcurr", "currs", "currencies" };
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.ListCurrencies };

        public override void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            var currList = Api.GetCurrencies();
            var currNames = currList.Select(x => x.DisplayName);

            var page = param.ElementAtOrDefault(0) == default ? 1 : int.Parse(param[0]);
            var pageSize = 10;

            PaginationTools.SendPage(player, page, currNames, pageSize, new PaginationTools.Settings()
            {
                HeaderFormat = $"Currencies ({page}/{(currNames.Count() / pageSize)+1})",
                IncludeFooter = ((currNames.Count() / pageSize) + 1) > page,
                FooterFormat = $"Type /{ShortestAlias} {{0}} for more.",
                NothingToDisplayString = "There are no currencies to display."
            });
            
        }
    }
}
