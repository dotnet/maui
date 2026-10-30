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

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkWindowHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkWindowHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
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
				VirtualView?.Created();
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkWindow window)
			{
				window.CloseRequest -= OnCloseRequest;
				if (_root is not null)
					_root.SizeChanged -= OnRootSizeChanged;

				_root?.ClearChildren();
				_root = null;
			}

			base.DisconnectHandler(platformView);
		}

		void OnCloseRequest(object o, global::Gtk.CloseRequestArgs args)
		{
			VirtualView?.Destroying();
			args.RetVal = false;
		}

		void OnIsActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (PlatformView is not GtkWindow window || VirtualView is null)
				return;

			if (window.IsActive)
				VirtualView.Activated();
			else
				VirtualView.Deactivated();
		}

		void OnRootSizeChanged(int width, int height) => VirtualView?.FrameChanged(new Rect(0, 0, width, height));

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

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.Title), (h, v, w) => w.Title = v.Title ?? string.Empty);
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.Content), (h, v, w) =>
			{
				if (h is GtkWindowHandler handler)
					UpdateContent(handler, v);
			});
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.Width), (h, v, w) => UpdateDefaultSize(w, v));
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.Height), (h, v, w) => UpdateDefaultSize(w, v));
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.MinimumWidth), (h, v, w) => UpdateMinimumSize(w, v));
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.MinimumHeight), (h, v, w) => UpdateMinimumSize(w, v));
			mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.FlowDirection), (h, v, w) =>
				w.Direction = v.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.TextDirection.Rtl : global::Gtk.TextDirection.Ltr);

			// GTK 4 cannot position a toplevel; X/Y/Maximum* are accepted and ignored.
			foreach (var key in new[] { nameof(IWindow.X), nameof(IWindow.Y), nameof(IWindow.MaximumWidth), nameof(IWindow.MaximumHeight) })
				mapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(key, (h, v, w) => { });

			CommandMapper.MapGtk<IWindow, IWindowHandler, GtkWindow>(nameof(IWindow.RequestDisplayDensity), (h, v, args, w) =>
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
				_handler.VirtualView?.Content?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;

			public Size CrossPlatformArrange(Rect bounds)
			{
				_handler.VirtualView?.Content?.Arrange(bounds);
				return bounds.Size;
			}
		}
	}
}
