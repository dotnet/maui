using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout)
		{
			if (handler.PlatformView is LayoutViewGroup layoutViewGroup)
			{
				// Handle input transparent for this view
				layoutViewGroup.InputTransparent = layout.InputTransparent;
			}

			layout.UpdateDescendantInputTransparent();
		}
	}
}
