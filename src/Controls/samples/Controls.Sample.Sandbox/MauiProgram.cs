using MauiApp2;
using Microsoft.Extensions.DependencyInjection;
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
			var builder = MauiApp
				.CreateBuilder().UseMauiMaps()
				.UseMauiApp<MauiApp2.App>();

			builder.Services.AddSingleton<MauiApp2.MainPage>();
			builder.Services.AddSingleton<MauiApp2.MainPageModel>();
			builder.Services.AddSingleton<MauiApp2.Child>();
			builder.Services.AddSingleton<MauiApp2.ChildModel>();
			return builder
				
				.Build();
		}
	}

	class App : Application
	{
		protected override Window CreateWindow(IActivationState activationState) =>
			new Window(new NavigationPage(new MainPage()));
	}
}