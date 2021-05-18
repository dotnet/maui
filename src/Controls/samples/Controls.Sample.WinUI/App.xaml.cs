using Microsoft.Maui;

namespace Maui.Controls.Sample.WinUI
{
	// TODO: this is not nice.
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