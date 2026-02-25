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
					if (view is VisualElement visualElement)
					{
						bool isSelected = false;
						if (_view is SelectableItemsView selectableItemsView && selectableItemsView.SelectionMode != SelectionMode.None)
						{
							if (selectableItemsView.SelectionMode == SelectionMode.Single)
								isSelected = selectableItemsView.SelectedItem == templateContext.Item;
							else
								isSelected = selectableItemsView.SelectedItems.Contains(templateContext.Item);
						}

						VisualStateManager.GoToState(visualElement, isSelected
							? VisualStateManager.CommonStates.Selected
							: VisualStateManager.CommonStates.Normal);
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

	internal partial class ElementWrapper(IMauiContext context) : ContentControl
	{
		public IView? VirtualView { get; private set; }

		private IMauiContext _context = context;

		public bool IsHeaderOrFooter { get; set; }

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

				// In MAUI, View.Margin is handled by the cross-platform layout system
				// (ComputeDesiredSize/ComputeFrame) in the MAUI parent's layout pass.
				// But in CollectionView2, the root template view's layout is managed by
				// WinUI's ItemsRepeater, not a MAUI parent — so the margin is never
				// applied. Set it as WinUI Margin on the platform view so the native
				// layout respects the spacing.
				if (VirtualView is View mauiView && platformView is FrameworkElement fe)
				{
					var margin = mauiView.Margin;
					fe.Margin = new Microsoft.UI.Xaml.Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);
				}
			}
		}

		void OnLoadedCreatePlatformView(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoadedCreatePlatformView;
			EnsurePlatformViewCreated();
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			EnsurePlatformViewCreated();
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
