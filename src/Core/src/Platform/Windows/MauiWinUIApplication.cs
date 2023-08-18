using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	/// <summary>
	/// Defines the core behavior of a .NET MAUI application running on Windows.
	/// </summary>
	public abstract class MauiWinUIApplication : UI.Xaml.Application, IPlatformApplication
	{
		/// <summary>
		/// When overridden in a derived class, creates the <see cref="MauiApp"/> to be used in this application.
		/// Typically a <see cref="MauiApp"/> is created by calling <see cref="MauiApp.CreateBuilder(bool)"/>, configuring
		/// the returned <see cref="MauiAppBuilder"/>, and returning the built app by calling <see cref="MauiAppBuilder.Build"/>.
		/// </summary>
		/// <returns>The built <see cref="MauiApp"/>.</returns>
		protected abstract MauiApp CreateMauiApp();

		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			// Windows running on a different thread will "launch" the app again
			if (Application != null)
			{
				Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
				Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
				return;
			}

			IPlatformApplication.Current = this;
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			var applicationContext = rootContext.MakeApplicationScope(this);

			Services = applicationContext.Services;

			DeploymentManagerAutoInitializer.LogIfFailed(Services);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, applicationContext);

			this.CreatePlatformWindow(Application, args);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}