using PetaPoco;
using PetaPoco.Providers;
using TSEconomy.Database.Models;
using TShockAPI;

namespace TSEconomy.Database
{
    public class Database
    {
        protected IDatabase DB;

        /// <summary>
        /// Initializes & connects to the database
        /// </summary>
        /// <param name="useMySQL">If set to true, MySQL will be used. Otherwise, use SQLite</param>
        public void InitializeDB(bool useMySQL, string customSQLitePath = "")
        {
            if (useMySQL) // using mysql
            {
                var tshockConfig = TShock.Config.Settings;
                var connString = $"Server={tshockConfig.MySqlHost};Database={tshockConfig.MySqlDbName};Username={tshockConfig.MySqlUsername};Password={tshockConfig.MySqlPassword};";

                DB = DatabaseConfiguration.Build()
                    .UsingConnectionString(connString)
                    .UsingProvider<MySqlDatabaseProvider>()
                    .Create();

                try
                {

                    EnsureTableStructure(DBType.MySQL);

                    TShock.Log.Info($"TSEconomy database connected! (MySQL)");
                }
                catch (Exception ex)
                {
                    TShock.Log.Info($"TSEconomy experienced a database error! (MySQL)");
                    TShock.Log.Info(ex.Message);
                }

            }
            else // using sqlite
            {
                var connString = customSQLitePath == "" ? $"Data Source=tshock/TSEconomy.sqlite;" : $"Data Source={customSQLitePath};";
                DB = DatabaseConfiguration.Build()
                    .UsingConnectionString(connString)
                    .UsingProvider<SQLiteDatabaseProvider>()
                    .Create();

                DB.Execute("PRAGMA foreign_keys = ON;");

                try
                {
                    EnsureTableStructure(DBType.SQLite);
                    TShock.Log.Info($"TSEconomy database connected! (SQLite)");
                }
                catch (Exception ex)
                {
                    TShock.Log.Info($"TSEconomy experienced a database error! (SQLite)");
                    TShock.Log.Info(ex.Message);
                }

            }
        }

        public void EnsureTableStructure(DBType type)
        {
            new Table<BankAccount>(DB, type);
        }
    }
}
