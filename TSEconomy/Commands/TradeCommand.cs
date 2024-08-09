using Microsoft.Xna.Framework;
using NuGet.Protocol.Plugins;
using Terraria;
using Terraria.Localization;
using TSEconomy.Api;
using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Extensions;
using TSEconomy.Lang;
using TShockAPI;
using Item = TSEconomy.Database.Models.Item;

namespace TSEconomy.Commands
{
    internal class TradeCommand : CommandBase
    {
        public override string[] PermissionNodes { get; set; } = { Permissions.Trade };

        public override async void Execute(CommandArgs args)
        {
            var player = args.Player;

            if(!player.RealPlayer)
            {
                player.SendErrorMessage("[TSEconomy Trade] You need to be ingame to use this command!");
                return;
            }

            var parameters = args.Parameters;
            bool admin = player.HasPermission(Permissions.Admin);

            int currentlyTradingWith = player.GetData<int>(TSEconomy.CURRENTLY_TRADING_WITH);
            int beingAskedBy = player.GetData<int>(TSEconomy.BEING_ASKED_TO_TRADE_BY);
            int asking = player.GetData<int>(TSEconomy.ASKING_TO_TRADE_WITH);
            bool hasComfirmedTrade = player.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE);

            bool isInTrade = currentlyTradingWith > -1;
            bool isAskingToTrade = asking > -1;
            bool isBeingAskedToTrade = beingAskedBy > -1; 
            

            if (parameters.Count < 1 || parameters.Count > 3)
            {
                player.SendWarningMessage("[TSEconomy Trade] [i:855] Trade Command usage:");
                player.SendWarningMessage("\'{0}{1} request [player]\' - sends a trade request to the specified player.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} cancel\' - cancels the current trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} add [currency] [amount]\' - add an amount of currency to the current trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} addItem\' - add the currently held item to the trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} removeitem [item]\' - removes the specified item from the trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} removecurr [curr]\' - removes the specified currency from the trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} approve\' - approve the current trade request.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                player.SendWarningMessage("\'{0}{1} confirm\' - comfirm/uncomfirm the current trade .", TShock.Config.Settings.CommandSpecifier, ShortestAlias);

                return;
            }


            var subcmd = parameters[0].ToLower();

            switch (subcmd)
            {
                case "request":
                case "rqst":
                case "rq":

                    if(isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please please finish or cancel the current trade to start a new one.");
                        return;
                    }

                    if(parameters.Count < 2)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please specify the name of the player you want to trade with.");
                        return;
                    }

                    TSPlayer targetedPlayer = TSPlayer.FindByNameOrID(parameters.ElementAtOrDefault(1)).FirstOrDefault();

                    if (targetedPlayer == null)
                    {
                        player.SendErrorMessage(Localization.TryGetString("[i:855]That player does not exist!", "Trade"));
                        return;
                    }

                    if (targetedPlayer.GetData<int>(TSEconomy.CURRENTLY_TRADING_WITH) > -1)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]The player you want to trade with is already in another trade.");
                        return;
                    }


                    player.SetData(TSEconomy.ASKING_TO_TRADE_WITH, targetedPlayer.Index);
                    targetedPlayer.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, targetedPlayer.Index);

                    targetedPlayer.SendWarningMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + "has sent you a trade request! " + TShock.Utils.ColorTag("Input /trade approve to accept!", Color.Green));
                    player.SendSuccessMessage("[TSEconomy Trade] Successfully sent trade request to " + TShock.Utils.ColorTag(targetedPlayer.Name, Color.Green) + "!");

                    return;
                case "approve":
                case "ap":
                case "apr":

                    if (isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please please finish or cancel the current trade to start a new one.");
                        return;
                    }

                    if (!isBeingAskedToTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]You have no trade current requests.");
                        return;
                    }


                    var targetedPlayer1 = TShock.Players.ElementAtOrDefault(beingAskedBy);

                    if (targetedPlayer1 == null)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] The person you wish to trade with disconnected.");
                        player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                        return;
                    }

                    if (targetedPlayer1.GetData<int>(TSEconomy.ASKING_TO_TRADE_WITH) != player.Index)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] The person you wish to trade with no longer wants to trade with you.");
                        player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                        return;
                    }

                    if (targetedPlayer1.GetData<int>(TSEconomy.CURRENTLY_TRADING_WITH) > -1)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] The person you wish to trade with is currently in another trade.");
                        player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                        return;
                    }

                    targetedPlayer1.SetData(TSEconomy.CURRENTLY_TRADING_WITH, player.Index);
                    targetedPlayer1.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
                    targetedPlayer1.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);

                    player.SetData(TSEconomy.CURRENTLY_TRADING_WITH, targetedPlayer1.Index);
                    player.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
                    player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);

                    player.SendInfoMessage("Initiated trade with " + TShock.Utils.ColorTag(targetedPlayer1.Name, Color.Green));
                    player.SendInfoMessage("Use \'trade add\' and \'trade additem\' to add money and items to the trade.");
                    targetedPlayer1.SendInfoMessage(TShock.Utils.ColorTag(player.Name, Color.Green) + "initiated trade with you!");
                    targetedPlayer1.SendInfoMessage("Use \'trade add\' and \'trade additem\' to add money and items to the trade.");
                    return;
                case "cancel":
                case "cnl":
                case "cn":
                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    var targetedPlayer2 = TShock.Players.ElementAtOrDefault(beingAskedBy);

                    player.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);
                    targetedPlayer2.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);

                    await TradeInventoryApi.GivePlayerTradeInventoryAsync(player, player.GetTradeInventory());
                    await TradeInventoryApi.GivePlayerTradeInventoryAsync(targetedPlayer2, targetedPlayer2.GetTradeInventory());

                    player.SendInfoMessage("[TSEconomy Trade] cancelled the trade, money and items have been given back.");
                    targetedPlayer2.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " cancelled the trade, money and items have been given back.");

                    return;

                case "add":
                case "a":
                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    var targetedPlayer3 = TShock.Players[currentlyTradingWith];

                    if (hasComfirmedTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot add currencies, you already comfirmed the trade, uncomfirm to add currencies");
                        return;
                    }

                    if (targetedPlayer3.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE))
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot add currencies, your peer already comfirmed, ask them uncomfirm to add currencies");
                        return;
                    }

                    if (parameters.Count < 3)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please specify the name of the currency as well as the amount.");
                        return;
                    }

                    string curName = parameters[1];

                    var curr = Currency.Get(curName);

                    if (curr == null)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Invalid currency.");
                        return;
                    }

                    string sAmount = parameters[2];

                    bool couldParse = float.TryParse(sAmount, out var amount);

                    if (!couldParse)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] please enter a valid amount.");
                        return;
                    }

                    if (!AccountApi.HasEnough(player.GetBankAccount(), curr, amount))
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You dont have enough money in your account.");
                        return;

                    }

                    player.GetBankAccount().TryModifyBalance(amount, curr, BalanceOperation.Subtract);


                    var inv = player.GetTradeInventory();
                    var money = inv.getMoney();

                    if (!money.ContainsKey(curr.InternalName))
                        money.Add(curr.InternalName, amount);
                    else money[curr.InternalName] += amount;

                    inv.ModifyCurrentMoney(money);

                    player.SendInfoMessage("[TSEconomy Trade] Added " + curr.GetName(amount) + " to your trade inventory.");
                    targetedPlayer3.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " Added " + curr.GetName(amount) + " to their trade inventory.");

                    player.SendInfoMessage("Your trade inventory:\n" + inv.Format() + TShock.Utils.ColorTag(targetedPlayer3.Name, Color.Green) + "'s trade inventory:\n" + targetedPlayer3.GetTradeInventory().Format());
                    targetedPlayer3.SendInfoMessage("Your trade inventory:\n" + targetedPlayer3.GetTradeInventory().Format() + TShock.Utils.ColorTag(player.Name, Color.Green) + "'s trade inventory:\n" + inv.Format());

                    return;
                case "additem":
                case "ai":

                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    if (hasComfirmedTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot add items, you already comfirmed the trade, uncomfirm to add items");
                        return;
                    }
                    var targetedPlayer4 = TShock.Players[currentlyTradingWith];

                    if (targetedPlayer4.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE))
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot add items, your peer already comfirmed, ask them uncomfirm to add items");
                        return;
                    }

                    if (TShock.Utils.GetItemById(player.SelectedItem.netID) == null || player.SelectedItem.netID == 0)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Please hold an item to add to the trade inventory");
                        return;
                    }


                    var i = player.SelectedItem;

                    player.GetTradeInventory().AddItem(new Database.Models.Item { Amount = i.stack, ID = i.netID, Modifier = i.prefix });

                    player.SendInfoMessage("[TSEconomy Trade] Added " + "[i/s{0},p{1}:{2}]".SFormat(i.stack, i.prefix, i.netID) + " to your trade inventory.");
                    targetedPlayer4.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " Added " + "[i/s{0},p{1}:{2}]".SFormat(i.stack, i.prefix, i.netID) + " to their trade inventory.");

                    player.SendInfoMessage("Your trade inventory:\n" + player.GetTradeInventory().Format() + TShock.Utils.ColorTag(targetedPlayer4.Name, Color.Green) + "'s trade inventory:\n" + targetedPlayer4.GetTradeInventory().Format());
                    targetedPlayer4.SendInfoMessage("Your trade inventory:\n" + targetedPlayer4.GetTradeInventory().Format() + TShock.Utils.ColorTag(player.Name, Color.Green) + "'s trade inventory:\n" + player.GetTradeInventory().Format());

                    player.SelectedItem.stack = 0;
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.SelectedItem.Name), player.Index, player.TPlayer.selectedItem);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.SelectedItem.Name), player.Index, player.TPlayer.selectedItem);

                    return;

                case "rmi":
                case "removeitem":
                case "remi":
                case "ri":
                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    if (hasComfirmedTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot remove items, you already comfirmed the trade, uncomfirm to remove items");
                        return;
                    }
                    var targetedPlayer5 = TShock.Players[currentlyTradingWith];

                    if (targetedPlayer5.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE))
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot remove items, your peer already comfirmed, ask them uncomfirm to remove items");
                        return;
                    }

                    if (parameters.Count < 2)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please specify the name of the item.");
                        return;
                    }

                    string name = parameters[1];

                    Item found = null;
                    var items = player.GetTradeInventory().getItems();
                    
                    
                    foreach (var item in items)
                    {
                        var tItem = TShock.Utils.GetItemById(item.ID);
                        tItem.Prefix(item.Modifier);

                        if (tItem.HoverName.Contains(name))
                        {
                            found = item;
                            items.Remove(item);
                            player.GetTradeInventory().ModifyCurrentItems(items);
                            break;
                        }

                    }

                    if (found != null)
                    {
                        player.GiveItem(found.ID, found.Amount, found.Modifier);

                        player.SendInfoMessage("[TSEconomy Trade] Removed " + "[i/s{0},p{1}:{2}]".SFormat(found.Amount, found.Modifier, found.ID) + " from your trade inventory.");
                        targetedPlayer5.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " Removed " + "[i/s{0},p{1}:{2}]".SFormat(found.Amount, found.Modifier, found.ID) + " from their trade inventory.");

                        player.SendInfoMessage("Your trade inventory:\n" + player.GetTradeInventory().Format() + TShock.Utils.ColorTag(targetedPlayer5.Name, Color.Green) + "'s trade inventory:\n" + targetedPlayer5.GetTradeInventory().Format());
                        targetedPlayer5.SendInfoMessage("Your trade inventory:\n" + targetedPlayer5.GetTradeInventory().Format() + TShock.Utils.ColorTag(player.Name, Color.Green) + "'s trade inventory:\n" + player.GetTradeInventory().Format());
                        return;
                    }

                    player.SendErrorMessage("[TSEconomy Trade] Could not find Item name containing the words " + name + " in your trade inventory!");

                    return;

                case "removecurr":
                case "rc":
                case "rmcu":
                case "rmc":
                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    if (hasComfirmedTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot remove currencies, you already comfirmed the trade, uncomfirm to remove currencies");
                        return;
                    }
                    var targetedPlayer6 = TShock.Players[currentlyTradingWith];

                    if (targetedPlayer6.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE))
                    {
                        player.SendErrorMessage("[TSEconomy Trade] Cannot remove currencies, your peer already comfirmed, ask them uncomfirm to remove currencies");
                        return;
                    }

                    if (parameters.Count < 2)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] [i:855]Please specify the name of the currency.");
                        return;
                    }


                    Currency curr1 = Currency.Get(parameters[1]);
                    
                    if (curr1 == null)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] that is not a valid currency.");
                        return;
                    }


                    bool found1 = false;
                    KeyValuePair<string, float> foundKVP = default;
                    var money1 = player.GetTradeInventory().getMoney();

                    foreach (KeyValuePair<string, float> kvp in money1)
                    {
                        if (curr1.InternalName == kvp.Key)
                        {
                            found1 = true;
                            foundKVP = kvp;
                            money1.Remove(kvp.Key);
                            player.GetTradeInventory().ModifyCurrentMoney(money1);
                            break;
                        }
                    }

                    if (found1)
                    {
                        player.GetBankAccount().TryModifyBalance(foundKVP.Value, curr1, BalanceOperation.Add);

                        player.SendInfoMessage("[TSEconomy Trade] Removed " + curr1.GetName(foundKVP.Value) + " from your trade inventory.");
                        targetedPlayer6.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " Removed " + curr1.GetName(foundKVP.Value) + " from their trade inventory.");

                        player.SendInfoMessage("Your trade inventory:\n" + player.GetTradeInventory().Format() + TShock.Utils.ColorTag(targetedPlayer6.Name, Color.Green) + "'s trade inventory:\n" + targetedPlayer6.GetTradeInventory().Format());
                        targetedPlayer6.SendInfoMessage("Your trade inventory:\n" + targetedPlayer6.GetTradeInventory().Format() + TShock.Utils.ColorTag(player.Name, Color.Green) + "'s trade inventory:\n" + player.GetTradeInventory().Format());
                        return;

                    }

                    return;

                case "comfirm":
                case "cm":
                case "com":
                case "cmf":

                    if (!isInTrade)
                    {
                        player.SendErrorMessage("[TSEconomy Trade] You cannot cancel since you are currently not trading");
                        return;
                    }

                    var targetedPlayer7 = TShock.Players[currentlyTradingWith];

                    if (hasComfirmedTrade)
                    {
                        player.SetData(TSEconomy.HAS_COMFIRMED_TRADE, false);
                        player.SendInfoMessage("[TSEconomy Trade] Uncomfirmed the trade");
                        targetedPlayer7.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " uncomfirmed the trade");
                        return;
                    }

                    player.SetData(TSEconomy.HAS_COMFIRMED_TRADE, true);

                    player.SendInfoMessage("[TSEconomy Trade] comfirmed the trade");
                    targetedPlayer7.SendInfoMessage("[TSEconomy Trade] " + TShock.Utils.ColorTag(player.Name, Color.Green) + " comfirmed the trade");

                    if (targetedPlayer7.GetData<bool>(TSEconomy.HAS_COMFIRMED_TRADE))
                    {
                        player.SendInfoMessage("[TSEconomy Trade] both players comfirmed the trade, initiating transfer.");
                        targetedPlayer7.SendInfoMessage("[[TSEconomy Trade] both players comfirmed the trade, initiating transfer.");

                        player.SendInfoMessage("[TSEconomy Trade] you traded: \n" + player.GetTradeInventory().Format() + "For: \n" + targetedPlayer7.GetTradeInventory().Format());
                        targetedPlayer7.SendInfoMessage("[TSEconomy Trade] you traded: \n" + targetedPlayer7.GetTradeInventory().Format() + "For: \n" + player.GetTradeInventory().Format());


                        await TradeInventoryApi.GivePlayerTradeInventoryAsync(player, targetedPlayer7.GetTradeInventory());
                        await TradeInventoryApi.GivePlayerTradeInventoryAsync(targetedPlayer7, player.GetTradeInventory());

                        player.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
                        player.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                        player.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);
                        player.SetData(TSEconomy.HAS_COMFIRMED_TRADE, false);


                        targetedPlayer7.SetData(TSEconomy.ASKING_TO_TRADE_WITH, -1);
                        targetedPlayer7.SetData(TSEconomy.BEING_ASKED_TO_TRADE_BY, -1);
                        targetedPlayer7.SetData(TSEconomy.CURRENTLY_TRADING_WITH, -1);
                        targetedPlayer7.SetData(TSEconomy.HAS_COMFIRMED_TRADE, false);
                    }

                    return;

                default:

                    player.SendWarningMessage("[TSEconomy Trade] [i:855] Trade Command usage:");
                    player.SendWarningMessage("\'{0}{1} request [player]\' - sends a trade request to the specified player.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    player.SendWarningMessage("\'{0}{1} cancel\' - cancels the current trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    player.SendWarningMessage("\'{0}{1} add [currency] [amount]\' - add an amount of currency to the current trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    player.SendWarningMessage("\'{0}{1} addItem\' - add the currently held item to the trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    player.SendWarningMessage("\'{0}{1} remove [curr/item]\' - removes an item or currency from the trade.", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    player.SendWarningMessage("\'{0}{1} approve\' - approve the current trade..", TShock.Config.Settings.CommandSpecifier, ShortestAlias);
                    
                    return;
            }
        }
    }
}
