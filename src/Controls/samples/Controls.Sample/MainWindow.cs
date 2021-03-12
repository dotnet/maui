using Maui.Controls.Sample.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using MauiApplication = Microsoft.Maui.Application;

namespace Maui.Controls.Sample
{
	public class MainWindow : Window
	{
		public MainWindow() : this(MauiApplication.Current.Services.GetRequiredService<IPage>())
		{
		}

		public MainWindow(IPage page)
		{
			Page = page;
		}
	}
}
