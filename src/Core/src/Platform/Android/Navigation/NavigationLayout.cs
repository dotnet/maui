using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public class NavigationLayout : CoordinatorLayout, NavController.IOnDestinationChangedListener
	{
		NavHostFragment? _navHost;
		FragmentNavigator? _fragmentNavigator;
		Toolbar? _toolbar;
		AppBarLayout? _appBar;

		internal NavigationStackNavGraph NavGraphDestination =>
			(NavigationStackNavGraph)NavHost.NavController.Graph;

		internal IView? VirtualView { get; private set; }
		internal INavigationView? NavigationView { get; private set; }

		public IMauiContext MauiContext => VirtualView?.Handler?.MauiContext ??
			throw new InvalidOperationException($"MauiContext cannot be null");

#pragma warning disable CS0618 //FIXME: [Preserve] is obsolete
		[Preserve(Conditional = true)]
		public NavigationLayout(Context context) : base(context)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		[Preserve(Conditional = true)]
		protected NavigationLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
#pragma warning restore CS0618 //FIXME: [Preserve] is obsolete

		internal NavHostFragment NavHost
		{
			get => _navHost ?? throw new InvalidOperationException($"NavHost cannot be null");
			set => _navHost = value;
		}

		internal FragmentNavigator FragmentNavigator
		{
			get => _fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");
			set => _fragmentNavigator = value;
		}

		internal Toolbar Toolbar
		{
			get => _toolbar ?? throw new InvalidOperationException($"ToolBar cannot be null");
			set => _toolbar = value;
		}

		internal AppBarLayout AppBar
		{
			get => _appBar ?? throw new InvalidOperationException($"AppBar cannot be null");
			set => _appBar = value;
		}

		public virtual void SetVirtualView(IView navigationView)
		{
			_toolbar = FindViewById<Toolbar>(Resource.Id.maui_toolbar);
			_appBar = FindViewById<AppBarLayout>(Resource.Id.appbar);

			VirtualView = navigationView;
			NavigationView = (INavigationView)navigationView;
		}

		internal void Connect()
		{
			var fragmentManager = Context?.GetFragmentManager();
			_ = fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

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

			var navGraphSwap = new NavigationStackNavGraph(navGraphNavigator);
			navGraphSwap.Initialize(
				NavigationView.NavigationStack,
				this);

			NavHost.NavController.AddOnDestinationChangedListener(this);
			NavHost.ChildFragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleCallback(this), false);
		}

		// Fragments are always destroyed if they aren't visible
		// The Handler/NativeView associated with the visible IView remain intact
		// The performance hit of destorying/recreating fragments should be negligible
		// Hopefully this behavior survives implementation
		// This will need to be tested with Maps and WebViews to make sure they behave efficiently
		// being removed and then added back to a different Fragment
		// 
		// I'm firing NavigationFinished from here instead of FragmentAnimationFinished because
		// this event appears to fire slightly after `FragmentAnimationFinished` and it also fires
		// if we aren't using animations
		private void OnPageFragmentDestroyed(AndroidX.Fragment.App.FragmentManager fm, NavigationViewFragment navHostPageFragment)
		{
			_ = NavigationView ?? throw new InvalidOperationException($"NavigationView cannot be null");

			if (NavGraphDestination.IsNavigating)
			{
				NavGraphDestination.NavigationFinished(this.NavigationView);
			}
		}

		internal void ToolbarPropertyChanged() => UpdateToolbar();

		protected virtual void UpdateToolbar()
		{

		}

		protected virtual void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, NavigationViewFragment navHostPageFragment)
		{
		}

		public virtual void RequestNavigation(MauiNavigationRequestedEventArgs e)
		{
			var graph = (NavigationStackNavGraph)NavHost.NavController.Graph;
			graph.ApplyNavigationRequest(e, this);
		}

		internal void BackButtonPressed()
		{
			_ = NavigationView ?? throw new InvalidOperationException($"NavigationView cannot be null");

			var graph = (NavigationStackNavGraph)NavHost.NavController.Graph;
			var stack = new List<IView>(graph.NavigationStack);
			stack.RemoveAt(stack.Count - 1);
			graph.ApplyNavigationRequest(new MauiNavigationRequestedEventArgs(stack, true) , this);
		}

		#region IOnDestinationChangedListener

		protected virtual void OnDestinationChanged(NavController navController, NavDestination navDestination, Bundle bundle)
		{
		}

		void NavController.IOnDestinationChangedListener.OnDestinationChanged(
			NavController p0, NavDestination p1, Bundle p2)
		{
			OnDestinationChanged(p0, p1, p2);
		}
		#endregion

		class FragmentLifecycleCallback : AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks
		{
			NavigationLayout _navigationLayout;

			public FragmentLifecycleCallback(NavigationLayout navigationLayout)
			{
				_navigationLayout = navigationLayout;
			}

			public override void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavigationViewFragment pf)
					_navigationLayout.OnFragmentResumed(fm, pf);
			}

			public override void OnFragmentAttached(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f, Context context)
			{
				base.OnFragmentAttached(fm, f, context);
			}

			public override void OnFragmentViewDestroyed(
				AndroidX.Fragment.App.FragmentManager fm,
				AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavigationViewFragment pf)
					_navigationLayout.OnPageFragmentDestroyed(fm, pf);

				base.OnFragmentViewDestroyed(fm, f);
			}
		}

	}
}
