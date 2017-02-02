namespace Xamarin.Forms
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
	}
}