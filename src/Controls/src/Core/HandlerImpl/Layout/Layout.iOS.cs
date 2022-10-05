namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) =>
			MapInputTransparent((ILayoutHandler)handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout)
		{
			handler.PlatformView?.UpdateInputTransparent(handler, layout);
			layout.UpdateDescendantInputTransparent();
		}
	}
}
