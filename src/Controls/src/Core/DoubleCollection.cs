#nullable disable
using System;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>An observable collection of <see cref="double"/> values, used for stroke dash patterns and similar properties.</summary>
	[System.ComponentModel.TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{
		/// <summary>Initializes a new empty <see cref="DoubleCollection"/>.</summary>
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
