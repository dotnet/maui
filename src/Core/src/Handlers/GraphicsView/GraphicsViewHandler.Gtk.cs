using System;

namespace Microsoft.Maui.Handlers
{

	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, Microsoft.Maui.Graphics.Native.Gtk.GtkGraphicsView>
	{

		protected override Microsoft.Maui.Graphics.Native.Gtk.GtkGraphicsView CreateNativeView() => new();

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			if (handler.NativeView is { } nativeView)
			{
				nativeView.Drawable = graphicsView.Drawable;
			}
		}

	}

}