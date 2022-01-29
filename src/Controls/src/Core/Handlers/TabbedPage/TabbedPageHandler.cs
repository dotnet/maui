using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class TabbedPageHandler
	{
		public static PropertyMapper<TabbedPage, TabbedPageHandler> Mapper =
				new PropertyMapper<TabbedPage, TabbedPageHandler>(ViewMapper)
				{
					//[TabbedPage.BarBackgroundProperty.PropertyName] = MapBarBackground,
					//[TabbedPage.BarBackgroundColorProperty.PropertyName] = MapBarBackgroundColor,
					[TabbedPage.BarTextColorProperty.PropertyName] = MapBarTextColor,
					//[TabbedPage.UnselectedTabColorProperty.PropertyName] = MapUnselectedTabColor,
					//[TabbedPage.SelectedTabColorProperty.PropertyName] = MapSelectedTabColor,
					[MultiPage<TabbedPage>.ItemsSourceProperty.PropertyName] = MapItemsSource,
					[MultiPage<TabbedPage>.ItemTemplateProperty.PropertyName] = MapItemTemplate,
					[MultiPage<TabbedPage>.SelectedItemProperty.PropertyName] = MapSelectedItem,
					[nameof(TabbedPage.CurrentPage)] = MapCurrentPage
				};

		public static CommandMapper<TabbedPage, TabbedPageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{

		};

		public TabbedPageHandler() : base(Mapper, CommandMapper)
		{
		}
	}
}
