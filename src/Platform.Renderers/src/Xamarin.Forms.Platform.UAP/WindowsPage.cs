namespace Xamarin.Forms.Platform.UWP
{
	public abstract class WindowsPage : WindowsBasePage
	{
		protected override Platform CreatePlatform()
		{
			return new WindowsPlatform(this);
		}
	}
}