using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WDataPackageOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal partial class ItemFactory(ItemsView view) : IElementFactory
	{
		private readonly ItemsView _view = view;
		private Dictionary<DataTemplate, List<ItemContainer>> _recyclePool = new();
		private object? _currentDraggedItem;
		private DispatcherTimer? _autoScrollTimer;
		private const double AutoScrollThreshold = 50.0; // pixels from edge to trigger auto-scroll
		private const double AutoScrollSpeed = 10.0; // pixels per timer tick

		internal static readonly BindableProperty OriginTemplateProperty =
			BindableProperty.CreateAttached(
				"OriginTemplate", typeof(DataTemplate), typeof(ItemFactory), null);

		public UIElement? GetElement(ElementFactoryGetArgs args)
		{
			// NOTE: 1.6: replace w/ RecyclePool
			if (args.Data is ItemTemplateContext2 templateContext)
			{
				DataTemplate? template = templateContext.MauiDataTemplate;
				if (template is DataTemplateSelector selector)
				{
					template = selector.SelectTemplate(templateContext.Item, _view);
				}

				if (template is null)
				{
					template = _view.EmptyViewTemplate;
				}

				ItemContainer? container = null;
				ElementWrapper? wrapper = null;

				if (_recyclePool.TryGetValue(template, out var itemContainers))
				{
					if (itemContainers.Count > 0)
					{
						container = itemContainers[0];
						if (container is not null)
						{
							wrapper = container.Child as ElementWrapper;
						}

						itemContainers.RemoveAt(0);
					}
				}

				if (wrapper is null)
				{
					var viewContent = template.CreateContent() as View;
					if (_view.Handler?.MauiContext is not null && viewContent is not null)
					{
						wrapper = new ElementWrapper(_view.Handler.MauiContext);
						wrapper.HorizontalAlignment = HorizontalAlignment.Stretch;
						wrapper.HorizontalContentAlignment = HorizontalAlignment.Stretch;
						wrapper.SetContent(viewContent);

						if (wrapper is not null)
						{
							wrapper.IsHeaderOrFooter = templateContext.IsHeader || templateContext.IsFooter;
							
							// Enable drag and drop for reorderable items
							if (_view is ReorderableItemsView reorderableView && reorderableView.CanReorderItems && !wrapper.IsHeaderOrFooter)
							{
								wrapper.CanDrag = true;
								wrapper.AllowDrop = true;

								wrapper.DragStarting += OnDragStarting;
								wrapper.Drop += OnDrop;
								wrapper.DragOver += OnDragOver;
							}
						}
						
						if (wrapper?.VirtualView is View virtualView)
						{
							virtualView.SetValue(OriginTemplateProperty, template);
						}
						
					}
				}

				if (wrapper?.VirtualView is View view)
				{
					view.BindingContext = templateContext.Item ?? _view.BindingContext;
					_view.AddLogicalChild(view);
					if (_view is SelectableItemsView selectableItemsView && selectableItemsView.SelectionMode != SelectionMode.None)
					{
						bool isSelected = false;
						if (selectableItemsView.SelectionMode == SelectionMode.Single)
							isSelected = selectableItemsView.SelectedItem == templateContext.Item;
						else
							isSelected = selectableItemsView.SelectedItems.Contains(templateContext.Item);

						if (isSelected && view is VisualElement visualElement)
						{
							VisualStateManager.GoToState(visualElement, VisualStateManager.CommonStates.Selected);
						}
					}

				}

				container ??= new ItemContainer()
				{
					Child = wrapper,
					HorizontalAlignment = HorizontalAlignment.Stretch
				};

				return container;
			}

			return null;
		}

		private void OnDragStarting(UIElement sender, UI.Xaml.DragStartingEventArgs args)
		{
			if (sender is ElementWrapper wrapper && wrapper.VirtualView is View view)
			{
				var item = view.BindingContext;
				if (item is not null)
				{
					// Store the item reference locally instead of in DataPackage to avoid serialization issues
					_currentDraggedItem = item;
					
					// Put a simple marker in the DataPackage to indicate this is a reorder operation
					args.Data.SetText("MauiReorder");
					args.Data.RequestedOperation = WDataPackageOperation.Move;

					// Start auto-scroll timer
					StartAutoScrollTimer();
				}
			}
		}

		private void OnDragOver(object sender, UI.Xaml.DragEventArgs e)
		{
			if (e.DataView.Contains(StandardDataFormats.Text) && _currentDraggedItem is not null)
			{
				e.AcceptedOperation = WDataPackageOperation.Move;
				e.DragUIOverride.Caption = "Reorder";
				e.DragUIOverride.IsCaptionVisible = true;
				e.DragUIOverride.IsGlyphVisible = true;

				// Handle auto-scrolling based on drag position
				HandleAutoScroll(e);
			}
		}

		private void OnDrop(object sender, UI.Xaml.DragEventArgs e)
		{
			// Stop auto-scroll timer
			StopAutoScrollTimer();

			if (sender is ElementWrapper targetWrapper && targetWrapper.VirtualView is View targetView)
			{
				var targetItem = targetView.BindingContext;
				var draggedItem = _currentDraggedItem;
				
				// Clear the dragged item reference
				_currentDraggedItem = null;
				
				if (draggedItem is not null && targetItem is not null && 
					_view is ReorderableItemsView reorderableView)
				{
					// Get the underlying collection
					var itemsSource = reorderableView.ItemsSource;
					
					if (itemsSource is System.Collections.IList list)
					{
						var oldIndex = list.IndexOf(draggedItem);
						var newIndex = list.IndexOf(targetItem);
						
						if (oldIndex != -1 && newIndex != -1 && oldIndex != newIndex)
						{
							list.RemoveAt(oldIndex);
							list.Insert(newIndex, draggedItem);
							
							// Notify that reorder completed
							reorderableView.SendReorderCompleted();
						}
					}
				}
			}
		}

		private void StartAutoScrollTimer()
		{
			if (_autoScrollTimer is null)
			{
				_autoScrollTimer = new DispatcherTimer
				{
					Interval = System.TimeSpan.FromMilliseconds(16) // ~60 FPS
				};
				_autoScrollTimer.Tick += AutoScrollTimer_Tick;
			}

			_autoScrollTimer.Start();
		}

		private void StopAutoScrollTimer()
		{
			_autoScrollTimer?.Stop();
		}

		private global::Windows.Foundation.Point? _lastDragPosition;

		private void HandleAutoScroll(UI.Xaml.DragEventArgs e)
		{
			if (_view.Handler is not ItemsViewHandler2<ItemsView> handler || handler.PlatformView is null)
				return;

			var platformView = handler.PlatformView;
			var scrollView = platformView.ScrollView;

			if (scrollView is null)
				return;

			// Get drag position relative to the scroll view
			_lastDragPosition = e.GetPosition(scrollView);
		}

		private void AutoScrollTimer_Tick(object? sender, object e)
		{
			if (_lastDragPosition is null || _view.Handler is not ItemsViewHandler2<ItemsView> handler || handler.PlatformView is null)
				return;

			var platformView = handler.PlatformView;
			var scrollView = platformView.ScrollView;

			if (scrollView?.ScrollPresenter is null)
				return;

			var scrollPresenter = scrollView.ScrollPresenter;
			var dragY = _lastDragPosition.Value.Y;
			var dragX = _lastDragPosition.Value.X;
			var scrollViewHeight = scrollView.ActualHeight;
			var scrollViewWidth = scrollView.ActualWidth;

			double verticalOffset = 0;
			double horizontalOffset = 0;

			// Check if we need to scroll vertically
			if (dragY < AutoScrollThreshold && scrollPresenter.VerticalOffset > 0)
			{
				// Scroll up - speed increases as we get closer to the edge
				var proximity = 1.0 - (dragY / AutoScrollThreshold);
				verticalOffset = -AutoScrollSpeed * proximity;
			}
			else if (dragY > scrollViewHeight - AutoScrollThreshold && 
				scrollPresenter.VerticalOffset < scrollPresenter.ScrollableHeight)
			{
				// Scroll down - speed increases as we get closer to the edge
				var proximity = 1.0 - ((scrollViewHeight - dragY) / AutoScrollThreshold);
				verticalOffset = AutoScrollSpeed * proximity;
			}

			// Check if we need to scroll horizontally (for horizontal layouts)
			if (dragX < AutoScrollThreshold && scrollPresenter.HorizontalOffset > 0)
			{
				// Scroll left
				var proximity = 1.0 - (dragX / AutoScrollThreshold);
				horizontalOffset = -AutoScrollSpeed * proximity;
			}
			else if (dragX > scrollViewWidth - AutoScrollThreshold && 
				scrollPresenter.HorizontalOffset < scrollPresenter.ScrollableWidth)
			{
				// Scroll right
				var proximity = 1.0 - ((scrollViewWidth - dragX) / AutoScrollThreshold);
				horizontalOffset = AutoScrollSpeed * proximity;
			}

			// Apply scroll if needed
			if (verticalOffset != 0 || horizontalOffset != 0)
			{
				scrollPresenter.ScrollBy(horizontalOffset, verticalOffset);
			}
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
		{
			var item = args.Element as ItemContainer;
			var wrapper = item?.Child as ElementWrapper;
			var wrapperView = wrapper?.VirtualView as View;
			DataTemplate? template = wrapperView?.GetValue(OriginTemplateProperty) as DataTemplate;
			
			// Unsubscribe from drag/drop events when recycling
			if (wrapper is not null && wrapper.CanDrag)
			{
				wrapper.DragStarting -= OnDragStarting;
				wrapper.Drop -= OnDrop;
				wrapper.DragOver -= OnDragOver;
			}
			
			if (template != null && item is not null)
			{
				if (_recyclePool.TryGetValue(template, out var itemContainers))
				{
					itemContainers.Add(item);
				}
				else
				{
					_recyclePool[template] = new List<ItemContainer> { item };
				}
			}

			_view.RemoveLogicalChild(wrapperView);
		}
	}

	internal partial class ElementWrapper(IMauiContext context) : ContentControl
	{
		public IView? VirtualView { get; private set; }
		
		private IMauiContext _context = context;

		public bool IsHeaderOrFooter { get; set; }	

		public void SetContent(IView view)
		{
			if (VirtualView is null || VirtualView.Handler is null)
			{
				Content = view.ToPlatform(_context);
				VirtualView = view;
			}
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			// Access handler through view parent chain (same pattern as iOS)
			CollectionViewHandler2? handler = null;
			if (VirtualView is View view &&
				view.Parent is ItemsView itemsView &&
				itemsView.Handler is CollectionViewHandler2 cvHandler)
			{
				handler = cvHandler;
			}

			// Check if we should use cached first item size
			var cachedSize = handler?.GetCachedFirstItemSize() ?? global::Windows.Foundation.Size.Empty;

			if (!cachedSize.IsEmpty)
			{
				// Use cached size for MeasureFirstItem strategy
				base.MeasureOverride(cachedSize);
				return cachedSize;
			}

			// Measure normally
			var measuredSize = base.MeasureOverride(availableSize);

			// Cache the size if this is the first item being measured
			if (handler != null && !IsHeaderOrFooter)
			{
				var currentCached = handler.GetCachedFirstItemSize();
				if (currentCached.IsEmpty)
				{
					handler.SetCachedFirstItemSize(measuredSize);
				}
			}

			return measuredSize;
		}
	}
}
