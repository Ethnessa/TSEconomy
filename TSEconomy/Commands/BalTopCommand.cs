using Microsoft.Xna.Framework;
using TSEconomy.Api;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TShockAPI;
using TShockAPI.DB;

namespace TSEconomy.Commands
{
    internal class BalTopCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.BalTop };

        public override void Execute(CommandArgs args)
        {
            var player = args.Player;
            var parameters = args.Parameters;

            if (parameters.Count < 1 || parameters.Count > 2)
            {
                player.SendWarningMessage("[TSEconomy BalTop] [i:855] BalTop Command usage:");
                player.SendWarningMessage("\'{0}{1} [currency] (page)\' - shows the list of the balance for each players starting from the richest for the specified currency.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} [currency] [player]\' - shows the page with the rank and balance of the specified player.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                return;
            }

            if (AccountApi.BankAccounts.Count <= 1)
            {
                player.SendErrorMessage("[TSEconomy BalTop] There is currently no one on the leaderboard.");
                return;
            }

            var curr = Currency.Get(parameters[0]);

            if (curr == null)
            {
                player.SendErrorMessage("[TSEconomy BalTop] {0} is not a valid currency!".SFormat(parameters[0]));
                return;
            }


            int page = 1;
            UserAccount account = null;

            if (args.Parameters.Count == 2 && !PaginationTools.TryParsePageNumber(args.Parameters, 1, null, out page))
            {
                account = AccountApi.GetUser(parameters[1], out _);

                if (account == null)
                {
                    player.SendErrorMessage("[TSEconomy BalTop] could not find player {0}!".SFormat(parameters[1]));
                    return;
                }

            }

            List<string> rankStrings = new();

            (string, int, string)[] ranks = TSEconomy.Config.BalTopLeaderboard.LeaderBoardRanks;

            int i = 1;
            int j = -1;

            List<BankAccount> sortedAccounts = AccountApi.BankAccounts.Values.OrderBy(i => -i.GetBalance(curr)).ToList();
            sortedAccounts.RemoveAll(i => i.IsWorldAccount());

            int sequence = 0;

            int fibonacciLastNum = 0;

            int fibonacciCurrentNum = 1;

            var conf = TSEconomy.Config.BalTopLeaderboard;

            int maxPage = (int)Math.Ceiling((decimal)sortedAccounts.Count / conf.MaxRanksPerPage);

            if (page > maxPage)
                page = maxPage;

            if(account != null)
            {
                var bankAccount = sortedAccounts.First(i => i.GetAccountName() == account.Name);
                int index = sortedAccounts.IndexOf(bankAccount) + 1;

                page = (int)Math.Ceiling((double)(maxPage / sortedAccounts.Count) * index);

                player.SendInfoMessage("[TSEconomy Leaderboard] Showing page for player {0}.".SFormat(bankAccount.GetAccountName()));
            }


            foreach (var acc in sortedAccounts)
            {
                if ((int)Math.Ceiling((decimal)i / conf.MaxRanksPerPage) > page)
                    break;

                if (sequence == 0 && j < ranks.Length - 1)
                {
                    if (!conf.UseFibonacciRankStyle)
                        sequence += conf.LeaderboardPositionsPerRanks;

                    else
                    {
                        int lastNum = fibonacciCurrentNum;

                        sequence = fibonacciCurrentNum = fibonacciLastNum + fibonacciCurrentNum;

                        fibonacciLastNum = lastNum;

                    }

                    j++;
                }


                if (page == (int)Math.Ceiling((decimal)i / conf.MaxRanksPerPage))
                    rankStrings.Add(String.Format("[c/#FFFAE4:{1}] [c/{0}:| {2}] [c/{0}:-] [i:{3}] [c/{0}:{4}] [i:{3}] [c/{0}::] {5}", ranks[j].Item3, i,
                                    ranks[j].Item1, ranks[j].Item2, acc.GetAccountName(), curr.GetName((double)acc.GetBalance(curr))));

                i++;
                sequence--;
            }


            player.SendMessage("TSEconomy Leaderboard ({0}/{1}):".SFormat(page, maxPage), Color.Green);

            foreach (string str in rankStrings)
                player.SendInfoMessage(str);

            if (page != maxPage)
                player.SendMessage("Type {0}bank lb {1} for more.".SFormat(TShock.Config.Settings.CommandSpecifier, page + 1), Color.Yellow);
            else
                player.SendMessage("You are on the last page.", Color.Yellow);
        }
    }
}
