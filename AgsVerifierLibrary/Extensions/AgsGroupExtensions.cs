using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsGroupExtensions
    {
        private static readonly PropertyInfo[] _groupProperties = typeof(AgsGroup).GetProperties();

        public static void SetGroupDescriptorRowNumber(this AgsGroup group, Descriptor descriptor, int value)
        {
            _groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name() + "Row", StringComparison.InvariantCultureIgnoreCase))
                .SetValue(group, value, null);
        }

        public static int GetGroupDescriptorRowNumber(this AgsGroup group, Descriptor descriptor)
        {
            return (int)_groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name() + "Row", StringComparison.InvariantCultureIgnoreCase))
                .GetValue(group);
        }

        public static IEnumerable<string> ReturnGroupNames(this List<AgsGroup> groups)
        {
            return groups.Select(c => c.Name);
        }

        public static IEnumerable<string> ReturnAllHeadings(this List<AgsGroup> groups)
        {
            string[] exclusion = new string[] { Descriptor.HEADING.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllUnits(this List<AgsGroup> groups)
        {
            string[] exclusion = new string[] { Descriptor.UNIT.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllTypes(this List<AgsGroup> groups)
        {
            string[] exclusion = new string[] { Descriptor.TYPE.ToString(), string.Empty, null };
            return groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfType(this List<AgsGroup> groups, DataType dataType)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.Type == dataType.Name()));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this List<AgsGroup> groups, string headingName)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.Heading == headingName));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this List<AgsGroup> groups, string headingName, string excludingGroup)
        {
            return groups.SelectMany(g => g.Columns.Where(c => c.PartOfGroup != excludingGroup && c.Heading == headingName));
        }

        public static IEnumerable<AgsColumn> GetColumnsOfType(this AgsGroup group, DataType dataType)
        {
            return group.Columns.Where(c => c.Type == dataType.Name());
        }

        public static IEnumerable<AgsColumn> GetColumnsOfStatus(this AgsGroup group, Status status)
        {
            return group.Columns.Where(c => c.Type.Contains(status.Name()));
        }

        private static Dictionary<string, string> SingleRow(this AgsGroup group, int rowIndex)
        {
            Dictionary<string, string> output = new();

            foreach (var column in group.Columns)
            {
                AddField(output, column, rowIndex);
            }

            return output;
        }

        public static IEnumerable<Dictionary<string, string>> GetRows(this AgsGroup group)
        {
            var column = group[0];

            for (int i = 0; i < column.Data.Count; i++)
            {
                yield return SingleRow(group, i);
            }
        }

        public static IEnumerable<Dictionary<string, string>> GetRowsByFilter(this AgsGroup group, string headingName, string filterText)
        {
            var column = group[headingName];

            for (int i = 0; i < column.Data.Count; i++)
            {
                if (column.Data[i] == filterText)
                    yield return SingleRow(group, i);
            }
        }

        public static IEnumerable<Dictionary<string, string>> GetRowsByFilter(this AgsGroup group, string headingName, Descriptor descriptor)
        {
            var column = group[headingName];

            for (int i = 0; i < column.Data.Count; i++)
            {
                if (column.Data[i] == descriptor.Name())
                    yield return SingleRow(group, i);
            }
        }

        private static void AddField(Dictionary<string, string> dict, AgsColumn agsColumn, int rowIndex)
        {
            dict.Add(agsColumn.Heading, agsColumn.Data[rowIndex]);
        }
    }
}
