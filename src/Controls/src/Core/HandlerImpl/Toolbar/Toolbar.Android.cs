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

		MaterialToolbar NativeView => Handler?.NativeView as MaterialToolbar ?? throw new InvalidOperationException("Native View not set");

		void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			var toolbarItems = ToolbarItems;
			List<ToolbarItem> newToolBarItems = new List<ToolbarItem>();
			if (toolbarItems != null)
				newToolBarItems.AddRange(toolbarItems);

			if (sender is ToolbarItem ti)
				NativeView.OnToolbarItemPropertyChanged(e, ti, newToolBarItems, MauiContext!, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			ToolbarExtensions.UpdateMenuItemIcon(MauiContext, menuItem, toolBarItem, null);
		}

		void UpdateMenu()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));

			if (_currentMenuItems == null)
				return;

			NativeView.UpdateMenuItems(ToolbarItems, MauiContext, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
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
					NativeView.AddView(_nativeTitleView);
				}

				_nativeTitleView.Child = (INativeViewHandler?)_nativeTitleViewHandler;
			}
		}

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateBarTextColor(arg2);
		}

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateBarBackground(arg2);
		}

		public static void MapBarBackgroundColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateBarBackgroundColor(arg2);
		}

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateBackButton(arg2);
		}

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateMenu();
		}

		public static void MapTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateTitle(arg2);
		}

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateIconColor(arg2);
		}

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateTitleView();
		}

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateTitleIcon(arg2);
		}

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateBackButton(arg2);
		}

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.NativeView.UpdateIsVisible(arg2);
		}

		internal class Container : ViewGroup
		{
			INativeViewHandler? _child;

			public Container(Context context) : base(context)
			{
			}

			public INativeViewHandler? Child
			{
				set
				{
					if (_child != null)
						RemoveView(_child.NativeView);

					_child = value;

					if (value != null)
						AddView(value.NativeView);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (_child?.NativeView == null)
					return;

				_child.NativeView.Layout(l, t, r, b);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child?.NativeView == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				_child.NativeView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(_child.NativeView.MeasuredWidth, _child.NativeView.MeasuredHeight);
			}
		}
	}
}
