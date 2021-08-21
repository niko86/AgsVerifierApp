using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Enums.EnumTools;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsContainerExtensions
    {
        public static IEnumerable<string> ReturnAllHeadings(this AgsContainer ags)
        {
            string[] exclusion = new string[] { FastStr(AgsDescriptor.HEADING), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Heading)).Select(x => x.Heading));
        }

        public static IEnumerable<string> ReturnAllUnits(this AgsContainer ags)
        {
            string[] exclusion = new string[] { FastStr(AgsDescriptor.UNIT), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllTypes(this AgsContainer ags)
        {
            string[] exclusion = new string[] { FastStr(AgsDescriptor.TYPE), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Type)).Select(x => x.Type));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfType(this AgsContainer ags, AgsDataType dataType)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Type is not null && c.Type.Contains(FastStr(dataType))));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this AgsContainer ags, string headingName)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Heading == headingName));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this AgsContainer ags, string headingName, AgsGroup excludingGroup)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Group != excludingGroup && c.Heading == headingName));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfNumericType(this AgsContainer ags)
        {
            string[] inclusion = new string[] { FastStr(AgsDataType.DT), FastStr(AgsDataType.MC), FastStr(AgsDataType.SCI), FastStr(AgsDataType.T) };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Type is not null && (inclusion.Contains(c.Type) ||  char.IsDigit(c.Type[0]))));
        }

        public static IEnumerable<string> ReturnGroupNames(this AgsContainer ags)
        {
            return ags.Groups.Select(c => c.Name);
        }
    }
}
