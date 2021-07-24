using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Extensions
{
    public static class DataFrameExtensions
    {
        public static DataFrameRowCollection FilterRowsByColumn(this DataFrame df, string columnToFilter, string rowFilter)
        {
            PrimitiveDataFrameColumn<bool> mask = df[columnToFilter].ElementwiseEquals(rowFilter);
            return df.Filter(mask).Rows;
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
