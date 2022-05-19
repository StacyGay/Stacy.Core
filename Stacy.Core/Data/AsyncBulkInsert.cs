using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stacy.Core.Debug;
using Dapper;

namespace Stacy.Core.Data
{
    public class AsyncBulkInsert<T>
        where T : new()
    {
        private readonly List<Tuple<string, string>> fieldMappings = new List<Tuple<string, string>>();
        private readonly List<Tuple<string, string>> fieldExclusions = new List<Tuple<string, string>>();
        private IEnumerable<ColumnDetails> tableSchema;

        private IDbConnection _preservedConnection;

        public String TempTable { get; set; }
        public bool StrictMap { get; set; }
        public bool SuccessLogging { get; set; } = false;
        public string LoggingTable { get; set; }
        public int Timeout { get; set; } = 60000;

        public AsyncBulkInsert()
        {
            StrictMap = false;
            TempTable = "";
        }

        public string Table { get; set; }
        public string ConnectionString { get; set; }

        ~AsyncBulkInsert()
        {
            FreeData();
//            _preservedConnection?.Dispose();
        }

        public void FreeData()
        {
            try
            {
                if (TempTable.Length > 0)
                {
                    using (var db = DataSource.ConnectCustom(ConnectionString))
                        db.Execute(String.Format(
                        @"IF OBJECT_ID('{0}', 'U') IS NOT NULL
							DROP TABLE {0};",
                        TempTable));
                }
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public AsyncBulkInsert<T> SetStrictMap(bool strictMap = true)
        {
            StrictMap = strictMap;
            return this;
        }

        public AsyncBulkInsert<T> SetTable(string table)
        {
            Table = table;
            return this;
        }

        public AsyncBulkInsert<T> SetTempTable(string tempTableName = "")
        {
            TempTable = tempTableName;
            CreateTempTable();
            return this;
        }

        public AsyncBulkInsert<T> SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public string CreateTempTable()
        {
            if(string.IsNullOrEmpty(Table))
                throw new Exception("AsyncBulkInsert Exception: No destination table set; you must declare a destination table before creating a temporary table");

            try
            {
                if (String.IsNullOrEmpty(TempTable))
                    TempTable = "##T" + Guid.NewGuid().ToString().Replace("-", "_");

                _preservedConnection = DataSource.ConnectCustom(ConnectionString);
                var sql = string.Format(@"IF OBJECT_ID('{0}', 'U') IS NOT NULL DROP TABLE {0};", TempTable);
                _preservedConnection.Execute(sql);

                var tableCreator = new SqlTableCreator(_preservedConnection as SqlConnection)
                {
                    DestinationTableName = TempTable
                };
                tableSchema = tableCreator.CreateFromSqlTable(Table);

            }
            catch (Exception ex)
            { 
                // TODO: Add additional logging
                throw new Exception("AsyncBulkInsert error creating temporary table", ex);
            }

            return TempTable;
        }

        public AsyncBulkInsert<T> AddMapping(string src, string dest)
        {
            fieldMappings.Add(new Tuple<string, string>(src, dest));
            return this;
        }

        public AsyncBulkInsert<T> AddExclusion(string src, string dest)
        {
            fieldExclusions.Add(new Tuple<string, string>(src, dest));
            return this;
        }

        public async Task AddData(List<T> data)
        {
            data = PrepData(data);
            using (var dataTable = data.ToDataTable())
            {
                var addTask = Task.Run(() => AddDataTask(dataTable));

                try
                {
                    await addTask;
                    dataTable.Dispose();
                }
                catch (Exception ex)
                {
                    dataTable.Dispose();
                    throw new Exception("Error inserting to temp table", ex);
                }
            }

                

        }

        public async Task AddDataTask(DataTable dataTable)
        {
            using (var db = (SqlConnection)DataSource.ConnectCustom(ConnectionString))
            {
                using (var bulkCopy = new SqlBulkCopy(db))
                {
                    bulkCopy.DestinationTableName = TempTable;

                    if (fieldMappings.Count > 0) // map using supplied mappings
                    {
                        fieldMappings.ForEach(m => bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(m.Item1, m.Item2)));
                    }
                    else if (tableSchema != null && tableSchema.Any()) // map by table schema
                    {
                        var props = new T().GetType().GetProperties(); // check to make sure table fields exist in object

                        var propsToMap = tableSchema.Where(c => !c.IsIdentity && props.Any(p => p.Name == c.Name)).ToList();
                        propsToMap.ForEach(c => bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.Name, c.Name)));
                    }
                    else // map by reflection
                    {
                        var props = new T().GetType().GetProperties();
                        props.ToList().ForEach(p => bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(p.Name, p.Name)));
                    }

                    fieldExclusions.ForEach(e => bulkCopy.ColumnMappings.Remove(new SqlBulkCopyColumnMapping(e.Item1, e.Item2)));

                    bulkCopy.BulkCopyTimeout = Timeout;
                    bulkCopy.BatchSize = 5000;

                    await bulkCopy.WriteToServerAsync(dataTable);
                    dataTable.Dispose();
                }
            }
        }

        private string GetDedupeSourceQuery(IEnumerable<string> keys, IEnumerable<string> columns)
        {
            var keyList = keys.Select(k => "[" + k + "]").ToList();
            var columnList = columns
                .Select(c => "[" + c + "]")
                .Where(c => !keyList.Any(k => k.ToLower().Equals(c.ToLower())))
                .Select(c => String.Format("MAX({0}) as {0}", c))
                .ToList();

            var keyParam = String.Join(", ", keyList);

            var columnParam = String.Join(", ", columnList);

            var query = String.Format(
                @"SELECT {0}, {1}
				FROM {2}
				GROUP BY {0}",
                keyParam,
                columnParam,
                TempTable);

            return query;
        }

        public AsyncBulkInsert<T> ExecuteMerge(IEnumerable<string> keys, bool delete = false, string deleteParam = "")
        {
            var keyList = keys.ToSafeList();

            var keysCondition = new List<string>();
            keyList.ToList().ForEach(k =>
            {
                var condition = "t.[" + k + "]=s.[" + k + "]";
                keysCondition.Add(condition);
            });

            var identityList = tableSchema
                .Where(c => c.IsIdentity)
                .Select(k => k.Name).ToList();

            var columns = new List<string>();

            if (StrictMap)
            {
                columns = fieldMappings.Except(fieldExclusions).Select(m => m.Item2).ToList();
            }
            else
            {
                foreach (var column in tableSchema)
                {
                    if (identityList.All(k => k != column.Name) && fieldExclusions.All(e => e.Item2 != column.Name))
                        columns.Add("[" + column.Name + "]");
                }
            }

            var columnsUpdate = new List<string>();
            var columnsInsert = new List<string>();
            columns.ForEach(c =>
            {
                string currentColumn = c + " = s." + c;
                columnsUpdate.Add(currentColumn);
                string insertColumn = "s." + c;
                columnsInsert.Add(insertColumn);
            });

            var dedupeSourceQuery = GetDedupeSourceQuery(keyList, columns);

            //(" + String.Join(", ", columns) + @") merge_hint
            string mergeQuery =
                @"merge " + Table + @" as t
				using (" + dedupeSourceQuery + @") as s 
					on (" + String.Join(" and ", keysCondition) + @")
				when matched then 
					update set " + String.Join(", ", columnsUpdate) + @" 
				when not matched by target then 
					insert (" + String.Join(", ", columns) + @") 
					values (" + String.Join(", ", columnsInsert) + @") ";
            if (delete)
            {
                if (!String.IsNullOrWhiteSpace(deleteParam))
                {
                    mergeQuery +=
                    @"when not matched by source and " + deleteParam + @" then
						delete";
                }
                else
                {
                    mergeQuery +=
                    @"when not matched by source then
						delete";
                }

            }
            mergeQuery += ";";

            try
            {
                using (var db = DataSource.ConnectCustom(ConnectionString))
                {
                    var trans = db.BeginTransaction();
                    db.Execute(mergeQuery, transaction: trans, commandTimeout: Timeout);
                    trans.Commit();

                    if (SuccessLogging && !string.IsNullOrEmpty(LoggingTable))
                    {
                        db.Execute(@"INSERT INTO " + LoggingTable + " (MergeQuery) VALUES (@mergeQuery)",
                            new {mergeQuery});
                    }
                }
            }
            finally
            {
                FreeData();
            }

            return this;
        }

        private static List<T> PrepData(List<T> data)
        {
            var props = typeof(T).GetProperties();
            var dateProps = props.Where(p => p.PropertyType == typeof(DateTime)).ToList();
            foreach (var item in data)
            {
                dateProps.Where(p => ((DateTime)p.GetValue(item)) < new DateTime(1900, 1, 1))
                    .ToList()
                    .ForEach(p => p.SetValue(item, new DateTime(1900, 01, 01)));

            }

            return data;
        }
    }
}
