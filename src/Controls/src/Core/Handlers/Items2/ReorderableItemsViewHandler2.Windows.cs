using System;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Windows-specific handler base for <see cref="ReorderableItemsView"/> in
/// CollectionView2 (Items2). Adds drag-and-drop reorder support on top of
/// <see cref="ItemsViewHandler2{TItemsView}"/> by wiring the platform
/// <see cref="MauiItemsView"/> to the virtual view's reorder pipeline.
/// </summary>
public partial class ReorderableItemsViewHandler2<TItemsView> : ItemsViewHandler2<ReorderableItemsView>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderableItemsViewHandler2{TItemsView}"/> class.
    /// </summary>
    /// <param name="mapper">An optional property mapper. When <c>null</c>, the default mapper is used.</param>
    public ReorderableItemsViewHandler2(PropertyMapper? mapper = null)
        : base(mapper)
    {
    }

    /// <inheritdoc />
    protected override IItemsLayout Layout => ItemsView.ItemsLayout;

    /// <summary>
    /// Property-mapper entry that forwards <see cref="ReorderableItemsView.CanReorderItems"/>
    /// changes from the virtual view to the platform <see cref="MauiItemsView"/>.
    /// </summary>
    public static void MapCanReorderItems(ReorderableItemsViewHandler2<TItemsView> handler, ReorderableItemsView itemsView)
    {
        if (handler.PlatformView is MauiItemsView mauiItemsView)
        {
            mauiItemsView.UpdateCanReorderItems(itemsView.CanReorderItems);
        }
    }

    /// <inheritdoc />
    protected override void ConnectHandler(WItemsView platformView)
    {
        base.ConnectHandler(platformView);

        if (PlatformView is MauiItemsView mauiItemsView)
        {
            mauiItemsView.AttachReorderableView(VirtualView);
            mauiItemsView.ReorderCompleted += OnReorderCompleted;
        }
    }

    /// <inheritdoc />
    protected override void DisconnectHandler(WItemsView platformView)
    {
        if (PlatformView is MauiItemsView mauiItemsView)
        {
            mauiItemsView.ReorderCompleted -= OnReorderCompleted;
            mauiItemsView.AttachReorderableView(null);
        }

        base.DisconnectHandler(platformView);
    }

    void OnReorderCompleted(object? sender, EventArgs e)
    {
        VirtualView?.SendReorderCompleted();
    }
}
