#nullable enable

using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;
using System;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, AView>
	//, IManageFragments, IOnClickListener, ILifeCycleState
	{
		private NavHostFragment? _navHost;
		private FragmentNavigator? _fragmentNavigator;
		private Toolbar? _toolbar;

		NavHostFragment NavHost
		{
			get => _navHost ?? throw new InvalidOperationException($"NavHost cannot be null");
			set => _navHost = value;
		}

		FragmentNavigator FragmentNavigator
		{
			get => _fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");
			set => _fragmentNavigator = value;
		}

		int NativeNavigationStackCount => NavHost?.NavController.BackStack.Size() - 1 ?? 0;
		int NavigationStackCount => VirtualView?.Navigation.NavigationStack.Count ?? 0;

		internal Toolbar Toolbar 
		{ 
			get => _toolbar ?? throw new InvalidOperationException($"ToolBar cannot be null");
			set => _toolbar = value;
		}

		protected override AView CreateNativeView()
		{
			LayoutInflater? li = LayoutInflater.From(ContextWithValidation());
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.navigationlayout, null).JavaCast<NavigationLayout>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");

			_toolbar = view.FindViewById<Toolbar>(Resource.Id.maui_toolbar);
			return view;
		}

		protected override void ConnectHandler(AView nativeView)
		{
			var fragmentManager = ContextWithValidation().GetFragmentManager();
			_ = fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			NavHost = (NavHostFragment)
				fragmentManager.FindFragmentById(Resource.Id.nav_host);

			FragmentNavigator =
				(FragmentNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));


			var navGraphNavigator =
				(NavGraphNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(NavGraphNavigator)));

			base.ConnectHandler(nativeView);

			INavigationPageController navController = VirtualViewWithValidation();
			navController.PushRequested += OnPushed;
			navController.PopRequested += OnPopped;
			var inflater = NavHost.NavController.NavInflater;
			NavGraph graph = new NavGraph(navGraphNavigator);

			NavDestination navDestination;
			List<int> destinations = new List<int>();
			foreach (var page in VirtualView.Navigation.NavigationStack)
			{
				navDestination =
					MauiFragmentNavDestination.
						AddDestination(
							page,
							this,
							graph,
							FragmentNavigator);

				destinations.Add(navDestination.Id);
			}

			graph.StartDestination = destinations[0];

			NavHost.NavController.SetGraph(graph, null);

			for (var i = NativeNavigationStackCount; i < NavigationStackCount; i++)
			{
				var dest = destinations[i];
				NavHost.NavController.Navigate(dest);
			}
		}

		protected override void DisconnectHandler(AView nativeView)
		{
			base.DisconnectHandler(nativeView);
			var navController = (INavigationPageController)VirtualViewWithValidation();
			navController.PushRequested -= OnPushed;
			navController.PopRequested -= OnPopped;
		}

		void OnPushed(object? sender, NavigationRequestedEventArgs e)
		{
			var destination =
				MauiFragmentNavDestination.AddDestination(e.Page, this, NavHost.NavController.Graph, FragmentNavigator);

			NavHost.NavController.Navigate(destination.Id, null);
		}

		internal void OnPop()
		{
			VirtualViewWithValidation()
				.Navigation
				.PopAsync()
				.FireAndForget((e) =>
			{
				Log.Warning(nameof(NavigationPageHandler), $"{e}");
			});
		}

		void OnPopped(object? sender, NavigationRequestedEventArgs e)
		{
			NavHost.NavController.NavigateUp();
		}

		void UpdatePadding()
		{
		}

		void UpdateTitleColor()
		{
		}
		void UpdateNavigationBarBackground()
		{
		}
		void UpdateTitleIcon()
		{
		}
		void UpdateTitleView()
		{
		}


		public static void MapPadding(NavigationPageHandler handler, NavigationPage view)
			=> handler.UpdatePadding();

		public static void MapTitleColor(NavigationPageHandler handler, NavigationPage view)
			=> handler.UpdateTitleColor();

		public static void MapNavigationBarBackground(NavigationPageHandler handler, NavigationPage view)
			=> handler.UpdateNavigationBarBackground();

		// TODO MAUI: Task Based Mappers?
		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view)
			=> handler.UpdateTitleIcon();

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view)
			=> handler.UpdateTitleView();
	}
}
