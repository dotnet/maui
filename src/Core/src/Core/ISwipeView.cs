using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface ISwipeView : IContentView
	{
		public double Threshold { get; }

		public ISwipeItems LeftItems { get; }

		public ISwipeItems RightItems { get; }

		public ISwipeItems TopItems { get; }

		public ISwipeItems BottomItems { get; }

		public bool IsOpen { get; set; }

		public SwipeTransitionMode SwipeTransitionMode { get; }

		public void SwipeStarted(SwipeViewSwipeStarted swipeStarted);

		public void SwipeChanging(SwipeViewSwipeChanging swipeChanging);

		public void SwipeEnded(SwipeViewSwipeEnded swipeEnded);

		public void RequestOpen(SwipeViewOpenRequest swipeOpenRequest);

		public void RequestClose(SwipeViewCloseRequest swipeCloseRequest);
	}
}
