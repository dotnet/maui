using System.Collections;
using System.Collections.Generic;

using ESize = ElmSharp.Size;
using XLabel = System.Maui.Label;

namespace System.Maui.Platform.Tizen.Native
{
	public class EmptyItemAdaptor : ItemTemplateAdaptor, IEmptyAdaptor
	{
		static DataTemplate s_defaultEmptyTemplate = new DataTemplate(typeof(EmptyView));
		public EmptyItemAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(itemsView, items, template)
		{
		}

		public static EmptyItemAdaptor Create(ItemsView itemsView)
		{
			DataTemplate template = null;
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

		public override ElmSharp.Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return new ESize(widthConstraint, heightConstraint);
		}

		class EmptyView : StackLayout
		{
			public EmptyView()
			{
				HorizontalOptions = LayoutOptions.FillAndExpand;
				VerticalOptions = LayoutOptions.FillAndExpand;
				Children.Add(
					new XLabel
					{
						Text = "No items found",
						VerticalOptions = LayoutOptions.CenterAndExpand,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						HorizontalTextAlignment = System.Maui.TextAlignment.Center,
						VerticalTextAlignment = System.Maui.TextAlignment.Center,
					}
				);
			}
		}
	}
}
