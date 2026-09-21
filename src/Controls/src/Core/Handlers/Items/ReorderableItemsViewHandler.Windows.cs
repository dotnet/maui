#nullable disable
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class ReorderableItemsViewHandler<TItemsView> : GroupableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
	{
		bool _trackerAllowDrop;

		protected override void ConnectHandler(ListViewBase platformView)
		{
			base.ConnectHandler(platformView);

			platformView.DragItemsStarting += HandleDragItemsStarting;
			platformView.DragItemsCompleted += HandleDragItemsCompleted;
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			platformView.DragItemsStarting -= HandleDragItemsStarting;
			platformView.DragItemsCompleted -= HandleDragItemsCompleted;

			base.DisconnectHandler(platformView);
		}

		void HandleDragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			// Built in reordering only supports ungrouped sources & observable collections.
			var supportsReorder = Element != null && !Element.IsGrouped && Element.ItemsSource is INotifyCollectionChanged;
			if (supportsReorder)
			{
				// The AllowDrop property needs to be enabled when we start the drag operation.
				// We can't simply enable it when we set CanReorderItems because the VisualElementTracker also updates this property.
				// That means the tracker can overwrite any set we do in UpdateCanReorderItems.
				// To avoid that possibility, let's force it to true when the user begins to drag an item.
				// Reset it back to what it was when finished.
				_trackerAllowDrop = ListViewBase.AllowDrop;
				ListViewBase.AllowDrop = true;
			}
			else
			{
				e.Cancel = true;
			}
		}

		void HandleDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			ListViewBase.AllowDrop = _trackerAllowDrop;

			Element?.SendReorderCompleted();
		}

		public static void MapCanReorderItems(ReorderableItemsViewHandler<TItemsView> handler, ReorderableItemsView itemsView)
		{
			handler.UpdateCanReorderItems();
		}

		void UpdateCanReorderItems()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			if (Element.CanReorderItems)
			{
				ListViewBase.CanDragItems = true;
				ListViewBase.CanReorderItems = true;
				ListViewBase.IsSwipeEnabled = true; // Needed so user can reorder with touch (according to docs).
			}
			else
			{
				ListViewBase.CanDragItems = false;
				ListViewBase.CanReorderItems = false;
				ListViewBase.IsSwipeEnabled = false;
			}
		}
	}
}