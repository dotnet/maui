using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContent(ISwipeViewHandler handler, ISwipeView view)
		{
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
		}
	}
}