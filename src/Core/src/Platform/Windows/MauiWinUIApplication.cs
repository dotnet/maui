using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using global::Windows.ApplicationModel.Activation;

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
			var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent()?.GetActivatedEventArgs();
			if (activatedEventArgs?.Kind == Windows.AppLifecycle.ExtendedActivationKind.Protocol)
			{
				IProtocolActivatedEventArgs? protocolArgs = activatedEventArgs?.Data as IProtocolActivatedEventArgs;
				if (protocolArgs is not null && Security.Authentication.OAuth.OAuth2Manager.CompleteAuthRequest(protocolArgs.Uri))
				{
					System.Diagnostics.Process.GetCurrentProcess().Kill();
					return; // We relaunched the application after compliting a OAuth sign-in, so close this instance
				}
			}

			// Windows running on a different thread will "launch" the app again
			if (_application != null && _services != null)
			{
				_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
				_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
				return;
			}

			IPlatformApplication.Current = this;
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			var applicationContext = rootContext.MakeApplicationScope(this);

			_services = applicationContext.Services;

			_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			_application = _services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(_application, applicationContext);

			this.CreatePlatformWindow(_application, args);

			_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		IServiceProvider? _services;

		IApplication? _application;

		// TODO: we should investigate throwing an exception or changing the public API
		IServiceProvider IPlatformApplication.Services => _services!;

		IApplication IPlatformApplication.Application => _application!;

		// TODO NET9 MARK THESE AS OBSOLETE. We didn't mark them obsolete in NET8 because that
		// was causing warnings to generate for our WinUI projects, so we need to workaround that
		// before we mark this as obsolete.
		/// <summary>
		/// Use the IPlatformApplication.Current.Services instead.
		/// </summary>
		public IServiceProvider Services
		{
			get => _services!;
			protected set => _services = value;
		}

		// TODO NET9 MARK THESE AS OBSOLETE. We didn't mark them obsolete in NET8 because that
		// was causing warnings to generate for our WinUI projects, so we need to workaround that
		// before we mark this as obsolete.
		/// <summary>
		/// Use the IPlatformApplication.Current.Application instead.
		/// </summary>
		public IApplication Application
		{
			get => _application!;
			protected set => _application = value;
		}
	}
}
