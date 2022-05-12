using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Platform
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			InitializeComponent();
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}