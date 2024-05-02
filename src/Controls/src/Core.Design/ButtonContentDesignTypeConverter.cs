using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	public class ButtonContentDesignTypeConverter : StringConverter
	{
		private static readonly char[] Separators = { ',' };

		// MUST MATCH values of ButtonContentConverter.ImagePosition. NOTE that we use enum rather than strings
		// to better match ButtonContentConverter. First, Enum.Parse will accept int values like 1 or 20. Second,
		// values passed to converter contain numbers and enum values, i.e. there is a possibility that we might
		// need to handle ints in place of enums. ButtonContentConverter does not properly handle corner cases.
		// For example, it does not trim incoming strings, so " 15" will be converted as
		// ButtonContentLayout((ImagePorition)15, DefaultSpacing/*10*/)
		private enum ImagePosition { Left, Top, Right, Bottom };

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH ButtonContentConverter.ConvertFrom
			string stringValue = value?.ToString();
			if (string.IsNullOrEmpty(stringValue))
				return false;

			string[] parts = stringValue.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 2)
				return false;

			if (char.IsDigit(parts[0][0]))
			{
				// Examples: "5" or "10, Top"
				if (!double.TryParse(parts[0], out _))
					return false; // bogus number, e.g. 5a
				if (parts.Length == 2 && !Enum.TryParse(parts[1], true, out ImagePosition _))
					return false; // bogus position, e.g. "Hello"
			}
			else
			{
				// Examples: "Right" or "Bottom, 5"
				if (!Enum.TryParse(parts[0], true, out ImagePosition _))
					return false; // bogus position, e.g. "Hello"
				if (parts.Length == 2 && !double.TryParse(parts[1], out _))
					return false; // bogus number, e.g. 5a
			}

			return true;
		}
	}
}
