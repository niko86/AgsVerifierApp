using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Enums.EnumTools;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsGroupExtensions
    {
        private static readonly PropertyInfo[] _groupProperties = typeof(AgsGroup).GetProperties();
        private static readonly PropertyInfo[] _columnProperties = typeof(AgsColumn).GetProperties();

        public static IEnumerable<AgsColumn> ColumnsByStatus(this AgsGroup group, AgsStatus status)
        {
            return group.Columns
                .Where(c =>
                    c.Status is not null
                    && c.Status.Contains(FastStr(status), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<AgsColumn> ColumnsByType(this AgsGroup group, AgsDataType dataType)
        {
            return group.Columns
                .Where(c =>
                    c.Type is not null
                    && c.Type.Contains(FastStr(dataType), StringComparison.InvariantCultureIgnoreCase));
        }

        public static void SetGroupDescriptorRowNumber(this AgsGroup group, AgsDescriptor descriptor, int value)
        {
            _groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(FastStr(descriptor) + "Row", StringComparison.InvariantCultureIgnoreCase))
                .SetValue(group, value, null);
        }

        public static int GetGroupDescriptorRowNumber(this AgsGroup group, AgsDescriptor descriptor)
        {
            return (int)_groupProperties
                .FirstOrDefault(p => p.Name
                    .Contains(FastStr(descriptor) + "Row", StringComparison.InvariantCultureIgnoreCase))
                .GetValue(group);
        }

        public static IEnumerable<AgsColumn> GetColumnsOfType(this AgsGroup group, AgsDataType dataType)
        {
            return group.Columns.Where(c => c.Type == FastStr(dataType));
        }

        public static IEnumerable<AgsColumn> GetColumnsOfStatus(this AgsGroup group, AgsStatus status)
        {
            string[] exclusions = new string[] { string.Empty, null, "Index", "HEADING" };

            return group.Columns.Where(c => !exclusions.Contains(c.Heading) && c.Status.Contains(FastStr(status)));
        }

        public static IEnumerable<string> ReturnDescriptor(this AgsGroup group, AgsDescriptor descriptor)
        {
            string[] exclusions = new string[] { string.Empty, null, "Index", "HEADING" };

            PropertyInfo propertyInfo = _columnProperties
                .FirstOrDefault(p => p.Name
                    .Contains(FastStr(descriptor), StringComparison.InvariantCultureIgnoreCase));

            return group.Columns.Where(g => !exclusions.Contains(g.Heading)).Select(x => propertyInfo.GetValue(x).ToString());
        }
    }
}
