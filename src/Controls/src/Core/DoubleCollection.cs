using System;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{
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
}