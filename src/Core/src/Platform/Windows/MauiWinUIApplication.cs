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
			var current = Microsoft.UI.Xaml.Application.Current;

			if (!current.Resources.ContainsKey("RootContainerStyle"))
			{
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(new UI.Xaml.ResourceDictionary
				{
					Source = new Uri("ms-appx:///Microsoft.Maui.Controls.Compatibility/Windows/Resources.xbf")
				});
			}

			if (!current.Resources.ContainsKey("ShellNavigationView"))
			{
				var myResourceDictionary = new Microsoft.UI.Xaml.ResourceDictionary();
				myResourceDictionary.Source = new Uri("ms-appx:///Microsoft.Maui.Controls.Compatibility/Windows/Shell/ShellStyles.xbf");
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
			}

			return new Canvas
			{
				Style = (Microsoft.UI.Xaml.Style)current.Resources["RootContainerStyle"]
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