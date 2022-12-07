namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) =>
			UpdateInputTransparent(handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) =>
			UpdateInputTransparent(handler, layout);

		static void MapInputTransparent(IViewHandler handler, IView layout) =>
			UpdateInputTransparent(handler, layout);

		static void UpdateInputTransparent(IViewHandler viewHandler, IView view)
		{
			if (viewHandler is not IPlatformViewHandler handler ||
				handler.PlatformView == null || 
				view is not Layout layout)
			{
				return;
			}

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
