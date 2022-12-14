#nullable enable

using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;
using GColor = Microsoft.Maui.Graphics.Color;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSectionItemAdaptor : ItemTemplateAdaptor
	{
		ItemAppearance _itemAppearance;

		public ShellSectionItemAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate())
		{
			_itemAppearance = new ItemAppearance();
		}

		protected override bool IsSelectable => true;

		public void UpdateItemsColor(GColor? titleColor, GColor? unselectedColor)
		{
			_itemAppearance.TitleColor = titleColor;
			_itemAppearance.UnselectedColor = unselectedColor;
		}

		public override NView CreateNativeView(int index)
		{
			var nativeView = base.CreateNativeView(index);

			var view = GetTemplatedView(nativeView);
			view?.SetBinding(ShellSectionItemView.SelectedColorProperty, new Binding("TitleColor", source: _itemAppearance));
			view?.SetBinding(ShellSectionItemView.UnselectedColorProperty, new Binding("UnselectedColor", source: _itemAppearance));

			return nativeView;
		}

		static DataTemplate GetTemplate()
		{
			return new ShellSectionDataTemplateSelector();
		}

		class ShellSectionDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate ShellSectionItemTemplate { get; }
			DataTemplate MoreItemTemplate { get; }

			public ShellSectionDataTemplateSelector()
			{
				ShellSectionItemTemplate = new DataTemplate(() =>
				{
					return new ShellSectionItemView(false);
				});

				MoreItemTemplate = new DataTemplate(() =>
				{
					return new ShellSectionItemView(true);
				});
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				if (item is ShellSection)
					return ShellSectionItemTemplate;

				return MoreItemTemplate;
			}
		}
	}
}
