using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSEconomy.Database.Models.Properties
{
    /// <summary>
    /// Enum to identify whether a transaction adds or set a value
    /// can be used to track a player's real balance via TSEconomy's
    /// transaction database
    /// </summary>
    public enum TransactionProperties
    {
        Set,
        Add
    }
}
