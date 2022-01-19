using System;
using System.Collections.Generic;
using System.Text;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	//TODO : Need to implement
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, EvasObject>
	{
		protected override EvasObject CreateNativeElement()
		{
			throw new NotImplementedException();
		}

		public static void MapTextColor(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapCharacterSpacing(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapVisibility(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		void OnSetImageSource(EvasObject? obj)
		{
			throw new NotImplementedException();
		}
	}
}
