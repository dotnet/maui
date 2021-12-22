using Tizen.UIExtensions.NUI.GraphicsView;
using Tizen.UIExtensions.Common;
using GSize = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : SkiaGraphicsView, IMeasurable
	{
		protected virtual double DefaultSize => 40d;

		Size IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			return new GSize(DefaultSize, DefaultSize).ToPixel();
		}
	}
}