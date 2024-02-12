namespace MauiApp1;

public class MyShell : Shell
{
	public MyShell()
	{
		this.Items.Add(new FlyoutItem
		{
			Title = "Dashboard",
			Items =
			{
				new ShellContent
				{
					ContentTemplate = new DataTemplate(typeof(DashboardPage)),
				}
			}
		});

		this.Items.Add(new FlyoutItem
		{
			Title = "Add Vocab",
			Items =
			{
				new ShellContent
				{
					ContentTemplate = new DataTemplate(typeof(AddVocabularyPage)),
				}
			}
		});
	}
}