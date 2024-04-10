using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CollectionViewPerformanceMaui.Services;
using CollectionViewPerformanceMaui.ViewModels;
using CollectionViewPerformanceMaui.Views;
using Microsoft.Extensions.DependencyInjection;

#if ANDROID
using CollectionViewPerformanceMaui.Controls;
using CollectionViewPerformanceMaui.Platforms.Android.Handlers;
#endif

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseMauiMaps()
				.UseMauiApp<CollectionViewPerformanceMaui.App>()
			.ConfigureMauiHandlers(handlers =>
			{
#if ANDROID
					handlers.AddHandler<Card, CardHandler>();

					CardHandler.SetupMapper();
#endif
			})
				.RegisterServices()
				.RegisterViewModels()
				.RegisterViews()
				.Build();


		public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
		{
			mauiAppBuilder.Services.AddSingleton<IDataService, DataService>();

			return mauiAppBuilder;
		}

		public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
		{
			mauiAppBuilder.Services.AddSingleton<DataViewModel>();

			return mauiAppBuilder;
		}

		public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
		{
			mauiAppBuilder.Services.AddSingleton<DataView>();

			return mauiAppBuilder;
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