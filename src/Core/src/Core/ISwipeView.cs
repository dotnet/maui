namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a container that provides access to contextual commands through touch interactions.
	/// </summary>
	public interface ISwipeView : IContentView
	{
		/// <summary>
		/// Gets a value that represents the minimum swipe distance that must be achieved for a swipe to be recognized.
		/// </summary>
		public double Threshold { get; }

		/// <summary>
		/// Gets the items that can be invoked when the control is swiped from the left side.
		/// </summary>
		public ISwipeItems LeftItems { get; }

		/// <summary>
		/// Gets the items that can be invoked when the control is swiped from the right side.
		/// </summary>
		public ISwipeItems RightItems { get; }

		/// <summary>
		/// Gets the items that can be invoked when the control is swiped from the top down.
		/// </summary>
		public ISwipeItems TopItems { get; }

		/// <summary>
		/// Gets the items that can be invoked when the control is swiped from the bottom up.
		/// </summary>
		public ISwipeItems BottomItems { get; }

		/// <summary>
		/// Gets a value indicating whether the swipe view is open or not.
		/// </summary>
		public bool IsOpen { get; set; }

		/// <summary>
		/// Define the swipe transition in the control.
		/// </summary>
		public SwipeTransitionMode SwipeTransitionMode { get; }

		/// <summary>
		/// Event that is fired when the swipe starts.
		/// </summary>
		/// <param name="swipeStarted">Provides information related to the swipe gesture with data such as the swipe direction.</param>
		public void SwipeStarted(SwipeViewSwipeStarted swipeStarted);

		/// <summary>
		/// Event that is fired when the swipe starts.
		/// </summary>
		/// <param name="swipeChanging">Provides information related to the swipe gesture with data such as the swipe direction.</param>
		public void SwipeChanging(SwipeViewSwipeChanging swipeChanging);

		/// <summary>
		/// Event that is fired when the swipe is finished.
		/// </summary>
		/// <param name="swipeEnded">Provides information related to the swipe gesture with data such as the swipe direction.</param>
		public void SwipeEnded(SwipeViewSwipeEnded swipeEnded);

		/// <summary>
		/// Open the swipe view.
		/// </summary>
		public void RequestOpen(SwipeViewOpenRequest swipeOpenRequest);

		/// <summary>
		/// Closes the swipe view.
		/// </summary>
		public void RequestClose(SwipeViewCloseRequest swipeCloseRequest);
	}
}
