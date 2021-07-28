using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Extensions
{
    public static class DataFrameExtensions
    {
        public static DataFrame FilterColumnByValue(this DataFrame df, string columnToFilter, string rowFilter)
        {
            PrimitiveDataFrameColumn<bool> mask = df[columnToFilter].ElementwiseEquals(rowFilter);
            return df.Filter(mask);
        }

        public static DataFrame FilterColumnContains(this DataFrame df, string columnToFilter, string rowFilter)
        {
            List<bool> bools = new();

            for (int i = 0; i < df.Rows.Count; i++)
            {
                bool check = df["DICT_STAT"][i].ToString().Contains("key", StringComparison.InvariantCultureIgnoreCase);

                if (check)
                    bools.Add(true);
                else
                    bools.Add(false);

            }

            PrimitiveDataFrameColumn<bool> mask = new("Temp", bools);
            return df.Filter(mask);
        }

        public static DataFrame AgsDictGroupHeadings(this DataFrame df, string groupName)
        {
            return df.FilterColumnByValue("DICT_GRP", groupName).FilterColumnByValue("DICT_TYPE", "HEADING");
        }

        public static DataFrame RemoveColumnsExcept(this DataFrame df, string[] excludedColumnNames)
        {
            var columnNames = df.Columns.Select(col => col.Name).ToArray();
            foreach (var columnName in columnNames)
            {
                if (excludedColumnNames.Contains(columnName) == false)
                {
                    df.Columns.Remove(columnName);
                }
            }
            return df;
        }

        public static List<dynamic> FilterColumnToList(this DataFrame df, string columnToFilter, string rowFilter, string columnToReturn)
        {
            PrimitiveDataFrameColumn<bool> mask = df[columnToFilter].ElementwiseEquals(rowFilter);
            var data = df.Filter(mask)[columnToReturn];
            Type type = data.DataType;

            if (type == typeof(int))
                return ((IEnumerable<dynamic>)data.Cast<int>()).ToList();

            else if (type == typeof(decimal))
                return ((IEnumerable<dynamic>)data.Cast<decimal>()).ToList();

            else if (type == typeof(DateTime))
                return ((IEnumerable<dynamic>)data.Cast<DateTime>()).ToList();

            else if (type == typeof(TimeSpan))
                return ((IEnumerable<dynamic>)data.Cast<TimeSpan>()).ToList();
            else
                return ((IEnumerable<dynamic>)data.Cast<string>()).ToList();
        }
    }
}
