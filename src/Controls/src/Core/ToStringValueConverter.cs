#nullable disable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	class ToStringValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return null;
After:
			{
				return null;
			}
*/
			{

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return formattable.ToString(parameter?.ToString(), culture);
After:
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return formattable.ToString(parameter?.ToString(), culture);
After:
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return formattable.ToString(parameter?.ToString(), culture);
After:
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return formattable.ToString(parameter?.ToString(), culture);
After:
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return formattable.ToString(parameter?.ToString(), culture);
After:
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}
*/
				return null;
			}

			if (value is IFormattable formattable)
			{
				return formattable.ToString(parameter?.ToString(), culture);
			}

			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
