using Maui.Controls.Sample.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

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
