using Windows.UI.Core;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal class WindowsPlatformServices : WindowsBasePlatformServices
	{
		public WindowsPlatformServices(CoreDispatcher dispatcher) : base(dispatcher)
		{
		}
	}
}