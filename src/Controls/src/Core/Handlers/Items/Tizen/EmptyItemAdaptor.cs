#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class EmptyItemAdaptor : ItemTemplateAdaptor, IEmptyAdaptor
	{
		static DataTemplate s_defaultEmptyTemplate = new DataTemplate(typeof(EmptyView));

		IMauiContext _context;

		public EmptyItemAdaptor(ItemsView itemsView) : this(itemsView, itemsView.ItemsSource, itemsView.ItemTemplate) { }

		public EmptyItemAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(itemsView, items, template)
		{
			_context = itemsView.Handler!.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
		}

		public static EmptyItemAdaptor Create(ItemsView itemsView)
		{
			DataTemplate? template = null;
			if (itemsView.EmptyView is View emptyView)
			{
				template = new DataTemplate(() =>
				{
					return emptyView;
				});
			}
			else
			{
				template = itemsView.EmptyViewTemplate ?? s_defaultEmptyTemplate;
			}
			var empty = new List<object>
			{
				itemsView.EmptyView ?? new object()
			};
			return new EmptyItemAdaptor(itemsView, empty, template);
		}

		public override Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}

		public override EvasObject CreateNativeView(int index, EvasObject parent)
		{
			View? emptyView = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				emptyView = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				emptyView = ItemTemplate.CreateContent() as View;
			}

			var header = CreateHeaderView();
			var footer = CreateFooterView();
			var layout = new StackLayout();

			if (header != null)
			{
				layout.Children.Add(header);
			}
			layout.Children.Add(emptyView);
			if (footer != null)
			{
				layout.Children.Add(footer);
			}

			layout.Parent = Element;

			return layout.ToPlatform(_context);
		}

		public override void RemoveNativeView(EvasObject native)
		{
			native.Unrealize();
		}

		class EmptyView : StackLayout
		{
			public EmptyView()
			{
				HorizontalOptions = LayoutOptions.Fill;
				VerticalOptions = LayoutOptions.Fill;
				Children.Add(
					new Label
					{
						Text = "No items found",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center,
					}
				);
			}
		}
	}
}
