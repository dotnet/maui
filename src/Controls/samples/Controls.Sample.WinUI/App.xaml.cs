using Microsoft.Maui;

namespace Maui.Controls.Sample.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App()
		{
			InitializeComponent();
		}

		protected override IStartup OnCreateStartup() => new Startup();
	}
}