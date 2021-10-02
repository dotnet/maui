using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Samples.UWP
{
	public class MiddleApp : MauiWinUIApplication
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}

	public partial class App : MiddleApp
	{
		public App()
		{
			InitializeComponent();
		}
	}
}