#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Handlers;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MaterialToolbar>
	{
		NavigationRootManager? NavigationRootManager =>
			MauiContext?.GetNavigationRootManager();

		protected override MaterialToolbar CreateNativeElement()
		{
			LayoutInflater? li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Microsoft.Maui.Resource.Layout.maui_toolbar, null)?.JavaCast<MaterialToolbar>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.maui_toolbar view not found");

			view.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, MauiContext?.Context?.GetActionBarHeight() ?? LP.WrapContent)
			{
				ScrollFlags = 0
			};

			return view;
		}

		DrawerLayout? _drawerLayout;
		NavController? _navController;
		StackNavigationManager? _stackNavigationManager;

		internal void SetupWithDrawerLayout(DrawerLayout? drawerLayout)
		{
			if (_drawerLayout == drawerLayout)
				return;

			_drawerLayout = drawerLayout;
			SetupToolbar();
		}

		internal void SetupWithNavController(NavController navController, StackNavigationManager stackNavigationManager)
		{
			if (_navController == navController && _stackNavigationManager == stackNavigationManager)
				return;

			_navController = navController;
			_stackNavigationManager = stackNavigationManager;
			SetupToolbar();
		}

		void SetupToolbar()
		{
			if (_stackNavigationManager == null || _navController == null)
				return;

			var appbarConfigBuilder =
					   new AppBarConfiguration
						   .Builder(_stackNavigationManager.NavGraph);

			if (_drawerLayout != null)
				appbarConfigBuilder = appbarConfigBuilder.SetOpenableLayout(_drawerLayout);

			var appbarConfig =
				appbarConfigBuilder.Build();

			NavigationUI
				.SetupWithNavController(NativeView, _navController, appbarConfig);
		}
	}
}
