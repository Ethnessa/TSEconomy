using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Protocol;
using PetaPoco;
using TSEconomy.Api;
using TShockAPI;

namespace TSEconomy.Database.Models
{
    [TableName("TradeInventories")]
    [PrimaryKey("ID")]
    public class TradeInventory
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("Player")]
        public string PlayerName { get; set; }

        [Column("Items")]
        public string Items { get; set; }

        [Column("Money")]
        public string Money { get; set; }

        public List<Item>? getItems()
        {
            if (String.IsNullOrEmpty(Items)) Items = JsonConvert.SerializeObject(new List<Item>());

            return (List<Item>)JsonConvert.DeserializeObject(Items, typeof(List<Item>));
        }

        public void ModifyCurrentItems(List<Item> list)
        {
            Items = list.ToJson();
            TradeInventoryApi.UpdateTradeInventory(this);
        }

        public Dictionary<string, float>? getMoney()
        {
            if (String.IsNullOrEmpty(Money)) Money = JsonConvert.SerializeObject(new Dictionary<string, float>());

            return (Dictionary<string, float>)JsonConvert.DeserializeObject(Money, typeof(Dictionary<string, float>));
        }

        public void ModifyCurrentMoney(Dictionary<string, float> dict)
        {
            Money = dict.ToJson();
            TradeInventoryApi.UpdateTradeInventory(this);
        }

        public string Format()
        {
            var items = getItems();
            var money = getMoney();

            if (money.Count == 0 && items.Count == 0)
                return "The trade inventory contains nothing!";

            string s = "";
            if (items.Count != 0)
            {
                s += "Current items in the trade inventory:\n[ ";
                
                for (int i = 0; i > items.Count; i++)
                {
                    items.TryGetValue(i, out var item);

                    if (i != 0)
                        s += ", ";

                    s += "[i/s{0},p{1}:{2}]".SFormat(item.Amount, item.Modifier, item.ID);
                }
                s += " ]\n";
            }

            if (money.Count != 0)
            {
                s += "Money stored the trade inventory:\n[ ";

                for (int i = 0; i > money.Count; i++)
                {
                    money.TryGetValue(i, out var kvp);

                    if (i != 0)
                        s += ", ";

                    s += CurrencyApi.Currencies[kvp.Key].GetName(kvp.Value);

                }

                s += " ]\n";
            }

            return s;
            
        }

        public void AddItem(Item item) {
            var items = getItems();

            int maxStack = TShock.Utils.GetItemById(item.ID).maxStack;

            if (!items.Any(i => i.ID == i.ID && i.Amount < maxStack))
            {
                items.Add(item);
                return;
            }

            while (item.Amount > 0)
            {
                var i = items.FirstOrDefault(i => i.ID == i.ID && i.Amount < maxStack);

                if (i == null)
                {
                    items.Add(item);
                    item.Amount = 0;
                }
                int spaceAvailable = maxStack - i.Amount;

                item.Amount -= spaceAvailable;

                i.Amount += spaceAvailable;
                
            }

            ModifyCurrentItems(items);

        }


    }

}