using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Extensions
{
    public static class AgsRowExtensions
    {
        private static readonly FieldInfo _groupField = typeof(AgsRow).GetField("_group", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<AgsRow> AndBy(this IEnumerable<AgsRow> rows, string key, string filter)
        {
            return rows?.Where(d => d.Contains(key, filter));
        }

        public static IEnumerable<AgsRow> AndBy(this IEnumerable<AgsRow> rows, string key, Descriptor descriptor)
        {
            return rows
                .Where(d => (string)d[key] == descriptor.Name());
        }

        public static IEnumerable<AgsRow> AndBy(this IEnumerable<AgsRow> rows, string key, Status status)
        {
            return rows
                .Where(d => (string)d[key] == status.Name());
        }

        public static IEnumerable<AgsRow> AndBy(this IEnumerable<AgsRow> rows, string key, int filter)
        {
            return rows.Where(d => (int)d[key] == filter);
        }

        public static string FirstOf(this IEnumerable<AgsRow> rows, string key)
        {
            return (string)rows?.FirstOrDefault()?[key];
        }

        public static IEnumerable<dynamic> AllOf(this IEnumerable<AgsRow> rows, string key)
        {
            return rows?.Select(d => d[key]);
        }

        public static bool Contains(this AgsRow row, string key, string filter)
        {
            return row[key].ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string ToStringByStatus(this AgsRow row, Status status)
        {
            // Using reflection to keep logic out of Row model
            AgsGroup group = (AgsGroup)_groupField.GetValue(row);

            if (status is Status.KEY)
            {
                return string.Join('|', group.GetColumnsOfStatus(status).Select(c => row[c.Heading]));
            }
            else if (status is Status.REQUIRED)
            {
                List<string> temp = new();

                for (int i = 0; i < row.Count(); i++)
                {
                    if (i == 0) // Skips index column, inelegant hack
                        continue;

                    if (group[i].Status == Status.REQUIRED.Name() && string.IsNullOrWhiteSpace(row[i].ToString()))
                    {
                        temp.Add("???");
                        continue;
                    }

                    temp.Add(row[i].ToString());
                }

                return string.Join('|', temp);
            }

            return row.ToString();
        }

        public static string ToStringByMask(this AgsRow row, IEnumerable<string> mask)
        {
            List<string> temp = new();

            foreach (var item in mask)
            {
                try
                {
                    temp.Add(row[item].ToString());
                }
                catch (Exception)
                {
                    //TODO 
                    throw;
                }
            }
            return string.Join('|', temp);
        }
    }
}
