using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;


namespace Microsoft.Maui.Platform
{
	public class ContentViewGroup : ViewGroup, IMeasurable
	{
		IView? _virtualView;
		Size _measureCache;

		public ContentViewGroup(IView? view)
		{
			_virtualView = view;
			LayoutUpdated += OnLayoutUpdated;
		}

		public Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		public Func<Rect, Size>? CrossPlatformArrange { get; set; }

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return CrossPlatformMeasure?.Invoke(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}


		void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (CrossPlatformArrange == null || CrossPlatformMeasure == null)
				return;

			var platformGeometry = this.GetBounds().ToDP();

			var measured = CrossPlatformMeasure(platformGeometry.Width, platformGeometry.Height);
			if (measured != _measureCache && _virtualView?.Parent is IView parentView)
			{
				parentView?.InvalidateMeasure();
			}
			_measureCache = measured;

			if (platformGeometry.Width > 0 && platformGeometry.Height > 0)
			{
				platformGeometry.X = 0;
				platformGeometry.Y = 0;
				CrossPlatformArrange(platformGeometry);
			}
		}
	}
}
