namespace Microsoft.Maui.Controls
{
	public interface IPanGestureController
	{
		void SendPan(Element sender, double totalX, double totalY, int gestureId);

		void SendPanCanceled(Element sender, int gestureId);

		void SendPanCompleted(Element sender, int gestureId);

		void SendPanStarted(Element sender, int gestureId);
	}
}