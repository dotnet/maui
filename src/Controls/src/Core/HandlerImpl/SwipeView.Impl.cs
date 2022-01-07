#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeView : ISwipeView
	{
		ISwipeItems ISwipeView.LeftItems => new HandlerSwipeItems(LeftItems);

		ISwipeItems ISwipeView.RightItems => new HandlerSwipeItems(RightItems);

		ISwipeItems ISwipeView.TopItems => new HandlerSwipeItems(TopItems);

		ISwipeItems ISwipeView.BottomItems => new HandlerSwipeItems(BottomItems);

		bool ISwipeView.IsOpen { get; set; }


		class HandlerSwipeItems : List<Maui.ISwipeItem>, ISwipeItems
		{
			readonly SwipeItems _swipeItems;

			public HandlerSwipeItems(SwipeItems swipeItems) : base(swipeItems)
			{
				_swipeItems = swipeItems;
			}

			public SwipeMode Mode => _swipeItems.Mode;

			public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked => _swipeItems.SwipeBehaviorOnInvoked;
		}
	}
}
