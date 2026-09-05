namespace MauiApp._1;

public class AppShell : Shell
{
	public AppShell()
	{
		Title = "MauiApp._1";

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = nameof(MainPage),
			ContentTemplate = new DataTemplate(typeof(MainPage))
		});
	}
}
