using System;
using System.Collections.Generic;
using System.Text;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	//TODO : Need to implement
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, EvasObject>
	{
		protected override EvasObject CreatePlatformElement()
		{
			throw new NotImplementedException();
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		void OnSetImageSource(EvasObject? obj)
		{
			throw new NotImplementedException();
		}
	}
}
