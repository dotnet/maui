using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items2;
/// <summary>
/// Element factory that creates, recycles, and manages <see cref="ItemContainer"/> elements
/// for the WinUI ItemsView/ItemsRepeater, using a template-keyed recycle pool.
/// </summary>
internal partial class ItemFactory(ItemsView view) : IElementFactory
{
	readonly ItemsView _view = view;
	Dictionary<DataTemplate, List<ItemContainer>> _recyclePool = new();

	internal static readonly BindableProperty OriginTemplateProperty =
		BindableProperty.CreateAttached(
			"OriginTemplate", typeof(DataTemplate), typeof(ItemFactory), null);

	/// <summary>
	/// Creates or retrieves a recycled <see cref="ItemContainer"/> for the given data context.
	/// </summary>
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
					wrapper.HorizontalAlignment = viewContent.HorizontalOptions.Alignment switch
						{
							LayoutAlignment.Start => HorizontalAlignment.Left,
							LayoutAlignment.Center => HorizontalAlignment.Center,
							LayoutAlignment.End => HorizontalAlignment.Right,
							_ => HorizontalAlignment.Stretch
						};
						wrapper.VerticalAlignment = viewContent.VerticalOptions.Alignment switch
						{
							LayoutAlignment.Start => VerticalAlignment.Top,
							LayoutAlignment.Center => VerticalAlignment.Center,
							LayoutAlignment.End => VerticalAlignment.Bottom,
							_ => VerticalAlignment.Stretch
						};
					wrapper.HorizontalContentAlignment = HorizontalAlignment.Stretch;
					wrapper.VerticalContentAlignment = VerticalAlignment.Stretch;
					wrapper.SetContent(viewContent);
					wrapper.IsHeaderOrFooter = templateContext.IsHeader || templateContext.IsFooter;

					if (wrapper.VirtualView is View virtualView)
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
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			return container;
		}

		return null;
	}

	/// <summary>
	/// Returns an element to the recycle pool, keyed by its original <see cref="DataTemplate"/>.
	/// </summary>
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

	/// <summary>
	/// Clears the recycle pool and removes logical children held by pooled elements.
	/// Must be called when the items source changes or when the handler disconnects
	/// to prevent memory leaks from pooled ItemContainers holding strong references.
	/// </summary>
	internal void CleanUp()
	{
		foreach (var kvp in _recyclePool)
		{
			foreach (var container in kvp.Value)
			{
				var wrapper = container?.Child as ElementWrapper;
				var wrapperView = wrapper?.VirtualView as View;
				if (wrapperView is not null)
				{
					_view.RemoveLogicalChild(wrapperView);
				}
			}
		}

		_recyclePool.Clear();
	}
}

/// <summary>
/// A <see cref="ContentControl"/> wrapper that hosts a MAUI <see cref="IView"/> inside a WinUI element tree.
/// Handles MeasureFirstItem optimization by caching the first measured size.
/// </summary>
internal partial class ElementWrapper(IMauiContext context) : ContentControl
{
	/// <summary>The MAUI virtual view hosted by this wrapper.</summary>
	public IView? VirtualView { get; private set; }

	IMauiContext _context = context;

	/// <summary>Whether this wrapper hosts a group header or footer (excluded from size caching).</summary>
	public bool IsHeaderOrFooter { get; set; }

	/// <summary>
	/// Sets the MAUI view content, converting it to a platform element.
	/// Only sets content if not already initialized.
	/// </summary>
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

		// Cache the size if this is the first item being measured
		if (handler != null && !IsHeaderOrFooter)
		{
			var currentCached = handler.GetCachedFirstItemSize();
			if (currentCached.IsEmpty)
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

					void OnContentUnloaded(object? sender, RoutedEventArgs e)
					{
						// Unwire both events to prevent memory leaks
						if (sender is FrameworkElement element)
						{
							element.SizeChanged -= OnContentSizeChanged;
							element.Unloaded -= OnContentUnloaded;
						}
					}

					// Subscribe once
					content.SizeChanged += OnContentSizeChanged;
					content.Unloaded += OnContentUnloaded;
				}				
				
				handler.SetCachedFirstItemSize(measuredSize);
			}
		}

		return measuredSize;
	}
}