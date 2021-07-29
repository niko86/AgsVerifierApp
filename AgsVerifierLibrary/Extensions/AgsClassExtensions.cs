using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsClassExtensions
    {
        private static readonly PropertyInfo[] _groupProperties = typeof(AgsGroupModel).GetProperties();
        private static readonly PropertyInfo[] _columnProperties = typeof(AgsColumnModel).GetProperties();

        public static void SetGroupDescriptorRowNumber(this AgsGroupModel group, Descriptor descriptor, int value)
        {
            _groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.ToString() + "Row", StringComparison.InvariantCultureIgnoreCase))
                .SetValue(group, value, null);
        }

        public static void SetColumnDescriptor(this AgsColumnModel column, Descriptor descriptor, string value)
        {
            _columnProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .SetValue(column, value, null);
        }

        public static IEnumerable<string> ReturnDataDistinctNonBlank(this AgsColumnModel column)
        {
            return column.Data.Where(i => string.IsNullOrWhiteSpace(i) == false).Distinct();
        }

        public static int GetGroupDescriptorRowNumber(this AgsGroupModel group, Descriptor descriptor)
        {
            return (int) _groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.ToString() + "Row", StringComparison.InvariantCultureIgnoreCase))
                .GetValue(group);
        }

        public static AgsGroupModel GetGroup(this List<AgsGroupModel> groups, string groupName)
        {
            return groups.FirstOrDefault(c => c.Name == groupName);
        }

        public static IEnumerable<string> ReturnGroupNames(this List<AgsGroupModel> groups)
        {
            return groups.Select(c => c.Name);
        }

        public static IEnumerable<string> ReturnAllHeadings(this List<AgsGroupModel> groups)
        {
            string[] exclusion = new string[] { Descriptor.HEADING.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllUnits(this List<AgsGroupModel> groups)
        {
            string[] exclusion = new string[] { Descriptor.UNIT.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllTypes(this List<AgsGroupModel> groups)
        {
            string[] exclusion = new string[] { Descriptor.TYPE.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<AgsColumnModel> GetAllColumnsOfType(this List<AgsGroupModel> groups, DataType dataType)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.Type == dataType.ToString()));
        }

        public static IEnumerable<AgsColumnModel> GetAllColumnsOfHeading(this List<AgsGroupModel> groups, string headingName)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.Heading == headingName));
        }

        public static IEnumerable<AgsColumnModel> GetAllColumnsOfHeading(this List<AgsGroupModel> groups, string headingName, string excludingGroup)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.Group != excludingGroup && c.Heading == headingName));
        }

        public static IEnumerable<AgsColumnModel> GetColumnsOfType(this AgsGroupModel group, DataType dataType)
        {
            return group.Columns.Where(c => c.Type == dataType.ToString());
        }

        public static IEnumerable<string> MergeData(this IEnumerable<AgsColumnModel> columns)
        {
            string[] exclusion = new string[] { string.Empty, null };
            return columns.SelectMany(c => c.Data.Where(x => !exclusion.Contains(x))); //SelectMany(i => i.Data);
        }

        public static AgsColumnModel GetColumn(this AgsGroupModel group, string headingName)
        {
            return group?.Columns.FirstOrDefault(c => c.Heading == headingName);
        }

        public static AgsColumnModel GetColumn(this AgsGroupModel group, int index)
        {
            return group.Columns.FirstOrDefault(c => c.Index == index);
        }
        
        public static IEnumerable<string> ReturnDescriptor(this IEnumerable<AgsColumnModel> columns, Descriptor descriptor)
        {
            PropertyInfo propertyInfo = _columnProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.ToString(), StringComparison.InvariantCultureIgnoreCase));

            return columns.Select(x => propertyInfo.GetValue(x).ToString());
        }

        public static IEnumerable<AgsColumnModel> ByStatus(this IEnumerable<AgsColumnModel> columns, Status status)
        {
            return columns
                .Where(c =>
                    c.Status is not null
                    && c.Status.Contains(status.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<AgsColumnModel> ByType(this IEnumerable<AgsColumnModel> columns, DataType dataType)
        {
            return columns
                .Where(c =>
                    c.Type is not null
                    && c.Type.Contains(dataType.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        private static Dictionary<string, string> SingleRow(this AgsGroupModel group, int rowIndex)
        {
            Dictionary<string, string> output = new();

            foreach (var column in group.Columns)
            {
                AddField(output, column, rowIndex);
            }

            return output;
        }

        public static IEnumerable<Dictionary<string, string>> GetRows(this AgsGroupModel group)
        {
            var column = group.GetColumn(0);

            for (int i = 0; i < column.Data.Count; i++)
            {
                yield return SingleRow(group, i);
            }
        }

        public static IEnumerable<Dictionary<string, string>> GetRowsByFilter(this AgsGroupModel group, string headingName, string filterText)
        {
            var column = group.GetColumn(headingName);

            for (int i = 0; i < column.Data.Count; i++)
            {
                if (column.Data[i] == filterText)
                    yield return SingleRow(group, i);
            }
        }

        public static IEnumerable<string> ReturnRows(this IEnumerable<AgsColumnModel> columns, string delimiter)
        {
            for (int i = 0; i < columns.FirstOrDefault().Data.Count; i++)
            {
                foreach (var column in columns)
                {
                    yield return string.Join(delimiter, columns.Select(c => c.Data[i]));
                }
            }
        }

        public static IEnumerable<List<string>> ReturnRows(this IEnumerable<AgsColumnModel> columns)
        {
            for (int i = 0; i < columns.FirstOrDefault().Data.Count; i++)
            {
                foreach (var column in columns)
                {
                    yield return columns.Select(c => c.Data[i]).ToList();
                }
            }
        }

        public static IEnumerable<Dictionary<string, string>> AndBy(this IEnumerable<Dictionary<string, string>> dict, string key, string filterText)
        {
            return dict?
                .Where(d => d
                    .GetValueOrDefault(key)
                    .Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<Dictionary<string, string>> AndBy(this IEnumerable<Dictionary<string, string>> dict, string key, Descriptor filterText)
        {
            return dict?
                .Where(d => d
                    .GetValueOrDefault(key)
                    .Contains(filterText.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<Dictionary<string, string>> AndBy(this IEnumerable<Dictionary<string, string>> dict, string key, DataType filterText)
        {
            return dict?
                .Where(d => d
                    .GetValueOrDefault(key)
                    .Contains(filterText.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static string ReturnFirstValueOf(this IEnumerable<Dictionary<string, string>> dict, string key)
        {
            return dict?.FirstOrDefault()?.GetValueOrDefault(key) ?? string.Empty;
        }

        public static string ReturnValueOfByIndex(this IEnumerable<Dictionary<string, string>> dict, string key, int index)
        {
            return dict?.ElementAtOrDefault(index)?.GetValueOrDefault(key) ?? string.Empty;
        }

        public static IEnumerable<string> ReturnAllValuesOf(this IEnumerable<Dictionary<string, string>> dict, string key)
        {
            return dict?.Select(d => d.GetValueOrDefault(key));
        }

        private static void AddField(Dictionary<string, string> dict, AgsColumnModel agsColumn, int rowIndex)
        {
            dict.Add(agsColumn.Heading, agsColumn.Data[rowIndex]);
        }
    }
}
