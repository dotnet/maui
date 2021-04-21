using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class MainWindow : Window
	{
		public MainWindow(IPage page)
		{
			Title = nameof(MainWindow);
			Page = (ContentPage)page;
		}
	}
}
