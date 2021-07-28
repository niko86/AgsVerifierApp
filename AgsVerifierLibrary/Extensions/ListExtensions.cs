using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Extensions
{
    public static class ListExtensions
    {
		private static readonly Regex _dateTimeRegex = new(@"(?=(?<!h:)m{2,4}(?!\s))(?<!\s)m{2,4}(?!:s)|(?<!h:)m{2,4}(?!:s)", RegexOptions.Compiled);

		public static IEnumerable<DateTime?> CoerseToDateTime(this List<string> list, string unit)
		{
			if (list is null) yield break;

			var correctedUnit = _dateTimeRegex.Replace(unit, m => m.Value.ToUpper());

			foreach (var item in list)
			{
				dynamic parsedValue = DateTime.TryParseExact(item, correctedUnit, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outDateTime) ? outDateTime : null;

				yield return parsedValue;
			}
		}

		public static IEnumerable<TimeSpan?> CoerseToTimeSpan(this List<string> list, string unit)
		{
			if (list is null) yield break;

			foreach (var item in list)
			{
				dynamic parsedValue = TimeSpan.TryParseExact(item, unit, CultureInfo.InvariantCulture, out TimeSpan outTimeSpan) ? outTimeSpan : null;

				yield return parsedValue;
			}
		}

		public static IEnumerable<int?> CoerseToInt(this List<string> list)
		{
			if (list is null) yield break;

			foreach (var item in list)
			{
				dynamic parsedValue = int.TryParse(item, NumberStyles.AllowExponent, null, out int outInteger) ? outInteger : null;

				yield return parsedValue;
			}
		}

		public static IEnumerable<decimal?> CoerseToDecimal(this List<string> list)
		{
			if (list is null) yield break;

			foreach (var item in list)
			{
				dynamic parsedValue = decimal.TryParse(item, NumberStyles.AllowExponent, null, out decimal outDecimal) ? outDecimal : null;

				yield return parsedValue;
			}
		}
	}
}
