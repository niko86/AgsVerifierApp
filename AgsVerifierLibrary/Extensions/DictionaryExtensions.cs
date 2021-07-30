using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Extensions
{
    public static class DictionaryExtensions
    {
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
                    .Contains(filterText.Name(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<Dictionary<string, string>> AndBy(this IEnumerable<Dictionary<string, string>> dict, string key, DataType filterText)
        {
            return dict?
                .Where(d => d
                    .GetValueOrDefault(key)
                    .Contains(filterText.Name(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<Dictionary<string, string>> AndBy(this IEnumerable<Dictionary<string, string>> dict, string key, Status filterText)
        {
            return dict?
                .Where(d => d
                    .GetValueOrDefault(key)
                    .Contains(filterText.Name(), StringComparison.InvariantCultureIgnoreCase));
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

    }
}
