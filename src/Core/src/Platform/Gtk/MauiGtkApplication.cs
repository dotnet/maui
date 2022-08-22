using System;
using System.Diagnostics;
using Gdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{

	public abstract class MauiGtkApplication
	{

		protected abstract MauiApp CreateMauiApp();

		// https://docs.gtk.org/gio/type_func.Application.id_is_valid.html
		// TODO: find a better algo for id
		public virtual string ApplicationId => $"{typeof(MauiGtkApplication).Namespace}.{typeof(MauiGtkApplication).Name}.{Name}".PadRight(255, ' ').Substring(0, 255).Trim();

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

		public void Run()
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

			DispatchPendingEvents();

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
			var startup = CreateMauiApp();

			Services = startup.Services;
			Services.InvokeLifecycleEvents<GtkLifecycle.OnLaunching>(del => del(this, args));

			var mauiContext = new MauiContext(Services);
			Services.InvokeLifecycleEvents<GtkLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext);

			Application = Services.GetRequiredService<IApplication>();

			var window = Application.CreateWindow(activationState);

			CreateMainWindow(window, mauiContext);

			MainWindow.QueueDraw();
			MainWindow.ShowAll();

			MainWindow.Present();

			Services?.InvokeLifecycleEvents<GtkLifecycle.OnLaunched>(del => del(CurrentGtkApplication, args));
		}

		void CreateMainWindow(IWindow window, MauiContext context)
		{
			MainWindow = new MauiGtkMainWindow();
			CurrentGtkApplication.AddWindow(MainWindow);

			context.Window = MainWindow;

			MainWindow.SetWindow(window, context);
			Services.InvokeLifecycleEvents<GtkLifecycle.OnCreated>(del => del(MainWindow, EventArgs.Empty));

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

		protected void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			//future use: there will be a need of GtkNativeServices, eg. for WebView
		}

		public static void DispatchPendingEvents()
		{
			// The loop is limited to 1000 iterations as a workaround for an issue that some users
			// have experienced. Sometimes EventsPending starts return 'true' for all iterations,
			// causing the loop to never end.

			int n = 1000;
#pragma warning disable 612
			Gdk.Threads.Enter();
#pragma warning restore 612

			while (Gtk.Application.EventsPending() && --n > 0)
			{
				Gtk.Application.RunIteration(false);
			}

#pragma warning disable 612
			Gdk.Threads.Leave();
#pragma warning restore 612
		}

		public static void Invoke(System.Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			// Switch to no Invoke(Action) once a gtk# release is done.
			Gtk.Application.Invoke((o, args) =>
			{
				action();
			});
		}

	}

}