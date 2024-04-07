using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Commands
{
    internal class ListCurrenciesCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.User, Permissions.ListCurrencies };

        public override async void Execute(CommandArgs args)
        {
            var param = args.Parameters;
            var player = args.Player;

            var currList = Api.GetCurrencies();
            currList.RemoveAt(0);

            var currNames = currList.Select(x => x.DisplayName);

            var page = param.ElementAtOrDefault(0) == default ? 1 : int.Parse(param[0]);
            var pageSize = 10;

            PaginationTools.SendPage(player, page, currNames, pageSize, new PaginationTools.Settings()
            {
                HeaderFormat = Localization.TryGetString("[i:855] Currencies ({0}/{1})").SFormat(page, (currNames.Count() / pageSize) + 1),
                IncludeFooter = ((currNames.Count() / pageSize) + 1) > page,
                FooterFormat = Localization.TryGetString("[i:855]Type /{0} {{0}} for more.").SFormat(ShortestAlias),
                NothingToDisplayString = Localization.TryGetString("[i:855]There are no currencies to display.", "ListCurrencies")
            });

        }
    }
}
