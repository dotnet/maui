namespace Xamarin.Forms
{
	public interface INavigationMenuController : IViewController
	{
		void SendTargetSelected(Page target);
	}
}