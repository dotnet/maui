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

		static void UpdateInputTransparent(IViewHandler handler, IView view)
		{
			if (handler.PlatformView is not Microsoft.Maui.Platform.LayoutViewGroup platformView ||
				view is not Layout layout)
			{
				return;
			}

			if (layout.CascadeInputTransparent)
			{
				// Sensitive property on NUI View was false, disabled all touch event including children
				platformView.Sensitive = !layout.InputTransparent;
				platformView.InputTransparent = false;
			}
			else
			{
				// InputTransparent property on LayoutViewGroup was false,
				// Only LayoutViewGroup event was disabled but children are allowed
				platformView.InputTransparent = layout.InputTransparent;
				platformView.Sensitive = true;
			}
		}
	}
}
