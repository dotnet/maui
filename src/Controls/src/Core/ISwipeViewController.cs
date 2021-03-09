namespace Microsoft.Maui.Controls
{
	public interface ISwipeViewController
	{
		bool IsOpen { get; set; }
		void SendSwipeStarted(SwipeStartedEventArgs args);
		void SendSwipeChanging(SwipeChangingEventArgs args);
		void SendSwipeEnded(SwipeEndedEventArgs args);
	}
}