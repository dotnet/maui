using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.Pages;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class MainWindow : Window
	{
		public MainWindow() : this(App.Current.Services.GetRequiredService<IPage>())
		{
		}

		public MainWindow(IPage page)
		{
			Page = page;
		}
	}
}
