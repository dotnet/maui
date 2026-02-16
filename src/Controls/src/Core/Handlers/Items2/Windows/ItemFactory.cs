using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal partial class ItemFactory(ItemsView view) : IElementFactory
	{
		private readonly ItemsView _view = view;
		private Dictionary<DataTemplate, List<ItemContainer>> _recyclePool = new();

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
						wrapper.VerticalAlignment = VerticalAlignment.Stretch;
						wrapper.VerticalContentAlignment = VerticalAlignment.Stretch;
						wrapper.SetContent(viewContent);

						if(wrapper is not null)
						{
							wrapper.IsHeaderOrFooter = templateContext.IsHeader || templateContext.IsFooter;
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
						isSelected = object.Equals(selectableItemsView.SelectedItem, templateContext.Item);
					else
						isSelected = selectableItemsView.SelectedItems.Contains(templateContext.Item);						if (isSelected && view is VisualElement visualElement)
						{
							VisualStateManager.GoToState(visualElement, VisualStateManager.CommonStates.Selected);
						}
					}

				}

				container ??= new ItemContainer()
				{
					Child = wrapper,
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Stretch
				};

				return container;
			}

			return null;
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
		{
			var item = args.Element as ItemContainer;
			var wrapper = item?.Child as ElementWrapper;
			var wrapperView = wrapper?.VirtualView as View;
			DataTemplate? template = wrapperView?.GetValue(OriginTemplateProperty) as DataTemplate;
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

			// Always measure to allow content to load and render
			var measuredSize = base.MeasureOverride(availableSize);

			if (!cachedSize.IsEmpty)
			{
				// For MeasureFirstItem: Return cached size for uniform layout
				return cachedSize;
			}

			// Cache the first item's size for MeasureFirstItem optimization
			if (handler != null && !IsHeaderOrFooter)
			{
				// For first item with images: Hook into content's SizeChanged to update cache when images load
				if (VirtualView is View firstView && Content is FrameworkElement content)
				{
					void OnContentSizeChanged(object? sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
					{
						// Update cached size with actual loaded size
						var currentCache = handler.GetCachedFirstItemSize();
						if (!currentCache.IsEmpty && (e.NewSize.Width > currentCache.Width || e.NewSize.Height > currentCache.Height))
						{
							handler.SetCachedFirstItemSize(e.NewSize);
							// Force layout update
							InvalidateMeasure();
						}
					}

					// Subscribe once
					content.SizeChanged -= OnContentSizeChanged;
					content.SizeChanged += OnContentSizeChanged;
				}

				handler.SetCachedFirstItemSize(measuredSize);
			}

			return measuredSize;
		}
	}
}
