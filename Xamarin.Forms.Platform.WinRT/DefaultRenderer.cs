using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal sealed class DefaultRenderer : ViewRenderer<View, FrameworkElement>
	{
	}
}