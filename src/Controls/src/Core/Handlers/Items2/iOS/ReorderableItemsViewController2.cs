#nullable disable
using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class ReorderableItemsViewController2<TItemsView> : GroupableItemsViewController2<TItemsView>
		where TItemsView : ReorderableItemsView
	{
		bool _disposed;
		UILongPressGestureRecognizer _longPressGestureRecognizer;

#if MACCATALYST
		const double defaultMacCatalystPressDuration = 0.1;
#endif

		public ReorderableItemsViewController2(TItemsView reorderableItemsView, UICollectionViewLayout layout)
			: base(reorderableItemsView, layout)
		{
			// The UICollectionViewController has built-in recognizer for reorder that can be installed by setting "InstallsStandardGestureForInteractiveMovement".
			// For some reason it only seemed to work when the CollectionView was inside the Flyout section of a FlyoutPage.
			// The UILongPressGestureRecognizer is simple enough to set up so let's just add our own.
			InstallsStandardGestureForInteractiveMovement = false;
#if MACCATALYST
			// On Mac Catalyst, the default normal press and drag interactions occur, causing the CanMixGroups = false logic to not work. 
			// Since all reordering logic is handled exclusively by UILongPressGestureRecognizer, we can set DragInteractionEnabled to false, ensuring that only the long press gesture is used.
			CollectionView.DragInteractionEnabled = false;
#endif
		}

		public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return ItemsView?.CanReorderItems == true;
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new ReorderableItemsViewDelegator2<TItemsView, ReorderableItemsViewController2<TItemsView>>(ItemsViewLayout, this);
		}

		protected override Items.IItemsViewSource CreateItemsViewSource()
		{
			// There's a bug in the current Maui Controls library.
			// It will call "CreateItemsViewSource" 2x in a row when opening a page.
			// It's invoked from both ViewDidLoad & UpdateItemsSource
			// For the time being, until the issue is fixed, we need to dispose of the current source if one already exist.
			ItemsSource?.Dispose();
			return base.CreateItemsViewSource();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (_longPressGestureRecognizer != null)
				{
					CollectionView.RemoveGestureRecognizer(_longPressGestureRecognizer);
					_longPressGestureRecognizer.Dispose();
					_longPressGestureRecognizer = null;
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		void HandleLongPress(UILongPressGestureRecognizer gestureRecognizer)
		{
			var collectionView = CollectionView;
			if (collectionView == null)
				return;

			var location = gestureRecognizer.LocationInView(collectionView);

			// We are updating "CancelsTouchesInView" so views can still receive touch events when this gesture runs.
			// Those events shouldn't be aborted until they've actually moved the position of the CollectionView item.
			switch (gestureRecognizer.State)
			{
				case UIGestureRecognizerState.Began:
					var indexPath = collectionView?.IndexPathForItemAtPoint(location);
					if (indexPath == null)
					{
						return;
					}
					gestureRecognizer.CancelsTouchesInView = false;
					collectionView.BeginInteractiveMovementForItem(indexPath);
					break;
				case UIGestureRecognizerState.Changed:
					gestureRecognizer.CancelsTouchesInView = true;
					collectionView.UpdateInteractiveMovement(location);
					break;
				case UIGestureRecognizerState.Ended:
					collectionView.EndInteractiveMovement();
					break;
				default:
					collectionView.CancelInteractiveMovement();
					break;
			}
		}

		public override void MoveItem(UICollectionView collectionView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
		{
			var itemsSource = ItemsSource;
			var itemsView = ItemsView;

			if (itemsSource == null || itemsView == null)
			{
				return;
			}

			if (itemsView.IsGrouped)
			{
				var fromList = itemsSource.Group(sourceIndexPath) as IList;
				var fromItemsSource = fromList is INotifyCollectionChanged ? itemsSource.GroupItemsViewSource(sourceIndexPath) : null;
				var fromItemIndex = sourceIndexPath.Row;

				var toList = itemsSource.Group(destinationIndexPath) as IList;
				var toItemsSource = toList is INotifyCollectionChanged ? itemsSource.GroupItemsViewSource(destinationIndexPath) : null;
				var toItemIndex = destinationIndexPath.Row;

				if (fromList != null && toList != null)
				{
					var fromItem = fromList[fromItemIndex];
					SetObserveChanges(fromItemsSource, false);
					SetObserveChanges(toItemsSource, false);
					fromList.RemoveAt(fromItemIndex);
					toList.Insert(toItemIndex, fromItem);
					SetObserveChanges(fromItemsSource, true);
					SetObserveChanges(toItemsSource, true);
					itemsView.SendReorderCompleted();
				}
			}
			else if (itemsView.ItemsSource is IList list)
			{
				var fromPosition = sourceIndexPath.Row;
				var toPosition = destinationIndexPath.Row;
				var fromItem = list[fromPosition];
				SetObserveChanges(itemsSource, false);
				list.RemoveAt(fromPosition);
				list.Insert(toPosition, fromItem);
				SetObserveChanges(itemsSource, true);
				itemsView.SendReorderCompleted();
			}
		}

		void SetObserveChanges(Items.IItemsViewSource itemsSource, bool enable)
		{
			if (itemsSource is Items.IObservableItemsViewSource observableSource)
			{
				observableSource.ObserveChanges = enable;
			}
		}

		public void UpdateCanReorderItems()
		{
			if (ItemsView.CanReorderItems)
			{
				if (_longPressGestureRecognizer == null)
				{
					_longPressGestureRecognizer = new UILongPressGestureRecognizer(HandleLongPress);
#if MACCATALYST
					// On Mac Catalyst, we disable the default drag interaction and instead handle dragging using a long press gesture recognizer.
					// Since a long press typically takes more time to trigger than the system's default drag interaction, 
					// we reduce the minimum press duration to 0.1 seconds to better match the previous behavior.
					_longPressGestureRecognizer.MinimumPressDuration = defaultMacCatalystPressDuration;
#endif
					CollectionView.AddGestureRecognizer(_longPressGestureRecognizer);
				}
			}
			else
			{
				if (_longPressGestureRecognizer != null)
				{
					CollectionView.RemoveGestureRecognizer(_longPressGestureRecognizer);
					_longPressGestureRecognizer.Dispose();
					_longPressGestureRecognizer = null;
				}
			}
		}
	}
}