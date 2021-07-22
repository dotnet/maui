using Microsoft.Maui;

namespace Samples.UWP
{
	public class MiddleApp : MauiWinUIApplication
	{
		protected override MauiAppBuilder CreateAppBuilder() => MauiProgram.CreateAppBuilder();
	}

	public partial class App : MiddleApp
	{
		public App()
		{
			InitializeComponent();
		}
	}
}