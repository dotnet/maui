using ElmSharp;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui
{
	public class MauiBoxView : SkiaGraphicsView
	{
		public MauiBoxView(EvasObject parent) : base(parent)
		{
			DeviceScalingFactor = (float)Tizen.UIExtensions.Common.DeviceInfo.ScalingFactor;
		}
	}
}