using Microsoft.Maui.Graphics.Skia.Views;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, SkiaGraphicsView>
	{
		protected override SkiaGraphicsView CreateNativeView()
		{
			return new SkiaGraphicsView(NativeParent)
			{
				DeviceScalingFactor = (float)Tizen.UIExtensions.Common.DeviceInfo.ScalingFactor
			};
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateDrawable(graphicsView);
		}
	}
}