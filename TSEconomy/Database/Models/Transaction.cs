using PetaPoco;
using TSEconomy.Database.Models.Properties;

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

        [Column("Flags")]
        public TransactionProperties Flags { get; set; }

        public Transaction(int UserID, string internalCurrencyName, double amountChanged, string transDetails, DateTime timeStamp, TransactionProperties flags)
        {
            this.UserID = UserID;
            this.InternalCurrencyName = internalCurrencyName;
            this.Amount = amountChanged;
            this.TransactionDetails = transDetails;
            this.Timestamp = timeStamp;
            this.Flags = flags;
        }

        public Transaction(int UserID, string internalCurrencyName, double amountChanged, string transDetails, TransactionProperties flags)
        {
            this.UserID = UserID;
            this.InternalCurrencyName = internalCurrencyName;
            this.Amount = amountChanged;
            this.TransactionDetails = transDetails;
            this.Timestamp = DateTime.UtcNow;
            this.Flags = flags;
        }
    }
}
