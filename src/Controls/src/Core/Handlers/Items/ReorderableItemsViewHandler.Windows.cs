#nullable disable
using System;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using WDataTransfer = Windows.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class ReorderableItemsViewHandler<TItemsView> : GroupableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
	{
		// Drag state
		bool _trackerAllowDrop;
		object _draggedItem;
		SelectorItem _sourceContainer;
		int _insertionIndex = -1;
		bool _insertAfter;
		bool _reorderPerformed;

		// Drop target indicator — the container currently highlighted as the insertion target.
		SelectorItem _dropTargetContainer;
		bool _dropTargetIsAfter;

		// Dim opacity applied to non-source containers during a drag to signal "reorder mode".
		const double DragDimOpacity = 0.4;

		protected override void ConnectHandler(ListViewBase platformView)
		{
			base.ConnectHandler(platformView);

			platformView.DragItemsStarting += HandleDragItemsStarting;
			platformView.DragItemsCompleted += HandleDragItemsCompleted;
			platformView.DragOver += HandleListViewDragOver;
			platformView.Drop += HandleListViewDrop;
			platformView.DragLeave += HandleListViewDragLeave;
			platformView.ContainerContentChanging += HandleContainerContentChanging;
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			platformView.DragItemsStarting -= HandleDragItemsStarting;
			platformView.DragItemsCompleted -= HandleDragItemsCompleted;
			platformView.DragOver -= HandleListViewDragOver;
			platformView.Drop -= HandleListViewDrop;
			platformView.DragLeave -= HandleListViewDragLeave;
			platformView.ContainerContentChanging -= HandleContainerContentChanging;

			CleanupDragState();

			base.DisconnectHandler(platformView);
		}

		// ──────────────────────────────────────────────────────────────────────
		// CanReorderItems mapping
		// ──────────────────────────────────────────────────────────────────────

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
				// CanReorderItems = false: we implement the drop-target-indicator approach
				// ourselves so the native "live shuffle" ghost is suppressed. The native
				// reorder also shows the entire ListViewItem container (padding + chrome)
				// as the ghost, whereas our implementation shows only the item content.
				ListViewBase.CanReorderItems = false;
				ListViewBase.IsSwipeEnabled = true; // Needed so user can reorder with touch.
			}
			else
			{
				ListViewBase.CanDragItems = false;
				ListViewBase.CanReorderItems = false;
				ListViewBase.IsSwipeEnabled = false;
			}
		}

		// ──────────────────────────────────────────────────────────────────────
		// Per-container DragStarting wiring via ContainerContentChanging
		// ──────────────────────────────────────────────────────────────────────

		void HandleContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.ItemContainer is SelectorItem si)
			{
				// Always unsub first to avoid double-registration on recycled containers.
				si.DragStarting -= OnItemContainerDragStarting;

				if (Element?.CanReorderItems == true)
				{
					si.DragStarting += OnItemContainerDragStarting;
				}
			}
		}

		void OnItemContainerDragStarting(UIElement sender, DragStartingEventArgs args)
		{
			var container = (SelectorItem)sender;

			// Record the source container so DragItemsStarting can reference it.
			_sourceContainer = container;

			// Apply card appearance SYNCHRONOUSLY — WinUI captures the drag ghost
			// immediately after DragStarting returns, so this gives the ghost a solid
			// card background and shadow instead of a transparent/full-chrome container.
			ApplyDragGhostAppearance(container);

			// Deferred: remove ghost styling (no longer needed on live container),
			// hide the source slot, and dim the remaining containers.
			ListViewBase.DispatcherQueue.TryEnqueue(
				Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
				{
					RemoveDragGhostAppearance(container);

					if (_sourceContainer is not null)
					{
						_sourceContainer.Opacity = 0;
						_sourceContainer.IsHitTestVisible = false;
					}

					DimNonSourceContainers();
				});
		}

		// ──────────────────────────────────────────────────────────────────────
		// DragItemsStarting / DragItemsCompleted
		// ──────────────────────────────────────────────────────────────────────

		void HandleDragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			// Reordering only supported for ungrouped INotifyCollectionChanged sources.
			var supportsReorder = Element != null
				&& !Element.IsGrouped
				&& Element.ItemsSource is INotifyCollectionChanged;

			if (!supportsReorder || e.Items.Count == 0)
			{
				e.Cancel = true;
				return;
			}

			// The AllowDrop property needs to be enabled so our Drop handler fires.
			// The VisualElementTracker may overwrite AllowDrop, so we force it true
			// here and restore it when the drag ends (same pattern as before).
			_trackerAllowDrop = ListViewBase.AllowDrop;
			ListViewBase.AllowDrop = true;

			var rawItem = e.Items[0];
			// Unwrap ItemTemplateContext wrappers so _draggedItem matches the original source items.
			_draggedItem = rawItem is ItemTemplateContext itc ? itc.Item : rawItem;
			_reorderPerformed = false;

			// Wire DropCompleted for source opacity restoration (fires even on cancel/escape).
			if (_sourceContainer is not null)
			{
				_sourceContainer.DropCompleted -= OnSourceContainerDropCompleted;
				_sourceContainer.DropCompleted += OnSourceContainerDropCompleted;
			}

			e.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;
		}

		void OnSourceContainerDropCompleted(UIElement sender, DropCompletedEventArgs args)
		{
			if (sender is SelectorItem si)
			{
				si.DropCompleted -= OnSourceContainerDropCompleted;
				si.Opacity = 1;
				si.IsHitTestVisible = true;
			}

			RestoreAllContainersOpacity();
			_sourceContainer = null;
		}

		void HandleDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			ListViewBase.AllowDrop = _trackerAllowDrop;

			if (_reorderPerformed)
			{
				Element?.SendReorderCompleted();
			}

			CleanupDragState();
		}

		// ──────────────────────────────────────────────────────────────────────
		// DragOver / Drop / DragLeave
		// ──────────────────────────────────────────────────────────────────────

		void HandleListViewDragOver(object sender, DragEventArgs e)
		{
			if (_draggedItem is null || Element?.CanReorderItems != true)
			{
				e.AcceptedOperation = WDataTransfer.DataPackageOperation.None;
				return;
			}

			e.DragUIOverride.IsGlyphVisible = false;
			e.DragUIOverride.IsCaptionVisible = false;
			e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;

			var target = FindContainerUnderPointer(e);
			if (target is null)
				return;

			var pt = e.GetPosition(target);
			int targetIndex = GetContainerFlatIndex(target);
			if (targetIndex < 0)
				return;

			bool isHorizontal = IsHorizontalLayout();
			_insertAfter = isHorizontal
				? pt.X >= target.ActualWidth / 2
				: pt.Y >= target.ActualHeight / 2;

			_insertionIndex = _insertAfter ? targetIndex + 1 : targetIndex;

			UpdateDropTargetIndicator(target, _insertAfter);
			e.Handled = true;
		}

		void HandleListViewDrop(object sender, DragEventArgs e)
		{
			if (_draggedItem is null || _insertionIndex < 0)
			{
				CleanupDragState();
				return;
			}

			if (Element?.ItemsSource is IList itemsList)
			{
				_reorderPerformed = PerformReorder(itemsList);
			}

			ClearDropTargetIndicator();
			e.Handled = true;
		}

		void HandleListViewDragLeave(object sender, DragEventArgs e)
		{
			ClearDropTargetIndicator();
		}

		// ──────────────────────────────────────────────────────────────────────
		// Reorder logic
		// ──────────────────────────────────────────────────────────────────────

		bool PerformReorder(IList itemsList)
		{
			if (_draggedItem is null)
				return false;

			int oldIndex = -1;
			for (int i = 0; i < itemsList.Count; i++)
			{
				var current = itemsList[i];
				if (ReferenceEquals(current, _draggedItem) || Equals(current, _draggedItem))
				{
					oldIndex = i;
					break;
				}
			}

			if (oldIndex < 0)
				return false;

			int adjustedIndex = _insertionIndex;
			if (oldIndex < adjustedIndex)
				adjustedIndex--;

			if (oldIndex == adjustedIndex)
				return false;

			var item = itemsList[oldIndex];
			itemsList.RemoveAt(oldIndex);
			adjustedIndex = Math.Clamp(adjustedIndex, 0, itemsList.Count);
			itemsList.Insert(adjustedIndex, item);
			return true;
		}

		// ──────────────────────────────────────────────────────────────────────
		// Container helpers
		// ──────────────────────────────────────────────────────────────────────

		SelectorItem FindContainerUnderPointer(DragEventArgs e)
		{
			var position = e.GetPosition(ListViewBase);
			var hostPosition = ListViewBase.TransformToVisual(null)
				.TransformPoint(position);

			var elements = VisualTreeHelper.FindElementsInHostCoordinates(
				hostPosition, ListViewBase, false);

			foreach (var el in elements)
			{
				if (el is SelectorItem si
					&& !ReferenceEquals(si, _sourceContainer)
					&& GetContainerFlatIndex(si) >= 0)
				{
					return si;
				}
			}

			// Fallback: linear scan (handles cases where FindElementsInHostCoordinates
			// returns nothing because the pointer is over the source container's invisible slot).
			if (CollectionViewSource?.View is not null)
			{
				SelectorItem lastVisible = null;
				for (int i = 0; i < CollectionViewSource.View.Count; i++)
				{
					if (ListViewBase.ContainerFromIndex(i) is not SelectorItem candidate
						|| ReferenceEquals(candidate, _sourceContainer))
					{
						continue;
					}

					var containerPos = candidate.TransformToVisual(ListViewBase)
						.TransformPoint(new Windows.Foundation.Point(0, 0));

					bool isInBounds = IsHorizontalLayout()
						? position.X >= containerPos.X && position.X <= containerPos.X + candidate.ActualWidth
						: position.Y >= containerPos.Y && position.Y <= containerPos.Y + candidate.ActualHeight;

					if (isInBounds)
						return candidate;

					lastVisible = candidate;
				}

				// If pointer is past the last item, snap to it.
				if (lastVisible is not null)
				{
					var lastPos = lastVisible.TransformToVisual(ListViewBase)
						.TransformPoint(new Windows.Foundation.Point(0, 0));

					bool isBeyondLast = IsHorizontalLayout()
						? position.X > lastPos.X + lastVisible.ActualWidth
						: position.Y > lastPos.Y + lastVisible.ActualHeight;

					if (isBeyondLast)
						return lastVisible;
				}
			}

			return null;
		}

		int GetContainerFlatIndex(SelectorItem container)
		{
			return ListViewBase.IndexFromContainer(container);
		}

		bool IsHorizontalLayout()
		{
			return Layout is LinearItemsLayout lin
					   && lin.Orientation == ItemsLayoutOrientation.Horizontal
				   || Layout is GridItemsLayout grid
					   && grid.Orientation == ItemsLayoutOrientation.Horizontal;
		}

		// ──────────────────────────────────────────────────────────────────────
		// Drop target indicator
		// ──────────────────────────────────────────────────────────────────────

		void UpdateDropTargetIndicator(SelectorItem target, bool insertAfter)
		{
			if (ReferenceEquals(target, _dropTargetContainer) && insertAfter == _dropTargetIsAfter)
				return;

			ClearDropTargetIndicator();

			if (ReferenceEquals(target, _sourceContainer))
				return;

			_dropTargetContainer = target;
			_dropTargetIsAfter = insertAfter;

			const double LineThickness = 2.0;
			target.BorderBrush = TryGetAccentBrush();

			if (IsHorizontalLayout())
			{
				target.BorderThickness = insertAfter
					? new Thickness(0, 0, LineThickness, 0)  // right edge
					: new Thickness(LineThickness, 0, 0, 0); // left edge
			}
			else
			{
				target.BorderThickness = insertAfter
					? new Thickness(0, 0, 0, LineThickness)  // bottom edge
					: new Thickness(0, LineThickness, 0, 0); // top edge
			}
		}

		void ClearDropTargetIndicator()
		{
			if (_dropTargetContainer is not null)
			{
				_dropTargetContainer.BorderBrush = null;
				_dropTargetContainer.BorderThickness = default;
				_dropTargetContainer = null;
			}
		}

		static Brush TryGetAccentBrush()
		{
			try
			{
				if (Microsoft.UI.Xaml.Application.Current.Resources
						.TryGetValue("SystemAccentColor", out var raw)
					&& raw is Windows.UI.Color accent)
				{
					return new SolidColorBrush(accent);
				}
			}
			catch { }

			return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212));
		}

		// ──────────────────────────────────────────────────────────────────────
		// Ghost appearance (applied before WinUI captures the drag snapshot)
		// ──────────────────────────────────────────────────────────────────────

		static void ApplyDragGhostAppearance(SelectorItem container)
		{
			Brush cardBrush;
			if (Microsoft.UI.Xaml.Application.Current.Resources
					.TryGetValue("LayerFillColorDefaultBrush", out var raw)
				&& raw is Brush b)
			{
				cardBrush = b;
			}
			else
			{
				cardBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
			}

			container.Background = cardBrush;
			container.Shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
			container.Translation = new System.Numerics.Vector3(0, 0, 8);
		}

		static void RemoveDragGhostAppearance(SelectorItem container)
		{
			container.Background = null;
			container.Shadow = null;
			container.Translation = System.Numerics.Vector3.Zero;
		}

		// ──────────────────────────────────────────────────────────────────────
		// Dim / restore
		// ──────────────────────────────────────────────────────────────────────

		void DimNonSourceContainers()
		{
			if (_draggedItem is null || CollectionViewSource?.View is null)
				return;

			for (int i = 0; i < CollectionViewSource.View.Count; i++)
			{
				if (ListViewBase.ContainerFromIndex(i) is SelectorItem si
					&& !ReferenceEquals(si, _sourceContainer))
				{
					si.Opacity = DragDimOpacity;
				}
			}
		}

		void RestoreAllContainersOpacity()
		{
			if (CollectionViewSource?.View is null)
				return;

			for (int i = 0; i < CollectionViewSource.View.Count; i++)
			{
				if (ListViewBase.ContainerFromIndex(i) is SelectorItem si)
				{
					si.Opacity = 1;
					si.IsHitTestVisible = true;
				}
			}
		}

		// ──────────────────────────────────────────────────────────────────────
		// Cleanup
		// ──────────────────────────────────────────────────────────────────────

		void CleanupDragState()
		{
			ClearDropTargetIndicator();

			if (_sourceContainer is not null)
			{
				RemoveDragGhostAppearance(_sourceContainer);
				_sourceContainer.Opacity = 1;
				_sourceContainer.IsHitTestVisible = true;
				_sourceContainer.DropCompleted -= OnSourceContainerDropCompleted;
				_sourceContainer = null;
			}

			RestoreAllContainersOpacity();

			_draggedItem = null;
			_insertionIndex = -1;
			_insertAfter = false;
			_reorderPerformed = false;
		}
	}
}