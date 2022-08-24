using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : SkiaGraphicsView, IMeasurable
	{
		Size IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			return new Size(0, 0);
		}
	}
}