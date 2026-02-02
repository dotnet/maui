using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Windows-specific handler for ReorderableItemsView in Items2 (CollectionView2).
	/// </summary>
	public partial class ReorderableItemsViewHandler2<TItemsView> : ItemsViewHandler2<ReorderableItemsView>
	{
		public ReorderableItemsViewHandler2(PropertyMapper? mapper = null) : base(mapper)
		{
		}

		protected override void ConnectHandler(UI.Xaml.Controls.ItemsView platformView)
		{
			base.ConnectHandler(platformView);
			
			if (PlatformView is MauiItemsView mauiItemsView)
			{
				mauiItemsView.UpdateCanReorderItems(VirtualView.CanReorderItems);
				mauiItemsView.ReorderCompleted += OnReorderCompleted;
			}
		}

		protected override void DisconnectHandler(UI.Xaml.Controls.ItemsView platformView)
		{
			if (PlatformView is MauiItemsView mauiItemsView)
			{
				mauiItemsView.ReorderCompleted -= OnReorderCompleted;
			}
			
			base.DisconnectHandler(platformView);
		}

		void OnReorderCompleted(object? sender, System.EventArgs e)
		{
			VirtualView?.SendReorderCompleted();
		}

		protected override IItemsLayout Layout { get => ItemsView.ItemsLayout; }
	}
}
