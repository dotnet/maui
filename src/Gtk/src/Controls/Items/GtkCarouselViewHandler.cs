using System;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Gtk.Hosting;
using GtkBox = Gtk.Box;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>
	/// The handler of <see cref="CarouselView"/> on GTK 4: a horizontal (or vertical) <c>Gtk.ListView</c> whose rows are as wide as the
	/// viewport (minus the peek area), that snaps to the nearest item when scrolling settles.
	/// </summary>
	/// <remarks>
	/// <para>The scroll position and <see cref="CarouselView.Position"/>/<see cref="CarouselView.CurrentItem"/> follow each other; scrolling
	/// by the app is instant (<c>IsScrollAnimated</c> is ignored). No libadwaita dependency: the snap is a short timer after the last scroll
	/// change.</para>
	/// <para>Not supported: <see cref="CarouselView.Loop"/> (the carousel stops at both ends) and bounce.</para>
	/// </remarks>
	public class GtkCarouselViewHandler : CarouselViewHandler
	{
		const uint SnapDelayMilliseconds = 120;

		uint _snapTimer;
		bool _updating;
		int _lastWidth;

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

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var context = MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
			Controller = new GtkItemsController(VirtualView, context) { FixedRowWidth = () => PageWidth };
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
				controller.SourceChanged += OnSourceChanged;
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			CancelSnap();

			if (Controller is { } controller)
			{
				controller.Root.SizeAllocated -= OnRootAllocated;
				controller.Scroller.Hadjustment.ValueChanged -= OnScrolled;
				controller.SourceChanged -= OnSourceChanged;
				controller.Dispose();
			}

			Controller = null;
			base.DisconnectHandler(platformView);
		}

		void OnRootAllocated(object? sender, EventArgs args)
		{
			int width = Controller?.Scroller.Width ?? 0;
			if (width == _lastWidth)
				return;

			_lastWidth = width;

			// Pages are as wide as the viewport: measure them again and keep the current page in view.
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
			int width = PageWidth;
			if (width <= 0 || Controller is null)
				return 0;

			bool horizontal = Controller.List?.Orientation != global::Gtk.Orientation.Vertical;
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

			int width = PageWidth;
			bool horizontal = Controller.List?.Orientation != global::Gtk.Orientation.Vertical;

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
			ScrollTo(Math.Max(0, Math.Min(carousel.Position, Controller.Source.Count - 1)));
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
					H(h)?.UpdateScrolling();
				})
				.MapGtk<GtkBox>(nameof(CarouselView.IsSwipeEnabled), (h, v, w) => H(h)?.UpdateScrolling())
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
