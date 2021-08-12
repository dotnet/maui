using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TSize = Tizen.UIExtensions.Common.Size;
using Size = Microsoft.Maui.Graphics.Size;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;

namespace Microsoft.Maui
{
	public class LayoutCanvas : Canvas, IMeasurable
	{
		Rectangle _arrangeCache;
		IView _virtualView;

		public LayoutCanvas(EvasObject parent, IView view) : base(parent)
		{
			_arrangeCache = default(Rectangle);
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

			if (_arrangeCache == nativeGeometry)
				return;

			if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0)
			{
				nativeGeometry.X = _virtualView.Frame.X;
				nativeGeometry.Y = _virtualView.Frame.Y;
				CrossPlatformMeasure!(nativeGeometry.Width, nativeGeometry.Height);
				CrossPlatformArrange!(nativeGeometry);
			}
		}
	}
}
