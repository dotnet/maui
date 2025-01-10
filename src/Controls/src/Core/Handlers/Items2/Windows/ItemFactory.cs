#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class ItemFactory(ItemsView view) : IElementFactory
	{
		private readonly ItemsView _view = view;

		public UIElement GetElement(ElementFactoryGetArgs args)
		{
			// NOTE: 1.6: replace w/ RecyclePool
			if (args.Data is ItemTemplateContext2 templateContext)
			{
				Microsoft.Maui.Controls.DataTemplate template = templateContext.MauiDataTemplate;
				if (template is Microsoft.Maui.Controls.DataTemplateSelector selector)
				{
					template = selector.SelectTemplate(templateContext.Item, _view);
				}

				if (template is null)
				{
					template = _view.EmptyViewTemplate;
				}

				ItemContainer container = null;
				ElementWrapper wrapper = null;
				var pool = RecyclePool.GetPoolInstance(template);
				if (pool is not null)
				{
					container = pool.TryGetElement(string.Empty, args.Parent) as ItemContainer;
					if (container is not null)
					{
						wrapper = container.Child as ElementWrapper;
					}
				}

				if (wrapper is null)
				{
					var viewContent = template.CreateContent() as View;
					wrapper = new ElementWrapper(_view.Handler.MauiContext);
					wrapper.SetContent(viewContent);

					((View)wrapper.VirtualView).SetValue(RecyclePool.OriginTemplateProperty, template);
				}

				if (wrapper.VirtualView is View view)
				{
					view.BindingContext = templateContext.Item ?? _view.BindingContext;
					_view.AddLogicalChild(view);
				}

				container ??= new ItemContainer()
				{
					Child = wrapper,
					IsEnabled = !templateContext.IsHeader && !templateContext.IsFooter
					// CanUserSelect = !templateContext.IsHeader // 1.6 feature
				};
				return container;

			}
			return null;
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
		{
			var item = args.Element as ItemContainer;
			var wrapper = item.Child as ElementWrapper;
			var wrapperView = wrapper.VirtualView as View;
			Microsoft.Maui.Controls.DataTemplate template =
				wrapperView.GetValue(RecyclePool.OriginTemplateProperty) as Microsoft.Maui.Controls.DataTemplate;

			var recyclePool = RecyclePool.GetPoolInstance(template);
			if (recyclePool == null)
			{
				// No Recycle pool in the template, create one.
				recyclePool = new RecyclePool();
				RecyclePool.SetPoolInstance(template, recyclePool);
			}
			recyclePool.PutElement(element: item, key: string.Empty, owner: args.Parent);
			_view.RemoveLogicalChild(wrapperView);
		}
	}

	internal class ElementWrapper(IMauiContext context) : UserControl
	{
		public IView VirtualView { get; private set; }
		
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
