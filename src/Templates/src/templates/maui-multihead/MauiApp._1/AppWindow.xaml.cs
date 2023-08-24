namespace MauiApp._1;

public partial class AppWindow : Window
{
	public AppWindow(AppShell shell)
	{
		InitializeComponent();

		Page = shell;
	}
}
