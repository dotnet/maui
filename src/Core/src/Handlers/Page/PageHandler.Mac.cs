using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<ILayout, NSView>
	{
		protected override NSView CreateView()
		{
			return new NSView();
		}

		public static void MapTitle(PageHandler handler, IPage page)
		{
		}
	}
}
