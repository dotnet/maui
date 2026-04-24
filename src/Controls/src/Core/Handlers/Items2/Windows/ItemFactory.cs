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

	/// <summary>
	/// A minimal ControlTemplate for ItemContainer that contains no selection visuals
	/// (no PART_SelectionCheckbox, no PART_SelectionVisual, no PART_CommonVisual).
	/// Defined in ItemsViewStyles.xaml and applied to group header/footer containers
	/// so they can never show a checkbox or selection highlight.
	/// </summary>
	static Microsoft.UI.Xaml.Controls.ControlTemplate NonSelectableItemContainerTemplate =>
		(Microsoft.UI.Xaml.Controls.ControlTemplate)Microsoft.UI.Xaml.Application.Current.Resources["NonSelectableItemContainerTemplate"];

	/// <summary>
	/// Caches the default ItemContainer template so it can be restored
	/// when a header/footer container is recycled for a regular item.
	/// </summary>
	static Microsoft.UI.Xaml.Controls.ControlTemplate? _defaultItemContainerTemplate;
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

				// Apply the initial visual state so that VisualState setters (e.g., TextColor,
				// Background) defined in Normal or Selected states take effect immediately,
				// before the platform handler is created during the deferred ToPlatform() call.
				// Without this, items display with default property values instead of the
				// values defined in VisualState setters. (Fixes #27086)
				if (_view is SelectableItemsView selectableItemsView && selectableItemsView.SelectionMode
					!= SelectionMode.None)
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

			// Prevent group headers/footers from being selectable by swapping
			// the ItemContainer's ControlTemplate to one that has no checkbox
			// or selection visuals. This is stable across all selection modes
			// and visual state transitions.
			// Must be set every time to handle recycled containers correctly.
			if (wrapper is not null)
			{
				bool isHeaderOrFooter = wrapper.IsHeaderOrFooter;
				if (isHeaderOrFooter)
				{
					// Cache the default template once for later restoration
					_defaultItemContainerTemplate ??= container.Template;
					container.Template = NonSelectableItemContainerTemplate;
				}
				else
				{
					// Restore the default template for regular items (recycled from header/footer)
					if (_defaultItemContainerTemplate is not null && container.Template == NonSelectableItemContainerTemplate)
					{
						container.Template = _defaultItemContainerTemplate;
					}
				}
			}
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
internal partial class ElementWrapper : ContentControl
{
	/// <summary>The MAUI virtual view hosted by this wrapper.</summary>
	public IView? VirtualView { get; private set; }
	IMauiContext _context;

	/// <summary>Whether this wrapper hosts a group header or footer (excluded from size caching).</summary>
	public bool IsHeaderOrFooter { get; set; }

	public ElementWrapper(IMauiContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Sets the MAUI view content, converting it to a platform element.
	/// Only sets content if not already initialized.
	/// </summary>
	public void SetContent(IView view)
	{
		if (VirtualView is null || VirtualView.Handler is null)
		{
			// Store the virtual view but defer ToPlatform() until MeasureOverride.
			// At this point the ElementWrapper is not yet in the WinUI visual tree
			// (no XamlRoot). Calling ToPlatform() here would create a handler and
			// GesturePlatformManager, which tries to subscribe pointer events on
			// a disconnected element — causing a COM exception when the view has
			// a PointerOver visual state. By deferring to MeasureOverride, the
			// element is already in the visual tree with a valid XamlRoot.
			VirtualView = view;
		}
	}

	void EnsurePlatformViewCreated()
	{
		if (VirtualView is not null && Content is null)
		{
			if (XamlRoot is null)
			{
				// Element is not yet in the visual tree. Defer ToPlatform()
				// to avoid a COM exception when GesturePlatformManager tries
				// to subscribe pointer events on a disconnected element.
				// Re-trigger layout once the element is loaded.
				Loaded += OnLoadedCreatePlatformView;
				return;
			}

			var platformView = VirtualView.ToPlatform(_context);
			Content = platformView;
		}
	}

	void OnLoadedCreatePlatformView(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoadedCreatePlatformView;
		EnsurePlatformViewCreated();
	}

	CollectionViewHandler2? GetCollectionViewHandler()
	{
		if (VirtualView is View view &&
			view.Parent is ItemsView itemsView &&
			itemsView.Handler is CollectionViewHandler2 cvHandler)
		{
			return cvHandler;
		}
		return null;
	}
	protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
	{
		EnsurePlatformViewCreated();
		var handler = GetCollectionViewHandler();
		// Check if we should use cached first item size
		var cachedSize = handler?.GetCachedFirstItemSize() ?? global::Windows.Foundation.Size.Empty;
		if (!cachedSize.IsEmpty)
		{
			// For MeasureFirstItem: pin ONLY the along-axis (scroll direction) to the
			// cached first-item size so every item is uniform in that direction. Measure
			// the child with the current cross-axis availableSize so its internal layout
			// (e.g. MAUI Grid column widths) is computed against the real arrange width —
			// without this, right-aligned content in the template would position against
			// the cached narrow width and get truncated on later items. See #25191.
			bool isHorizontal = handler?.PlatformView is MauiItemsView miv && miv.IsHorizontalLayout;
			double measureWidth = isHorizontal
				? cachedSize.Width
				: (double.IsInfinity(availableSize.Width) ? cachedSize.Width : availableSize.Width);
			double measureHeight = isHorizontal
				? (double.IsInfinity(availableSize.Height) ? cachedSize.Height : availableSize.Height)
				: cachedSize.Height;
			var constrainedSize = new global::Windows.Foundation.Size(measureWidth, measureHeight);
			base.MeasureOverride(constrainedSize);
			// Return the constrained size (cross-axis = current viewport, along-axis = cached)
			// so DesiredSize tracks viewport changes and the list re-flows when the viewport
			// shrinks (e.g. window minimize). Along-axis uniformity is preserved via the
			// cached value.
			return constrainedSize;
		}
		// Measure normally with the original available size
		var measuredSize = base.MeasureOverride(availableSize);
		// Cache the size if this is the first item being measured and the size is valid
		if (handler != null && !IsHeaderOrFooter && measuredSize.Width > 0 && measuredSize.Height > 0)
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

	protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
	{
		var handler = GetCollectionViewHandler();
		var cachedSize = handler?.GetCachedFirstItemSize() ?? global::Windows.Foundation.Size.Empty;
		if (!cachedSize.IsEmpty && !IsHeaderOrFooter)
		{
			// For MeasureFirstItem: Enforce uniformity on the along-axis (scroll direction)
			// only. The cross-axis uses finalSize so items continue to stretch across the
			// viewport (matching non-MeasureFirstItem behavior and Android/iOS parity).
			// Without this, items whose template has no explicit cross-axis size (e.g. a
			// Border with only HeightRequest) would be clipped to their natural content
			// width, producing a narrow column instead of full-width rows.
			// See: https://github.com/dotnet/maui/issues/25191
			bool isHorizontal = handler?.PlatformView is MauiItemsView miv && miv.IsHorizontalLayout;
			var arrangeSize = isHorizontal
				? new global::Windows.Foundation.Size(cachedSize.Width, finalSize.Height)
				: new global::Windows.Foundation.Size(finalSize.Width, cachedSize.Height);
			base.ArrangeOverride(arrangeSize);
			Clip = new Microsoft.UI.Xaml.Media.RectangleGeometry
			{
				Rect = new global::Windows.Foundation.Rect(0, 0, arrangeSize.Width, arrangeSize.Height)
			};
			return arrangeSize;
		}
		// Clear any clip from a previously cached state (recycled container scenario)
		if (Clip is not null)
		{
			Clip = null;
		}
		return base.ArrangeOverride(finalSize);
	}
}