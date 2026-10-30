using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// The panel behind a <c>RefreshView</c>: the content, and a spinner drawn over its top edge while refreshing. A pull gesture (scrolling up while the
	/// content is already at its top) past <see cref="PullThreshold"/> raises <see cref="RefreshRequested"/>.
	/// </summary>
	public sealed class MauiGtkRefreshPanel : MauiGtkPanel
	{
		/// <summary>The accumulated upward scroll, in pixels, that triggers a refresh.</summary>
		public const double PullThreshold = 100;

		const double WheelStep = 40;
		const int EdgeMargin = 8;

		readonly global::Gtk.Spinner _spinner = new() { Visible = false, CanTarget = false };
		readonly global::Gtk.EventControllerScroll _scroll;
		global::Gtk.Widget? _content;
		bool _isRefreshing;
		double _pull;

		/// <summary>Creates the panel.</summary>
		public MauiGtkRefreshPanel()
		{
			_spinner.Parent = this;

			// Capture phase: the pull is decided before the scrollable content (if any) sees the event.
			_scroll = new global::Gtk.EventControllerScroll(global::Gtk.EventControllerScrollFlags.Vertical)
			{
				PropagationPhase = global::Gtk.PropagationPhase.Capture,
			};
			_scroll.Scroll += OnScroll;
			_scroll.ScrollEnd += OnScrollEnd;
			AddController(_scroll);
		}

		/// <summary>The spinner shown while refreshing.</summary>
		public global::Gtk.Spinner Spinner => _spinner;

		/// <summary>Whether pulling triggers a refresh.</summary>
		public bool IsRefreshEnabled { get; set; } = true;

		/// <summary>Whether a refresh is in progress (shows the spinner).</summary>
		public bool IsRefreshing
		{
			get => _isRefreshing;
			set
			{
				_isRefreshing = value;
				_spinner.Visible = value;
				_spinner.Spinning = value;
				_pull = 0;
				QueueAllocate();
			}
		}

		/// <summary>Raised when the user pulled far enough to request a refresh.</summary>
		public event Action? RefreshRequested;

		/// <summary>Replaces the content widget.</summary>
		/// <param name="content">The content, or <see langword="null"/>.</param>
		public void SetContent(global::Gtk.Widget? content)
		{
			if (_content is not null && ReferenceEquals(_content.Parent, this))
				RemoveChild(_content);
			else if (_content?.Parent is { } wrapper && ReferenceEquals(wrapper.Parent, this))
				RemoveChild(wrapper);

			_content = content;
			if (content is not null)
				InsertChild(0, content);
		}

		/// <summary>Feeds an upward scroll of <paramref name="pixels"/> (positive = pulling down) into the pull gesture.</summary>
		/// <param name="pixels">The scroll distance; negative values (scrolling back) reset the pull.</param>
		/// <returns><see langword="true"/> when the scroll was consumed by the pull.</returns>
		internal bool Pull(double pixels)
		{
			if (!IsRefreshEnabled || _isRefreshing)
				return false;

			if (pixels <= 0 || !IsContentAtTop())
			{
				_pull = 0;
				return false;
			}

			_pull += pixels;
			if (_pull >= PullThreshold)
			{
				_pull = 0;
				RefreshRequested?.Invoke();
			}

			return true;
		}

		void OnScroll(object o, global::Gtk.ScrollArgs args)
		{
			// dy < 0 scrolls up (towards the top edge): pulling down on the content.
			double pixels = -args.Dy * (_scroll.Unit == Gdk.ScrollUnit.Wheel ? WheelStep : 1);
			args.RetVal = Pull(pixels);
		}

		void OnScrollEnd(object? sender, EventArgs e) => _pull = 0;

		bool IsContentAtTop()
		{
			var scrolled = FindScrolledWindow(_content);
			if (scrolled?.Vadjustment is not { } adjustment)
				return true;

			return adjustment.Value <= adjustment.Lower + 0.5;
		}

		static global::Gtk.ScrolledWindow? FindScrolledWindow(global::Gtk.Widget? widget)
		{
			if (widget is global::Gtk.ScrolledWindow scrolled)
				return scrolled;

			for (var child = widget?.FirstChild; child is not null; child = child.NextSibling)
			{
				if (FindScrolledWindow(child) is { } found)
					return found;
			}

			return null;
		}

		/// <inheritdoc/>
		protected override void ArrangeChildren(int width, int height)
		{
			base.ArrangeChildren(width, height);

			if (!_spinner.Visible)
				return;

			_spinner.Measure(global::Gtk.Orientation.Horizontal, -1, out int minWidth, out int naturalWidth, out _, out _);
			_spinner.Measure(global::Gtk.Orientation.Vertical, naturalWidth, out int minHeight, out int naturalHeight, out _, out _);
			int spinnerWidth = Math.Max(naturalWidth, minWidth);
			int spinnerHeight = Math.Max(naturalHeight, minHeight);

			_spinner.Allocate(spinnerWidth, spinnerHeight, -1,
				new Gsk.Transform().Translate(GtkInterop.ToGraphene((width - spinnerWidth) / 2.0, EdgeMargin)));
		}
	}

	/// <summary>
	/// <see cref="RefreshViewHandler"/> over a <see cref="MauiGtkRefreshPanel"/>: pulling the content down at its top edge sets <c>IsRefreshing</c>; while it is
	/// set a spinner (in <c>RefreshColor</c>) is shown over the content.
	/// </summary>
	public class GtkRefreshViewHandler : RefreshViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkRefreshViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkRefreshViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkRefreshPanel { CrossPlatformLayout = Layout, IsMeasuredByMaui = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not MauiGtkRefreshPanel panel)
				return;

			panel.CrossPlatformLayout = Layout;
			panel.RefreshRequested += OnRefreshRequested;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkRefreshPanel panel)
			{
				panel.RefreshRequested -= OnRefreshRequested;
				panel.SetContent(null);
				panel.CrossPlatformLayout = null;
			}

			base.DisconnectHandler(platformView);
		}

		// Controls' RefreshView is a content view and lays itself out; any other implementation gets a plain fill layout.
		ICrossPlatformLayout Layout => VirtualView as ICrossPlatformLayout ?? new FillLayout(VirtualView);

		void OnRefreshRequested()
		{
			if (((IElementHandler)this).VirtualView is IRefreshView { IsRefreshing: false } view)
				view.IsRefreshing = true;
		}

		static void UpdateContent(IRefreshViewHandler handler, IRefreshView view, MauiGtkRefreshPanel panel)
		{
			var context = handler.MauiContext;
			var content = (view as IContentView)?.PresentedContent ?? view.Content;
			panel.SetContent(context is not null && content is not null ? (global::Gtk.Widget)content.ToPlatform(context) : null);
		}

		internal static readonly IPropertyMapper<IRefreshView, IRefreshViewHandler> GtkMapper =
			new GtkMapperBuilder<IRefreshView, IRefreshViewHandler>(RefreshViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkRefreshPanel>(nameof(IRefreshView.Content), (h, v, w) => UpdateContent(h, v, w))
				.MapGtk<MauiGtkRefreshPanel>(nameof(IRefreshView.IsRefreshing), (h, v, w) => w.IsRefreshing = v.IsRefreshing)
				.MapGtk<MauiGtkRefreshPanel>(nameof(IRefreshView.IsRefreshEnabled), (h, v, w) => w.IsRefreshEnabled = v.IsRefreshEnabled)
				.MapGtk<MauiGtkRefreshPanel>(nameof(IRefreshView.RefreshColor), (h, v, w) =>
					GtkTextStyle.SetColor(w.Spinner, v.RefreshColor?.ToColor()))
				.Build();

		internal static readonly CommandMapper<IRefreshView, IRefreshViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IRefreshView, IRefreshViewHandler>(RefreshViewHandler.CommandMapper).AddViewCommands().Build();

		sealed class FillLayout : ICrossPlatformLayout
		{
			readonly IRefreshView _view;

			public FillLayout(IRefreshView view) => _view = view;

			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
				_view.Content?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;

			public Size CrossPlatformArrange(Rect bounds)
			{
				_view.Content?.Arrange(bounds);
				return bounds.Size;
			}
		}
	}
}
