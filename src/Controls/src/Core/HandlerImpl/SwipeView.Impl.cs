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

#if IOS
		SwipeTransitionMode ISwipeView.SwipeTransitionMode =>
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SwipeView.GetSwipeTransitionMode(this);
#elif ANDROID
		SwipeTransitionMode ISwipeView.SwipeTransitionMode =>
			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.SwipeView.GetSwipeTransitionMode(this);
#else
		SwipeTransitionMode ISwipeView.SwipeTransitionMode => SwipeTransitionMode.Reveal;
#endif


		void ISwipeView.SwipeStarted(SwipeViewSwipeStarted swipeStarted)
		{
			var swipeStartedEventArgs = new SwipeStartedEventArgs(swipeStarted.SwipeDirection);
			((ISwipeViewController)this).SendSwipeStarted(swipeStartedEventArgs);
		}

		void ISwipeView.SwipeChanging(SwipeViewSwipeChanging swipeChanging)
		{
			var swipeChangingEventArgs = new SwipeChangingEventArgs(swipeChanging.SwipeDirection, swipeChanging.Offset);
			((ISwipeViewController)this).SendSwipeChanging(swipeChangingEventArgs);

		}

		void ISwipeView.SwipeEnded(SwipeViewSwipeEnded swipeEnded)
		{
			var swipeEndedEventArgs = new SwipeEndedEventArgs(swipeEnded.SwipeDirection, swipeEnded.IsOpen);
			((ISwipeViewController)this).SendSwipeEnded(swipeEndedEventArgs);
		}

		void ISwipeView.RequestOpen(SwipeViewOpenRequest swipeOpenRequest)
		{
			Handler?.Invoke(nameof(ISwipeView.RequestOpen), swipeOpenRequest);
		}

		void ISwipeView.RequestClose(SwipeViewCloseRequest swipeCloseRequest)
		{
			Handler?.Invoke(nameof(ISwipeView.RequestClose), swipeCloseRequest);
		}

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
