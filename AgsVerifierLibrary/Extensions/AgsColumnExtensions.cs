using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsColumnExtensions
    {
        private static readonly PropertyInfo[] _columnProperties = typeof(AgsColumn).GetProperties();

        public static void SetColumnDescriptor(this AgsColumn column, Descriptor descriptor, string value)
        {
            _columnProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name(), StringComparison.InvariantCultureIgnoreCase))
                .SetValue(column, value, null);
        }

        public static IEnumerable<string> ReturnDataDistinctNonBlank(this AgsColumn column)
        {
            return column?.Data.Where(i => string.IsNullOrWhiteSpace(i) == false).Distinct();
        }

        public static IEnumerable<string> MergeData(this IEnumerable<AgsColumn> columns)
        {
            string[] exclusion = new string[] { string.Empty, null };
            return columns.SelectMany(c => c.Data.Where(x => !exclusion.Contains(x))); //SelectMany(i => i.Data);
        }

        public static IEnumerable<string> ReturnDescriptor(this IEnumerable<AgsColumn> columns, Descriptor descriptor)
        {
            PropertyInfo propertyInfo = _columnProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name(), StringComparison.InvariantCultureIgnoreCase));

            return columns.Select(x => propertyInfo.GetValue(x).ToString());
        }

        public static IEnumerable<AgsColumn> ByStatus(this IEnumerable<AgsColumn> columns, Status status)
        {
            return columns
                .Where(c =>
                    c.Status is not null
                    && c.Status.Contains(status.Name(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<AgsColumn> ByType(this IEnumerable<AgsColumn> columns, DataType dataType)
        {
            return columns
                .Where(c =>
                    c.Type is not null
                    && c.Type.Contains(dataType.Name(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<string> ReturnRows(this IEnumerable<AgsColumn> columns, string delimiter)
        {
            for (int i = 0; i < columns.FirstOrDefault().Data.Count; i++)
            {
                foreach (var column in columns)
                {
                    yield return string.Join(delimiter, columns.Select(c => c.Data[i]));
                }
            }
        }

        public static IEnumerable<List<string>> ReturnRows(this IEnumerable<AgsColumn> columns)
        {
            for (int i = 0; i < columns.FirstOrDefault().Data.Count; i++)
            {
                foreach (var column in columns)
                {
                    yield return columns.Select(c => c.Data[i]).ToList();
                }
            }
        }

        private static Dictionary<string, string> SingleRow(this IEnumerable<AgsColumn> columns, int rowIndex)
        {
            Dictionary<string, string> output = new();

            foreach (var column in columns)
            {
                AddField(output, column, rowIndex);
            }

            return output;
        }

        public static IEnumerable<Dictionary<string, string>> GetRows(this IEnumerable<AgsColumn> columns)
        {
            for (int i = 0; i < columns.First().Data.Count; i++)
            {
                yield return SingleRow(columns, i);
            }
        }

        public static IEnumerable<Dictionary<string, string>> GetRowsByFilter(this IEnumerable<AgsColumn> columns, string headingName, string filterText)
        {
            var column = columns.FirstOrDefault(c => c.Heading == headingName);

            for (int i = 0; i < column.Data.Count; i++)
            {
                if (column.Data[i] == filterText)
                    yield return SingleRow(columns, i);
            }
        }

        private static void AddField(Dictionary<string, string> dict, AgsColumn agsColumn, int rowIndex)
        {
            dict.Add(agsColumn.Heading, agsColumn.Data[rowIndex]);
        }
    }
}
