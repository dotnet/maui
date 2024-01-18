#nullable disable
using System;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DoubleCollection.xml" path="Type[@FullName='Microsoft.Maui.Controls.DoubleCollection']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(DoubleCollectionConverter))]
	[ValueConverter(typeof(DoubleCollectionValueConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DoubleCollection.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public DoubleCollection()
		{ }

		public DoubleCollection(double[] values)
			: base(values)
		{
		}

		public static implicit operator DoubleCollection(double[] d)
			=> d == null ? new() : new(d);

		public static implicit operator DoubleCollection(float[] f)
			=> f == null ? new() : new(Array.ConvertAll(f, x => (double)x));

		internal float[] ToFloatArray()
		{
			var array = new float[Count];
			for (int i = 0; i < Count; i++)
				array[i] = (float)this[i];
			return array;
		}
	}

#nullable enable
	internal sealed class DoubleCollectionValueConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> null;
		
		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> value switch
			{
				double[] doubles => (DoubleCollection)doubles,
				float[] floats => (DoubleCollection)floats,
				_ => null,
			};
	}
}
