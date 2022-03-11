using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal interface ITitleProvider
	{
		WBrush BarBackgroundBrush { set; }

		WBrush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}