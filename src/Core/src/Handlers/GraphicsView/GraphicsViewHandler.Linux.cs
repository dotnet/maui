using System;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, Gtk.Widget>
	{
		protected override Gtk.Widget CreateNativeView() => throw new NotImplementedException();

		public static void MapDrawable(IViewHandler handler, IGraphicsView graphicsView) { }
	}
}