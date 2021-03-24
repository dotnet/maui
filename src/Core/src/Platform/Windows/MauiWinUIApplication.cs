using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiWinUIApplication<TStartup> : MauiWinUIApplication
		where TStartup : IStartup, new()
	{
		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			LaunchActivatedEventArgs = args;

			// TODO: This should not be here. CreateWindow should do it.
			MainWindow = new MauiWinUIWindow();

			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;
			Application = Services.GetRequiredService<IApplication>();

			var mauiContext = new MauiContext(Services);

			var activationState = new ActivationState(mauiContext, args);
			var window = Application.CreateWindow(activationState);
			window.MauiContext = mauiContext;

			var content = (window.Page as IView) ?? window.Page.View;

			var canvas = CreateRootContainer();

			canvas.Children.Add(content.ToNative(window.MauiContext));

			MainWindow.Content = canvas;

			MainWindow.Activate();
		}

		Canvas CreateRootContainer()
		{
			// TODO WINUI should this be some other known constant or via some mechanism? Or done differently?
			Resources.TryGetValue("RootContainerStyle", out object style);

			return new Canvas
			{
				Style = style as UI.Xaml.Style
			};
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
		}
	}

	public abstract class MauiWinUIApplication : UI.Xaml.Application
	{
		protected MauiWinUIApplication()
		{
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		public MauiWinUIWindow MainWindow { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}