using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MauiApp1
{
	public class Application : MauiApp
	{
		public override IAppHostBuilder CreateBuilder() =>
			base.CreateBuilder().ConfigureServices((ctx, services) =>
				{
					services.AddTransient<MainPage>();
					services.AddTransient<IWindow, MainWindow>();
				});

		public override IWindow CreateWindow(IActivationState state)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(state);
			return Services.GetService<IWindow>();
		}
	}
}