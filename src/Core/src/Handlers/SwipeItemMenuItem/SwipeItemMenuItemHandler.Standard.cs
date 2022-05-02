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

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		void OnSetImageSource(object? obj)
		{
			throw new NotImplementedException();
		}
	}
}
