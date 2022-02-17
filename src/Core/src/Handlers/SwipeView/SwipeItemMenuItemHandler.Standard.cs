using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, object>
	{
		protected override object CreatePlatformElement()
		{
			throw new NotImplementedException();
		}

		public static void MapTextColor(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapCharacterSpacing(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapVisibility(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		void OnSetImageSource(object? obj)
		{
			throw new NotImplementedException();
		}
	}
}
