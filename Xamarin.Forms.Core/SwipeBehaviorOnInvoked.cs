namespace Xamarin.Forms
{
	public enum SwipeBehaviorOnInvoked
	{
		Auto,       // In Reveal mode, the SwipeView closes after an item is invoked. In Execute mode, the SwipeView remains open.
		Close,      // The SwipeView closes after an item is invoked.
		RemainOpen  // The SwipeView remains open after an item is invoked.
	}
}