using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace MauiApp1
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>()
				.SetImageDirectory("Assets");

			MainPage = new MainPage();
		}
	}
}
