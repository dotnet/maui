using Microsoft.Maui.Controls;

namespace ShapesDemos;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new MainPage());
	}
}
