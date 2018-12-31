using System.Data;
using System.Data.Common;

namespace DCI.Data
{
    public static class DbHelper
    {
        public static DbConnection OpenConnection(DbProviderFactory dbProviderFactory, string connection)
        {
            var conn = dbProviderFactory.CreateConnection();
            conn.ConnectionString = connection;
            conn.Open();
            return conn;
        }
        public static DbDataReader ExecuteCommandText(DbConnection dbConnection, string text, params object[] parameters)
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = text;
            if (parameters != null)
            {
                var count = 0;
                foreach (var p in parameters)
                {
                    count++;
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"P{count}";
                    parameter.Value = p;
                    parameter.DbType = DbType.String;
                    command.Parameters.Add(parameter);
                }
            }
            return command.ExecuteReader();

        }
    }
}