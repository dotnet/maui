using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			InitializeComponent();
			this.UnhandledException += App_UnhandledException;
		}

		private void App_UnhandledException(object sender, UI.Xaml.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
