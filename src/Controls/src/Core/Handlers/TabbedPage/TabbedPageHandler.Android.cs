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
	public sealed class TabbedPageHandler : ViewHandler<TabbedPage, ViewPager2>
	{
		public TabLayout TabLayout =>
			_tabbedPageManager.IsBottomTabPlacement ? null :
				_tabbedPageManager.TabLayout;

		public BottomNavigationView BottomNavigationView =>
			_tabbedPageManager.IsBottomTabPlacement ? _tabbedPageManager.BottomNavigationView :
				null;

		TabbedPageManager _tabbedPageManager;
		public TabbedPageHandler() : base(ViewHandler.ViewMapper, null)
		{
		}

		protected override ViewPager2 CreateNativeView()
		{
			_tabbedPageManager ??= new TabbedPageManager(MauiContext!);
			return _tabbedPageManager.ViewPager;
		}

		public override void SetVirtualView(IView view)
		{
			_tabbedPageManager.SetElement(VirtualView);
			base.SetVirtualView(view);
		}
	}
}
