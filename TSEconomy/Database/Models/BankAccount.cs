using PetaPoco;

namespace TSEconomy.Database.Models
{
    [TableName("BankAccounts")]
    [PrimaryKey("ID")]
    public class BankAccount
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("Currency")]
        public string InternalCurrencyName { get; set; }

        private double _balance;

        [Column("Balance")]
        public double Balance { get { return _balance; } set { _balance = value; TSEconomy.DB.DB.Update(this);  } }
    }
}
