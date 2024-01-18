#nullable disable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(Shapes.PointCollectionConverter))]
	[ValueConverter(typeof(PointCollectionValueConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{
		public PointCollection() : base()
		{
		}

		public PointCollection(Point[] p)
			: base(p)
		{
		}

		public static implicit operator PointCollection(Point[] d)
			=> d == null ? new() : new(d);
	}

#nullable enable
	internal sealed class PointCollectionValueConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> null;

		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> value switch
			{
				Point[] points => (PointCollection)points,
				_ => null,
			};
	}
}