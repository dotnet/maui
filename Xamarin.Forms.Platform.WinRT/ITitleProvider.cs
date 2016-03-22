using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal interface ITitleProvider
	{
		Brush BarBackgroundBrush { set; }

		Brush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}