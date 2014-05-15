using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Stacy.Core.Data
{
	public class Querier
	{
		private string connectAddress = "sqlcluster2";
		private string db;
		private string devAddress = @"10.10.146.10\GUESTDESK_STAGE,52823";
		private ArrayList parameters;
		private string pw = "FictionFaction305";
		private string userName = "fuelapp";

		public Querier(string database, bool dev = false)
		{
			db = database;
			parameters = new ArrayList();
			devMode = dev;
		}

		public string lastQueryString { get; set; }
		public bool devMode { get; set; }

		~Querier()
		{
			//sqlCon.Close();	
		}

		public void NewSQLConnection(string address, string user, string pass, string database, bool dev = false)
		{
			if (dev)
				devAddress = address;
			else
				connectAddress = address;
			userName = user;
			pw = pass;
			db = database;
		}

		public String AddParameter(object data, SqlDbType type)
		{
			var parameter = new SqlParameter("@Param" + (parameters.Count + 1).ToString(), type);
			parameter.Value = data;
			parameters.Add(parameter);
			return "@Param" + parameters.Count.ToString();
		}

		public String QueryParam(object data, SqlDbType type)
		{
			return AddParameter(data, type);
		}

		public void ClearParameters()
		{
			parameters = new ArrayList();
		}

		public DataTable Query(string strSQL)
		{
			var returnDS = new DataSet();
			var returnTable = new DataTable();

			lastQueryString = strSQL;
			parameters.Reverse();
			foreach (SqlParameter p in parameters)
				lastQueryString = lastQueryString.Replace(p.ParameterName, p.Value.ToString());
			parameters.Reverse();

			try
			{
				using (var sqlCon = new SqlConnection(createSQLConString(db)))
				{
					var daClients = new SqlDataAdapter(strSQL, sqlCon);

					foreach (SqlParameter p in parameters)
						daClients.SelectCommand.Parameters.Add(p);
					daClients.Fill(returnDS);
				}
			}
			catch (Exception e)
			{
				throw new QuerierException("Querier Error: " + e.Message + "\n Query String: \n" + lastQueryString, e);
			}

			// clear old parameters
			parameters = new ArrayList();

			if (returnDS.Tables.Count > 0)
				returnTable = returnDS.Tables[0];
			return returnTable;
		}

		private string createSQLConString(string database)
		{
			var sqlConString = new SqlConnectionStringBuilder();

			if (devMode)
				sqlConString.DataSource = devAddress;
			else
				sqlConString.DataSource = connectAddress;

			sqlConString.UserID = userName;
			sqlConString.Password = pw;
			sqlConString.InitialCatalog = database;

			return sqlConString.ConnectionString;
		}

		public String[] ColumnToArray(DataTable dataTable, string columnName)
		{
			return Array.ConvertAll(dataTable.Select(), delegate(DataRow row) { return row[columnName].ToString(); });
		}
	}

	public class QuerierException : _ErrorException
	{
		public QuerierException(string errorMessage) : base(errorMessage)
		{
		}

		public QuerierException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx)
		{
		}
	}
}