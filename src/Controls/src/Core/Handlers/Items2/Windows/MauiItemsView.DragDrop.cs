using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WDataTransfer = Windows.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Partial class containing drag and drop implementation for MauiItemsView.
	/// </summary>
	internal partial class MauiItemsView
	{
		// Drag and drop fields
		object? _draggedItem;
		int _insertionIndex = -1;
		bool _insertAfter = false;
		bool _canReorderItems = true;

		// Auto-scroll fields - Smooth scrolling based on drag position
		Microsoft.UI.Dispatching.DispatcherQueueTimer? _autoScrollTimer;
		double _targetScrollVelocity;
		double _currentScrollVelocity;
		const double AutoScrollThreshold = 60.0;
		const double AutoScrollMaxSpeed = 25.0;  // Increased from 15
		const double AutoScrollMinSpeed = 3.0;   // Increased from 2
		const double ScrollAcceleration = 0.3;

		/// <summary>
		/// Event fired when a reorder operation completes successfully.
		/// </summary>
		public event EventHandler? ReorderCompleted;

		/// <summary>
		/// Updates drag and drop capabilities for reordering items.
		/// </summary>
		/// <param name="canReorderItems">Whether items can be reordered by the user</param>
		public void UpdateCanReorderItems(bool canReorderItems)
		{
			_canReorderItems = canReorderItems;
			
			if (_scrollView is not null)
			{
				if (canReorderItems)
				{
					WireUpDragDropEvents();
				}
				else
				{
					UnwireDragDropEvents();
				}
			}
		}

		
		#region Drag and Drop Event Wiring

		void WireUpDragDropEvents()
		{
			if (_scrollView is null)
				return;

			_scrollView.AllowDrop = true;
			_scrollView.DragEnter -= ScrollView_DragEnter;
			_scrollView.DragOver -= ScrollView_DragOver;
			_scrollView.DragLeave -= ScrollView_DragLeave;
			_scrollView.Drop -= ScrollView_Drop;

			_scrollView.DragEnter += ScrollView_DragEnter;
			_scrollView.DragOver += ScrollView_DragOver;
			_scrollView.DragLeave += ScrollView_DragLeave;
			_scrollView.Drop += ScrollView_Drop;

			if (_itemsRepeater is not null)
			{
				_itemsRepeater.ElementPrepared += ItemsRepeater_ElementPrepared;
			}
		}

		void UnwireDragDropEvents()
		{
			if (_scrollView is not null)
			{
				_scrollView.AllowDrop = false;
				_scrollView.DragEnter -= ScrollView_DragEnter;
				_scrollView.DragOver -= ScrollView_DragOver;
				_scrollView.DragLeave -= ScrollView_DragLeave;
				_scrollView.Drop -= ScrollView_Drop;
			}

			if (_itemsRepeater is not null)
			{
				_itemsRepeater.ElementPrepared -= ItemsRepeater_ElementPrepared;
				_itemsRepeater.ElementClearing -= ItemsRepeater_ElementClearing;
			}

			StopAutoScroll();
		}

		#endregion

		#region ItemsRepeater Element Management

		void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
		{
			if (!_canReorderItems)
				return;
			
			if (args.Element is ItemContainer itemContainer)
			{
				itemContainer.Tag = args.Index;
				itemContainer.CanDrag = true;
				itemContainer.DragStarting -= ItemContainer_DragStarting;
				itemContainer.DragStarting += ItemContainer_DragStarting;
			}
		}
		
		void ItemsRepeater_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
		{
			if (args.Element is ItemContainer itemContainer)
			{
				itemContainer.CanDrag = false;
				itemContainer.DragStarting -= ItemContainer_DragStarting;
				itemContainer.Tag = null;
			}
		}

		#endregion

		#region Drag Event Handlers

		void ItemContainer_DragStarting(UIElement sender, UI.Xaml.DragStartingEventArgs args)
		{
			var itemContainer = (ItemContainer)sender;
			
			object? item = null;
			if (itemContainer.Tag is int index && index >= 0 && ItemsSource is IList itemsList && index < itemsList.Count)
			{
				item = GetItemAtIndex(index, itemsList);
			}
			
			if (item == null || ItemsSource == null)
			{
				args.Cancel = true;
				return;
			}
			
			_draggedItem = item;
			args.Data.Properties.Add("DragSource", "MauiItemsView");
			args.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;
		}
		
		void ScrollView_DragEnter(object sender, UI.Xaml.DragEventArgs e)
		{
			if (!_canReorderItems)
			{
				e.AcceptedOperation = WDataTransfer.DataPackageOperation.None;
				return;
			}
			
			e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
			e.DragUIOverride.IsGlyphVisible = false;
			e.DragUIOverride.IsCaptionVisible = false;
		}
		
		void ScrollView_DragOver(object sender, UI.Xaml.DragEventArgs e)
		{

			if (!_canReorderItems)
			{
				e.AcceptedOperation = WDataTransfer.DataPackageOperation.None;
				return;
			}

			e.DragUIOverride.IsGlyphVisible = false;
			e.DragUIOverride.IsCaptionVisible = false;

			// Check for auto-scroll
			HandleAutoScroll(e);

			var targetContainer = FindContainerUnderPointer(e);
			if (targetContainer is null)
			{
				System.Diagnostics.Debug.WriteLine($"DragOver: No container under pointer");
				return;
			}
			
			var pt = e.GetPosition(targetContainer);
			
			int targetIndex = GetContainerIndex(targetContainer);
			System.Diagnostics.Debug.WriteLine($"DragOver: targetIndex={targetIndex}, isHorizontal={_isHorizontalLayout}");
			
			if (targetIndex < 0)
			{
				return;
			}
			
			// Calculate if we should insert before or after based on pointer position
			if (_isHorizontalLayout)
			{
				_insertAfter = pt.X >= targetContainer.ActualWidth / 2;
			}
			else
			{
				_insertAfter = pt.Y >= targetContainer.ActualHeight / 2;
			}
			
			// Insert AT target position or AFTER based on pointer position
			_insertionIndex = _insertAfter ? targetIndex + 1 : targetIndex;

			System.Diagnostics.Debug.WriteLine($"DragOver: insertAfter={_insertAfter}, _insertionIndex={_insertionIndex}");

			e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
			e.Handled = true;
			
		}
		
		void ScrollView_DragLeave(object sender, UI.Xaml.DragEventArgs e)
		{
			StopAutoScroll();
		}
		
		void ScrollView_Drop(object sender, UI.Xaml.DragEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"=== DROP EVENT ===");
			System.Diagnostics.Debug.WriteLine($"  _canReorderItems: {_canReorderItems}");
			System.Diagnostics.Debug.WriteLine($"  ItemsSource is IList: {ItemsSource is IList}");
			System.Diagnostics.Debug.WriteLine($"  _draggedItem: {_draggedItem}");
			System.Diagnostics.Debug.WriteLine($"  _insertionIndex: {_insertionIndex}");
			
			if (!_canReorderItems || ItemsSource is not IList itemsList || _draggedItem is null || _insertionIndex < 0)
			{
				System.Diagnostics.Debug.WriteLine($"  DROP CANCELLED - Validation failed");
				CleanupDragState();
				return;
			}
			
			System.Diagnostics.Debug.WriteLine($"  Proceeding with reorder...");
			
			try
			{
				bool reordered = PerformReorder(itemsList);
				
				System.Diagnostics.Debug.WriteLine($"  Reorder result: {reordered}");
				
				if (reordered)
				{
					ReorderCompleted?.Invoke(this, EventArgs.Empty);
				}
			}
			finally
			{
				CleanupDragState();
			}
		}

		#endregion

		#region Reordering Logic

		bool PerformReorder(IList itemsList)
		{
			if (_draggedItem is null)
			{
				System.Diagnostics.Debug.WriteLine($"  PerformReorder: _draggedItem is null");
				return false;
			}
			
			int oldIndex = IndexOfItem(_draggedItem, itemsList);
			System.Diagnostics.Debug.WriteLine($"  PerformReorder: oldIndex={oldIndex}, _insertionIndex={_insertionIndex}");
			
			if (oldIndex < 0)
			{
				System.Diagnostics.Debug.WriteLine($"  PerformReorder: oldIndex < 0");
				return false;
			}
			
			int adjustedInsertionIndex = _insertionIndex;
			
			if (oldIndex < adjustedInsertionIndex)
			{
				adjustedInsertionIndex--;
				System.Diagnostics.Debug.WriteLine($"  Adjusted insertion index: {adjustedInsertionIndex}");
			}
			
			if (oldIndex == adjustedInsertionIndex)
			{
				System.Diagnostics.Debug.WriteLine($"  Same position - no reorder needed");
				return false;
			}
			
			// Store the wrapper object (ItemTemplateContext2), not the unwrapped item
			var wrapperToMove = itemsList[oldIndex];
			System.Diagnostics.Debug.WriteLine($"  Moving wrapper from {oldIndex} to {adjustedInsertionIndex}");
			
			itemsList.RemoveAt(oldIndex);
			adjustedInsertionIndex = Math.Clamp(adjustedInsertionIndex, 0, itemsList.Count);
			itemsList.Insert(adjustedInsertionIndex, wrapperToMove);
			
			System.Diagnostics.Debug.WriteLine($"  Reorder complete - final index: {adjustedInsertionIndex}");
			
			DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
			{
				_itemsRepeater?.UpdateLayout();
				UpdateAllContainerIndices();

				var container = FindContainerByIndex(adjustedInsertionIndex);
				container?.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true });
			});
			
			return true;
		}

		#endregion

		#region Container and Item Management

		FrameworkElement? FindContainerUnderPointer(UI.Xaml.DragEventArgs e)
		{
			if (_itemsRepeater is null)
				return null;
			
			var position = e.GetPosition(_itemsRepeater);
			
			var elements = VisualTreeHelper.FindElementsInHostCoordinates(
				_itemsRepeater.TransformToVisual(null).TransformPoint(position),
				_itemsRepeater,
				false);
			
			foreach (var element in elements)
			{
				if (element is ItemContainer itemContainer && itemContainer.Tag is int)
				{
					return itemContainer;
				}
			}
			
			// Fallback: find by position
			if (ItemsSource is IList itemsList && itemsList.Count > 0)
			{
				var allContainers = FindAllContainers().ToList();
				
				foreach (var container in allContainers)
				{
					try
					{
						var containerPosition = container.TransformToVisual(_itemsRepeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));
						
						bool isInBounds;
						if (_isHorizontalLayout)
						{
							isInBounds = position.X >= containerPosition.X && 
										position.X <= containerPosition.X + container.ActualWidth;
						}
						else
						{
							isInBounds = position.Y >= containerPosition.Y && 
										position.Y <= containerPosition.Y + container.ActualHeight;
						}
						
						if (isInBounds)
							return container;
					}
					catch { }
				}
				
				// Return last container if pointer is beyond all containers
				if (allContainers.Count > 0)
				{
					var lastContainer = allContainers.Last();
					var lastPos = lastContainer.TransformToVisual(_itemsRepeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));
					
					bool isBeyondLast;
					if (_isHorizontalLayout)
					{
						isBeyondLast = position.X > lastPos.X + lastContainer.ActualWidth;
					}
					else
					{
						isBeyondLast = position.Y > lastPos.Y + lastContainer.ActualHeight;
					}
					
					if (isBeyondLast)
						return lastContainer;
				}
			}
			
			return null;
		}
		
		FrameworkElement? FindContainerByIndex(int index)
		{
			if (ItemsSource is not IList itemsList || index < 0 || index >= itemsList.Count)
				return null;
			
			return FindAllContainers().FirstOrDefault(c => GetContainerIndex(c) == index);
		}
		
		IEnumerable<FrameworkElement> FindAllContainers()
		{
			if (_itemsRepeater is not null && ItemsSource is IList itemsList)
			{
				for (int i = 0; i < itemsList.Count; i++)
				{
					var container = _itemsRepeater.TryGetElement(i);
					if (container is FrameworkElement fe)
						yield return fe;
				}
			}
		}
		
		int GetContainerIndex(FrameworkElement container)
		{
			if (container.Tag is int index)
				return index;
			
			var allContainers = FindAllContainers().ToList();
			return allContainers.IndexOf(container);
		}
		
		int IndexOfItem(object item, IList itemsList)
		{
			for (int i = 0; i < itemsList.Count; i++)
			{
				var currentItem = GetItemAtIndex(i, itemsList);
				if (Equals(currentItem, item))
					return i;
			}
			return -1;
		}
		
		object? GetItemAtIndex(int index, IList itemsList)
		{
			var item = itemsList[index];
			
			if (item is ItemTemplateContext2 itc)
				return itc.Item;
			
			return item;
		}

		void UpdateAllContainerIndices()
		{
			if (ItemsSource is not IList itemsList)
				return;

			var containers = FindAllContainers().ToList();

			for (int i = 0; i < containers.Count && i < itemsList.Count; i++)
			{
				var container = containers[i];
				container.Tag = i;
			}
		}

		#endregion

		#region Cleanup

		void CleanupDragState()
		{
			_draggedItem = null;
			_insertionIndex = -1;
			_insertAfter = false;
			StopAutoScroll();
		}

		#endregion

		#region Auto-Scroll During Drag

		void HandleAutoScroll(UI.Xaml.DragEventArgs e)
		{
			if (_scrollView is null)
				return;

			var position = e.GetPosition(_scrollView);

			if (_isHorizontalLayout)
			{
				HandleHorizontalAutoScroll(position);
			}
			else
			{
				HandleVerticalAutoScroll(position);
			}
		}

		void HandleVerticalAutoScroll(global::Windows.Foundation.Point position)
		{
			if (_scrollView is null)
				return;

			var height = _scrollView.ActualHeight;
			var distanceFromTop = position.Y;
			var distanceFromBottom = height - position.Y;

			// Check if near top edge
			if (distanceFromTop < AutoScrollThreshold && _scrollView.ScrollPresenter.VerticalOffset > 0)
			{
				// Calculate velocity based on distance from edge (closer = faster)
				var normalizedDistance = 1.0 - (distanceFromTop / AutoScrollThreshold);
				_targetScrollVelocity = -(AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
				StartAutoScroll();
			}
			// Check if near bottom edge
			else if (distanceFromBottom < AutoScrollThreshold &&
					 _scrollView.ScrollPresenter.VerticalOffset < _scrollView.ScrollPresenter.ScrollableHeight)
			{
				// Calculate velocity based on distance from edge (closer = faster)
				var normalizedDistance = 1.0 - (distanceFromBottom / AutoScrollThreshold);
				_targetScrollVelocity = AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
				StartAutoScroll();
			}
			else
			{
				// Not in auto-scroll zone
				_targetScrollVelocity = 0;
				if (Math.Abs(_currentScrollVelocity) < 0.1)
				{
					StopAutoScroll();
				}
			}
		}

		void HandleHorizontalAutoScroll(global::Windows.Foundation.Point position)
		{
			if (_scrollView is null)
				return;

			var width = _scrollView.ActualWidth;
			var distanceFromLeft = position.X;
			var distanceFromRight = width - position.X;

			// Check if near left edge
			if (distanceFromLeft < AutoScrollThreshold && _scrollView.ScrollPresenter.HorizontalOffset > 0)
			{
				// Calculate velocity based on distance from edge (closer = faster)
				var normalizedDistance = 1.0 - (distanceFromLeft / AutoScrollThreshold);
				_targetScrollVelocity = -(AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
				StartAutoScroll();
			}
			// Check if near right edge
			else if (distanceFromRight < AutoScrollThreshold &&
					 _scrollView.ScrollPresenter.HorizontalOffset < _scrollView.ScrollPresenter.ScrollableWidth)
			{
				// Calculate velocity based on distance from edge (closer = faster)
				var normalizedDistance = 1.0 - (distanceFromRight / AutoScrollThreshold);
				_targetScrollVelocity = AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
				StartAutoScroll();
			}
			else
			{
				// Not in auto-scroll zone
				_targetScrollVelocity = 0;
				if (Math.Abs(_currentScrollVelocity) < 0.1)
				{
					StopAutoScroll();
				}
			}
		}

		double CalculateScrollVelocity(double edgeDistance)
		{
			// Linear interpolation: closer to edge = faster scroll
			var normalizedDistance = Math.Min(edgeDistance / AutoScrollThreshold, 1.0);
			return normalizedDistance * AutoScrollMaxSpeed;
		}

		void StartAutoScroll()
		{
			if (_autoScrollTimer?.IsRunning == false)
			{
				_autoScrollTimer.Start();
			}
			else if (_autoScrollTimer is null)
			{
				_autoScrollTimer = DispatcherQueue.CreateTimer();
				_autoScrollTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
				_autoScrollTimer.Tick += AutoScrollTimer_Tick;
				_autoScrollTimer.Start();
			}
		}

		void StopAutoScroll()
		{
			_autoScrollTimer?.Stop();
			_targetScrollVelocity = 0;
			_currentScrollVelocity = 0;
		}

		void AutoScrollTimer_Tick(object? sender, object e)
		{
			if (_scrollView is null)
			{
				StopAutoScroll();
				return;
			}

			// Smooth acceleration/deceleration using linear interpolation
			_currentScrollVelocity = Lerp(_currentScrollVelocity, _targetScrollVelocity, ScrollAcceleration);

			// Only scroll if velocity is significant
			if (Math.Abs(_currentScrollVelocity) < 0.01)
			{
				if (_targetScrollVelocity == 0)
				{
					StopAutoScroll();
				}
				return;
			}

			try
			{
				// Calculate new offset based on orientation
				double newOffset;
				if (_isHorizontalLayout)
				{
					newOffset = _scrollView.ScrollPresenter.HorizontalOffset + _currentScrollVelocity;
					newOffset = Math.Clamp(newOffset, 0, _scrollView.ScrollPresenter.ScrollableWidth);
					_scrollView.ScrollPresenter.ScrollTo(newOffset, _scrollView.ScrollPresenter.VerticalOffset);
				}
				else
				{
					newOffset = _scrollView.ScrollPresenter.VerticalOffset + _currentScrollVelocity;
					newOffset = Math.Clamp(newOffset, 0, _scrollView.ScrollPresenter.ScrollableHeight);
					_scrollView.ScrollPresenter.ScrollTo(_scrollView.ScrollPresenter.HorizontalOffset, newOffset);
				}
			}
			catch
			{
				StopAutoScroll();
			}
		}

		static double Lerp(double start, double end, double amount)
		{
			return start + (end - start) * amount;
		}

		#endregion
	}
}
