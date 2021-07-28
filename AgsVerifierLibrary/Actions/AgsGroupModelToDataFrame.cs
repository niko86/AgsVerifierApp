using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using Microsoft.Data.Analysis;
using System;
using System.Linq;

namespace AgsVerifierLibrary.Actions
{
    public static class AgsGroupModelToDataFrame
    {
        public static DataFrame ReturnDataFrame(AgsGroupModel group)
        {
            DataFrame df = new();

            foreach (var column in group.Columns)
            {
                string columnHeading = column.Heading;

                if (df.Columns.Any(c => c.Name.Contains(columnHeading)))
                {
                    int matchCount = df.Columns.Where(c => c.Name.Contains(columnHeading)).Count();
                    columnHeading = $"{columnHeading}{matchCount}";
                }

                if (column.Type == "DT")
                    df.Columns.Add(new PrimitiveDataFrameColumn<DateTime>(columnHeading, column.Data.CoerseToDateTime(column.Unit)));
                
                else if (column.Type == "T")
                    df.Columns.Add(new PrimitiveDataFrameColumn<TimeSpan>(columnHeading, column.Data.CoerseToTimeSpan(column.Unit)));

                else if (column.Type == "MC" || column.Type == "U")
                    df.Columns.Add(new PrimitiveDataFrameColumn<int>(columnHeading, column.Data.CoerseToInt()));

                else if (char.IsNumber(column.Type[0]))
                {
                    if (column.Type[0] == 0)
                        df.Columns.Add(new PrimitiveDataFrameColumn<int>(columnHeading, column.Data.CoerseToInt()));

                    else
                        df.Columns.Add(new PrimitiveDataFrameColumn<decimal>(columnHeading, column.Data.CoerseToDecimal()));
                }

                else
                    df.Columns.Add(new StringDataFrameColumn(columnHeading, column.Data));
            }
            
            return df;
        }
    }
}
