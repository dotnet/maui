using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Gtk;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui
{
	public abstract class MauiGtkApplication : IPlatformApplication
	{
		protected abstract MauiApp CreateMauiApp();

		// https://docs.gtk.org/gio/type_func.Application.id_is_valid.html
		// TODO: find a better algo for id
		public virtual string ApplicationId => $"{typeof(MauiGtkApplication).Namespace}.{nameof(MauiGtkApplication)}.{Name}".PadRight(255, ' ').Substring(0, 255).Trim();

		string? _name;

		// https://docs.gtk.org/gio/type_func.Application.id_is_valid.html
		public string? Name
		{
			get => _name ??= $"A{Guid.NewGuid()}";
			set { _name = value; }
		}

		// https://docs.gtk.org/gtk3/class.Application.html
		public static Gtk.Application CurrentGtkApplication { get; internal set; } = null!;

		public static MauiGtkApplication Current { get; internal set; } = null!;

		public MauiGtkMainWindow MainWindow { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;

		public void Run(params string[] args)
		{
			Launch(EventArgs.Empty);
		}

		protected void RegisterLifecycleEvents(Gtk.Application app)
		{
			app.Startup += OnStartup!;
			app.Shutdown += OnShutdown!;
			app.Opened += OnOpened;
			app.WindowAdded += OnWindowAdded;
			app.Activated += OnActivated!;
			app.WindowRemoved += OnWindowRemoved;
			app.CommandLine += OnCommandLine;
		}

		protected void OnStartup(object sender, EventArgs args)
		{
			Services.InvokeLifecycleEvents<GtkLifecycle.OnStartup>(del => del(CurrentGtkApplication, args));
		}

		protected void OnOpened(object o, GLib.OpenedArgs args)
		{
			Services?.InvokeLifecycleEvents<GtkLifecycle.OnOpened>(del => del(CurrentGtkApplication, args));
		}

		protected void OnActivated(object sender, EventArgs args)
		{
			StartupLauch(sender, args);

			Services?.InvokeLifecycleEvents<GtkLifecycle.OnApplicationActivated>(del => del(CurrentGtkApplication, args));
		}

		protected void OnShutdown(object sender, EventArgs args)
		{
			Services?.InvokeLifecycleEvents<GtkLifecycle.OnShutdown>(del => del(CurrentGtkApplication, args));

			Dispatcher.DispatchPendingEvents();
		}

		protected void OnCommandLine(object o, GLib.CommandLineArgs args)
		{
			// future use: to resolove command line arguments at cross platform Application level
		}

		protected void OnWindowRemoved(object o, WindowRemovedArgs args)
		{
			// future use: to have notifications at cross platform Window level
		}

		protected void OnWindowAdded(object o, WindowAddedArgs args)
		{
			// future use: to have notifications at cross platform Window level
		}

		protected void StartupLauch(object sender, EventArgs args)
		{
			IPlatformApplication.Current = this;

			var mauiApp = CreateMauiApp();

			Services = mauiApp.Services;
			Services.InvokeLifecycleEvents<GtkLifecycle.OnLaunching>(del => del(this, args));

			var rootContext = new MauiContext(Services);
			var applicationContext = rootContext.MakeApplicationScope(CurrentGtkApplication);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnMauiContextCreated>(del => del(applicationContext));

			Application = Services.GetRequiredService<IApplication>();

			CurrentGtkApplication.SetApplicationHandler(Application, applicationContext);

			CurrentGtkApplication.CreatePlatformWindow(Application, new PersistedState());

			Services?.InvokeLifecycleEvents<GtkLifecycle.OnLaunched>(del => del(CurrentGtkApplication, args));
		}

		protected void Launch(EventArgs args)
		{
			Gtk.Application.Init();
			var app = new Gtk.Application(ApplicationId, GLib.ApplicationFlags.None);

			RegisterLifecycleEvents(app);

			CurrentGtkApplication = app;

			Current = this;

			((GLib.Application)app).Run();
		}
	}
}