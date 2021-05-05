using System;
using System.Collections.Generic;
using System.Text;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Microsoft.Maui.Controls.Handlers;
using AView = Android.Views.View;

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

				// TODO MAUI put this elsewhere once we figure out how attached property handlers work
				bool showNavBar = false;
				if (NavDestination.Page is BindableObject bo)
					showNavBar = NavigationPage.GetHasNavigationBar(bo);

				var appBar = NavDestination.NavigationPageHandler.AppBar;
				if (!showNavBar)
				{
					if (appBar.LayoutParameters is CoordinatorLayout.LayoutParams cl)
					{
						cl.Height = 0;
						appBar.LayoutParameters = cl;
					}
				}
				else
				{
					if (appBar.LayoutParameters is CoordinatorLayout.LayoutParams cl)
					{
						cl.Height = ActionBarHeight();
						appBar.LayoutParameters = cl;
					}
				}
			}


			int ActionBarHeight()
			{
				int attr = Resource.Attribute.actionBarSize;

				int actionBarHeight = (int)Context.GetThemeAttributePixels(Resource.Attribute.actionBarSize);

				//if (actionBarHeight <= 0)
				//	return Device.Info.CurrentOrientation.IsPortrait() ? (int)Context.ToPixels(56) : (int)Context.ToPixels(48);

				//if (Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentStatus) || Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentNavigation))
				//{
				//	if (_toolbar.PaddingTop == 0)
				//		_toolbar.SetPadding(0, GetStatusBarHeight(), 0, 0);

				//	return actionBarHeight + GetStatusBarHeight();
				//}

				return actionBarHeight;
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
