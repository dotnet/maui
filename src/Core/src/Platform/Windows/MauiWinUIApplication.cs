using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiWinUIApplication<TApplication> : Microsoft.UI.Xaml.Application
		 where TApplication : MauiApp
	{
		MauiWinUIWindow? m_window;

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			var host = app.CreateBuilder().ConfigureServices(ConfigureNativeServices).Build(app);

			if (MauiApp.Current == null || MauiApp.Current.Services == null)
				throw new InvalidOperationException("App was not intialized");

			var mauiContext = new MauiContext(MauiApp.Current.Services);
			m_window = new MauiWinUIWindow();
			var window = app.CreateWindow(new ActivationState(args, m_window, mauiContext));

			window.MauiContext = mauiContext;

			//Hack for now we set this on the App Static but this should be on IFrameworkElement
			app.SetHandlerContext(window.MauiContext);

			var content = window.Page.View;//(window.Page as IView) ?? window.Page.View;

			m_window.Content = new ContentControl()
			{
				Content = content.ToNative(window.MauiContext)
			};

			m_window.Activate();

		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}
