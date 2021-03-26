using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Brush : IBrush
	{
#if __IOS__ || IOS
		public abstract CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default);
#endif
	}

	public partial class SolidColorBrush : ISolidColorBrush
	{
#if __IOS__ || IOS
		public override CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			this.CreateCALayer(frame);
#endif
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
#if __IOS__ || IOS
		public override CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			this.CreateCALayer(frame);
#endif
	}

	public partial class RadialGradientBrush : IRadialGradientBrush
	{
#if __IOS__ || IOS
		public override CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			this.CreateCALayer(frame);
#endif
	}
}