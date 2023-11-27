using PetaPoco;

namespace TSEconomy.Database.Models
{
    [TableName("BankAccounts")]
    [PrimaryKey("ID")]
    internal class BankAccount
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("Currency")]
        public string InternalCurrencyName { get; set; }

        [Column("Balance")]
        public double Balance { get; set; }
    }
}
