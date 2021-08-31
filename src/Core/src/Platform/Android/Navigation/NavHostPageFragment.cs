using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	internal class NavHostPageFragment : Fragment
	{
		NavigationLayout? _navigationLayout;
		NavigationLayout NavigationLayout => _navigationLayout ??= NavDestination.NavigationLayout;

		FragmentNavDestination? _navDestination;

		ProcessBackClick BackClick { get; }

		NavHostFragment NavHost =>
				   (Context?.GetFragmentManager()?.FindFragmentById(Resource.Id.nav_host)
			  as NavHostFragment) ?? throw new InvalidOperationException($"NavHost cannot be null here");

		NavGraphDestination Graph =>
				   (NavHost.NavController.Graph as NavGraphDestination) 
			?? throw new InvalidOperationException($"Graph cannot be null here");

		public FragmentNavDestination NavDestination
		{
			get => _navDestination ?? throw new InvalidOperationException($"NavDestination cannot be null here");
			private set => _navDestination = value;
		}

		protected NavHostPageFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			BackClick = new ProcessBackClick(this);
		}

		public NavHostPageFragment()
		{
			BackClick = new ProcessBackClick(this);
		}

		public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
		{
			int id = 0;

			Animation? returnValue;

			// This means th operation currently being processed shouldn't be animated
			if (Graph.IsPopping == null || !Graph.IsAnimated)
			{
				returnValue = null;
			}
			else
			{
				if (Graph.IsPopping.Value)
				{
					if (!enter)
					{
						id = Resource.Animation.nav_default_pop_exit_anim;
					}
					else
					{
						id = Resource.Animation.nav_default_pop_enter_anim;
					}
				}
				else
				{
					if (enter)
					{
						id = Resource.Animation.nav_default_enter_anim;
					}
					else
					{
						id = Resource.Animation.nav_default_exit_anim;
					}
				}

				if (id > 0)
				{
					returnValue = AnimationUtils.LoadAnimation(Context, id);
				}
				else
				{
					returnValue = base.OnCreateAnimation(transit, enter, id);
				}
			}

			return returnValue!;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (_navDestination == null)
			{
				NavDestination =
					(FragmentNavDestination)
						NavHost.NavController.CurrentDestination;
			}

			_ = NavDestination ?? throw new ArgumentNullException(nameof(NavDestination));

			// TODO Maui can we tranplant the page better?
			// do we care?
			//NavDestination.Page.Handler?.DisconnectHandler();
			//NavDestination.Page.Handler = null;
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
				.SetupWithNavController(NavDestination.NavigationLayout.Toolbar, controller, appbarConfig);

			NavDestination.NavigationLayout.Toolbar.SetNavigationOnClickListener(BackClick);
		}

		public override void OnDestroyView()
		{
			_navigationLayout = null;
			base.OnDestroyView();
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
			NavDestination.NavigationLayout.BackButtonPressed();
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
