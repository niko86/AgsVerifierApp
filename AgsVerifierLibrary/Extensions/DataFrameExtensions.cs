using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Extensions
{
    public static class DataFrameExtensions
    {
        public static DataFrameRowCollection FilterRowsByColumn(this DataFrame df, string columnName, string rowFilter)
        {
            PrimitiveDataFrameColumn<bool> mask = df[columnName].ElementwiseEquals(rowFilter);
            return df.Filter(mask).Rows;
        }
    }
}
