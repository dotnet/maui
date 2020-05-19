using System.ComponentModel;

namespace System.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISliderController
	{
		void SendDragStarted();
		void SendDragCompleted();
	}
}
