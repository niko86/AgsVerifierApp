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
        private static readonly PropertyInfo[] _groupProperties = typeof(AgsGroupModel).GetProperties();

        public static void SetGroupDescriptorRowNumber(this AgsGroupModel group, Descriptor descriptor, int value)
        {
            _groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name() + "Row", StringComparison.InvariantCultureIgnoreCase))
                .SetValue(group, value, null);
        }

        public static int GetGroupDescriptorRowNumber(this AgsGroupModel group, Descriptor descriptor)
        {
            return (int)_groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(descriptor.Name() + "Row", StringComparison.InvariantCultureIgnoreCase))
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
            return groups.SelectMany(g => g.Columns.Where(c => c.Type == dataType.Name()));
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
            return group.Columns.Where(c => c.Type == dataType.Name());
        }

        public static IEnumerable<AgsColumnModel> GetColumnsOfStatus(this AgsGroupModel group, Status status)
        {
            return group.Columns.Where(c => c.Type.Contains(status.Name()));
        }

        public static AgsColumnModel GetColumn(this AgsGroupModel group, string headingName)
        {
            return group?.Columns.FirstOrDefault(c => c.Heading == headingName);
        }

        public static AgsColumnModel GetColumn(this AgsGroupModel group, int index)
        {
            return group.Columns.FirstOrDefault(c => c.Index == index);
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

        private static void AddField(Dictionary<string, string> dict, AgsColumnModel agsColumn, int rowIndex)
        {
            dict.Add(agsColumn.Heading, agsColumn.Data[rowIndex]);
        }
    }
}
