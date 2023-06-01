namespace MauiApp._1;

public class AppShell : Shell
{
	public AppShell()
	{
		ShellContent mainPage = new()
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(MainPage)),
			Route = "MainPage",
		};

		Items.Add(mainPage);
	}
}
