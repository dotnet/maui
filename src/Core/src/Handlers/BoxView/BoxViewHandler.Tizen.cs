using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class BoxViewHandler : ViewHandler<IBoxView, MauiBoxView>
	{
		protected override MauiBoxView CreateNativeView()
		{
			return new MauiBoxView(NativeParent!)
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
