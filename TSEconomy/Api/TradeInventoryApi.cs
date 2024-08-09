using PetaPoco;
using TSEconomy.Database.Models;
using TSEconomy.Database.Models.Properties;
using TSEconomy.Extensions;
using TShockAPI;

namespace TSEconomy.Api
{
    /// <summary>
    /// Manages trade inventory operations.
    /// </summary>
    public static class TradeInventoryApi
    {
        private static readonly IDatabase DB = TSEconomy.DB.DB;

        /// <summary>
        /// A dictionary of trade inventories with the player's name as the key.
        /// </summary>
        public static Dictionary<string, TradeInventory> TradeInventories { get; private set; } = new();

        internal static void LoadTradeInventories()
        {
            var invs = DB.Query<TradeInventory>("SELECT * FROM TradeInventories").ToList();
            foreach (var inv in invs)
            {
                if (inv.PlayerName != null)
                {
                    TradeInventories.Add(inv.PlayerName, inv);
                }
            }
        }

        public static void InsertTradeInventory(TradeInventory inv)
        {
            DB.Insert(inv);
            if (inv.PlayerName != null)
            {
                TradeInventories.Add(inv.PlayerName, inv);
            }
        }

        public static async Task InsertTradeInventoryAsync(TradeInventory inv)
        {
            await Task.Run(() => InsertTradeInventory(inv));
        }

        public static void RemoveTradeInventory(TradeInventory inv)
        {
            DB.Delete(inv);
            if (inv.PlayerName != null)
            {
                TradeInventories.Remove(inv.PlayerName);
            }
        }

        public static async Task RemoveTradeInventoryAsync(TradeInventory inv)
        {
            await Task.Run(() => RemoveTradeInventory(inv));
        }

        public static void UpdateTradeInventory(TradeInventory inv)
        {
            DB.Update(inv);
            if (inv.PlayerName != null)
            {
                TradeInventories[inv.PlayerName] = inv;
            }
        }

        public static async Task UpdateTradeInventoryAsync(TradeInventory inv)
        {
            await Task.Run(() => UpdateTradeInventory(inv));
        }

        public static void GivePlayerTradeInventory(TSPlayer player, TradeInventory inv)
        {
            var items = inv.getItems();
        
            foreach (Item i in items)
                player.GiveItem(i.ID, i.Amount, i.Modifier);
            
            var currencies = inv.getMoney();
            
            foreach (KeyValuePair<string, float> kvp in currencies)
                player.GetBankAccount().TryModifyBalance(kvp.Value, CurrencyApi.Currencies[kvp.Key], BalanceOperation.Add);

            items.Clear();
            currencies.Clear();

            inv.ModifyCurrentItems(items);
            inv.ModifyCurrentMoney(currencies);
            
        }

        public static async Task GivePlayerTradeInventoryAsync(TSPlayer player, TradeInventory inv)
        {
            await Task.Run(() => GivePlayerTradeInventory(player, inv));
        }
        
    }
}
