using System.Collections;
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

	public partial class GradientBrush : IGradientBrush
	{
		IGradientStopCollection IGradientBrush.GradientStops
		{
			get => new MauiCollection(GradientStops);
		}

		class MauiCollection : IGradientStopCollection
		{
			private GradientStopCollection _gradientStops;

			public MauiCollection(GradientStopCollection gradientStops)
			{
				_gradientStops = gradientStops;
			}

			public IGradientStop this[int index] => _gradientStops[index];

			public int Count => _gradientStops.Count;

			public IEnumerator<IGradientStop> GetEnumerator() => _gradientStops.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}

	public partial class LinearGradientBrush : ILinearGradientBrush
	{
	}

	public partial class RadialGradientBrush : IRadialGradientBrush
	{
	}
}