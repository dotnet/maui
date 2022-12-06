using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) => MapInputTransparent((ILayoutHandler)handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout)
		{
			if (handler.PlatformView is LayoutViewGroup layoutViewGroup)
			{
				// Handle input transparent for this view
				layoutViewGroup.InputTransparent = layout.InputTransparent;
			}

			layout.UpdateDescendantInputTransparent();
		}

		static void MapInputTransparent(IViewHandler handler, IView layout)
		{
			if (handler is ILayoutHandler lh && layout is Layout l)
				MapInputTransparent(lh, l);
		}
	}
}
