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
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	class NavHostPageFragment : Fragment
	{
		private MauiFragmentNavDestination? _navDestination;

		ProcessBackClick BackClick { get; }

		NavHostFragment NavHost =>
				   (Context?.GetFragmentManager()?.FindFragmentById(Resource.Id.nav_host)
			  as NavHostFragment) ?? throw new InvalidOperationException($"NavHost cannot be null here");

		MauiFragmentNavDestination NavDestination
		{
			get => _navDestination ?? throw new InvalidOperationException($"NavDestination cannot be null here");
			set => _navDestination = value;
		}

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
			if (_navDestination == null)
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

			var titledElement = NavDestination.Page as ITitledElement;

			NavDestination.NavigationPageHandler.Toolbar.Title = titledElement?.Title;

			if (Context.GetActivity() is AppCompatActivity aca)
			{
				aca.SupportActionBar.Title = titledElement?.Title;

				// TODO MAUI put this elsewhere once we figure out how attached property handlers work
				bool showNavBar = true;
				//if (NavDestination.Page is BindableObject bo)
				//	showNavBar = NavigationPage.GetHasNavigationBar(bo);

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

			public void OnClick(AView? v)
			{
				HandleOnBackPressed();
			}
		}
	}
}
