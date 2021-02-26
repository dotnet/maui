using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : MauiApp
	{
		public override IAppHostBuilder CreateBuilder() => 
			base.CreateBuilder().ConfigureServices((ctx, services) =>
				{
					services.AddTransient<MainPage>();
					services.AddTransient<IWindow, MainWindow>();
				});

		public override IWindow GetWindowFor(IActivationState state) =>
			Services.GetService<IWindow>();
	}
}