using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class TabbedPageHandler : ViewHandler<TabbedPage, UIView>
	{
		protected override UIView CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void MapBarBackground(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapBarBackgroundColor(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapBarTextColor(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapUnselectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapSelectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapItemsSource(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapItemTemplate(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapSelectedItem(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapCurrentPage(TabbedPageHandler handler, TabbedPage view)
		{

		}
	}
}
