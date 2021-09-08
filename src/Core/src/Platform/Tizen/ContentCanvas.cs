using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui
{
	public class ContentCanvas : Canvas, IMeasurable
	{
		IView _virtualView;
		Size _measureCache;

		public ContentCanvas(EvasObject parent, IView view) : base(parent)
		{
			_virtualView = view;
			LayoutUpdated += OnLayoutUpdated;
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return CrossPlatformMeasure?.Invoke(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }

		protected void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			var nativeGeometry = Geometry.ToDP();

			var measured = CrossPlatformMeasure!(nativeGeometry.Width, nativeGeometry.Height);
			if (measured != _measureCache)
			{
				_virtualView?.Parent?.InvalidateMeasure();
			}
			_measureCache = measured;

			if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0)
			{
				nativeGeometry.X = 0;
				nativeGeometry.Y = 0;
				CrossPlatformArrange!(nativeGeometry);
			}
		}
	}
}
