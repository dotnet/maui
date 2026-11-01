using System;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Gtk.Hosting;
using GtkBox = Gtk.Box;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>
	/// The handler of <see cref="CarouselView"/> on GTK 4: a horizontal (or vertical) <c>Gtk.ListView</c> whose rows are as wide (as high for a
	/// vertical carousel) as the viewport (minus the peek area), that snaps to the nearest item when scrolling settles.
	/// </summary>
	/// <remarks>
	/// <para>The scroll position and <see cref="CarouselView.Position"/>/<see cref="CarouselView.CurrentItem"/> follow each other (also for a
	/// vertical carousel); scrolling by the app is instant (<c>IsScrollAnimated</c> is ignored). No libadwaita dependency: the snap is a short
	/// timer after the last scroll change.</para>
	/// <para><see cref="CarouselView.Loop"/> wraps at the edges: dragging past the last page jumps to the first one and the other way round (a
	/// jump, not a seamless ring; the wheel does not overshoot, so it does not wrap), and a <see cref="CarouselView.Position"/> past the end
	/// wraps around. <see cref="CarouselView.IsBounceEnabled"/> <see langword="false"/> hides GTK's overshoot effect at the edges.</para>
	/// </remarks>
	public class GtkCarouselViewHandler : CarouselViewHandler
	{
		const uint SnapDelayMilliseconds = 120;

		const long WrapCooldownMilliseconds = 400;

		const string CarouselClass = "maui-carousel";

		const string NoBounceClass = "maui-carousel-no-bounce";

		static bool s_cssInstalled;

		uint _snapTimer;
		bool _updating;
		int _lastWidth;
		int _lastHeight;
		long _lastWrapTick;

		/// <summary>Creates the handler with the Controls mapper.</summary>
		public GtkCarouselViewHandler()
			: base(ControlsMapper)
		{
		}

		/// <summary>Creates the handler with a custom mapper.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		public GtkCarouselViewHandler(PropertyMapper? mapper)
			: base(mapper ?? ControlsMapper)
		{
		}

		internal GtkItemsController? Controller { get; private set; }

		/// <summary>The width of one page: the viewport minus the peek area; 0 until the first layout.</summary>
		internal int PageWidth
		{
			get
			{
				if (Controller is null || VirtualView is null)
					return 0;

				int viewport = Controller.Scroller.Width;
				if (viewport <= 0)
					return 0;

				var insets = VirtualView.PeekAreaInsets;
				return Math.Max(1, viewport - (int)Math.Round(insets.Left + insets.Right));
			}
		}

		/// <summary>The height of one page of a vertical carousel: the viewport minus the peek area; 0 until the first layout.</summary>
		internal int PageHeight
		{
			get
			{
				if (Controller is null || VirtualView is null)
					return 0;

				int viewport = Controller.Scroller.Height;
				if (viewport <= 0)
					return 0;

				var insets = VirtualView.PeekAreaInsets;
				return Math.Max(1, viewport - (int)Math.Round(insets.Top + insets.Bottom));
			}
		}

		bool IsHorizontal => Controller?.List?.Orientation != global::Gtk.Orientation.Vertical;

		/// <summary>The extent of one page along the scroll axis.</summary>
		internal int PageSize => IsHorizontal ? PageWidth : PageHeight;

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var context = MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
			Controller = new GtkItemsController(VirtualView, context)
			{
				FixedRowWidth = () => IsHorizontal ? PageWidth : 0,
				FixedRowHeight = () => IsHorizontal ? 0 : PageHeight,
			};
			return Controller.Root;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (Controller is { } controller)
			{
				controller.Refresh();
				UpdateScrolling();
				controller.Root.SizeAllocated += OnRootAllocated;
				controller.Scroller.Hadjustment.ValueChanged += OnScrolled;
				controller.Scroller.Vadjustment.ValueChanged += OnScrolled;
				controller.Scroller.EdgeOvershot += OnEdgeOvershot;
				controller.SourceChanged += OnSourceChanged;
				UpdateBounce();
				UpdateRowPadding();
			}
		}

		// GTK pads every list row (and descendant selectors of a per-widget provider do not reach the rows): a page must be exactly as large
		// as the viewport or the snap drifts by the padding of each page, so a display-wide rule keyed on a marker class removes it.
		internal void UpdateRowPadding()
		{
			EnsureCss();
			Controller?.List?.AddCssClass(CarouselClass);
		}

		static void EnsureCss()
		{
			if (s_cssInstalled || global::Gdk.Display.Default is not { } display)
				return;

			var provider = new global::Gtk.CssProvider();
			provider.LoadFromString(
				"listview." + CarouselClass + " > row { padding: 0; margin: 0; border: 0; min-height: 0; } " +
				"scrolledwindow." + NoBounceClass + " overshoot, scrolledwindow." + NoBounceClass + " undershoot { background-image: none; }");
#pragma warning disable CS0612 // StyleContext is deprecated since GTK 4.10 but is the only provider hook of a display in this binding.
			global::Gtk.StyleContext.AddProviderForDisplay(display, provider, (uint)global::Gtk.StyleProviderPriority.Application);
#pragma warning restore CS0612
			s_cssInstalled = true;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			CancelSnap();

			if (Controller is { } controller)
			{
				controller.Root.SizeAllocated -= OnRootAllocated;
				controller.Scroller.Hadjustment.ValueChanged -= OnScrolled;
				controller.Scroller.Vadjustment.ValueChanged -= OnScrolled;
				controller.Scroller.EdgeOvershot -= OnEdgeOvershot;
				controller.SourceChanged -= OnSourceChanged;
				controller.Dispose();
			}

			Controller = null;
			base.DisconnectHandler(platformView);
		}

		void OnRootAllocated(object? sender, EventArgs args)
		{
			int width = Controller?.Scroller.Width ?? 0;
			int height = Controller?.Scroller.Height ?? 0;
			if (width == _lastWidth && height == _lastHeight)
				return;

			_lastWidth = width;
			_lastHeight = height;

			// Pages are as large as the viewport: measure them again and keep the current page in view.
			if (Controller is { } controller)
			{
				foreach (var host in controller.BoundHosts)
					host.QueueResize();
				controller.List?.QueueResize();
			}

			ScrollToPosition();
		}

		void OnSourceChanged()
		{
			if (VirtualView is not { } carousel)
				return;

			int count = Controller?.Source?.Count ?? 0;
			if (count > 0 && carousel.Position >= count)
				SetPosition(count - 1);
		}

		void OnScrolled(object? sender, EventArgs args)
		{
			if (_updating)
				return;

			CancelSnap();
			_snapTimer = GLib.Timeout.Add(SnapDelayMilliseconds, () =>
			{
				_snapTimer = 0;
				Settle();
				return false;
			});
		}

		void OnEdgeOvershot(object? sender, global::Gtk.EdgeOvershotArgs args) => HandleOvershoot(args.Pos);

		internal void HandleOvershoot(global::Gtk.PositionType position)
		{
			bool horizontal = IsHorizontal;
			switch (position)
			{
				case global::Gtk.PositionType.Right when horizontal:
				case global::Gtk.PositionType.Bottom when !horizontal:
					WrapAtEdge(end: true);
					break;
				case global::Gtk.PositionType.Left when horizontal:
				case global::Gtk.PositionType.Top when !horizontal:
					WrapAtEdge(end: false);
					break;
			}
		}

		/// <summary>
		/// With <see cref="CarouselView.Loop"/>, jumps from the last page to the first one (<paramref name="end"/>) or from the first to the last
		/// when the user dragged past the edge.
		/// </summary>
		/// <param name="end">Whether the overshoot was at the end of the carousel.</param>
		/// <returns><see langword="true"/> when the carousel wrapped.</returns>
		internal bool WrapAtEdge(bool end)
		{
			if (VirtualView is not { Loop: true } || Controller?.Source is not { Count: > 1 } source)
				return false;

			// The kinetic overshoot keeps reporting while it settles.
			long now = Environment.TickCount64;
			if (now - _lastWrapTick < WrapCooldownMilliseconds)
				return false;

			int current = NearestPage();
			int target;
			if (end && current == source.Count - 1)
				target = 0;
			else if (!end && current == 0)
				target = source.Count - 1;
			else
				return false;

			_lastWrapTick = now;
			CancelSnap();
			ScrollTo(target);
			SetPosition(target);
			return true;
		}

		/// <summary>The page <paramref name="index"/> stands for: wrapped around with <see cref="CarouselView.Loop"/>, else clamped.</summary>
		internal int NormalizePage(int index, int count)
		{
			if (count <= 0)
				return 0;

			if (VirtualView is { Loop: true })
				return ((index % count) + count) % count;

			return Math.Max(0, Math.Min(index, count - 1));
		}

		/// <summary>Shows or hides GTK's overshoot effect at the edges according to <see cref="CarouselView.IsBounceEnabled"/>.</summary>
		internal void UpdateBounce()
		{
			if (Controller is null || VirtualView is not { } carousel)
				return;

			EnsureCss();
			if (carousel.IsBounceEnabled)
				Controller.Scroller.RemoveCssClass(NoBounceClass);
			else
				Controller.Scroller.AddCssClass(NoBounceClass);
		}

		void CancelSnap()
		{
			if (_snapTimer != 0)
			{
				GLib.Timeout.Remove(_snapTimer);
				_snapTimer = 0;
			}
		}

		/// <summary>The index of the page nearest to the scroll offset.</summary>
		internal int NearestPage()
		{
			int width = PageSize;
			if (width <= 0 || Controller is null)
				return 0;

			bool horizontal = IsHorizontal;
			double offset = horizontal ? Controller.Scroller.Hadjustment.Value : Controller.Scroller.Vadjustment.Value;
			int count = Controller.Source?.Count ?? 0;
			return Math.Max(0, Math.Min(count - 1, (int)Math.Round(offset / width)));
		}

		/// <summary>Snaps to the nearest page and publishes it as the carousel's position.</summary>
		internal void Settle()
		{
			if (Controller?.Source is null || Controller.Source.Count == 0)
				return;

			int index = NearestPage();
			ScrollTo(index);
			SetPosition(index);
		}

		void SetPosition(int index)
		{
			if (VirtualView is not { } carousel || carousel.Position == index)
				return;

			_updating = true;
			try
			{
				carousel.Position = index;

				// Not every Position change reaches CurrentItem on a platform-less build; keep the pair consistent.
				if (Controller?.Source is { } source && index >= 0 && index < source.Count)
					carousel.CurrentItem = source.RowAt(index).Data;
			}
			finally
			{
				_updating = false;
			}
		}

		void ScrollTo(int index)
		{
			if (Controller is null)
				return;

			int width = PageSize;
			bool horizontal = IsHorizontal;

			_updating = true;
			try
			{
				var adjustment = horizontal ? Controller.Scroller.Hadjustment : Controller.Scroller.Vadjustment;
				adjustment.Value = Math.Max(0, index) * width;
			}
			finally
			{
				_updating = false;
			}
		}

		/// <summary>Scrolls to <see cref="CarouselView.Position"/>.</summary>
		internal void ScrollToPosition()
		{
			if (VirtualView is not { } carousel || Controller?.Source is null || Controller.Source.Count == 0)
				return;

			CancelSnap();
			int index = NormalizePage(carousel.Position, Controller.Source.Count);
			ScrollTo(index);

			// A looping position past the end stands for a page at the start.
			if (index != carousel.Position)
				SetPosition(index);
		}

		internal void UpdateCurrentItem()
		{
			if (VirtualView is not { } carousel || Controller?.Source is null || carousel.CurrentItem is null)
				return;

			int index = Controller.Source.IndexOf(carousel.CurrentItem);
			if (index >= 0)
			{
				ScrollTo(index);
				SetPosition(index);
			}
		}

		internal void UpdateScrolling()
		{
			if (Controller is null || VirtualView is not { } carousel)
				return;

			bool horizontal = Controller.List?.Orientation != global::Gtk.Orientation.Vertical;
			var scrollable = carousel.IsSwipeEnabled ? global::Gtk.PolicyType.External : global::Gtk.PolicyType.Never;

			Controller.Scroller.HscrollbarPolicy = horizontal ? scrollable : global::Gtk.PolicyType.Never;
			Controller.Scroller.VscrollbarPolicy = horizontal ? global::Gtk.PolicyType.Never : scrollable;
			Controller.Scroller.KineticScrolling = carousel.IsSwipeEnabled;
		}

		internal static readonly PropertyMapper<CarouselView, CarouselViewHandler> ControlsMapper = CreateMapper();

		static PropertyMapper<CarouselView, CarouselViewHandler> CreateMapper()
		{
			static GtkCarouselViewHandler? H(CarouselViewHandler handler) => handler as GtkCarouselViewHandler;

			var mapper = new GtkMapperBuilder<CarouselView, CarouselViewHandler>(CarouselViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkBox>(nameof(ItemsView.ItemsSource), (h, v, w) => H(h)?.Controller?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ItemsView.ItemTemplate), (h, v, w) => H(h)?.Controller?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ItemsView.EmptyView), (h, v, w) => H(h)?.Controller?.UpdateEmptyView())
				.MapGtk<GtkBox>(nameof(ItemsView.EmptyViewTemplate), (h, v, w) => H(h)?.Controller?.UpdateEmptyView())
				.MapGtk<GtkBox>(nameof(CarouselView.ItemsLayout), (h, v, w) =>
				{
					H(h)?.Controller?.UpdateLayout();
					H(h)?.UpdateRowPadding();
					H(h)?.UpdateScrolling();
				})
				.MapGtk<GtkBox>(nameof(CarouselView.IsSwipeEnabled), (h, v, w) => H(h)?.UpdateScrolling())
				.MapGtk<GtkBox>(nameof(CarouselView.IsBounceEnabled), (h, v, w) => H(h)?.UpdateBounce())
				.MapGtk<GtkBox>(nameof(CarouselView.PeekAreaInsets), (h, v, w) =>
				{
					foreach (var host in H(h)?.Controller?.BoundHosts ?? Array.Empty<MauiGtkItemHost>())
						host.QueueResize();
					H(h)?.ScrollToPosition();
				})
				.MapGtk<GtkBox>(nameof(CarouselView.Position), (h, v, w) =>
				{
					if (H(h) is { _updating: false } handler)
						handler.ScrollToPosition();
				})
				.MapGtk<GtkBox>(nameof(CarouselView.CurrentItem), (h, v, w) =>
				{
					if (H(h) is { _updating: false } handler)
						handler.UpdateCurrentItem();
				})
				.Build();

			return (PropertyMapper<CarouselView, CarouselViewHandler>)mapper;
		}
	}
}
