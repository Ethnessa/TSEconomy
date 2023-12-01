using PetaPoco;
using System.Reflection;
using System.Text;
using TSEconomy.Database.Models;

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
            // Simplified mapping, needs to be expanded based on actual requirements
            if (type == typeof(int) || type == typeof(BankAccountProperties) 
                || type == typeof(TransactionProperties) ) return "INTEGER";
            if (type == typeof(double)) return "REAL";
            if (type == typeof(string)) return (dbProvider == DBType.SQLite ? "TEXT" : "VARCHAR(255)");
            if (type == typeof(DateTime)) return "DATETIME";

            throw new NotSupportedException($"Type {type} not supported.");
        }
    }
}
