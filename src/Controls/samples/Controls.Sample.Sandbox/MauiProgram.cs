using Autofac.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder =
			MauiApp
				.CreateBuilder()
				.UseMauiMaps()
				.UseMauiApp<App>();

			builder.ConfigureContainer(new AutofacServiceProviderFactory(), autofacBuilder =>
				{
					// Registrations
					// Don't call the autofacBuilder.Build() here - it is called behind the scenes
				});

			return builder.Build();
		}
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState? activationState)
		{
			// To test shell scenarios, change this to true
			bool useShell = false;

			if (!useShell)
			{
				return new Window(new NavigationPage(new MainPage()));
			}
			else
			{
				return new Window(new SandboxShell());
			}
		}
	}
}