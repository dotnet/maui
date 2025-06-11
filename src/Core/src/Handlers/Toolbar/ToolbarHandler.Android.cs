﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
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

		ProcessBackClick? _processBackClick;
		ProcessBackClick BackNavigationClick => _processBackClick ??= new ProcessBackClick(this);

		protected override MaterialToolbar CreatePlatformElement()
		{
			var context = MauiContext?.Context ?? throw new InvalidOperationException("Context cannot be null");
			return Microsoft.Maui.PlatformInterop.CreateToolbar(context, context.GetActionBarHeight(), -1);
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			if (platformView is MaterialToolbar mt && mt.IsAlive())
				mt.RemoveFromParent();

			PlatformView.NavigationClick -= PlatformViewNavigationClick;
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}

		DrawerLayout? _drawerLayout;
		Drawable? _drawerIcon;
		NavController? _navController;
		StackNavigationManager? _stackNavigationManager;

		internal void SetupWithDrawerLayout(DrawerLayout? drawerLayout)
		{
			if (_drawerLayout == drawerLayout)
			{
				PlatformView.SetNavigationOnClickListener(BackNavigationClick);
				return;
			}

			_drawerLayout = drawerLayout;
			SetupToolbar();
		}

		internal async void UpdateToolbarIcon(IImageSource imageSource)
		{
			var image = await imageSource.GetPlatformImageAsync(MauiContext!);
			var drawable = image?.Value;
			if (drawable != null)
			{
				_drawerIcon = drawable;
				PlatformView.NavigationIcon = drawable;
				PlatformView.NavigationClick -= PlatformViewNavigationClick;
				PlatformView.NavigationClick += PlatformViewNavigationClick;
			}
		}

		internal void SetupWithNavController(NavController navController, StackNavigationManager stackNavigationManager)
		{
			if (_navController == navController && _stackNavigationManager == stackNavigationManager)
			{
				PlatformView.SetNavigationOnClickListener(BackNavigationClick);
				return;
			}

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
				.SetupWithNavController(PlatformView, _navController, appbarConfig);

			// the call to SetupWithNavController resets the Navigation Icon
			UpdateValue(nameof(IToolbar.BackButtonVisible));
			PlatformView.SetNavigationOnClickListener(BackNavigationClick);

			if (!VirtualView.BackButtonVisible && _drawerIcon != null)
			{
				PlatformView.NavigationIcon = _drawerIcon;
			}
		}

		void PlatformViewNavigationClick(object? sender, Toolbar.NavigationClickEventArgs e)
		{
			BackClick();
		}

		void BackClick()
		{
			if (VirtualView.BackButtonVisible && VirtualView.IsVisible)
			{
				_ = MauiContext?.GetActivity().GetWindow()?.BackButtonClicked();
			}
			else
			{
				_drawerLayout?.Open();
			}
		}

		class ProcessBackClick : Java.Lang.Object, View.IOnClickListener
		{
			ToolbarHandler _toolbarHandler;

			public ProcessBackClick(ToolbarHandler toolbarHandler)
			{
				_toolbarHandler = toolbarHandler;
			}

			public void OnClick(View? v)
			{
				_toolbarHandler.BackClick();
			}
		}
	}
}
