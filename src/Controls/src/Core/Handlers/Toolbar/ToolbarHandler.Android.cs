#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	internal class ToolbarHandler : ElementHandler<Toolbar, MaterialToolbar>
	{
		public static PropertyMapper<Toolbar, ToolbarHandler> Mapper =
			   new PropertyMapper<Toolbar, ToolbarHandler>(ElementMapper)
			   {
				   [nameof(Toolbar.Visible)] = UpdateToolBar,
				   [nameof(Toolbar.BackButtonVisible)] = UpdateToolBar,
				   [nameof(Toolbar.TitleIcon)] = UpdateToolBar,
				   [nameof(Toolbar.TitleView)] = UpdateToolBar,
				   [nameof(Toolbar.IconColor)] = UpdateToolBar,
				   [nameof(Toolbar.Title)] = UpdateToolBar,
				   [nameof(Toolbar.ToolbarItems)] = UpdateToolBar,
				   [nameof(Toolbar.BackButtonTitle)] = UpdateToolBar,
				   [nameof(Toolbar.BarBackgroundColor)] = UpdateToolBar,
				   [nameof(Toolbar.BarBackground)] = UpdateToolBar,
				   [nameof(Toolbar.BarTextColor)] = UpdateToolBar,
				   [nameof(Toolbar.IconColor)] = UpdateToolBar,
			   };

		private static void UpdateToolBar(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.UpdateToolbar();
		}

		IViewHandler? _titleViewHandler;
		Container? _titleView;
		Android.Widget.ImageView? _titleIconView;
		ImageSource? _imageSource;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();
		Drawable? _defaultNavigationIcon;
		//ActionBarDrawerToggle? _drawerToggle;
		//DrawerLayout? _drawerLayout;
		//FlyoutPage? _flyoutPage;

		public static CommandMapper<Toolbar, ToolbarHandler> CommandMapper = new()
		{
		};

		NavigationRootManager? NavigationRootManager =>
			MauiContext?.GetNavigationRootManager();

		public ToolbarHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override MaterialToolbar CreateNativeElement()
		{
			return (MaterialToolbar)NavigationRootManager!.Toolbar;
		}

		protected virtual void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			var toolbarItems = VirtualView.ToolbarItems;
			List<ToolbarItem> newToolBarItems = new List<ToolbarItem>();
			if (toolbarItems != null)
				newToolBarItems.AddRange(toolbarItems);

			NativeView.OnToolbarItemPropertyChanged(e, (ToolbarItem?)sender, newToolBarItems, MauiContext!, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		void UpdateToolbarVisibility()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			_ = MauiContext.Context ?? throw new ArgumentNullException(nameof(MauiContext.Context));

			bool showNavBar = VirtualView.Visible;
			var lp = NativeView.LayoutParameters;
			if (lp == null)
				return;

			if (!showNavBar)
			{
				lp.Height = 0;
			}
			else
			{
				if (VirtualView.BarHeight != null)
					lp.Height = (int)MauiContext.Context.ToPixels(VirtualView.BarHeight.Value);
				else
					lp.Height = ActionBarHeight();
			}

			NativeView.LayoutParameters = lp;

			int ActionBarHeight()
			{
				int actionBarHeight = (int)MauiContext.Context.GetThemeAttributePixels(Resource.Attribute.actionBarSize);
				return actionBarHeight;
			}
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			ToolbarExtensions.UpdateMenuItemIcon(MauiContext, menuItem, toolBarItem, null);
		}



		internal void ToolbarPropertyChanged() => UpdateToolbar();

		void UpdateMenu()
		{
			if (_currentMenuItems == null)
				return;

			NativeView.UpdateMenuItems(VirtualView.ToolbarItems, MauiContext, null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		protected virtual void UpdateToolbar()
		{
			UpdateMenu();

			//ActionBarDrawerToggle? toggle = _drawerToggle;
			bool isNavigated = VirtualView.BackButtonVisible;

			_defaultNavigationIcon ??= NativeView.NavigationIcon;

			if (isNavigated)
			{
				if (VirtualView.BackButtonVisible)
				{
					NativeView.NavigationIcon ??= _defaultNavigationIcon;
					//if (toggle != null)
					//{
					//	toggle.DrawerIndicatorEnabled = false;
					//	toggle.SyncState();
					//}

					var backButtonTitle = VirtualView.BackButtonTitle;
					ImageSource image = VirtualView.TitleIcon;

					if (!string.IsNullOrEmpty(backButtonTitle))
					{
						NativeView.NavigationContentDescription = backButtonTitle;
					}
					else if (image == null ||
						NativeView.SetNavigationContentDescription(image) == null)
					{
						NativeView.SetNavigationContentDescription(Resource.String.nav_app_bar_navigate_up_description);
					}
				}
				//else if (toggle != null && _flyoutPage != null)
				//{
				//	toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
				//	toggle.SyncState();
				//}
				else
				{
					NativeView.NavigationIcon = null;
				}
			}
			else
			{
				//if (toggle != null && _flyoutPage != null)
				//{
				//	toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
				//	toggle.SyncState();
				//	NativeView.SetNavigationContentDescription(Resource.String.nav_app_bar_open_drawer_description);
				//}
			}

			var tintColor = VirtualView.BarBackgroundColor;

			if (tintColor == null)
				NativeView.BackgroundTintMode = null;
			else
			{
				NativeView.BackgroundTintMode = PorterDuff.Mode.Src;
				NativeView.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToNative());
			}

			Brush barBackground = VirtualView.BarBackground;
			NativeView.UpdateBackground(barBackground);

			var textColor = VirtualView.BarTextColor;
			if (textColor != null)
				NativeView.SetTitleTextColor(textColor.ToNative().ToArgb());

			var navIconColor = VirtualView.IconColor;
			if (navIconColor != null && NativeView.NavigationIcon != null)
				DrawableExtensions.SetColorFilter(NativeView.NavigationIcon, navIconColor, FilterMode.SrcAtop);

			NativeView.Title = VirtualView?.Title ?? string.Empty;

			if (NativeView.NavigationIcon != null && textColor != null)
			{
				var icon = this.NativeView.NavigationIcon as DrawerArrowDrawable;
				if (icon != null)
					icon.Color = textColor.ToNative().ToArgb();
			}

			UpdateTitleIcon();
			UpdateTitleView();
			UpdateToolbarVisibility();
		}

		void UpdateTitleIcon()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			_ = MauiContext.Context ?? throw new ArgumentNullException(nameof(MauiContext.Context));

			ImageSource source = VirtualView.TitleIcon;

			if (source == null || source.IsEmpty)
			{
				NativeView.RemoveView(_titleIconView);
				_titleIconView?.Dispose();
				_titleIconView = null;
				_imageSource = null;
				return;
			}

			if (_titleIconView == null)
			{
				_titleIconView = new Android.Widget.ImageView(MauiContext.Context);
				NativeView.AddView(_titleIconView, 0);
			}

			if (_imageSource != source)
			{
				_imageSource = source;
				_titleIconView.SetImageResource(global::Android.Resource.Color.Transparent);

				source.LoadImage(MauiContext, (result) =>
				{
					_titleIconView.SetImageDrawable(result?.Value);
					AutomationPropertiesProvider.AccessibilitySettingsChanged(_titleIconView, source);
				});
			}
		}

		void UpdateTitleView()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			_ = MauiContext.Context ?? throw new ArgumentNullException(nameof(MauiContext.Context));

			VisualElement titleView = VirtualView.TitleView;
			if (_titleViewHandler != null)
			{
				var reflectableType = _titleViewHandler as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _titleViewHandler.GetType();
				if (titleView == null || Internals.Registrar.Registered.GetHandlerTypeForObject(titleView) != rendererType)
				{
					if (_titleView != null)
						_titleView.Child = null;

					if (_titleViewHandler?.VirtualView != null)
						_titleViewHandler.VirtualView.Handler = null;

					_titleViewHandler = null;
				}
			}

			if (titleView == null)
				return;

			if (_titleViewHandler != null)
				_titleViewHandler.SetVirtualView(titleView);
			else
			{
				titleView.ToNative(MauiContext);
				_titleViewHandler = titleView.Handler;

				if (_titleView == null)
				{
					_titleView = new Container(MauiContext.Context);
					NativeView.AddView(_titleView);
				}

				_titleView.Child = (INativeViewHandler)_titleViewHandler;
			}
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

		//void RegisterToolbar()
		//{
		//	Context context = NavigationLayout.Context;
		//	AToolbar bar = Toolbar;
		//	Element page = NavigationView.RealParent;

		//	_flyoutPage = null;
		//	while (page != null)
		//	{
		//		if (page is FlyoutPage)
		//		{
		//			_flyoutPage = page as FlyoutPage;
		//			break;
		//		}
		//		page = page.RealParent;
		//	}

		//	if (_flyoutPage == null)
		//	{
		//		if (PageController.InternalChildren.Count > 0)
		//			_flyoutPage = PageController.InternalChildren[0] as FlyoutPage;

		//		if (_flyoutPage == null)
		//			return;
		//	}

		//	if (((IFlyoutPageController)_flyoutPage).ShouldShowSplitMode)
		//		return;

		//	var renderer = _flyoutPage.ToNative(NavigationView.Handler.MauiContext) as DrawerLayout;
		//	if (renderer == null)
		//		return;

		//	_drawerLayout = renderer;

		//	AutomationPropertiesProvider.GetDrawerAccessibilityResources(context, _flyoutPage, out int resourceIdOpen, out int resourceIdClose);

		//	if (_drawerToggle != null)
		//	{
		//		_drawerToggle.ToolbarNavigationClickListener = null;
		//		_drawerToggle.Dispose();
		//	}

		//	_drawerToggle = new ActionBarDrawerToggle(context.GetActivity(), _drawerLayout, bar,
		//		resourceIdOpen == 0 ? global::Android.Resource.String.Ok : resourceIdOpen,
		//		resourceIdClose == 0 ? global::Android.Resource.String.Ok : resourceIdClose)
		//	{
		//		ToolbarNavigationClickListener = new ClickListener(NavigationView)
		//	};

		//	if (_drawerListener != null)
		//	{
		//		_drawerLayout.RemoveDrawerListener(_drawerListener);
		//		_drawerListener.Dispose();
		//	}

		//	_drawerListener = new DrawerMultiplexedListener { Listeners = { _drawerToggle, (DrawerLayout.IDrawerListener)_drawerLayout } };
		//	_drawerLayout.AddDrawerListener(_drawerListener);
		//}
	}
}
