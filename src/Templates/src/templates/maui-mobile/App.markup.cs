using MauiApp._1.Resources.Styles;

namespace MauiApp._1;

public class App : Application
{
	public App()
	{
		Resources = new AppStyles();

		MainPage = new AppShell();
	}
}
