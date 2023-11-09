namespace MauiApp1;

public class NewAppshell1 : Shell
{
	public NewAppshell1()
	{
		var shell = new Shell()
		{
			Children =
			{
				new FlyoutItem{ Title = "Main" }
				new FlyoutItem{ Title = "Other" }
			}
		}
	}
}