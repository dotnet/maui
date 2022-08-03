using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemViewHandler : ViewHandler<ISwipeItemView, object>, ISwipeItemViewHandler
	{
		protected override object CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void MapContent(ISwipeItemViewHandler handler, ISwipeItemView page)
		{
		}

		public static void MapVisibility(ISwipeItemViewHandler handler, ISwipeItemView view)
		{
		}
	}
}
