using PetaPoco;

namespace TSEconomy.Database
{
    public class Table<T> where T : class
    {
        public IDatabase database;
        public DBType type;

        public Table(IDatabase db, DBType type)
        {
            database = db;
            this.type = type;

            var statement = SqlGenerator.GenerateCreateTableStatement(typeof(T), type);
            db.Execute(statement);
        }
    }
}
