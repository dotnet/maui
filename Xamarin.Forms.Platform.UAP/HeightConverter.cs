using System;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class HeightConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double def;

			var ps = parameter as string;
			if (string.IsNullOrWhiteSpace(ps) || !double.TryParse(ps, out def))
			{
				def = double.NaN;
			}

			var val = (double)value;
			return val > 0 ? val : def;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}