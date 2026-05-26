using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App() => InitializeComponent();

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
