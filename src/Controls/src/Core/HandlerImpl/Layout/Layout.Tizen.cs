namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) => MapInputTransparent((ILayoutHandler)handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout)
		{
			if (handler.PlatformView == null)
				return;

			if (layout.CascadeInputTransparent)
			{
				// Sensitive property on NUI View was false, disabled all touch event including children
				handler.PlatformView.Sensitive = !layout.InputTransparent;
				handler.PlatformView.InputTransparent = false;
			}
			else
			{
				// InputTransparent property on LayoutViewGroup was false,
				// Only LayoutViewGroup event was disabled but children are allowed
				handler.PlatformView.InputTransparent = layout.InputTransparent;
				handler.PlatformView.Sensitive = true;
			}
		}
	}
}
