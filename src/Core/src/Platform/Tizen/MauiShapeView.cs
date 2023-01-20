using Tizen.UIExtensions.Common;
using Microsoft.Maui.Graphics.Skia.Views;

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