using PetaPoco;
using TSEconomy.Database.Models.Properties;

namespace TSEconomy.Database.Models
{
    /// <summary>
    /// Represents a transaction in the database.
    /// </summary>
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
        
        public Transaction(int userId, string internalCurrencyName, double amountChanged, string transDetails, DateTime timeStamp, TransactionProperties flags)
        {
            UserID = userId;
            InternalCurrencyName = internalCurrencyName;
            Amount = amountChanged;
            TransactionDetails = transDetails;
            Timestamp = timeStamp;
            Flags = flags;
        }

        public Transaction(int userId, string internalCurrencyName, double amountChanged, string transDetails, TransactionProperties flags)
        {
            UserID = userId;
            InternalCurrencyName = internalCurrencyName;
            Amount = amountChanged;
            TransactionDetails = transDetails;
            Timestamp = DateTime.UtcNow;
            Flags = flags;
        }
    }
}
