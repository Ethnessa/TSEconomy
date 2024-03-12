using PetaPoco;
using System.Reflection;
using System.Text;
using TSEconomy.Database.Models.Properties;

namespace TSEconomy.Database
{
    public static class SqlGenerator
    {
        public static string GenerateCreateTableStatement(Type type, DBType dbProvider)
        {
            var tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute?.Value ?? type.Name;
            var primaryKeyAttribute = type.GetCustomAttribute<PrimaryKeyAttribute>();
            var primaryKey = primaryKeyAttribute?.Value ?? "ID";

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            foreach (var prop in type.GetProperties())
            {
                var columnAttribute = prop.GetCustomAttribute<ColumnAttribute>();
                var columnName = columnAttribute?.Name ?? prop.Name;
                var columnType = GetColumnType(prop.PropertyType, dbProvider);

                sb.Append($"    {columnName} {columnType}");

                if (columnName == primaryKey)
                {
                    sb.Append(" PRIMARY KEY");
                }

                sb.AppendLine(",");
            }

            sb.Remove(sb.Length - 3, 3); // Remove the last comma
            sb.AppendLine(");");

            return sb.ToString();
        }
        private static string GetColumnType(Type type, DBType dbProvider)
        {
            var supportedTypeMappings = new Dictionary<Type, string>
            {
                { typeof(int), "INTEGER" },
                { typeof(BankAccountProperties), "INTEGER" },
                { typeof(TransactionProperties), "INTEGER" },
                { typeof(double), "REAL" },
                { typeof(string), dbProvider == DBType.SQLite ? "TEXT" : "VARCHAR(255)" },
                { typeof(DateTime), "DATETIME" },
                { typeof(byte[]), "BLOB" },
                { typeof(bool), dbProvider == DBType.MySQL ? "BOOLEAN" : "INTEGER" }
            };

            if (supportedTypeMappings.TryGetValue(type, out var columnType))
            {
                return columnType;
            }

            throw new NotSupportedException($"Type {type} not supported.");
        }


    }
}
