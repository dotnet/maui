using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class TabbedPageHandler : ViewHandler<TabbedPage, ViewPager2>
	{
		TabbedPageManager _tabbedPageManager;
		protected override ViewPager2 CreateNativeView()
		{
			_tabbedPageManager ??= new TabbedPageManager(MauiContext!);
			return _tabbedPageManager.ViewPager;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_tabbedPageManager.SetElement((TabbedPage)view);
		}

		protected override void DisconnectHandler(ViewPager2 nativeView)
		{
			base.DisconnectHandler(nativeView);
			_tabbedPageManager.SetElement(null);
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
		public static void MapIndex(TabbedPageHandler handler, TabbedPage view)
		{
		}
		public static void MapCurrentPage(TabbedPageHandler handler, TabbedPage view)
		{

		}
	}
}
