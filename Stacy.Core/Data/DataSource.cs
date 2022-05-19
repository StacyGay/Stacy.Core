using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Stacy.Core.Data
{
    public interface IDataSource
    {
        DbConnectionWrapper CreateConnection(string database, DataSource.DatabaseUser user = DataSource.DatabaseUser.defaultUser);
        DbConnectionWrapper CreateCustomConnection(string connectionString);
        string CreateConnectionString(string database, DataSource.DatabaseUser user = DataSource.DatabaseUser.defaultUser);
    }

    public class DataSource : IDataSource
    {

        public DataSource()
        {
        }

        public enum DatabaseUser { defaultUser }

        public DbConnectionWrapper CreateConnection(string database, DatabaseUser user = DatabaseUser.defaultUser)
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var connection = factory.CreateConnection();
            connection.ConnectionString = GetConnectionString(database, user);
            connection.Open();

            return new DbConnectionWrapper(connection);
        }

        public DbConnectionWrapper CreateCustomConnection(string connectionString)
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            connection.Open();

            return new DbConnectionWrapper(connection);
        }

        public string CreateConnectionString(string database, DatabaseUser user = DatabaseUser.defaultUser)
        {
            return GetConnectionString(database, user);
        }

        public static IDbConnection ConnectCustom(string connectionString)
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            connection.Open();

            return connection;
        }

        public static string GetConnectionString(string database, DatabaseUser user = DatabaseUser.defaultUser)
        {
            const string connectAddress = ""; // TODO: Set up connect address
            
            string userName = Enum.GetName(typeof(DatabaseUser), user);
            string pw = "";
            bool staging;

            switch (user)
            {
                case DatabaseUser.defaultUser:
                    pw = ""; // TODO: Handle defaultUser password lookup
                    break;
            }

            var sqlConString = new SqlConnectionStringBuilder();

            switch (Environment.MachineName)
            {
                case "STAGING":
                    staging = true;
                    break;
                default:
                    staging = false;
                    break;
            }

            sqlConString.DataSource = connectAddress;

            sqlConString.UserID = userName;
            sqlConString.Password = pw;
            sqlConString.InitialCatalog = database;
            sqlConString.ConnectTimeout = 30;
            sqlConString.MinPoolSize = 100;
            sqlConString.MaxPoolSize = 2000;

            return sqlConString.ConnectionString + ";TransparentNetworkIPResolution=False";
            //return sqlConString.ConnectionString;
        }
    }
}