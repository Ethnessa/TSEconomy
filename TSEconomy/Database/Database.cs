using PetaPoco;
using PetaPoco.Providers;
using TSEconomy.Database.Models;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy.Database
{
    public class Database
    {
        public IDatabase DB;

        /// <summary>
        /// Initializes and connects to the database
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

                    TShock.Log.Info(Localization.TryGetString("TSEconomy database connected! (MySQL)", "InitializeDB"));
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(Localization.TryGetString("TSEconomy experienced a database error! (MySQL)", "InitializeDB"));
                    TShock.Log.ConsoleError(ex.Message);
                }

            }
            else // using sqlite
            {
                var connString = customSQLitePath == "" ? $"Data Source={TSEconomy.PluginDirectory}/TSEconomy.sqlite;" : $"Data Source={customSQLitePath};";
                DB = DatabaseConfiguration.Build()
                    .UsingConnectionString(connString)
                    .UsingProvider<SQLiteDatabaseProvider>()
                    .Create();

                DB.Execute("PRAGMA foreign_keys = ON;");

                try
                {
                    EnsureTableStructure(DBType.SQLite);
                    TShock.Log.Info(Localization.TryGetString("TSEconomy database connected! (SQLite)", "InitializeDB"));
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(Localization.TryGetString("TSEconomy experienced a database error! (SQLite)", "InitializeDB"));
                    TShock.Log.ConsoleError(ex.Message);
                }

            }
        }

        public void EnsureTableStructure(DBType type)
        {
            new Table<BankAccount>(DB, type);
            new Table<Models.Transaction>(DB, type);
        }
    }
}
