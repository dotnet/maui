using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MauiControlsSample.SingleProject
{
	public class MyApp : MauiApp
	{
		public override IAppHostBuilder CreateBuilder() => 
			base.CreateBuilder().ConfigureServices((ctx, services) =>
				{
					services.AddTransient<MainPage>();
					services.AddTransient<IWindow, MainWindow>();
				});

		public override IWindow CreateWindow(IActivationState state)
		{
#if (ANDROID || IOS)

			// This will probably go into a compatibility app or window
			Microsoft.Maui.Controls.Compatibility.Forms.Init(state);
#endif
			return Services.GetService<IWindow>();
		}
	}
}