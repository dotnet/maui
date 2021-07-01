namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	sealed partial class App : MiddleApp
	{
		public static bool RunningAsUITests { get; set; }

		public App()
		{
			InitializeComponent();
		}
	}

	public class MiddleApp : MauiWinUIApplication<WinUIStartup>
	{
	}
}