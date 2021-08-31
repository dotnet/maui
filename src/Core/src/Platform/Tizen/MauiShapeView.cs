using ElmSharp;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui
{
	public class MauiShapeView : SkiaGraphicsView
	{
		public MauiShapeView(EvasObject parent) : base(parent)
		{
			DeviceScalingFactor = (float)Tizen.UIExtensions.Common.DeviceInfo.ScalingFactor;
		}
	}
}