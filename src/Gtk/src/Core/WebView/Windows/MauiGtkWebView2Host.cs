using System;
using System.Threading.Tasks;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// The GTK widget that hosts a WebView2 control on Windows: an empty, focusable widget that owns a <see cref="CoreWebView2Controller"/>
	/// parented to the HWND of the GTK toplevel window and keeps that child window on top of the widget's allocation.
	/// </summary>
	/// <remarks>
	/// <para><b>Lifecycle.</b> The controller is created when the widget is realized (it then has a toplevel with an HWND) and closed when it is
	/// unrealized; <see cref="MauiGtkWebView2Engine"/> replays the last navigation on the controller of a re-realization.</para>
	/// <para><b>Placement.</b> <c>Bounds</c> are the widget's bounds in toplevel coordinates, plus the toplevel's surface transform (the client-side
	/// decoration shadow), times the surface scale (<c>gdk_surface_get_scale</c>), in the pixels of the HWND's client area. They are pushed on every
	/// size allocation, map, unmap, root change and at the end of every frame-clock layout phase (GTK does not notify a widget whose position
	/// alone changed), only when they changed.</para>
	/// <para><b>Visibility.</b> The web view is visible exactly while the widget is mapped, which GTK derives from the widget's own visibility and
	/// that of all its ancestors (hidden pages of a stack, a hidden window, a collapsed container).</para>
	/// <para><b>Focus.</b> Taking the GTK focus moves the keyboard focus into the web view; a click inside the web view makes the host the GTK
	/// focus widget; tabbing out of the web view returns the Win32 focus to the toplevel and moves the GTK focus on; when another widget of the active window takes the GTK focus (a click, a programmatic focus change) the Win32 focus returns to the toplevel too, otherwise typing would still go to the page.</para>
	/// <para><b>Airspace.</b> The web view is a native child window: it is always above the GTK content, is not clipped by GTK containers
	/// (scrolled windows) and cannot be transformed or faded; popovers and menus over it are hidden behind it.</para>
	/// <para><b>Seams for the HybridWebView and BlazorWebView tracks.</b> <see cref="Engine"/> is the <see cref="IGtkWebEngine"/> of the host;
	/// <see cref="CoreWebView2Initialized"/> is raised for each new controller before the engine navigates, which is the place to add
	/// <c>WebResourceRequested</c> filters, document-created scripts and <c>WebMessageReceived</c> handlers; <see cref="CoreWebView2"/> is the
	/// current web view (<see langword="null"/> until then). Set <see cref="Services"/> before the widget is realized to replace the platform
	/// calls.</para>
	/// </remarks>
	internal sealed class MauiGtkWebView2Host : global::Gtk.Widget
	{
		GtkWebView2HostServices _services = GtkWebView2HostServices.Default;
		IGtkWebView2Controller? _controller;
		IDisposable? _locationWatch;
		global::Gtk.EventControllerFocus? _focusController;
		IntPtr _toplevelHandle;
		int _generation;
		bool _creating;
		bool _syncingFocus;
		bool _returningFocus;
		(int X, int Y, int Width, int Height)? _lastBounds;
		bool? _lastVisible;

		/// <summary>Creates the host and its engine.</summary>
		public MauiGtkWebView2Host()
		{
			Focusable = true;
			CanFocus = true;
			Engine = new MauiGtkWebView2Engine(this);

			_focusController = new global::Gtk.EventControllerFocus();
			_focusController.Enter += OnFocusEnter;
			_focusController.Leave += OnFocusLeave;
			AddController(_focusController);
		}

		/// <summary>The engine that drives this host's web view.</summary>
		internal MauiGtkWebView2Engine Engine { get; }

		/// <summary>The platform services (see the type remarks); replace before the widget is realized.</summary>
		internal GtkWebView2HostServices Services
		{
			get => _services;
			set => _services = value ?? GtkWebView2HostServices.Default;
		}

		/// <summary>The current controller, or <see langword="null"/> while there is none (not realized, or still being created).</summary>
		internal IGtkWebView2Controller? Controller => _controller;

		/// <summary>The current web view, or <see langword="null"/> while there is no controller.</summary>
		internal CoreWebView2? CoreWebView2 => _controller?.Core;

		/// <summary>The HWND the controller is parented to, or <see cref="IntPtr.Zero"/> while there is none.</summary>
		internal IntPtr ToplevelHandle => _toplevelHandle;

		/// <summary>Raised when a controller was created and placed, before the engine replays its pending navigation.</summary>
		internal event EventHandler<IGtkWebView2Controller>? ControllerCreated;

		/// <summary>Raised for each new <see cref="CoreWebView2"/>; see the type remarks. Never raised for a fake controller.</summary>
		internal event EventHandler<CoreWebView2>? CoreWebView2Initialized;

		/// <summary>Raised just before the controller is closed.</summary>
		internal event EventHandler? ControllerClosing;

		/// <summary>Raised when the controller could not be created (the runtime vanished, the user data folder is locked).</summary>
		internal event EventHandler<Exception>? ControllerFailed;

		/// <summary>The size request of an empty widget is 0x0: the MAUI layout decides the size.</summary>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimum = natural = 0;
			minimumBaseline = naturalBaseline = -1;
		}

		/// <inheritdoc/>
		protected override void OnRealized()
		{
			base.OnRealized();
			BeginCreateController();
		}

		/// <inheritdoc/>
		protected override void OnUnrealized()
		{
			CloseController();
			base.OnUnrealized();
		}

		/// <inheritdoc/>
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);
			UpdateBounds();
		}

		/// <inheritdoc/>
		protected override void OnMapped()
		{
			base.OnMapped();
			UpdateBounds();
			UpdateVisibility();
		}

		/// <inheritdoc/>
		protected override void OnUnmapped()
		{
			base.OnUnmapped();
			UpdateVisibility();
		}

		void BeginCreateController()
		{
			if (_controller is not null || _creating)
				return;

			if (Native is not { } native)
				return;

			var hwnd = _services.GetToplevelHandle(native);
			if (hwnd == IntPtr.Zero)
				return;

			_toplevelHandle = hwnd;
			_creating = true;
			var generation = ++_generation;
			_ = CreateControllerAsync(hwnd, generation);
		}

		async Task CreateControllerAsync(IntPtr hwnd, int generation)
		{
			IGtkWebView2Controller? controller = null;
			Exception? failure = null;

			try
			{
				controller = await _services.CreateControllerAsync(hwnd);
			}
			catch (Exception e)
			{
				failure = e;
			}

			// WebView2 completes on the thread that started it (the GTK thread, through GDK's message pump); stay safe anyway.
			GtkRuntime.Post(() => CompleteCreate(controller, failure, generation));
		}

		void CompleteCreate(IGtkWebView2Controller? controller, Exception? failure, int generation)
		{
			if (generation != _generation)
			{
				// Unrealized (or re-realized) while the controller was being created.
				controller?.Dispose();
				return;
			}

			_creating = false;

			if (controller is null)
			{
				ControllerFailed?.Invoke(this, failure ?? new InvalidOperationException("The WebView2 controller was not created."));
				return;
			}

			_controller = controller;
			controller.GotFocus += OnControllerGotFocus;
			controller.FocusLeaveRequested += OnFocusLeaveRequested;
			_locationWatch = _services.WatchLocation(_toplevelHandle, NotifyToplevelMoved);
			_lastBounds = null;
			_lastVisible = null;
			HookFrameClock();

			UpdateBounds();
			UpdateVisibility();

			if (controller.Core is { } core)
				CoreWebView2Initialized?.Invoke(this, core);

			ControllerCreated?.Invoke(this, controller);

			if (HasFocus)
				controller.MoveFocusIn();
		}

		/// <summary>Closes the controller (unrealize, handler disconnect). A later realization creates a new one.</summary>
		internal void CloseController()
		{
			_generation++;
			_creating = false;

			var controller = _controller;
			if (controller is null)
				return;

			ControllerClosing?.Invoke(this, EventArgs.Empty);

			_controller = null;
			_locationWatch?.Dispose();
			_locationWatch = null;
			UnhookFrameClock();
			controller.GotFocus -= OnControllerGotFocus;
			controller.FocusLeaveRequested -= OnFocusLeaveRequested;
			controller.Dispose();
			_toplevelHandle = IntPtr.Zero;
			_lastBounds = null;
			_lastVisible = null;
		}

		/// <summary>The widget's bounds in the pixels of the toplevel HWND's client area, or <see langword="null"/> while it has no toplevel.</summary>
		internal (int X, int Y, int Width, int Height)? ComputeNativeBounds()
		{
			if (Native is not { } native || native is not global::Gtk.Widget toplevel)
				return null;

			if (!ComputeBounds(toplevel, out var rect))
				return null;

			native.GetSurfaceTransform(out var tx, out var ty);
			var scale = _services.GetScale(native);

			var left = (int)Math.Floor((rect.X + tx) * scale);
			var top = (int)Math.Floor((rect.Y + ty) * scale);
			var right = (int)Math.Ceiling((rect.X + rect.Width + tx) * scale);
			var bottom = (int)Math.Ceiling((rect.Y + rect.Height + ty) * scale);
			return (left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
		}

		/// <summary>Pushes the widget's current bounds to the controller when they changed.</summary>
		internal void UpdateBounds()
		{
			if (_controller is null || ComputeNativeBounds() is not { } bounds || bounds == _lastBounds)
				return;

			_lastBounds = bounds;
			_controller.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}

		/// <summary>Shows or hides the controller to match the widget's mapped state when that changed.</summary>
		internal void UpdateVisibility()
		{
			if (_controller is null)
				return;

			var visible = IsMapped;
			if (visible == _lastVisible)
				return;

			_lastVisible = visible;
			_controller.SetVisible(visible);
		}

		/// <summary>The toplevel moved on the screen: WebView2 repositions its popups (drop-downs, context menus).</summary>
		internal void NotifyToplevelMoved() => _controller?.NotifyParentWindowPositionChanged();

		void HookFrameClock()
		{
			if (FrameClock is { } clock)
			{
				clock.Layout -= OnFrameLayout;
				clock.Layout += OnFrameLayout;
			}
		}

		void UnhookFrameClock()
		{
			if (FrameClock is { } clock)
				clock.Layout -= OnFrameLayout;
		}

		void OnFrameLayout(object? sender, EventArgs e)
		{
			UpdateBounds();
			UpdateVisibility();
		}

		void OnFocusEnter(object? sender, EventArgs e) => FocusEntered();

		/// <summary>The host became the GTK focus widget of an active window: the keyboard focus moves into the web view.</summary>
		internal void FocusEntered()
		{
			if (!_syncingFocus)
				_controller?.MoveFocusIn();
		}

		void OnFocusLeave(object? sender, EventArgs e) => FocusLeft();

		/// <summary>
		/// The host lost the GTK focus to another widget: the Win32 keyboard focus is still in the WebView2 child window (clicking a GTK widget
		/// does not move it), so it returns to the toplevel. Nothing happens while the window itself is inactive (focus follows the window) or
		/// when the focus is already on its way back (tab out, <see cref="_returningFocus"/>).
		/// </summary>
		internal void FocusLeft()
		{
			if (_controller is null || _toplevelHandle == IntPtr.Zero || _returningFocus || _syncingFocus)
				return;

			if (Root is not global::Gtk.Window { IsActive: true })
				return;

			_services.FocusToplevel(_toplevelHandle);
		}

		void OnControllerGotFocus(object? sender, EventArgs e)
		{
			// A click inside the web view: make the host the GTK focus widget so Tab and focus rings continue from here.
			if (IsFocus)
				return;

			_syncingFocus = true;
			try
			{
				GrabFocus();
			}
			finally
			{
				_syncingFocus = false;
			}
		}

		void OnFocusLeaveRequested(object? sender, bool forward)
		{
			_returningFocus = true;
			try
			{
				if (_toplevelHandle != IntPtr.Zero)
					_services.FocusToplevel(_toplevelHandle);

				if (Root is global::Gtk.Widget root)
					root.ChildFocus(forward ? global::Gtk.DirectionType.TabForward : global::Gtk.DirectionType.TabBackward);
			}
			finally
			{
				_returningFocus = false;
			}
		}
	}
}
