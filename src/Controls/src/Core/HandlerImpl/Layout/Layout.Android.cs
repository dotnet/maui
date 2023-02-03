#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

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
			if (handler.PlatformView is LayoutViewGroup layoutViewGroup)
			{
				// Handle input transparent for this view
				layoutViewGroup.InputTransparent = layout.InputTransparent;
			}

			if (layout is Layout l)
			{
				l.UpdateDescendantInputTransparent();
			}
		}
	}
}
