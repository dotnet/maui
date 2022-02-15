﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		IViewHandler? _nativeTitleViewHandler;
		Container? _nativeTitleView;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();

		NavigationRootManager? NavigationRootManager =>
			Handler?.MauiContext?.GetNavigationRootManager();

		MaterialToolbar PlatformView => Handler?.PlatformView as MaterialToolbar ?? throw new InvalidOperationException("Native View not set");

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

			PlatformView.UpdateMenuItems(ToolbarItems, MauiContext, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		void UpdateTitleView()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			_ = MauiContext.Context ?? throw new ArgumentNullException(nameof(MauiContext.Context));

			VisualElement titleView = TitleView;
			if (_nativeTitleViewHandler != null)
			{
				var reflectableType = _nativeTitleViewHandler as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _nativeTitleViewHandler.GetType();
				if (titleView == null || Internals.Registrar.Registered.GetHandlerTypeForObject(titleView) != rendererType)
				{
					if (_nativeTitleView != null)
						_nativeTitleView.Child = null;

					if (_nativeTitleViewHandler?.VirtualView != null)
						_nativeTitleViewHandler.VirtualView.Handler = null;

					_nativeTitleViewHandler = null;
				}
			}

			if (titleView == null)
				return;

			if (_nativeTitleViewHandler != null)
				_nativeTitleViewHandler.SetVirtualView(titleView);
			else
			{
				titleView.ToPlatform(MauiContext);
				_nativeTitleViewHandler = titleView.Handler;

				if (_nativeTitleView == null)
				{
					_nativeTitleView = new Container(MauiContext.Context);
					PlatformView.AddView(_nativeTitleView);
				}

				_nativeTitleView.Child = (IPlatformViewHandler?)_nativeTitleViewHandler;
			}
		}

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarTextColor(arg2);
		}

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarBackground(arg2);
		}

		public static void MapBarBackgroundColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarBackgroundColor(arg2);
		}

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateMenu();
		}

		public static void MapTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIconColor(arg2);
		}

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateTitleView();
		}

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitleIcon(arg2);
		}

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2)
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
					if (_child != null)
						RemoveView(_child.PlatformView);

					_child = value;

					if (value != null)
						AddView(value.PlatformView);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (_child?.PlatformView == null)
					return;

				_child.PlatformView.Layout(l, t, r, b);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child?.PlatformView == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				_child.PlatformView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(_child.PlatformView.MeasuredWidth, _child.PlatformView.MeasuredHeight);
			}
		}
	}
}
