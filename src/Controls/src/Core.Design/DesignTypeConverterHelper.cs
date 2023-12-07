using System.Globalization;
using System.Linq;

namespace Controls.Core.Design
{
	internal static class DesignTypeConverterHelper
	{
		/// <summary>
		/// Returns count of numbers in the string. Returns null if some of the values are invalid or total count exceeds max count.
		/// </summary>
		public static int? TryParseNumbers(string numberCollection, char separator, int maxCount)
		{
			// Examples: "1,2" or "1 2 3 4". 
			if (string.IsNullOrEmpty(numberCollection))
				return null;

			string[] parts = numberCollection.Split(separator);
			if (parts.Length > maxCount)
				return null; // too many numbers

			if (parts.All(p => double.TryParse(p, NumberStyles.Number, CultureInfo.InvariantCulture, out _)))
				return parts.Length; // all numbers are valid

			return null;
		}
	}
}
