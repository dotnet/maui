#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		IViewHandler? _platformTitleViewHandler;
		Container? _platformTitleView;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();

		NavigationRootManager? NavigationRootManager =>
			Handler?.MauiContext?.GetNavigationRootManager();

		MaterialToolbar PlatformView => Handler?.PlatformView as MaterialToolbar ?? throw new InvalidOperationException("Native View not set");

		partial void OnHandlerChanging(IElementHandler oldHandler, IElementHandler newHandler)
		{
			if (newHandler == null)
			{
				if (_platformTitleView != null)
					_platformTitleView.Child = null;

				_platformTitleViewHandler?.DisconnectHandler();

				Controls.Platform.ToolbarExtensions.DisposeMenuItems(
					oldHandler?.PlatformView as AToolbar,
					ToolbarItems,
					OnToolbarItemPropertyChanged);
			}
		}

		void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			var toolbarItems = ToolbarItems;
			List<ToolbarItem> newToolBarItems = new List<ToolbarItem>();
			if (toolbarItems != null)
				newToolBarItems.AddRange(toolbarItems);

			if (sender is ToolbarItem ti)
				PlatformView.OnToolbarItemPropertyChanged(e, ti, newToolBarItems, MauiContext!, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			MauiContext.UpdateMenuItemIcon(menuItem, toolBarItem, null);
		}

		void UpdateMenu()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));

			if (_currentMenuItems == null)
				return;

			PlatformView.UpdateMenuItems(ToolbarItems, MauiContext, BarTextColor, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		void UpdateTitleView()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			_ = MauiContext.Context ?? throw new ArgumentNullException(nameof(MauiContext.Context));

			VisualElement titleView = TitleView;
			if (_platformTitleViewHandler != null)
			{
				Type? rendererType = null;

				if (titleView != null)
					rendererType = MauiContext.Handlers.GetHandlerType(titleView.GetType());

				if (titleView == null || titleView.Handler?.GetType() != rendererType)
				{
					if (_platformTitleView != null)
						_platformTitleView.Child = null;

					if (_platformTitleViewHandler?.VirtualView != null)
						_platformTitleViewHandler.VirtualView.Handler = null;

					_platformTitleViewHandler = null;
				}
			}

			if (titleView == null)
				return;

			if (_platformTitleViewHandler != null)
				_platformTitleViewHandler.SetVirtualView(titleView);
			else
			{
				titleView.ToPlatform(MauiContext);
				_platformTitleViewHandler = titleView.Handler;

				if (_platformTitleView == null)
				{
					var context = MauiContext.Context!;
					_platformTitleView = new Container(context);
					var layoutParams = new MaterialToolbar.LayoutParams(LP.MatchParent, LP.MatchParent);
					_platformTitleView.LayoutParameters = layoutParams;
					PlatformView.AddView(_platformTitleView);
				}

				_platformTitleView.Child = (IPlatformViewHandler?)_platformTitleViewHandler;
			}
		}

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarTextColor((IToolbarHandler)arg1, arg2);

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarBackground((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonTitle((IToolbarHandler)arg1, arg2);

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarItems((IToolbarHandler)arg1, arg2);

		public static void MapTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitle((IToolbarHandler)arg1, arg2);

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapIconColor((IToolbarHandler)arg1, arg2);

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleView((IToolbarHandler)arg1, arg2);

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleIcon((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonVisible((IToolbarHandler)arg1, arg2);

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapIsVisible((IToolbarHandler)arg1, arg2);



		public static void MapBarTextColor(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarTextColor(arg2);
		}

		public static void MapBarBackground(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarBackground(arg2);
		}

		public static void MapBackButtonTitle(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapToolbarItems(IToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateMenu();
		}

		public static void MapTitle(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}

		public static void MapIconColor(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIconColor(arg2);
		}

		public static void MapTitleView(IToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateTitleView();
		}

		public static void MapTitleIcon(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitleIcon(arg2);
		}

		public static void MapBackButtonVisible(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapIsVisible(IToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIsVisible(arg2);
		}





		internal class Container : ViewGroup
		{
			IPlatformViewHandler? _child;

			public Container(Context context) : base(context)
			{
			}

			public IPlatformViewHandler? Child
			{
				set
				{
					_child?.DisconnectHandler();
					RemoveAllViews();

					_child = value;

					if (_child != null)
					{
						var platformView = _child.ToPlatform();
						platformView.RemoveFromParent();
						if (platformView != null)
							AddView(platformView);
					}
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				_child?.LayoutVirtualView(l, t, r, b);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child?.PlatformView == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				var size = _child.MeasureVirtualView(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension((int)size.Width, (int)size.Height);
			}
		}
	}
}
