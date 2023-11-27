using PetaPoco;

namespace TSEconomy.Database.Models
{
    [TableName("Transactions")]
    [PrimaryKey("ID")]
    public class Transaction
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("Currency")]
        public string InternalCurrencyName { get; set; }

        [Column("Amount")]
        public double Amount { get; set; }

        [Column("TransactionDetails")]
        public string TransactionDetails { get; set; }

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        public Transaction() { }

        public Transaction(int UserID, string internalCurrencyName, double amountChanged, string transDetails)
        {
            this.UserID = UserID;
            this.InternalCurrencyName = internalCurrencyName;
            this.Amount = amountChanged;
            this.TransactionDetails = transDetails;
        }
    }
}
