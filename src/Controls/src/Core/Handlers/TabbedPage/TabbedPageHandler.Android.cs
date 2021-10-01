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
		public static PropertyMapper<TabbedPage, TabbedPageHandler> Mapper =
				new PropertyMapper<TabbedPage, TabbedPageHandler>(ViewMapper);

		TabbedPageManager _tabbedPageManager;
		public TabbedPageHandler() : base(Mapper, null)
		{
		}

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
	}
}
