using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using GtkApplication = Gtk.Application;
using GtkWindow = Gtk.Window;

namespace Microsoft.Maui
{
	/// <summary>
	/// The entry point of a MAUI application on GTK 4. Derive from it, return the app from <see cref="CreateMauiApp"/> and
	/// call <see cref="Run"/> from <c>Main</c>.
	/// </summary>
	/// <remarks>
	/// <para>The class owns a <c>Gtk.Application</c>: <c>startup</c> builds the <see cref="MauiApp"/> and the application-scope
	/// <see cref="IMauiContext"/>; <c>activate</c> creates the <see cref="IApplication"/> handler and the first window
	/// (further activations present the existing window); <c>shutdown</c> raises <see cref="GtkLifecycle.OnShutdown"/>.</para>
	/// <example>
	/// <code>
	/// class LinuxApp : MauiGtkApplication
	/// {
	///     protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	/// }
	/// [STAThread] // GDK on Windows initialises COM (OleInitialize)
	/// public static int Main(string[] args) =&gt; new LinuxApp().Run(args);
	/// </code>
	/// </example>
	/// </remarks>
	public abstract class MauiGtkApplication : IPlatformApplication
	{
		readonly string? _applicationId;
		readonly List<GtkWindow> _windows = new();
		GtkApplication? _application;
		MauiApp? _mauiApp;
		IMauiContext? _applicationContext;
		IApplication? _mauiApplication;

		/// <summary>Creates the application host.</summary>
		/// <param name="applicationId">The D-Bus style application id. Defaults to the <c>ApplicationId</c> assembly metadata that the SDK build emits from the project's <c>ApplicationId</c> (when it is a valid id; it names the generated <c>.desktop</c> file), else to one derived from the entry assembly name.</param>
		protected MauiGtkApplication(string? applicationId = null)
		{
			_applicationId = applicationId;
		}

		/// <summary>The running host, or <see langword="null"/>.</summary>
		public static MauiGtkApplication? Current { get; private set; }

		/// <summary>The underlying <c>Gtk.Application</c> (available from <see cref="Run"/> on).</summary>
		public GtkApplication GtkApplication => _application ?? throw new InvalidOperationException("The application has not been started.");

		/// <inheritdoc/>
		public IServiceProvider Services => _applicationContext?.Services ?? throw new InvalidOperationException("The application has not been started.");

		/// <inheritdoc/>
		public IApplication Application => _mauiApplication ?? throw new InvalidOperationException("The application has not been activated.");

		/// <summary>Builds the application.</summary>
		protected abstract MauiApp CreateMauiApp();

		/// <summary>Runs the GTK main loop until the last window closes.</summary>
		/// <param name="args">The command line arguments handed to <c>GApplication</c>; remove the own options of the app first, GApplication rejects unknown ones.</param>
		/// <returns>The process exit code.</returns>
		/// <exception cref="InvalidOperationException">On Windows, when the calling thread is not a single-threaded apartment (mark <c>Main</c> with <c>[STAThread]</c>).</exception>
		public int Run(string[] args)
		{
			EnsureApartment(OperatingSystem.IsWindows(), System.Threading.Thread.CurrentThread.GetApartmentState());
			GtkRuntime.EnsureInitialized();

			Current = this;
			IPlatformApplication.Current = this;

			_application = new GtkApplication(_applicationId ?? GtkApplicationIdentity.ResolveApplicationId(System.Reflection.Assembly.GetEntryAssembly()), GLib.ApplicationFlags.None);
			_application.Startup += OnStartup;
			_application.Activated += OnActivated;
			_application.Shutdown += OnShutdown;
			_application.WindowRemoved += (_, args) => _windows.Remove(args.Window);

			// The arguments go to GApplication (--help, --gapplication-service...): strip your own options before calling Run.
			var programName = Environment.GetCommandLineArgs() is { Length: > 0 } commandLine ? commandLine[0] : "maui";
			return ((GLib.Application)_application).Run(programName, args ?? Array.Empty<string>());
		}

		// GDK initialises COM with OleInitialize on Windows, which aborts the process (Gdk-ERROR "OleInitialize failed") on an MTA thread:
		// fail early with an actionable message instead.
		internal static void EnsureApartment(bool isWindows, System.Threading.ApartmentState state)
		{
			if (isWindows && state != System.Threading.ApartmentState.STA)
				throw new InvalidOperationException("GTK on Windows needs a single-threaded apartment: mark Main with [STAThread] (GDK initialises COM with OleInitialize).");
		}

		void OnStartup(object? sender, EventArgs e)
		{
			InstallApplicationIcon();

			_mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(_mauiApp.Services);
			_applicationContext = rootContext.MakeApplicationScope(this);
			((MauiContext)_applicationContext).AddSpecific(GtkApplication);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnStartup>(del => del(GtkApplication));
		}

		// The icon is cosmetic: a missing icon theme or display must never stop the app from starting.
		static void InstallApplicationIcon()
		{
			try
			{
				GtkApplicationIdentity.InstallIcon(AppContext.BaseDirectory);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		void OnActivated(object? sender, EventArgs e)
		{
			if (_applicationContext is null)
				return;

			if (_mauiApplication is not null)
			{
				// A second activation (for example the user launched the app again): bring the window forward.
				if (_windows.Count > 0)
					_windows[0].Present();
				return;
			}

			_mauiApplication = Services.GetRequiredService<IApplication>();
			ElementExtensions.SetApplicationHandler(this, _mauiApplication, _applicationContext);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnActivate>(del => del(GtkApplication));

			CreatePlatformWindow(_mauiApplication, null);
		}

		void OnShutdown(object? sender, EventArgs e)
		{
			Services.InvokeLifecycleEvents<GtkLifecycle.OnShutdown>(del => del(GtkApplication));
		}

		/// <summary>Creates a platform window for a new <see cref="IWindow"/> of <paramref name="application"/> and presents it.</summary>
		/// <param name="application">The MAUI application.</param>
		/// <param name="request">The open-window request, if any.</param>
		internal void CreatePlatformWindow(IApplication application, OpenWindowRequest? request)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var platformWindow = new global::Gtk.ApplicationWindow(GtkApplication);

			var windowContext = applicationContext.MakeWindowScope(platformWindow, out _);
			((MauiContext)windowContext).AddWeakSpecific<GtkWindow>(platformWindow);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnMauiContextCreated>(del => del(windowContext));

			var activationState = request?.State is { } state ? new ActivationState(windowContext, state) : new ActivationState(windowContext);
			var window = application.CreateWindow(activationState);

			_windows.Add(platformWindow);

			ElementExtensions.SetWindowHandler(platformWindow, window, windowContext);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnWindowCreated>(del => del(platformWindow));

			platformWindow.Present();
		}
	}
}
