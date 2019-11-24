namespace Xamarin.Forms
{
	public interface ISwipeViewController
	{
		void SendSwipeStarted(SwipeStartedEventArgs args);
		void SendSwipeChanging(SwipeChangingEventArgs args);
		void SendSwipeEnded(SwipeEndedEventArgs args);
	}
}