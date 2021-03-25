using Microsoft.Maui;

namespace MauiApp1
{
	public class MainWindow : IWindow
	{
		public MainWindow()
		{
			Page = new MainPage();
		}

		public IPage Page { get; set; }

		public IMauiContext MauiContext { get; set; }
	}
}