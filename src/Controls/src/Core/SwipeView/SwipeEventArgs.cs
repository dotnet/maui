#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides base event data for swipe events.
	/// </summary>
	public abstract class BaseSwipeEventArgs : EventArgs
	{
		protected BaseSwipeEventArgs(SwipeDirection swipeDirection)
		{
			SwipeDirection = swipeDirection;
		}

		/// <summary>
		/// Gets or sets the direction of the swipe gesture.
		/// </summary>
		public SwipeDirection SwipeDirection { get; set; }
	}

	public class CloseRequestedEventArgs : EventArgs
	{
		public CloseRequestedEventArgs(bool animated)
		{
			Animated = animated;
		}

		public bool Animated { get; set; }
	}

	public class OpenRequestedEventArgs : EventArgs
	{
		public OpenRequestedEventArgs(OpenSwipeItem openSwipeItem, bool animated)
		{
			OpenSwipeItem = openSwipeItem;
			Animated = animated;
		}

		public OpenSwipeItem OpenSwipeItem { get; set; }
		public bool Animated { get; set; }
	}

	/// <summary>
	/// Provides data for the <see cref="SwipeView.SwipeStarted"/> event.
	/// </summary>
	public class SwipeStartedEventArgs : BaseSwipeEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeStartedEventArgs"/> class.
		/// </summary>
		/// <param name="swipeDirection">The direction of the swipe gesture.</param>
		public SwipeStartedEventArgs(SwipeDirection swipeDirection) : base(swipeDirection)
		{

		}
	}

	/// <summary>
	/// Provides data for the <see cref="SwipeView.SwipeChanging"/> event.
	/// </summary>
	public class SwipeChangingEventArgs : BaseSwipeEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeChangingEventArgs"/> class.
		/// </summary>
		/// <param name="swipeDirection">The direction of the swipe gesture.</param>
		/// <param name="offset">The current swipe offset distance.</param>
		public SwipeChangingEventArgs(SwipeDirection swipeDirection, double offset) : base(swipeDirection)
		{
			Offset = offset;
		}

		/// <summary>
		/// Gets or sets the current swipe offset distance.
		/// </summary>
		public double Offset { get; set; }
	}

	/// <summary>
	/// Provides data for the <see cref="SwipeView.SwipeEnded"/> event.
	/// </summary>
	public class SwipeEndedEventArgs : BaseSwipeEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeEndedEventArgs"/> class.
		/// </summary>
		/// <param name="swipeDirection">The direction of the swipe gesture.</param>
		/// <param name="isOpen">Whether the swipe view is open after the gesture.</param>
		public SwipeEndedEventArgs(SwipeDirection swipeDirection, bool isOpen) : base(swipeDirection)
		{
			IsOpen = isOpen;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the swipe view is open.
		/// </summary>
		public bool IsOpen { get; set; }
	}
}