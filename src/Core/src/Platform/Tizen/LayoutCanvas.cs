using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutCanvas : Canvas, IMeasurable
	{
		IView _virtualView;
		Size _measureCache;

		public LayoutCanvas(EvasObject parent, IView view) : base(parent)
		{
			_virtualView = view;
			LayoutUpdated += OnLayoutUpdated;
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return CrossPlatformMeasure?.Invoke(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

		protected void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			var platformGeometry = Geometry.ToDP();

			var measured = CrossPlatformMeasure!(platformGeometry.Width, platformGeometry.Height);
			if (measured != _measureCache && _virtualView?.Parent is IView parentView)
			{
				parentView?.InvalidateMeasure();
			}
			_measureCache = measured;

			if (platformGeometry.Width > 0 && platformGeometry.Height > 0)
			{
				platformGeometry.X = 0;
				platformGeometry.Y = 0;
				CrossPlatformArrange!(platformGeometry);
			}
		}
	}
}
