using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{
		public DoubleCollection()
		{ }

		public DoubleCollection(double[] values)
			: base(values)
		{
		}

		public static implicit operator DoubleCollection(double[] d) => new DoubleCollection(d);

		public static implicit operator DoubleCollection(float[] f) => new(f.Cast<double>().ToArray());
	}
}