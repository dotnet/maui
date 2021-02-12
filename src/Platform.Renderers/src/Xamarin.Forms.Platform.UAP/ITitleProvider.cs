using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface ITitleProvider
	{
		WBrush BarBackgroundBrush { set; }

		WBrush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}