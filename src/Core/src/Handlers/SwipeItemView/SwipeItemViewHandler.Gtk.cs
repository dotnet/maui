using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemViewHandler : ViewHandler<ISwipeItemView, Gtk.Widget>, ISwipeItemViewHandler
	{
		protected override Gtk.Widget CreatePlatformView() => new NotImplementedView(nameof(ISwipeItemView));

		public static void MapContent(ISwipeItemViewHandler handler, ISwipeItemView page)
		{
		}

		public static void MapVisibility(ISwipeItemViewHandler handler, ISwipeItemView view)
		{
		}
	}
}
