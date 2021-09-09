using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers;
using static Android.Views.View;
using static Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage;
using ActionBarDrawerToggle = AndroidX.AppCompat.App.ActionBarDrawerToggle;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
{
	// This class is still a bit too over complicated
	// As we move things into Core we'll better split it apart
	public class ControlsNavigationManager : NavigationManager, IManageFragments
	{

		AppBarLayout _appBar;
		Fragment _tabLayoutFragment;
		internal AppBarLayout AppBar =>
			_appBar ??= NavigationLayout.FindViewById<AppBarLayout>(Resource.Id.appbar)
			?? throw new InvalidOperationException($"AppBar cannot be null");

		ActionBarDrawerToggle _drawerToggle;
		FragmentManager _fragmentManager;
		ToolbarTracker ToolbarTracker;
		DrawerMultiplexedListener _drawerListener;
		DrawerLayout _drawerLayout;
		FlyoutPage _flyoutPage;
		IViewHandler _titleViewHandler;
		Container _titleView;
		Android.Widget.ImageView _titleIconView;
		ImageSource _imageSource;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();

		new NavigationPage NavigationView => (NavigationPage)base.VirtualView;

		new Page CurrentPage => (Page)base.CurrentPage;
		public ControlsNavigationManager(IMauiContext mauiContext) : base(mauiContext)
		{
		}

		INavigationPageController NavigationPageController => NavigationView as INavigationPageController;

		IPageController PageController => NavigationView;


		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		public override void Connect(IView navigationView, NavigationLayout nativeView)
		{
			base.Connect(navigationView, nativeView);

			if (ToolbarTracker == null)
			{
				ToolbarTracker = new ToolbarTracker();
				ToolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
			}

			ToolbarTracker.AdditionalTargets = NavigationView.GetParentPages();
		}

		// These are only relevant when nested inside a drawer layout
		void AnimateArrowIn()
		{
			var icon = Toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(0, 1);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		void AnimateArrowOut()
		{
			var icon = Toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(1, 0);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		public void OnClick(AView v)
		{
			NavigationView?.PopAsync();
		}

		public override void RequestNavigation(NavigationRequest e)
		{
			NavAnimationInProgress = true;
			base.RequestNavigation(e);
			NavAnimationInProgress = false;

			var currentStack = NavigationStack;
			var newStackSize = e.NavigationStack.Count;

			bool animated = e.Animated;
			bool removed = currentStack.Count > newStackSize;

			if (animated)
			{
				var page = (Page)e.NavigationStack[newStackSize - 1];
				if (!removed)
				{
					UpdateToolbar();
					if (_drawerToggle != null && NavigationPageController.StackDepth == 2 &&
						NavigationPage.GetHasBackButton(page))
						AnimateArrowIn();
				}
				else if (_drawerToggle != null && NavigationPageController.StackDepth == 2 &&
					NavigationPage.GetHasBackButton(page))
				{
					AnimateArrowOut();
				}
			}
		}

		internal void SetTabLayout(TabbedPageHandler tabbedPageHandler)
		{
			if (tabbedPageHandler == null)
			{
				if (_tabLayoutFragment != null)
				{
					MauiContext
						.GetFragmentManager()
						.BeginTransaction()
						.Remove(_tabLayoutFragment)
						.SetReorderingAllowed(true)
						.Commit();

					_tabLayoutFragment = null;
				}

				return;
			}

			int id;
			if(tabbedPageHandler.BottomNavigationView != null)
			{
				id = Resource.Id.bottom_tab_container_view;
				_tabLayoutFragment = new ViewFragment(tabbedPageHandler.BottomNavigationView);
			}
			else
			{
				_tabLayoutFragment = new ViewFragment(tabbedPageHandler.TabLayout);
				id = Resource.Id.tab_container_view;
			}

			MauiContext
					.GetFragmentManager()
					.BeginTransaction()
					.Replace(id, _tabLayoutFragment)
					.SetReorderingAllowed(true)
					.Commit();
		}

		protected override void OnNavigationViewFragmentResumed(FragmentManager fm, NavigationViewFragment navHostPageFragment)
		{
			base.OnNavigationViewFragmentResumed(fm, navHostPageFragment);
			// This appears to be the best place to update the toolbar so that the tinting works
			// Any early and the tinting will be replaced by the native tinting
			UpdateToolbar();
		}

		protected override void OnDestinationChanged(NavController navController, NavDestination navDestination, Bundle bundle)
		{
			base.OnDestinationChanged(navController, navDestination, bundle);

			if (ToolbarTracker != null)
			{
				ToolbarTracker.Target = CurrentPage;
			}

			if (CurrentPage.Handler == null)
			{
				CurrentPage.HandlerChanged += OnHandlerChanged;
			}
			else
			{
				UpdateTablayout();
			}

			void OnHandlerChanged(object sender, EventArgs __)
			{
				UpdateTablayout();
				((Element)sender).HandlerChanged -= OnHandlerChanged;
			}

			void UpdateTablayout()
			{
				if (CurrentPage.Handler is TabbedPageHandler tph)
				{
					SetTabLayout(tph);
				}
				else
				{
					SetTabLayout(null);
				}
			}
		}

		void RegisterToolbar()
		{
			Context context = NavigationLayout.Context;
			AToolbar bar = Toolbar;
			Element page = NavigationView.RealParent;

			_flyoutPage = null;
			while (page != null)
			{
				if (page is FlyoutPage)
				{
					_flyoutPage = page as FlyoutPage;
					break;
				}
				page = page.RealParent;
			}

			if (_flyoutPage == null)
			{
				if (PageController.InternalChildren.Count > 0)
					_flyoutPage = PageController.InternalChildren[0] as FlyoutPage;

				if (_flyoutPage == null)
					return;
			}

			if (((IFlyoutPageController)_flyoutPage).ShouldShowSplitMode)
				return;

			var renderer = _flyoutPage.ToNative(NavigationView.Handler.MauiContext) as DrawerLayout;
			if (renderer == null)
				return;

			_drawerLayout = renderer;

			AutomationPropertiesProvider.GetDrawerAccessibilityResources(context, _flyoutPage, out int resourceIdOpen, out int resourceIdClose);

			if (_drawerToggle != null)
			{
				_drawerToggle.ToolbarNavigationClickListener = null;
				_drawerToggle.Dispose();
			}

			_drawerToggle = new ActionBarDrawerToggle(context.GetActivity(), _drawerLayout, bar,
				resourceIdOpen == 0 ? global::Android.Resource.String.Ok : resourceIdOpen,
				resourceIdClose == 0 ? global::Android.Resource.String.Ok : resourceIdClose)
			{
				ToolbarNavigationClickListener = new ClickListener(NavigationView)
			};

			if (_drawerListener != null)
			{
				_drawerLayout.RemoveDrawerListener(_drawerListener);
				_drawerListener.Dispose();
			}

			_drawerListener = new DrawerMultiplexedListener { Listeners = { _drawerToggle, (DrawerLayout.IDrawerListener)_drawerLayout } };
			_drawerLayout.AddDrawerListener(_drawerListener);
		}


		// AFAICT this is specific to ListView and Context Items
		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}

		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateMenu();
		}

		void UpdateMenu()
		{
			if (_currentMenuItems == null)
				return;

			Toolbar.UpdateMenuItems(ToolbarTracker?.ToolbarItems, NavigationView.FindMauiContext(), null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		protected virtual void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var toolbarItems = ToolbarTracker?.ToolbarItems;
			List<ToolbarItem> newToolBarItems = new List<ToolbarItem>();
			if (toolbarItems != null)
				newToolBarItems.AddRange(toolbarItems);

			Toolbar.OnToolbarItemPropertyChanged(e, (ToolbarItem)sender, newToolBarItems, NavigationView.FindMauiContext(), null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			ToolbarExtensions.UpdateMenuItemIcon(NavigationView.FindMauiContext(), menuItem, toolBarItem, null);
		}


		void UpdateToolbarVisibility()
		{
			bool showNavBar = NavigationPage.GetHasNavigationBar(CurrentPage);
			var lp = Toolbar.LayoutParameters;
			if (lp == null)
				return;

			if (!showNavBar)
			{
				lp.Height = 0;
			}
			else
			{
				if (NavigationView.IsSet(BarHeightProperty))
					lp.Height = NavigationView.OnThisPlatform().GetBarHeight();
				else
					lp.Height = ActionBarHeight();
			}

			Toolbar.LayoutParameters = lp;

			int ActionBarHeight()
			{
				int actionBarHeight = (int)NavigationLayout.Context.GetThemeAttributePixels(Resource.Attribute.actionBarSize);
				return actionBarHeight;
			}
		}


		Drawable _defaultNavigationIcon;

		internal void ToolbarPropertyChanged() => UpdateToolbar();

		protected virtual void UpdateToolbar()
		{
			ActionBarDrawerToggle toggle = _drawerToggle;

			if (Toolbar == null || NavigationStack.Count == 0 || CurrentPage == null || VirtualView == null)
				return;

			bool isNavigated = NavigationStack.Count > 1;
			Page currentPage = CurrentPage;

			_defaultNavigationIcon ??= Toolbar.NavigationIcon;

			if (isNavigated)
			{
				if (NavigationPage.GetHasBackButton(currentPage))
				{
					Toolbar.NavigationIcon ??= _defaultNavigationIcon;
					if (toggle != null)
					{
						toggle.DrawerIndicatorEnabled = false;
						toggle.SyncState();
					}

					var prevPage = (Page)NavigationStack[NavigationStack.Count - 2];
					var backButtonTitle = NavigationPage.GetBackButtonTitle(prevPage);

					ImageSource image = NavigationPage.GetTitleIconImageSource(currentPage);
					if (!string.IsNullOrEmpty(backButtonTitle))
					{
						Toolbar.NavigationContentDescription = backButtonTitle;
					}
					else if (image == null ||
						Toolbar.SetNavigationContentDescription(image) == null)
					{
						Toolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_navigate_up_description);
					}
				}
				else if (toggle != null && _flyoutPage != null)
				{
					toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
					toggle.SyncState();
				}
				else
				{
					Toolbar.NavigationIcon = null;
				}
			}
			else
			{
				if (toggle != null && _flyoutPage != null)
				{
					toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
					toggle.SyncState();
					Toolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_open_drawer_description);
				}
			}

			Color tintColor = NavigationView.BarBackgroundColor;

			if (tintColor == null)
				Toolbar.BackgroundTintMode = null;
			else
			{
				Toolbar.BackgroundTintMode = PorterDuff.Mode.Src;
				Toolbar.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToNative());
			}

			Brush barBackground = NavigationView.BarBackground;
			Toolbar.UpdateBackground(barBackground);

			Color textColor = NavigationView.BarTextColor;
			if (textColor != null)
				Toolbar.SetTitleTextColor(textColor.ToNative().ToArgb());

			Color navIconColor = NavigationPage.GetIconColor(CurrentPage);
			if (navIconColor != null && Toolbar.NavigationIcon != null)
				DrawableExtensions.SetColorFilter(Toolbar.NavigationIcon, navIconColor, FilterMode.SrcAtop);

			Toolbar.Title = currentPage?.Title ?? string.Empty;

			if (Toolbar.NavigationIcon != null && textColor != null)
			{
				var icon = this.Toolbar.NavigationIcon as DrawerArrowDrawable;
				if (icon != null)
					icon.Color = textColor.ToNative().ToArgb();
			}

			UpdateTitleIcon();
			UpdateTitleView();
			UpdateToolbarVisibility();
		}

		void UpdateTitleIcon()
		{
			Page currentPage = CurrentPage;

			if (currentPage == null)
				return;

			ImageSource source = NavigationPage.GetTitleIconImageSource(currentPage);

			if (source == null || source.IsEmpty)
			{
				Toolbar.RemoveView(_titleIconView);
				_titleIconView?.Dispose();
				_titleIconView = null;
				_imageSource = null;
				return;
			}

			if (_titleIconView == null)
			{
				_titleIconView = new Android.Widget.ImageView(NavigationLayout.Context);
				Toolbar.AddView(_titleIconView, 0);
			}

			if (_imageSource != source)
			{
				_imageSource = source;
				_titleIconView.SetImageResource(global::Android.Resource.Color.Transparent);

				ImageSourceLoader.LoadImage(source, MauiContext, (result) =>
				{
					_titleIconView.SetImageDrawable(result.Value);
					AutomationPropertiesProvider.AccessibilitySettingsChanged(_titleIconView, source);
				});
			}
		}

		void UpdateTitleView()
		{
			AToolbar bar = Toolbar;

			if (bar == null)
				return;

			Page currentPage = CurrentPage;

			if (currentPage == null)
				return;

			VisualElement titleView = NavigationPage.GetTitleView(currentPage);
			if (_titleViewHandler != null)
			{
				var reflectableType = _titleViewHandler as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _titleViewHandler.GetType();
				if (titleView == null || Internals.Registrar.Registered.GetHandlerTypeForObject(titleView) != rendererType)
				{
					if (_titleView != null)
						_titleView.Child = null;

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
					_titleView = new Container(NavigationLayout.Context);
					bar.AddView(_titleView);
				}

				_titleView.Child = (INativeViewHandler)_titleViewHandler;
			}
		}

		class ClickListener : Object, IOnClickListener
		{
			readonly NavigationPage _element;

			public ClickListener(NavigationPage element)
			{
				_element = element;
			}

			public void OnClick(AView v)
			{
				_element?.PopAsync();
			}
		}

		internal class Container : ViewGroup
		{
			INativeViewHandler _child;

			public Container(Context context) : base(context)
			{
			}

			public INativeViewHandler Child
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
				if (_child == null)
					return;

				_child.NativeView.Layout(l, t, r, b);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				_child.NativeView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(_child.NativeView.MeasuredWidth, _child.NativeView.MeasuredHeight);
			}
		}

		class DrawerMultiplexedListener : Object, DrawerLayout.IDrawerListener
		{
			public List<DrawerLayout.IDrawerListener> Listeners { get; } = new List<DrawerLayout.IDrawerListener>(2);

			public void OnDrawerClosed(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerClosed(drawerView);
			}

			public void OnDrawerOpened(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerOpened(drawerView);
			}

			public void OnDrawerSlide(AView drawerView, float slideOffset)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerSlide(drawerView, slideOffset);
			}

			public void OnDrawerStateChanged(int newState)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerStateChanged(newState);
			}
		}
	}
}
