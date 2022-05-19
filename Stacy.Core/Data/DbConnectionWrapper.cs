using System.Data;
using System.Data.Common;

namespace Stacy.Core.Data
{
    public class DbConnectionWrapper : DbConnection
    {
        private readonly DbConnection _connection;

        public DbConnectionWrapper(DbConnection connection)
        {
            _connection = connection;
        }

        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _connection.Close();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _connection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return _connection.CreateCommand();
        }

        public override void Open()
        {
            _connection.Open();
        }

        public override string ConnectionString
        {
            get { return _connection.ConnectionString; } 
            set { _connection.ConnectionString = value;  }
        }

        public override string Database => _connection.Database;

        public override string DataSource => _connection.DataSource;

        public override string ServerVersion => _connection.ServerVersion;

        public override ConnectionState State => _connection.State;
    }
}
