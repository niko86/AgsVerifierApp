using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsContainerExtensions
    {
        public static IEnumerable<string> ReturnAllHeadings(this AgsContainer ags)
        {
            string[] exclusion = new string[] { AgsDescriptor.HEADING.Name(), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Heading)).Select(x => x.Heading));
        }

        public static IEnumerable<string> ReturnAllUnits(this AgsContainer ags)
        {
            string[] exclusion = new string[] { AgsDescriptor.UNIT.Name(), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Unit)).Select(x => x.Unit));
        }

        public static IEnumerable<string> ReturnAllTypes(this AgsContainer ags)
        {
            string[] exclusion = new string[] { AgsDescriptor.TYPE.Name(), string.Empty, null };
            return ags.Groups.SelectMany(g => g.Columns.Where(c => !exclusion.Contains(c.Type)).Select(x => x.Type));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfType(this AgsContainer ags, AgsDataType dataType)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Type == dataType.Name()));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this AgsContainer ags, string headingName)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Heading == headingName));
        }

        public static IEnumerable<AgsColumn> GetAllColumnsOfHeading(this AgsContainer ags, string headingName, string excludingGroup)
        {
            return ags.Groups.SelectMany(g => g.Columns.Where(c => c.Group.Name != excludingGroup && c.Heading == headingName));
        }

        public static IEnumerable<string> ReturnGroupNames(this AgsContainer ags)
        {
            return ags.Groups.Select(c => c.Name);
        }
    }
}
