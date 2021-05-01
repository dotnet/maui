using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Controls.Handlers;
using AView = Android.Views.View;
using AndroidX.AppCompat.Widget;
using AndroidX.Navigation.UI;
using AndroidX.AppCompat.App;
using Android.Graphics;
using Android.Content.Res;

namespace Microsoft.Maui.Controls.Platform
{
	class NavHostPageFragment : Fragment
	{
		ProcessBackClick BackClick { get; }

		NavHostFragment NavHost =>
			   (NavHostFragment)
				   Context
					   .GetFragmentManager()
					   .FindFragmentById(Resource.Id.nav_host);

		MauiFragmentNavDestination NavDestination { get; set; }

		protected NavHostPageFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			BackClick = new ProcessBackClick(this);
		}

		public NavHostPageFragment()
		{
			BackClick = new ProcessBackClick(this);
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (NavDestination == null)
			{
				NavDestination =
					(MauiFragmentNavDestination)
						NavHost.NavController.CurrentDestination;
			}

			_ = NavDestination ?? throw new ArgumentNullException(nameof(NavDestination));

			var view = NavDestination.Page.ToNative(NavDestination.MauiContext);
			return view;
		}

		public override void OnViewCreated(AView view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			var controller = NavHostFragment.FindNavController(this);
			var appbarConfig =
				new AppBarConfiguration
					.Builder(controller.Graph)
					.Build();

			NavigationUI
				.SetupWithNavController(NavDestination.NavigationPageHandler.Toolbar, controller, appbarConfig);

			HasOptionsMenu = true;

			NavDestination.NavigationPageHandler.Toolbar.SetNavigationOnClickListener(BackClick);

			UpdateToolbar();
			NavDestination.NavigationPageHandler.Toolbar
				.Title = NavDestination.Page.Title;

			if (Context.GetActivity() is AppCompatActivity aca)
			{
				aca.SupportActionBar.Title = NavDestination.Page.Title;
			}
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RequireActivity()
				.OnBackPressedDispatcher
				.AddCallback(this, BackClick);
		}

		public void HandleOnBackPressed()
		{
			NavDestination.NavigationPageHandler.OnPop();
		}

		// TODO Move somewhere else
		void UpdateToolbar()
		{
			var context = Context;
			var bar = NavDestination.NavigationPageHandler.Toolbar;
			//ActionBarDrawerToggle toggle = _drawerToggle;

			if (bar == null)
				return;

			//bool isNavigated = NavigationPageController.StackDepth > 1;
			//bar.NavigationIcon = null;
			Page currentPage = (Page)NavDestination.Page;
			var navPage = NavDestination.NavigationPageHandler.VirtualView;

			//if (isNavigated)
			//{
			//	if (NavigationPage.GetHasBackButton(currentPage) && !Context.IsDesignerContext())
			//	{
			//		if (toggle != null)
			//		{
			//			toggle.DrawerIndicatorEnabled = false;
			//			toggle.SyncState();
			//		}

			//		var activity = (AppCompatActivity)context.GetActivity();
			//		var icon = new DrawerArrowDrawable(activity.SupportActionBar.ThemedContext);
			//		icon.Progress = 1;
			//		bar.NavigationIcon = icon;

			//		var prevPage = Element.Peek(1);
			//		var backButtonTitle = NavigationPage.GetBackButtonTitle(prevPage);
			//		_defaultNavigationContentDescription = backButtonTitle != null
			//			? bar.SetNavigationContentDescription(prevPage, backButtonTitle)
			//			: bar.SetNavigationContentDescription(prevPage, _defaultNavigationContentDescription);
			//	}
			//	else if (toggle != null && _flyoutPage != null)
			//	{
			//		toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
			//		toggle.SyncState();
			//	}
			//}
			//else
			//{
			//	if (toggle != null && _flyoutPage != null)
			//	{
			//		toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
			//		toggle.SyncState();
			//	}
			//}

			var tintColor = navPage.BarBackgroundColor;
			Brush barBackground = navPage.BarBackground;

			if (barBackground == null && tintColor != null)
				barBackground = new SolidColorBrush(tintColor);

			if(barBackground != null)
				bar.UpdateBackground(barBackground);
			//else if (tintColor == null)
			//	bar.BackgroundTintMode = null;
			//else
			//{
			//	bar.Background = null;
			//	bar.BackgroundTintMode = PorterDuff.Mode.Src;
			//	bar.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToNative());
			//}

			if (Context.GetActivity() is AppCompatActivity aca)
			{
			}

			var textColor = navPage.BarTextColor;
			if (textColor != null)
				bar.SetTitleTextColor(textColor.ToNative().ToArgb());

			var navIconColor = NavigationPage.GetIconColor(currentPage);
			if (navIconColor != null && bar.NavigationIcon != null)
				DrawableExtensions.SetColorFilter(bar.NavigationIcon, navIconColor, FilterMode.SrcAtop);

			bar.Title = currentPage?.Title ?? string.Empty;

			//if (_toolbar.NavigationIcon != null && textColor != null)
			//{
			//	var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
			//	if (icon != null)
			//		icon.Color = textColor.ToAndroid().ToArgb();
			//}

			//UpdateTitleIcon();

			//UpdateTitleView();
		}


		class ProcessBackClick : AndroidX.Activity.OnBackPressedCallback, AView.IOnClickListener
		{
			NavHostPageFragment _navHostPageFragment;

			public ProcessBackClick(NavHostPageFragment navHostPageFragment)
				: base(true)
			{
				_navHostPageFragment = navHostPageFragment;
			}

			public override void HandleOnBackPressed()
			{
				_navHostPageFragment.HandleOnBackPressed();
			}

			public void OnClick(AView v)
			{
				HandleOnBackPressed();
			}
		}
	}
}
