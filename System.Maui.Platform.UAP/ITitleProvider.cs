using global::Windows.UI.Xaml.Media;

namespace System.Maui.Platform.UWP
{
	internal interface ITitleProvider
	{
		Brush BarBackgroundBrush { set; }

		Brush BarForegroundBrush { set; }

		bool ShowTitle { get; set; }

		string Title { get; set; }
	}
}