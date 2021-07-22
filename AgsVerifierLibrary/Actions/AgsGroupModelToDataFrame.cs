using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Actions
{
    public static class AgsGroupModelToDataFrame
    {
        public static DataFrame ReturnDataFrame(AgsGroupModel group)
        {
            DataFrame df = new();

            foreach (var column in group.Columns)
            {
                if (column.Type == "DT")
                    df.Columns.Add(new PrimitiveDataFrameColumn<DateTime>(column.Heading, column.Data.CoerseToDateTime(column.Unit)));
                
                else if (column.Type == "T")
                    df.Columns.Add(new PrimitiveDataFrameColumn<TimeSpan>(column.Heading, column.Data.CoerseToTimeSpan(column.Unit)));

                else if (column.Type == "MC" || column.Type == "U")
                    df.Columns.Add(new PrimitiveDataFrameColumn<int>(column.Heading, column.Data.CoerseToInt()));

                else if (char.IsNumber(column.Type[0]))
                {
                    if (column.Type[0] == 0)
                        df.Columns.Add(new PrimitiveDataFrameColumn<int>(column.Heading, column.Data.CoerseToInt()));

                    else
                        df.Columns.Add(new PrimitiveDataFrameColumn<decimal>(column.Heading, column.Data.CoerseToDecimal()));
                }

                else
                    df.Columns.Add(new StringDataFrameColumn(column.Heading, column.Data));
            }
            
            return df;
        }
    }
}
