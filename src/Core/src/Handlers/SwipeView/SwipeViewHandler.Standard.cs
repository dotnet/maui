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

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest request)
			{
				return;
			}
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}
		}
	}
}