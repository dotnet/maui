namespace MauiApp1;

public class NewAppshell1 : Shell
{
	public NewAppshell1()
	{
		shell = new Shell()
		{
			Children =
			{
				new FlyoutItem{ Title = "Cats" }
				new FlyoutItem{ Title = "Dogs" }
			}
		}
	}
}