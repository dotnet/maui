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

	internal partial class ElementWrapper(IMauiContext context) : ContentControl
	{
		public IView? VirtualView { get; private set; }
		
		private IMauiContext _context = context;

		public void SetContent(IView view)
		{
			if (VirtualView is null || VirtualView.Handler is null)
			{
				Content = view.ToPlatform(_context);
				VirtualView = view;
			}
		}
	}
}
