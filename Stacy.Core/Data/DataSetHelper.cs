using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public static class DataSetHelpers
    {
        public static DataSet ToDataSet<T>(this IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                t.Columns.Add(propInfo.Name, ColType);
            }

            //go through each property on T and add each value to the table
            foreach (T item in list)
            {
                DataRow row = t.NewRow();

                foreach (var propInfo in elementType.GetProperties())
                {
                    row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value;
                }

                t.Rows.Add(row);
            }

            return ds;
        }

        // function that set the given object from the given data row
        public static T RowToObject<T>(this DataRow row)
            where T : new()
        {
            var obj = new T();

            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                var p = obj.GetType().GetProperty(c.ColumnName);

                if (p == null || row[c] == DBNull.Value)
                    continue;

                if(c.DataType == p.PropertyType)
                {
                    p.SetValue(obj, row[c], null);
                }

                try
                {
                    //var converter = TypeDescriptor.GetConverter(propType);
                    //var result = converter.ConvertFrom(myString);

                    var converted = Convert.ChangeType(row[c], p.PropertyType);
                    p.SetValue(obj, converted, null);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error converting DataRow, type mismatch: " + ex.Message);
                }
            }

            return obj;
        }

        // function that creates a list of an object from the given data table
        public static List<T> TableToList<T>(this DataTable tbl)
            where T : new()
        {
            // define return list
            var lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(r.RowToObject<T>());
            }

            // return the list
            return lst;
        }
    }
}
