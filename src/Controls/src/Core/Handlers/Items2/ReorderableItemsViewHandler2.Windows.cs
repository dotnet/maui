using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Windows-specific handler for <see cref="ReorderableItemsView"/> in Items2 (CollectionView2).
/// Owns drag-and-drop wiring on top of <see cref="ItemsViewHandler2{TItemsView}"/> so that any
/// CV2-derived handler (e.g. <see cref="CollectionViewHandler2"/>) inherits reorder support.
/// </summary>
public partial class ReorderableItemsViewHandler2<TItemsView> : ItemsViewHandler2<ReorderableItemsView>
	where TItemsView : ReorderableItemsView
{
	public ReorderableItemsViewHandler2() : base(ReorderableItemsViewMapper)
	{
	}

	public ReorderableItemsViewHandler2(PropertyMapper? mapper = null) : base(mapper ?? ReorderableItemsViewMapper)
	{
	}

	public static PropertyMapper<TItemsView, ReorderableItemsViewHandler2<TItemsView>> ReorderableItemsViewMapper = new(ItemsViewMapper)
	{
		[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems,
	};

	protected override IItemsLayout Layout { get => ItemsView.ItemsLayout; }

	/// <summary>
	/// Maps <see cref="ReorderableItemsView.CanReorderItems"/> to the platform view, enabling or
	/// disabling drag/drop reordering on the underlying <see cref="MauiItemsView"/>.
	/// </summary>
	public static void MapCanReorderItems(ReorderableItemsViewHandler2<TItemsView> handler, ReorderableItemsView itemsView)
	{
		if (handler.PlatformView is MauiItemsView mauiItemsView)
		{
			mauiItemsView.SetMauiVirtualView(itemsView);
			mauiItemsView.UpdateCanReorderItems(itemsView.CanReorderItems);
		}
	}

	protected override void ConnectHandler(WItemsView platformView)
	{
		base.ConnectHandler(platformView);

		if (platformView is MauiItemsView mauiItemsView)
		{
			mauiItemsView.SetMauiVirtualView(VirtualView);
			mauiItemsView.ReorderCompleted -= OnReorderCompleted;
			mauiItemsView.ReorderCompleted += OnReorderCompleted;
			mauiItemsView.UpdateCanReorderItems(VirtualView.CanReorderItems);
		}
	}

	protected override void DisconnectHandler(WItemsView platformView)
	{
		if (platformView is MauiItemsView mauiItemsView)
		{
			mauiItemsView.ReorderCompleted -= OnReorderCompleted;
			mauiItemsView.DisconnectDragDrop();
		}

		base.DisconnectHandler(platformView);
	}

	void OnReorderCompleted(object? sender, EventArgs e)
	{
		VirtualView?.SendReorderCompleted();
	}
}
