#nullable disable
using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) =>
			MapInputTransparent((ILayoutHandler)handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) =>
			layout.UpdateDescendantInputTransparent();

		[Obsolete]
		static void MapInputTransparent(IViewHandler handler, IView view)
		{
			if (handler is ILayoutHandler layoutHandler && view is Layout layout)
			{
				layoutHandler.PlatformView?.UpdateInputTransparent(layoutHandler, layout);
				layout.UpdateDescendantInputTransparent();
			}
			else
			{
				ControlsVisualElementMapper.UpdateProperty(handler, view, nameof(IView.InputTransparent));
			}
		}
	}
}