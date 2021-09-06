using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Handlers
{
	public partial class BoxViewHandler : ViewHandler<IBoxView, W2DGraphicsView>
	{
		protected override W2DGraphicsView CreateNativeView()
		{
			return new W2DGraphicsView
			{
				Drawable = new BoxViewDrawable(VirtualView)
			};
		}

		public static void MapColor(BoxViewHandler handler, IBoxView boxView)
		{
			handler.NativeView?.InvalidateBoxView(boxView);
		}

		public static void MapCornerRadius(BoxViewHandler handler, IBoxView boxView)
		{
			handler.NativeView?.InvalidateBoxView(boxView);
		}
	}
}
