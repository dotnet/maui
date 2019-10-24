
#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal sealed class WindowsPlatform : Platform
	{
		public WindowsPlatform(Windows.UI.Xaml.Controls.Page page) : base(page)
		{
		}
	}
}