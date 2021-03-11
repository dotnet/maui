using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace MauiApp1
{
	public class MainWindow : IWindow
	{
		public IPage Page { get; set; }
		public IMauiContext MauiContext { get; set; }

		public MainWindow()
		{
			Page = App.Current.Services.GetService<MainPage>();
		}
	}
}
