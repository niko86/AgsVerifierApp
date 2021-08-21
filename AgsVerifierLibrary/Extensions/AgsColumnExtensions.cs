using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Enums.EnumTools;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsColumnExtensions
    {
        private static readonly PropertyInfo[] _columnProperties = typeof(AgsColumn).GetProperties();
        private static readonly string[] _exclusions = new string[] { string.Empty, null, "Index" };

        public static void SetColumnDescriptor(this AgsColumn column, AgsDescriptor descriptor, string value)
        {
            _columnProperties
                .FirstOrDefault(p => p.Name.ToUpper() ==  FastStr(descriptor))
                .SetValue(column, value, null);
        }

        public static bool Contains(this AgsColumn column, string value)
        {
            return column.Data.Contains(value);
        }

        public static AgsRow Row(this AgsColumn column, int index)
        {
            return column.Group.Rows[index];
        }

        public static IEnumerable<dynamic> ReturnDataDistinctNonBlank(this AgsColumn column)
        {
            return column?.Data.Where(i => string.IsNullOrWhiteSpace(i) == false).Distinct();
        }

        public static IEnumerable<string> ReturnDescriptor(this IEnumerable<AgsColumn> columns, AgsDescriptor descriptor)
        {
            PropertyInfo propertyInfo = _columnProperties.FirstOrDefault(p => p.Name.ToUpper() == FastStr(descriptor));
            
            return columns.Where(x => !_exclusions.Contains(x.Heading)).Select(x => propertyInfo.GetValue(x).ToString());
        }

        public static IEnumerable<dynamic> MergeData(this IEnumerable<AgsColumn> columns)
        {
            string[] exclusion = new string[] { string.Empty, null };
            return columns.SelectMany(c => c.Data.Where(x => !exclusion.Contains((string)x)));
        }

        public static IEnumerable<string> DelimitedKeyRows(this IEnumerable<AgsColumn> columns, string delimiter)
        {
            for (int i = 0; i < columns.FirstOrDefault().Data.Count; i++)
            {
                foreach (AgsColumn column in columns)
                {
                    yield return string.Join(delimiter, columns.Select(c => c.Data[i]));
                }
            }
        }
    }
}
