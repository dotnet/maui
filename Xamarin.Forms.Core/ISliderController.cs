using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISliderController
	{
		void SendDragStarted();
		void SendDragCompleted();
	}
}