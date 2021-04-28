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

namespace Microsoft.Maui.Controls.Platform
{
	class NavHostPageFragment : Fragment
	{
		NavHostFragment NavHost =>
			   (NavHostFragment)
				   Context
					   .GetFragmentManager()
					   .FindFragmentById(Resource.Id.nav_host);

		protected NavHostPageFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			
		}

		public NavHostPageFragment()
		{
		}

		MauiFragmentNavDestination NavDestination { get; set; }

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (NavDestination == null)
			{
				NavDestination =
					(MauiFragmentNavDestination)
						NavHost.NavController.CurrentDestination;
			}

			_ = NavDestination ?? throw new ArgumentNullException(nameof(NavDestination));

			return NavDestination.Page.ToNative(NavDestination.MauiContext);
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RequireActivity()
				.OnBackPressedDispatcher
				.AddCallback(this, new ProcessBackClick(this));
		}

		public void HandleOnBackPressed()
		{
			NavDestination.NavigationPageHandler.OnPop();
		}

		class ProcessBackClick : AndroidX.Activity.OnBackPressedCallback
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
		}
	}
}
