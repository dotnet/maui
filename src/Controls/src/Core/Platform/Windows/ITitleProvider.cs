using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Platform
{
	internal interface ITitleProvider
	{
		WBrush BarBackgroundBrush { set; }

		WBrush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}