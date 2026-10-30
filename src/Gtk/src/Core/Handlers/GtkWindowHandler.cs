using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using GtkWindow = Gtk.Window;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="WindowHandler"/> over a <c>Gtk.Window</c>. The window's child is a <see cref="MauiGtkRootPanel"/> that
	/// lays out <see cref="IWindow.Content"/> to fill the window.
	/// </summary>
	/// <remarks>
	/// GTK 4 has no API to position or cap the size of a toplevel, so <c>X</c>, <c>Y</c>, <c>MaximumWidth</c> and
	/// <c>MaximumHeight</c> are ignored; <c>Width</c>/<c>Height</c> set the default size and the minimums set the size request.
	/// </remarks>
	public class GtkWindowHandler : WindowHandler
	{
		/// <summary>The default window size used when the <see cref="IWindow"/> does not specify one.</summary>
		public static readonly Size DefaultWindowSize = new(800, 600);

		MauiGtkRootPanel? _root;
		bool _created;
		bool _resumed;
		bool _activated;
		bool _destroyed;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkWindowHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkWindowHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The panel hosting the window content.</summary>
		internal MauiGtkRootPanel? RootPanel => _root;

		/// <inheritdoc/>
		protected override object CreatePlatformElement() =>
			MauiContext?.Services.GetService<GtkWindow>()
				?? throw new InvalidOperationException("MauiContext did not have a valid Gtk.Window.");

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not GtkWindow window)
				return;

			_root = new MauiGtkRootPanel { CrossPlatformLayout = new WindowContentLayout(this) };
			window.Child = _root;

			window.CloseRequest += OnCloseRequest;
			window.AddNotification("is-active", OnIsActiveChanged);
			_root.SizeChanged += OnRootSizeChanged;

			if (!_created)
			{
				_created = true;
				View?.Created();
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkWindow window)
			{
				window.CloseRequest -= OnCloseRequest;
				window.RemoveNotification("is-active", OnIsActiveChanged);
				if (_root is not null)
					_root.SizeChanged -= OnRootSizeChanged;

				_root?.ClearChildren();
				_root = null;
			}

			base.DisconnectHandler(platformView);
		}

		void OnCloseRequest(object o, global::Gtk.CloseRequestArgs args)
		{
			// The virtual window throws when it is destroyed twice (a hide-on-close window can be asked repeatedly).
			if (!_destroyed && View is { } view)
			{
				_destroyed = true;

				if (_activated)
				{
					_activated = false;
					view.Deactivated();
				}

				view.Destroying();
			}

			args.RetVal = false;
		}

		void OnIsActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (((IElementHandler)this).PlatformView is not GtkWindow window)
				return;

			UpdateActive(window.IsActive);
		}

		/// <summary>
		/// Forwards an activation change to the virtual window with the same guards as the <c>is-active</c> notification. Display servers
		/// without a window manager (a bare X server) never activate a toplevel, so hosts and tests use this to run the activation lifecycle.
		/// </summary>
		/// <param name="isActive">Whether the window is now the active one.</param>
		internal void UpdateActive(bool isActive)
		{
			if (View is not { } view)
				return;

			// The virtual window requires Resumed before the first Activated and strictly alternating Activated/Deactivated:
			// is-active also notifies without changing value, so only transitions are forwarded.
			if (isActive)
			{
				if (!_resumed)
				{
					_resumed = true;
					view.Resumed();
				}

				if (!_activated)
				{
					_activated = true;
					view.Activated();
				}
			}
			else if (_activated)
			{
				_activated = false;
				view.Deactivated();
			}
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IWindow? View => ((IElementHandler)this).VirtualView as IWindow;

		void OnRootSizeChanged(int width, int height)
		{
			// Report the size of the whole window (the default-size semantics), not just the content area under the title bar.
			var window = ((IElementHandler)this).PlatformView as GtkWindow;
			int windowWidth = window is { Width: > 0 } ? window.Width : width;
			int windowHeight = window is { Height: > 0 } ? window.Height : height;
			View?.FrameChanged(new Rect(0, 0, windowWidth, windowHeight));
		}

		/// <summary>Replaces the window content widget.</summary>
		internal static void UpdateContent(GtkWindowHandler handler, IWindow window)
		{
			var root = handler._root;
			if (root is null || handler.MauiContext is null)
				return;

			root.ClearChildren();

			if (window.Content is { } content)
			{
				var platformContent = (global::Gtk.Widget)content.ToPlatform(handler.MauiContext);
				root.AddChild(platformContent);
			}
		}

		internal static readonly IPropertyMapper<IWindow, IWindowHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IWindow, IWindowHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IWindow, IWindowHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IWindow, IWindowHandler>(WindowHandler.Mapper);
			var commands = new GtkCommandMapperBuilder<IWindow, IWindowHandler>(WindowHandler.CommandMapper);
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IWindow, IWindowHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IWindow, IWindowHandler>(WindowHandler.Mapper);
			var commands = new GtkCommandMapperBuilder<IWindow, IWindowHandler>(WindowHandler.CommandMapper);
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IWindow, IWindowHandler> mapper, GtkCommandMapperBuilder<IWindow, IWindowHandler> commands)
		{
			mapper.MapGtk<GtkWindow>(nameof(IWindow.Title), (h, v, w) => w.Title = v.Title ?? string.Empty);
			mapper.MapGtk<GtkWindow>(nameof(IWindow.Content), (h, v, w) =>
			{
				if (h is GtkWindowHandler handler)
					UpdateContent(handler, v);
			});
			mapper.MapGtk<GtkWindow>(nameof(IWindow.Width), (h, v, w) => UpdateDefaultSize(w, v));
			mapper.MapGtk<GtkWindow>(nameof(IWindow.Height), (h, v, w) => UpdateDefaultSize(w, v));
			mapper.MapGtk<GtkWindow>(nameof(IWindow.MinimumWidth), (h, v, w) => UpdateMinimumSize(w, v));
			mapper.MapGtk<GtkWindow>(nameof(IWindow.MinimumHeight), (h, v, w) => UpdateMinimumSize(w, v));
			mapper.MapGtk<GtkWindow>(nameof(IWindow.FlowDirection), (h, v, w) =>
				w.Direction = v.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.TextDirection.Rtl : global::Gtk.TextDirection.Ltr);

			mapper.MapGtk<GtkWindow>(nameof(IMenuBarElement.MenuBar), (h, v, w) =>
			{
				if (h is GtkWindowHandler handler)
					GtkWindowMenuBar.Update(handler, v, w);
			});
			// The toolbar of the window (set by Controls' NavigationPage, for example) is the window's title bar.
			mapper.MapGtk<GtkWindow>(nameof(IToolbarElement.Toolbar), (h, v, w) => GtkToolbarSupport.UpdateWindowToolbar(h, v as IToolbarElement));

			// GTK 4 cannot position a toplevel; X/Y/Maximum* are accepted and ignored.
			foreach (var key in new[] { nameof(IWindow.X), nameof(IWindow.Y), nameof(IWindow.MaximumWidth), nameof(IWindow.MaximumHeight) })
				mapper.MapGtk<GtkWindow>(key, (h, v, w) => { });

			commands.MapGtk<GtkWindow>(nameof(IWindow.RequestDisplayDensity), (h, v, args, w) =>
			{
				if (args is DisplayDensityRequest request)
					request.SetResult(w.ScaleFactor);
			});
		}

		static void UpdateDefaultSize(GtkWindow window, IWindow view)
		{
			int width = IsSpecified(view.Width) ? (int)Math.Ceiling(view.Width) : (int)DefaultWindowSize.Width;
			int height = IsSpecified(view.Height) ? (int)Math.Ceiling(view.Height) : (int)DefaultWindowSize.Height;
			window.SetDefaultSize(width, height);
		}

		static void UpdateMinimumSize(GtkWindow window, IWindow view)
		{
			int width = IsSpecified(view.MinimumWidth) ? (int)Math.Ceiling(view.MinimumWidth) : -1;
			int height = IsSpecified(view.MinimumHeight) ? (int)Math.Ceiling(view.MinimumHeight) : -1;
			window.SetSizeRequest(width, height);
		}

		static bool IsSpecified(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;

		/// <summary>Measures and arranges the window content to fill the window.</summary>
		sealed class WindowContentLayout : ICrossPlatformLayout
		{
			readonly GtkWindowHandler _handler;

			public WindowContentLayout(GtkWindowHandler handler) => _handler = handler;

			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
				_handler.View?.Content?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;

			public Size CrossPlatformArrange(Rect bounds)
			{
				_handler.View?.Content?.Arrange(bounds);
				return bounds.Size;
			}
		}
	}
}
