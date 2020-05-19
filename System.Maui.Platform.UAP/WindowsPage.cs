namespace Xamarin.Forms.Platform.UWP
{
	public class WindowsPage : WindowsBasePage
	{
		protected override Platform CreatePlatform()
		{
			return new WindowsPlatform(this);
		}
	}
}