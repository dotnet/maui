using System;
using System.ComponentModel;
using Controls.Core.Design;

namespace Microsoft.Maui.Controls.Design
{
	public class ThicknessTypeDesignConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH ThicknessTypeConverter.ConvertFrom
			string strValue = value?.ToString()?.Trim();
			if (string.IsNullOrEmpty(strValue))
				return false;

			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{
				int? count = DesignTypeConverterHelper.TryParseNumbers(value?.ToString(), ',', maxCount: 4);
				return count == 2 || count == 4;
			}
			else
			{
				// Example: "1 2 3 4". Any count of numbers between 1 and 4 is valid.
				// Note that ThicknessTypeConverter is sensitive to spaces, e.g.
				// "1 2" is valid but "1   2" is not. We match its behavior here.
				return DesignTypeConverterHelper.TryParseNumbers(strValue.Trim(), ' ', maxCount: 4) is int;
			}
		}
	}
}
