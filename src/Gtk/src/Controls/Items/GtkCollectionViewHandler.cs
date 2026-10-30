using System;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Gtk.Hosting;
using GtkBox = Gtk.Box;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>
	/// The handler of <see cref="CollectionView"/> on GTK 4: a <c>Gtk.ListView</c> (linear layouts) or <c>Gtk.GridView</c>
	/// (<see cref="GridItemsLayout"/>) over a C# <c>GListModel</c>, with native virtualisation.
	/// </summary>
	/// <remarks>
	/// <para>Supported: items source (observable collections update the rows), item template and template selector, linear and grid
	/// layouts (orientation, span, item spacing), header and footer (above and below the scrolling area), empty view, none/single/multiple
	/// selection, grouping with group header and footer templates (linear layouts), scroll bar visibility and
	/// <see cref="ItemsView.ScrollTo(int, int, ScrollToPosition, bool)"/> (the row is made visible; the position hint is ignored).</para>
	/// <para>Not supported: reordering, item sizing strategy (rows are always measured by their content), scroll events,
	/// <c>RemainingItemsThreshold</c>.</para>
	/// </remarks>
	public class GtkCollectionViewHandler : CollectionViewHandler
	{
		/// <summary>Creates the handler with the Controls mapper.</summary>
		public GtkCollectionViewHandler()
			: base(ControlsMapper)
		{
		}

		/// <summary>Creates the handler with a custom mapper.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		public GtkCollectionViewHandler(PropertyMapper? mapper)
			: base(mapper ?? ControlsMapper)
		{
		}

		internal GtkItemsController? Controller { get; private set; }

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var context = MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
			Controller = new GtkItemsController(VirtualView, context);
			return Controller.Root;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			VirtualView.ScrollToRequested += OnScrollToRequested;
			Controller?.Refresh();
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			VirtualView.ScrollToRequested -= OnScrollToRequested;
			Controller?.Dispose();
			Controller = null;
			base.DisconnectHandler(platformView);
		}

		void OnScrollToRequested(object? sender, ScrollToRequestEventArgs args) => Controller?.ScrollTo(args);

		internal static readonly PropertyMapper<CollectionView, CollectionViewHandler> ControlsMapper = CreateMapper();

		static PropertyMapper<CollectionView, CollectionViewHandler> CreateMapper()
		{
			static GtkItemsController? C(CollectionViewHandler handler) => (handler as GtkCollectionViewHandler)?.Controller;

			var mapper = new GtkMapperBuilder<CollectionView, CollectionViewHandler>(CollectionViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<GtkBox>(nameof(ItemsView.ItemsSource), (h, v, w) => C(h)?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(ItemsView.ItemTemplate), (h, v, w) => C(h)?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(GroupableItemsView.IsGrouped), (h, v, w) => C(h)?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(GroupableItemsView.GroupHeaderTemplate), (h, v, w) => C(h)?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(GroupableItemsView.GroupFooterTemplate), (h, v, w) => C(h)?.UpdateItemsSource())
				.MapGtk<GtkBox>(nameof(StructuredItemsView.ItemsLayout), (h, v, w) =>
				{
					C(h)?.UpdateLayout();
					C(h)?.UpdateItemsSource();
				})
				.MapGtk<GtkBox>(nameof(ItemsView.EmptyView), (h, v, w) => C(h)?.UpdateEmptyView())
				.MapGtk<GtkBox>(nameof(ItemsView.EmptyViewTemplate), (h, v, w) => C(h)?.UpdateEmptyView())
				.MapGtk<GtkBox>(nameof(StructuredItemsView.Header), (h, v, w) => C(h)?.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(StructuredItemsView.HeaderTemplate), (h, v, w) => C(h)?.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(StructuredItemsView.Footer), (h, v, w) => C(h)?.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(StructuredItemsView.FooterTemplate), (h, v, w) => C(h)?.UpdateHeaderAndFooter())
				.MapGtk<GtkBox>(nameof(SelectableItemsView.SelectionMode), (h, v, w) => C(h)?.UpdateSelectionMode())
				.MapGtk<GtkBox>(nameof(SelectableItemsView.SelectedItem), (h, v, w) => C(h)?.UpdateSelectionFromView())
				.MapGtk<GtkBox>(nameof(SelectableItemsView.SelectedItems), (h, v, w) => C(h)?.UpdateSelectionFromView())
				.MapGtk<GtkBox>(nameof(ItemsView.HorizontalScrollBarVisibility), (h, v, w) => C(h)?.UpdateScrollBars())
				.MapGtk<GtkBox>(nameof(ItemsView.VerticalScrollBarVisibility), (h, v, w) => C(h)?.UpdateScrollBars())
				.Build();

			return (PropertyMapper<CollectionView, CollectionViewHandler>)mapper;
		}
	}
}
