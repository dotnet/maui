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
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformElement() =>
			MauiContext?.Services.GetService<GtkApplication>()
				?? throw new InvalidOperationException("MauiContext did not have a valid Gtk.Application.");

		internal static readonly IPropertyMapper<IApplication, ApplicationHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IApplication, ApplicationHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IApplication, ApplicationHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IApplication, ApplicationHandler>(ApplicationHandler.Mapper);
			var commands = new GtkCommandMapperBuilder<IApplication, ApplicationHandler>(ApplicationHandler.CommandMapper);
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IApplication, ApplicationHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IApplication, ApplicationHandler>(ApplicationHandler.Mapper);
			var commands = new GtkCommandMapperBuilder<IApplication, ApplicationHandler>(ApplicationHandler.CommandMapper);
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IApplication, ApplicationHandler> mapper, GtkCommandMapperBuilder<IApplication, ApplicationHandler> commands)
		{
			commands.MapGtk<GtkApplication>(TerminateCommandKey, (h, v, args, app) => ((GLib.Application)app).Quit());

			commands.MapGtk<GtkApplication>(nameof(IApplication.OpenWindow), (h, v, args, app) =>
				MauiGtkApplication.Current?.CreatePlatformWindow(v, args as OpenWindowRequest));

			commands.MapGtk<GtkApplication>(nameof(IApplication.CloseWindow), (h, v, args, app) =>
			{
				if (args is IWindow window && window.Handler?.PlatformView is GtkWindow platformWindow)
					platformWindow.Close();
			});

			commands.MapGtk<GtkApplication>(nameof(IApplication.ActivateWindow), (h, v, args, app) =>
			{
				if (args is IWindow window && window.Handler?.PlatformView is GtkWindow platformWindow)
					platformWindow.Present();
			});
		}
	}
}
