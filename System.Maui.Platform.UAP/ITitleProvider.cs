using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface ITitleProvider
	{
		Brush BarBackgroundBrush { set; }

		Brush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}