using MauiApplication = Microsoft.Maui.Controls.Application;

namespace MauiApp._1;

public partial class App : MauiApplication
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
	}
}
