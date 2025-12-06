
using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Platform
{
	//TODO make this public on NET8
	internal static class CalendarDatePickerExtensions
	{
		public static string ToDateFormat(this string dateFormat)
		{
			// The WinUI CalendarDatePicker DateFormat property use this formatter:
			// https://docs.microsoft.com/en-us/uwp/api/Windows.Globalization.DateTimeFormatting.DateTimeFormatter?redirectedfrom=MSDN&view=winrt-22621#code-snippet-2

			if (string.IsNullOrEmpty(dateFormat) || CheckDateFormat(dateFormat))
				return string.Empty;

			// Handle standard .NET DateTime format strings (single characters)
			if (dateFormat.Length == 1)
			{
				string convertedFormat = ConvertStandardFormat(dateFormat);

				// If ConvertStandardFormat returns a WinUI format (starts with '{'), return it directly
				if (!string.IsNullOrEmpty(convertedFormat) && convertedFormat.StartsWith("{"))
					return convertedFormat;

				// If empty, use default
				if (string.IsNullOrEmpty(convertedFormat))
					return string.Empty;

				// Otherwise, it's a .NET pattern that needs conversion - let it fall through
				dateFormat = convertedFormat;
			}

			// Handle custom format strings (or resolved standard formats)
			string result = string.Empty;
			string separator = GetSeparator(dateFormat); var parts = dateFormat.Split(separator);

			if (parts.Length > 0)
			{
				for (int i = 0; i < parts.Length; i++)
				{
					if (i < parts.Length - 1)
						result += GetPart(parts[i]) + separator;
					else
						result += GetPart(parts[i]);
				}
			}

			return result;
		}

		internal static string ConvertStandardFormat(string format)
		{
			var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;

			switch (format)
			{
				// Supported formats - return .NET pattern to be converted through normal flow
				case "d": // Short date pattern
					return dtfi.ShortDatePattern;
				case "D": // Long date pattern
					return dtfi.LongDatePattern;
				case "m":
				case "M": // Month day pattern
					return dtfi.MonthDayPattern;
				case "y":
				case "Y": // Year month pattern
					return dtfi.YearMonthPattern;

				// Unsupported formats - return WinUI format string directly
				case "f": // Full date/time pattern (short time) - use long date since time is not applicable
					return "{dayofweek.full} {month.full} {day.integer}, {year.full} {hour.integer}:{minute.integer(2)} {period.abbreviated}";
				case "F": // Full date/time pattern (long time) - include seconds as per .NET standard
					return "{dayofweek.full} {month.full} {day.integer}, {year.full} {hour.integer}:{minute.integer(2)}:{second.integer(2)} {period.abbreviated}";
				case "g": // General date/time pattern (short time) - use short date since time is not applicable  
					return "{month.integer}/{day.integer}/{year.abbreviated} {hour.integer}:{minute.integer(2)} {period.abbreviated}";
				case "G": // General date/time pattern (long time) - use short date since time is not applicable
					return "{month.integer}/{day.integer}/{year.abbreviated} {hour.integer}:{minute.integer(2)}:{second.integer(2)} {period.abbreviated}";
				case "u":
					return "{year.full}-{month.integer(2)}-{day.integer(2)} {hour.integer(2)}:{minute.integer(2)}:{second.integer(2)}Z";
				case "U": // Universal full date/time pattern - use long date since time is not applicable
					return "{dayofweek.full} {month.full} {day.integer} {year.full} {hour.integer(2)}:{minute.integer(2)}:{second.integer(2)}";
				case "o":
				case "O": // Round-trip date/time pattern - use ISO 8601 format
					return "{year.full}-{month.integer(2)}-{day.integer(2)}T{hour.integer(2)}:{minute.integer(2)}:{second.integer(7)}";
				case "r":
				case "R": // RFC1123 pattern - use abbreviated format as close approximation
					return "{dayofweek.abbreviated}, {day.integer(2)} {month.abbreviated} {year.full} {hour.integer(2)}:{minute.integer(2)}:{second.integer(2)} GMT";
				case "s": // Sortable date/time pattern - use numeric format
					return "{year.full}-{month.integer(2)}-{day.integer(2)}T{hour.integer(2)}:{minute.integer(2)}:{second.integer(2)}";
				default:
					// For unrecognized formats, return empty string so that they use the default format
					return string.Empty;
			}
		}

		internal static string GetSeparator(string format)
		{
			string separator;

			if (format.Contains('/', StringComparison.CurrentCultureIgnoreCase))
				separator = "/";
			else if (format.Contains('-', StringComparison.CurrentCultureIgnoreCase))
				separator = "-";
			else if (format.Contains(' ', StringComparison.CurrentCultureIgnoreCase))
				separator = " ";
			else if (format.Contains('.', StringComparison.CurrentCultureIgnoreCase))
				separator = ".";
			else
				separator = string.Empty;

			return separator;
		}

		internal static string GetPart(string format)
		{
			if (IsDay(format))
				return GetDayFormat(format);
			else if (IsMonth(format))
				return GetMonthFormat(format);
			else if (IsYear(format))
				return GetYearFormat(format);
			else
				return string.Empty;
		}

		internal static bool IsDay(string day)
		{
			if (day.Contains('d', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetDayFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{day.integer}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{dayofweek.full}";
			}
			else
			{
				var day = format.Count(x => x == 'd');

				if (day == 3)
					return "{dayofweek.abbreviated}";
				else if (day == 4)
					return "{dayofweek.full}";
				else
					return $"{{day.integer({day})}}";
			}
		}

		internal static bool IsMonth(string day)
		{
			if (day.Contains('m', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetMonthFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{month}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{month.full}";
			}
			else
			{
				var month = format.Count(x => string.Equals(new string(new char[] { x }), "M", StringComparison.OrdinalIgnoreCase));

				if (month <= 2)
					return $"{{month.integer({month})}}";
				else if (month == 3)
					return "{month.abbreviated}";
				else
					return "{month.full}";
			}
		}

		internal static bool IsYear(string day)
		{
			if (day.Contains('y', StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		internal static string GetYearFormat(string format)
		{
			if (CheckDateFormat(format))
			{
				return "{year}";
			}
			else if (format.Equals("D", StringComparison.Ordinal))
			{
				return "{year.full}";
			}
			else
			{
				var year = format.Count(x => x == 'y');

				if (year <= 2)
					return "{year.abbreviated}";
				else
					return "{year.full}";
			}
		}

		internal static bool CheckDateFormat(string format)
		{
			return string.IsNullOrWhiteSpace(format) || format.Equals("d", StringComparison.Ordinal);
		}
	}
}
