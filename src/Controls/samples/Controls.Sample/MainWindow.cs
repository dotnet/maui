using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.Pages;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample
{
	public class MainWindow : Window
	{
		public MainWindow() : this(new Page1())
		{
		}

		public MainWindow(MainPage page)
		{
			Page = page;
		}
	}
}
