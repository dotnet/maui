
#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class WindowsPage : WindowsBasePage
	{
		protected override Platform CreatePlatform()
		{
			return new WindowsPlatform(this);
		}
	}
}