using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// What <see cref="MauiGtkWebView2Host"/> needs from a <see cref="CoreWebView2Controller"/>: the seam the host tests replace with a fake.
	/// </summary>
	internal interface IGtkWebView2Controller : IDisposable
	{
		/// <summary>The web view of the controller, or <see langword="null"/> for a fake.</summary>
		CoreWebView2? Core { get; }

		/// <summary>The real controller, or <see langword="null"/> for a fake.</summary>
		CoreWebView2Controller? Controller { get; }

		/// <summary>Sets the bounds in physical pixels, relative to the client area of the parent window.</summary>
		void SetBounds(int x, int y, int width, int height);

		/// <summary>Shows or hides the web view.</summary>
		void SetVisible(bool visible);

		/// <summary>Moves the keyboard focus into the web view.</summary>
		void MoveFocusIn();

		/// <summary>Tells the web view its parent window moved on the screen (so its popups follow).</summary>
		void NotifyParentWindowPositionChanged();

		/// <summary>Raised when the web view took the keyboard focus (a click inside it).</summary>
		event EventHandler? GotFocus;

		/// <summary>Raised when the user tabbed out of the web view: <see langword="true"/> forward, <see langword="false"/> backward.</summary>
		event EventHandler<bool>? FocusLeaveRequested;
	}

	/// <summary>
	/// The platform services of a <see cref="MauiGtkWebView2Host"/>: the real implementation talks to GDK-Win32 and WebView2, the tests
	/// override the members.
	/// </summary>
	internal class GtkWebView2HostServices
	{
		/// <summary>The shared instance used by hosts that were not given other services.</summary>
		internal static GtkWebView2HostServices Default { get; } = new();

		/// <summary>The HWND of the toplevel <paramref name="native"/> (<see cref="IntPtr.Zero"/> when it has no surface yet).</summary>
		internal virtual IntPtr GetToplevelHandle(global::Gtk.INative native)
		{
			var surface = native.Surface;
			return surface is null ? IntPtr.Zero : GtkWin32.GetSurfaceHandle(surface.Handle);
		}

		/// <summary>The device pixels per logical pixel of the surface of <paramref name="native"/> (<c>gdk_surface_get_scale</c>).</summary>
		internal virtual double GetScale(global::Gtk.INative native) =>
			native.Surface is { } surface ? GetSurfaceScale(surface) : 1;

		static double GetSurfaceScale(global::Gdk.Surface surface) =>
			Platform.GtkRuntime.AtLeast(4, 12) ? surface.Scale : surface.ScaleFactor;

		/// <summary>Creates a controller parented to <paramref name="parentWindow"/>.</summary>
		internal virtual async Task<IGtkWebView2Controller> CreateControllerAsync(IntPtr parentWindow)
		{
			var environment = await GtkWebView2Environment.GetAsync();
			var controller = await environment.CreateCoreWebView2ControllerAsync(parentWindow);
			return new GtkWebView2Controller(controller);
		}

		/// <summary>Calls <paramref name="moved"/> whenever the window <paramref name="hwnd"/> moves on the screen.</summary>
		internal virtual IDisposable? WatchLocation(IntPtr hwnd, Action moved) => GtkWin32.WatchLocation(hwnd, moved);

		/// <summary>Gives the Win32 keyboard focus back to the GTK toplevel window.</summary>
		internal virtual void FocusToplevel(IntPtr hwnd) => GtkWin32.Focus(hwnd);
	}

	/// <summary>An <see cref="IGtkWebView2Controller"/> over a real <see cref="CoreWebView2Controller"/>.</summary>
	internal sealed class GtkWebView2Controller : IGtkWebView2Controller
	{
		readonly CoreWebView2Controller _controller;

		internal GtkWebView2Controller(CoreWebView2Controller controller)
		{
			_controller = controller;
			controller.GotFocus += OnGotFocus;
			controller.MoveFocusRequested += OnMoveFocusRequested;
		}

		/// <inheritdoc/>
		public CoreWebView2 Core => _controller.CoreWebView2;

		/// <inheritdoc/>
		public CoreWebView2Controller Controller => _controller;

		/// <inheritdoc/>
		public event EventHandler? GotFocus;

		/// <inheritdoc/>
		public event EventHandler<bool>? FocusLeaveRequested;

		/// <inheritdoc/>
		public void SetBounds(int x, int y, int width, int height) =>
			_controller.Bounds = new System.Drawing.Rectangle(x, y, width, height);

		/// <inheritdoc/>
		public void SetVisible(bool visible) => _controller.IsVisible = visible;

		/// <inheritdoc/>
		public void MoveFocusIn() => _controller.MoveFocus(CoreWebView2MoveFocusReason.Programmatic);

		/// <inheritdoc/>
		public void NotifyParentWindowPositionChanged() => _controller.NotifyParentWindowPositionChanged();

		/// <inheritdoc/>
		public void Dispose()
		{
			_controller.GotFocus -= OnGotFocus;
			_controller.MoveFocusRequested -= OnMoveFocusRequested;
			_controller.Close();
		}

		void OnGotFocus(object? sender, object e) => GotFocus?.Invoke(this, EventArgs.Empty);

		void OnMoveFocusRequested(object? sender, CoreWebView2MoveFocusRequestedEventArgs e)
		{
			if (e.Reason == CoreWebView2MoveFocusReason.Programmatic)
				return;

			e.Handled = true;
			FocusLeaveRequested?.Invoke(this, e.Reason == CoreWebView2MoveFocusReason.Next);
		}
	}
}
