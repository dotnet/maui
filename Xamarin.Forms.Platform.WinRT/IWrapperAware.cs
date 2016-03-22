
#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal interface IWrapperAware
	{
		void NotifyWrapped();
	}
}