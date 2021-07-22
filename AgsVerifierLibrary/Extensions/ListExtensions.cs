using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Extensions
{
    public static class ListExtensions
    {
		public static List<DateTime?> CoerseToDateTime(this List<string> list, string unit)
		{
			List<DateTime?> output = new();
			string pattern = @"(?=(?<!h:)m{2,4}(?!\s))(?<!\s)m{2,4}(?!:s)|(?<!h:)m{2,4}(?!:s)";
			var correctedUnit = Regex.Replace(unit, pattern, m => m.Value.ToUpper());

			foreach (var item in list)
			{
				dynamic parsedValue = DateTime.TryParseExact(item, correctedUnit, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outDateTime) ? outDateTime : null;

				output.Add(parsedValue);
			}

			return output;
		}

		public static List<TimeSpan?> CoerseToTimeSpan(this List<string> list, string unit)
		{
			List<TimeSpan?> output = new();

			foreach (var item in list)
			{
				dynamic parsedValue = TimeSpan.TryParseExact(item, unit, CultureInfo.InvariantCulture, out TimeSpan outTimeSpan) ? outTimeSpan : null;

				output.Add(parsedValue);
			}

			return output;
		}

		public static List<int?> CoerseToInt(this List<string> list)
		{
			List<int?> output = new();

			foreach (var item in list)
			{
				dynamic parsedValue = int.TryParse(item, NumberStyles.AllowExponent, null, out int outInteger) ? outInteger : null;

				output.Add(parsedValue);
			}

			return output;
		}

		public static List<decimal?> CoerseToDecimal(this List<string> list)
		{
			List<decimal?> output = new();

			foreach (var item in list)
			{
				dynamic parsedValue = decimal.TryParse(item, NumberStyles.AllowExponent, null, out decimal outDecimal) ? outDecimal : null;

				output.Add(parsedValue);
			}

			return output;
		}
	}
}
