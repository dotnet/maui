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
		public LayoutCanvas(EvasObject parent) : base(parent) { }

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return CrossPlatformMeasure?.Invoke(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}
}
