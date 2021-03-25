using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Brush : IBrush
	{
	}

	public partial class SolidColorBrush : ISolidColorBrush
	{
	}

	public partial class GradientStop : IGradientStop
	{
	}

	public partial class GradientStopCollection : IGradientStopCollection
	{
		IGradientStop IList<IGradientStop>.this[int index] { get => this[index]; set => this[index] = (GradientStop)value; }

		bool ICollection<IGradientStop>.IsReadOnly => ((ICollection<GradientStop>)this).IsReadOnly;

		void ICollection<IGradientStop>.Add(IGradientStop item) => Add((GradientStop)item);

		bool ICollection<IGradientStop>.Contains(IGradientStop item) => Contains((GradientStop)item);

		void ICollection<IGradientStop>.CopyTo(IGradientStop[] array, int arrayIndex)
		{
			_ = array ?? throw new ArgumentNullException(nameof(array));

			for (int src = 0, dst = arrayIndex; src < Count && dst < array.Length - arrayIndex; src++, dst++)
			{
				array[dst] = this[src];
			}
		}

		IEnumerator<IGradientStop> IEnumerable<IGradientStop>.GetEnumerator() => GetEnumerator();

		int IList<IGradientStop>.IndexOf(IGradientStop item) => IndexOf((GradientStop)item);

		void IList<IGradientStop>.Insert(int index, IGradientStop item) => Insert(index, (GradientStop)item);

		bool ICollection<IGradientStop>.Remove(IGradientStop item) => Remove((GradientStop)item);
	}

	public partial class GradientBrush : IGradientBrush
	{
		IGradientStopCollection IGradientBrush.GradientStops
		{
			get => GradientStops;
			set => GradientStops = (GradientStopCollection)value;
		}
	}

	public partial class LinearGradientBrush : ILinearGradientBrush
	{
	}

	public partial class RadialGradientBrush : IRadialGradientBrush
	{
	}
}