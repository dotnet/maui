using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Handlers;
using GtkApplication = Gtk.Application;
using GtkWindow = Gtk.Window;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="ApplicationHandler"/> over the <c>Gtk.Application</c> owned by <see cref="MauiGtkApplication"/>.
	/// </summary>
	public class GtkApplicationHandler : ApplicationHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkApplicationHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformElement() =>
			MauiContext?.Services.GetService<GtkApplication>()
				?? throw new InvalidOperationException("MauiContext did not have a valid Gtk.Application.");

		internal static void ApplyMappers()
		{
			var commands = CommandMapper;

			commands.MapGtk<IApplication, ApplicationHandler, GtkApplication>(TerminateCommandKey, (h, v, args, app) => ((GLib.Application)app).Quit());

			commands.MapGtk<IApplication, ApplicationHandler, GtkApplication>(nameof(IApplication.OpenWindow), (h, v, args, app) =>
				MauiGtkApplication.Current?.CreatePlatformWindow(v, args as OpenWindowRequest));

			commands.MapGtk<IApplication, ApplicationHandler, GtkApplication>(nameof(IApplication.CloseWindow), (h, v, args, app) =>
			{
				if (args is IWindow window && window.Handler?.PlatformView is GtkWindow platformWindow)
					platformWindow.Close();
			});

			commands.MapGtk<IApplication, ApplicationHandler, GtkApplication>(nameof(IApplication.ActivateWindow), (h, v, args, app) =>
			{
				if (args is IWindow window && window.Handler?.PlatformView is GtkWindow platformWindow)
					platformWindow.Present();
			});
		}
	}
}
