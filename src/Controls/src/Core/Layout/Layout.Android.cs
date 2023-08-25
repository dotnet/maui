#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }
	}
}
