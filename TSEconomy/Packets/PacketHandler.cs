using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TSEconomy.Api;
using TSEconomy.Database.Models;
using TSEconomy.Extensions;
using TShockAPI;

namespace TSEconomy.Packets
{
    public static class PacketHandler
    {
        public static void OnGreet(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];

            if (player == null || !player.Active) return;

            player.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
            player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
            player.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);
            player.SetData(TSEconomy.HAS_COMFIRMED_TRADE, false);

            var inv = player.GetTradeInventory();

            if (inv.getItems().Count != 0 || inv.getMoney().Count != 0)
            {
                player.SendInfoMessage("[TSEconomy Trade] last time you left those were still in your trade inventory:\n" + inv.Format());
                TradeInventoryApi.GivePlayerTradeInventory(player, inv);
            }
        }

        public static void OnLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];

            if (player == null || !player.Active) return;

            if (player.GetData<int>(TSEconomy.BEING_ASKED_TO_TRADE_BY) > -1)
            {
                TShock.Players[player.GetData<int>(TSEconomy.BEING_ASKED_TO_TRADE_BY)].SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
            }
            
            if (player.GetData<int>(TSEconomy.ASKING_TO_TRADE_WITH) > -1)
            {
                TShock.Players[player.GetData<int>(TSEconomy.ASKING_TO_TRADE_WITH)].SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
            }

            if (player.GetData<int>(TSEconomy.CURRENTLY_TRADING_WITH) > -1)
            {
                var p = TShock.Players[player.GetData<int>(TSEconomy.CURRENTLY_TRADING_WITH)];
                p.SendWarningMessage("[TSEconomy trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " left cancelling trade!");
                if (p.GetTradeInventory().getItems().Count > 0 || p.GetTradeInventory().getMoney().Count > 0)
                {
                    p.SendWarningMessage("[TSEconomy trade] You got back:\n " + p.GetTradeInventory().Format());
                    TradeInventoryApi.GivePlayerTradeInventory(p, p.GetTradeInventory());
                }

                p.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
                p.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                p.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);
                p.SetData(TSEconomy.HAS_COMFIRMED_TRADE, false);

            }
        }


    }
}
