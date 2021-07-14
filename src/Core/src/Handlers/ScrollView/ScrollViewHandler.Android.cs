using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Core.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, NestedScrollView>
	{
		protected override NestedScrollView CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView) { }
		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView) { }
		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView) { }
		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView) { }
		public static void MapContentSize(ScrollViewHandler handler, IScrollView scrollView) { }
	}
}
