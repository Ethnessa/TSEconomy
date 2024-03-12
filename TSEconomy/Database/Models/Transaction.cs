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

        /// <summary>
        /// Represents a transaction in the database.
        /// </summary>
        public Transaction(int userID, string internalCurrencyName, double amountChanged, string transDetails, DateTime timeStamp, TransactionProperties flags)
        {
            UserID = userID;
            InternalCurrencyName = internalCurrencyName;
            Amount = amountChanged;
            TransactionDetails = transDetails;
            Timestamp = timeStamp;
            Flags = flags;
        }

        /// <summary>
        /// Represents a transaction in the database.
        /// </summary>
        public Transaction(int UserID, string internalCurrencyName, double amountChanged, string transDetails, TransactionProperties flags)
        {
            UserID = UserID;
            InternalCurrencyName = internalCurrencyName;
            Amount = amountChanged;
            TransactionDetails = transDetails;
            Timestamp = DateTime.UtcNow;
            Flags = flags;
        }
    }
}
