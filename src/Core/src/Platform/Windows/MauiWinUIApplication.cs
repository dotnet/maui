using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Windows.AppLifecycle;

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
			LaunchActivatedEventArgs = args;

			var launchActivation = AppInstance.GetCurrent().GetActivatedEventArgs();

			// A running WinUI app can be activated again without rebuilding the MAUI application.
			// Reuse the existing services and let activation handlers short-circuit the relaunch path.
			if (_application != null && _services != null)
			{
				if (launchActivation is AppActivationArguments activatedEventArgs && OnAppInstanceActivated(activatedEventArgs))
					return;

				_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
				_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
				return;
			}

			IPlatformApplication.Current = this;
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			var applicationContext = rootContext.MakeApplicationScope(this);

			_services = applicationContext.Services;

			// Future AppInstance activation callbacks need the app-level services to exist first.
			RegisterForAppInstanceActivated();

			// Run the initial activation after services are available, but before OnLaunching/window creation,
			// so handlers can redirect or suppress the default startup flow.
			if (launchActivation is AppActivationArguments initialActivation && OnAppInstanceActivated(initialActivation))
				return;

			_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			_application = _services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(_application, applicationContext);

			this.CreatePlatformWindow(_application, args);

			_services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		protected virtual bool OnAppInstanceActivated(AppActivationArguments args)
		{
			var wasHandled = false;

			_services?.InvokeLifecycleEvents<WindowsLifecycle.OnAppInstanceActivated>(del =>
			{
				// Preserve any earlier "handled" result so multiple listeners can participate safely.
				wasHandled = del(this, args) || wasHandled;
			});

			return wasHandled;
		}

		void RegisterForAppInstanceActivated()
		{
			if (_isRegisteredForAppInstanceActivated)
				return;

			_isRegisteredForAppInstanceActivated = true;

			// After startup, later file/protocol/redirected activations are delivered through AppInstance.
			AppInstance.GetCurrent().Activated += HandleAppInstanceActivated;

			void HandleAppInstanceActivated(object? sender, AppActivationArguments args)
			{
				OnAppInstanceActivated(args);
			}
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		bool _isRegisteredForAppInstanceActivated;

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
