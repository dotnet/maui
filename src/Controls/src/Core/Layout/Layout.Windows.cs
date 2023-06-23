#nullable disable
using Microsoft.UI.Xaml;

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

		static void UpdateInputTransparent(IViewHandler handler, IView layout)
		{
			if (handler is ILayoutHandler layoutHandler && layout is Layout controlsLayout)
			{
				layoutHandler.PlatformView?.UpdateInputTransparent(layoutHandler, controlsLayout);
				controlsLayout.UpdateDescendantInputTransparent();
			}
			else
			{
				ControlsVisualElementMapper.UpdateProperty(handler, layout, nameof(IView.InputTransparent));
			}
		}
	}
}