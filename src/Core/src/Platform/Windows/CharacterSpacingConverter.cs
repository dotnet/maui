using System;

namespace Microsoft.Maui.Platform;

internal sealed class CharacterSpacingConverter : UI.Xaml.Data.IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is double characterSpacing)
		{
			return characterSpacing.ToEm();
		}

		return 0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotSupportedException();
	}
}