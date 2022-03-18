namespace MauiApp._1;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new Shell
		{
			CurrentItem = new MainPage()
		};
	}
}
