namespace Microsoft.Maui.ManualTests;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

#pragma warning disable CS0618 // Type or member is obsolete
		MainPage = new AppShell();
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
