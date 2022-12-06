namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) => MapInputTransparent((ILayoutHandler)handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout)
		{
			if (handler is LayoutHandler h)
			{
				MapInputTransparent(h, layout);
			}
		}

		static void MapInputTransparent(IViewHandler handler, IView layout)
		{
			if (handler is ILayoutHandler lh && layout is Layout l)
				MapInputTransparent(lh, l);
		}
	}
}
