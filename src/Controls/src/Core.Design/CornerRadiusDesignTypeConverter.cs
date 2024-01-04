using System;
using System.ComponentModel;
using System.Globalization;
using Controls.Core.Design;

namespace Microsoft.Maui.Controls.Design
{
	public class CornerRadiusDesignTypeConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH CornerRadiusTypeConverter.ConvertFrom
			var strValue = value?.ToString();
			if (strValue != null)
			{
				if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
				{
					var parts = strValue.Split(',');

					// Example: "1,2,3,4"
					if (parts.Length == 4)
					{
						foreach (string part in parts)
						{
							if (!double.TryParse(part, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
								return false;
						}

						return true;
					}

					// Example: "1,a,b". CornerRadiusTypeConverter has unusual behavior
					// for 2 or 3 token string. We match its behavior here
					if (parts.Length < 4)
						return double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out _);
				}
				else
				{
					// Example: "1 2 3 4". Any count of numbers between 1 and 4 is valid.
					// Note that CornerRadiusTypeConverter is sensitive to spaces, e.g.
					// "1 2" is valid but "1   2" is not. We match its behavior here.
					return DesignTypeConverterHelper.TryParseNumbers(strValue.Trim(), ' ', maxCount: 4) is int;
				}
			}

			return false;
		}
	}
}
