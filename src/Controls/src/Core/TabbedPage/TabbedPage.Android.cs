using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		IMauiContext MauiContext => this.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");
		TabbedPageManager? _tabbedPageManager;

		public TabbedPageManager TabbedPageManager
		{
			get => _tabbedPageManager ??= new (MauiContext);
			set
			{
				if (_tabbedPageManager is not null && _tabbedPageManager != value)
				{
					throw new InvalidOperationException("TabbedPageManager cannot be assigned to new instance.");
				}

				_tabbedPageManager = value ?? new(MauiContext);
			}
		}

		ViewPager2 CreatePlatformView()
		{
			TabbedPageManager.SetElement(this);
			return TabbedPageManager.ViewPager;
		}

		static AView? OnCreatePlatformView(ViewHandler<ITabbedView, AView> arg)
		{
			if (arg.VirtualView is TabbedPage tabbedPage)
				return tabbedPage.CreatePlatformView();

			return null;
		}

		partial void OnHandlerChangingPartial(HandlerChangingEventArgs args)
		{
			if (args.NewHandler != null &&
				_tabbedPageManager != null &&
				args.NewHandler.MauiContext != _tabbedPageManager.MauiContext)
			{
				DisconnectHandler();
				_tabbedPageManager = null;
			}
			else if (args.OldHandler != null && args.NewHandler == null)
				DisconnectHandler();
		}

		void DisconnectHandler()
		{
			_tabbedPageManager?.SetElement(null);
		}

		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateBarBackground();
		}

		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateBarBackgroundColor();
		}

		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateTabItemStyle();
		}

		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateTabItemStyle();
		}

		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateTabItemStyle();
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{

		}

		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{

		}

		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{

		}

		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.ScrollToCurrentPage();
		}

		public static void MapIsSwipePagingEnabled(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateSwipePaging();
		}

		[Obsolete]
		internal static void MapOffscreenPageLimit(ITabbedViewHandler handler, TabbedPage view)
		{
			view._tabbedPageManager?.UpdateOffscreenPageLimit();
		}
	}
}
