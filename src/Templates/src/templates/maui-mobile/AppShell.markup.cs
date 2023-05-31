namespace MauiApp._1;

public class AppShell : Shell
{
	public AppShell()
	{
		ShellContent mainPage = new()
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(MainPageMarkup)),
			Route = "MainPage",
		};

		Items.Add(mainPage);
	}
}
