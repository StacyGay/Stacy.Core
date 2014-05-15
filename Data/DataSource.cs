using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Stacy.Core.Data
{
	public class DataSource
	{
		public enum DatabaseUser { fuelapp, fuelstorage }

		public static IDbConnection Connect(string database, DatabaseUser user = DatabaseUser.fuelapp)
		{
			var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
			var connection = factory.CreateConnection();
			connection.ConnectionString = GetConnectionString(database, user);
			connection.Open();

			return connection;
		}

		public static IDbConnection ConnectCustom(string connectionString)
		{
			var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
			var connection = factory.CreateConnection();
			connection.ConnectionString = connectionString;
			connection.Open();

			return connection;
		}

		public static string GetConnectionString(string database, DatabaseUser user = DatabaseUser.fuelapp)
		{
			const string connectAddress = "localhost";
			const string devAddress = @"127.0.0.1,52823";
			string userName = Enum.GetName(typeof (DatabaseUser), user);
			string pw = "";
			bool staging;

			switch (user)
			{
				case DatabaseUser.fuelapp: pw = "test";
					break;
				case DatabaseUser.fuelstorage: pw = "test";
					break;
			}

			var sqlConString = new SqlConnectionStringBuilder();

            switch (Environment.MachineName)
            {
                case "Test1":
                case "Test2": staging = true;
                    break;
                default: staging = false;
                    break;
            }

			if (staging)
				sqlConString.DataSource = devAddress;
			else
				sqlConString.DataSource = connectAddress;

			sqlConString.UserID = userName;
			sqlConString.Password = pw;
			sqlConString.InitialCatalog = database;
			sqlConString.ConnectTimeout = 120;

			return sqlConString.ConnectionString;
		}
	}
}