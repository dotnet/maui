namespace MauiApp._1;

public class AppShell : Shell
{
	public AppShell()
	{
		Title = "Home";

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = nameof(MainPage),
			ContentTemplate = new DataTemplate(typeof(MainPage))
		});
	}
}
