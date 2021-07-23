using Microsoft.Maui;

namespace Samples.UWP
{
	public class MiddleApp : MauiWinUIApplication<Startup>
	{
	}

	public partial class App : MiddleApp
	{
		public App()
		{
			InitializeComponent();
		}
	}
}