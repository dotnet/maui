using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Internals;
using GColor = Microsoft.Maui.Graphics.Color;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellContentItemAdaptor : ItemTemplateAdaptor
	{
		ItemAppearance _itemAppearance;

		public ShellContentItemAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate())
		{
			_itemAppearance = new ItemAppearance();
		}

		protected override bool IsSelectable => true;

		public void UpdateItemsColor(GColor? foregroundColor, GColor? titleColor, GColor? unselectedColor)
		{
			_itemAppearance.ForegroundColor = foregroundColor;
			_itemAppearance.TitleColor = titleColor;
			_itemAppearance.UnselectedColor = unselectedColor;
		}

		public override NView CreateNativeView(int index)
		{
			var nativeView = base.CreateNativeView(index);

			var view = GetTemplatedView(nativeView);
			view?.SetBinding(ShellContentItemView.SelectedTextColorProperty, static (ItemAppearance appearance) => appearance.TitleColor, source: _itemAppearance);
			view?.SetBinding(ShellContentItemView.SelectedBarColorProperty, static (ItemAppearance appearance) => appearance.ForegroundColor, source: _itemAppearance);
			view?.SetBinding(ShellContentItemView.UnselectedColorProperty, static (ItemAppearance appearance) => appearance.UnselectedColor, source: _itemAppearance);

			return nativeView;
		}

		static DataTemplate GetTemplate()
		{
			return new DataTemplate(() =>
			{
				return new ShellContentItemView();
			});
		}
	}
}
