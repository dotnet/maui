using System;

namespace Microsoft.Maui.LifecycleEvents
{
	/// <summary>
	/// Lifecycle events raised by the GTK backend (see <see cref="MauiGtkApplication"/>). Register with
	/// <c>ConfigureLifecycleEvents(events =&gt; events.AddGtk(gtk =&gt; gtk.OnActivate(...)))</c>.
	/// </summary>
	public static class GtkLifecycle
	{
		/// <summary>Raised when the <c>Gtk.Application</c> starts up (before any window exists).</summary>
		/// <param name="application">The GTK application.</param>
		public delegate void OnStartup(global::Gtk.Application application);

		/// <summary>Raised when the application is activated (a window is about to be created or presented).</summary>
		/// <param name="application">The GTK application.</param>
		public delegate void OnActivate(global::Gtk.Application application);

		/// <summary>Raised when the application shuts down.</summary>
		/// <param name="application">The GTK application.</param>
		public delegate void OnShutdown(global::Gtk.Application application);

		/// <summary>Raised after a platform window was created for an <see cref="IWindow"/>.</summary>
		/// <param name="window">The GTK window.</param>
		public delegate void OnWindowCreated(global::Gtk.Window window);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
	}

	/// <summary>Builder for GTK lifecycle events.</summary>
	public interface IGtkLifecycleBuilder : ILifecycleBuilder
	{
	}

	/// <summary>Extension methods to register <see cref="GtkLifecycle"/> delegates.</summary>
	public static class GtkLifecycleExtensions
	{
		/// <summary>Adds GTK lifecycle configuration to <paramref name="builder"/>.</summary>
		/// <param name="builder">The lifecycle builder.</param>
		/// <param name="configureDelegate">Registers the GTK events.</param>
		public static ILifecycleBuilder AddGtk(this ILifecycleBuilder builder, Action<IGtkLifecycleBuilder> configureDelegate)
		{
			var lifecycle = new GtkLifecycleBuilder(builder);
			configureDelegate?.Invoke(lifecycle);
			return builder;
		}

		/// <summary>Registers <see cref="GtkLifecycle.OnStartup"/>.</summary>
		/// <param name="lifecycle">The builder.</param>
		/// <param name="del">The delegate.</param>
		public static IGtkLifecycleBuilder OnStartup(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnStartup del) => lifecycle.OnEvent(del);

		/// <summary>Registers <see cref="GtkLifecycle.OnActivate"/>.</summary>
		/// <param name="lifecycle">The builder.</param>
		/// <param name="del">The delegate.</param>
		public static IGtkLifecycleBuilder OnActivate(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnActivate del) => lifecycle.OnEvent(del);

		/// <summary>Registers <see cref="GtkLifecycle.OnShutdown"/>.</summary>
		/// <param name="lifecycle">The builder.</param>
		/// <param name="del">The delegate.</param>
		public static IGtkLifecycleBuilder OnShutdown(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnShutdown del) => lifecycle.OnEvent(del);

		/// <summary>Registers <see cref="GtkLifecycle.OnWindowCreated"/>.</summary>
		/// <param name="lifecycle">The builder.</param>
		/// <param name="del">The delegate.</param>
		public static IGtkLifecycleBuilder OnWindowCreated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnWindowCreated del) => lifecycle.OnEvent(del);

		internal static IGtkLifecycleBuilder OnMauiContextCreated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);

		sealed class GtkLifecycleBuilder : IGtkLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public GtkLifecycleBuilder(ILifecycleBuilder builder) => _builder = builder;

			public void AddEvent<TDelegate>(string eventName, TDelegate action)
				where TDelegate : Delegate
			{
				_builder.AddEvent(eventName, action);
			}
		}
	}
}
