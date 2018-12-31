using System.Collections.Generic;
using System.Data.Common;

namespace DCI.Data
{
    public class DbDataSource
    {
        public DbDataSource(DbProviderFactory dbProviderFactory, string connectString)
        {
            this._connection = dbProviderFactory.CreateConnection();
            this._connection.ConnectionString = connectString;
            this._connection.Open();
        }

        private DbConnection _connection;

        public IEnumerable<double> GetData(string commandText, int index, params object[] parameters)
        {
            var dbDataReader = DbHelper.ExecuteCommandText(this._connection, commandText, parameters);
            var iterator = DbDataReadIterator.CreateIterator(index, dbDataReader);
            foreach(var value in iterator)
            {
                yield return value;
            }
        }
    }
}