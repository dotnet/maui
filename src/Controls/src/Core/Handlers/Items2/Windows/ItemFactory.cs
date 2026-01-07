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
						var handler = _view.Handler as ItemsViewHandler2<ItemsView>;
						wrapper = new ElementWrapper(_view.Handler.MauiContext, handler);
						wrapper.HorizontalAlignment = HorizontalAlignment.Stretch;
						wrapper.HorizontalContentAlignment = HorizontalAlignment.Stretch;
						wrapper.SetContent(viewContent);

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

	internal partial class ElementWrapper : ContentControl
	{
		public IView? VirtualView { get; private set; }

		private IMauiContext _context;
		private ItemsViewHandler2<ItemsView>? _handler;

		public ElementWrapper(IMauiContext context, ItemsViewHandler2<ItemsView>? handler = null)
		{
			_context = context;
			_handler = handler;
		}

		public void SetContent(IView view)
		{
			if (VirtualView is null || VirtualView.Handler is null)
			{
				Content = view.ToPlatform(_context);
				VirtualView = view;
			}
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			// Check if we should use cached first item size
			var cachedSize = _handler?.GetCachedFirstItemSize() ?? Windows.Foundation.Size.Empty;

			if (!cachedSize.IsEmpty)
			{
				// Use cached size for MeasureFirstItem strategy
				base.MeasureOverride(cachedSize);
				return cachedSize;
			}

			// Measure normally
			var measuredSize = base.MeasureOverride(availableSize);

			// Cache the size if this is the first item and using MeasureFirstItem
			if (_handler != null &&
				_handler.VirtualView is StructuredItemsView siv &&
				siv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				var currentCached = _handler.GetCachedFirstItemSize();
				if (currentCached.IsEmpty)
				{
					_handler.SetCachedFirstItemSize(measuredSize);
				}
			}

			return measuredSize;
		}
	}
}
