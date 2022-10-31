using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class StackNavigationManager
	{
		NavHostFragment? _navHost;
		FragmentNavigator? _fragmentNavigator;
		NavGraph? _navGraph;
		IView? _currentPage;
		Callbacks? _fragmentLifecycleCallbacks;
		internal IView? VirtualView { get; private set; }
		internal IStackNavigation? NavigationView { get; private set; }
		internal bool IsNavigating => ActiveRequestedArgs != null;
		internal bool IsInitialNavigation { get; private set; }
		internal bool? IsPopping { get; private set; }
		internal bool IsAnimated { get; set; } = true;
		internal NavigationRequest? ActiveRequestedArgs { get; private set; }
		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		internal NavHostFragment NavHost =>
			_navHost ?? throw new InvalidOperationException($"NavHost cannot be null");

		internal FragmentNavigator FragmentNavigator =>
			_fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");

		internal NavGraph NavGraph => _navGraph ??
			throw new InvalidOperationException($"NavGraph cannot be null");

		public IView CurrentPage
			=> _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");

		public IMauiContext MauiContext { get; }

		internal IToolbarElement? ToolbarElement =>
			MauiContext.GetNavigationRootManager().ToolbarElement;

		public StackNavigationManager(IMauiContext mauiContext)
		{
			var currentInflater = mauiContext.GetLayoutInflater();
			var inflater =
				new StackLayoutInflater(
					currentInflater,
					currentInflater.Context,
					this);

			MauiContext =
				mauiContext.MakeScoped(inflater, context: inflater.Context);
		}

		/*
		 * The important thing to know going into reading this method is that it's not possible to
		 * modify the backstack. You can only push and pop to and from the top of the stack.
		 * So if a user uses an API like `RemovePage` or `InsertPage` we will typically ignore processing those here
		 * unless it requires changes to the NavBar (i.e removing the first page with only 2 pages on the stack).
		 * Once the user performs an operation that changes the currently visible page then we process any stack changes
		 * that have occurred.
		 * Let's say the user has pages A,B,C,D on the stack 
		 * If they remove Page B and Page C then we don't do anything. Then if the user pushes E onto the stack
		 * we just transform A,B,C,D into A,D,E.
		 * Platform wise that's a "pop" but we use the correct animation for a "push" so visually it looks like a push.
		 * This is also the reason why we aren't using the custom animation features on the navigation component itself.
		 * Because we might be popping but visually pushing.
		 * 
		 * The Fragments that are on the stack also do not have a hard connection to the page they originally rendered.
		 * Whenever a fragment is the "visible" fragment it just figures out what the current page is and displays that.
		 * Fragments are recreated every time they are pushed on the stack but the handler renderer is not.
		 * It's just attached to a new fragment
		 * */
		void ApplyNavigationRequest(NavigationRequest args)
		{
			if (IsNavigating)
			{
				// This should really never fire for the developer. Our xplat code should be handling waiting for navigation to
				// complete before requesting another navigation from Core
				// Maybe some day we'll put a navigation queue into Core? For now we won't
				throw new InvalidOperationException("Previous Navigation Request is still Processing");
			}

			if (args.NavigationStack.Count == 0)
			{
				throw new InvalidOperationException("NavigationStack cannot be empty");
			}

			ActiveRequestedArgs = args;
			IReadOnlyList<IView> newPageStack = args.NavigationStack;
			bool animated = args.Animated;
			var navController = NavHost.NavController;
			var previousNavigationStack = NavigationStack;
			var previousNavigationStackCount = previousNavigationStack.Count;
			bool initialNavigation = NavigationStack.Count == 0;

			// This updates the graphs public navigation stack property so it's outwardly correct
			// But we've saved off the previous stack so we can correctly interpret navigation
			UpdateNavigationStack(newPageStack);

			// This indicates that this is the first navigation request so we need to initialize the graph
			if (initialNavigation)
			{
				IsInitialNavigation = true;
				Initialize(args.NavigationStack);
				return;
			}

			// If the new stack isn't changing the visible page or the app bar then we just ignore
			// the changes because there's no point to applying these to the platform back stack
			// We only apply changes when the currently visible page changes and/or the appbar
			// will change (gain a back button)
			if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
			{
				NavigationFinished(NavigationView);

				// There's only one page on the stack then we trigger back button visibility logic
				// so that it can add a back button if it needs to
				if (previousNavigationStackCount == 1 || newPageStack.Count == 1)
					TriggerBackButtonVisibleUpdate();

				return;
			}

			// The incoming fragment uses these variables to pick the correct animation for the current
			// incoming navigation request
			if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
			{
				IsPopping = null;
			}
			else
			{

				IsPopping = newPageStack.Count < previousNavigationStackCount;
			}

			IsAnimated = animated;

			var iterator = NavHost.NavController.BackQueue.Iterator();
			var fragmentNavDestinations = new List<FragmentNavigator.Destination>();

			while (iterator.HasNext)
			{
				if (iterator.Next() is NavBackStackEntry nbse &&
					nbse.Destination is FragmentNavigator.Destination nvd)
				{
					fragmentNavDestinations.Add(nvd);
				}
			}

			// Current BackStack has less entries then incoming new page stack
			// This will add Back Stack Entries until the back stack and the new stack 
			// match up
			if (fragmentNavDestinations.Count < newPageStack.Count)
			{
				for (int i = fragmentNavDestinations.Count; i < newPageStack.Count; i++)
				{
					var dest = AddFragmentDestination();
					navController.Navigate(dest.Id);
				}
			}
			// User just wants to replace the currently visible page but the number
			// of items in the stack are still the same. 
			// In theory we could just prompt the currently active fragment to swap out the new PageView
			// But this way we get an animation
			else if (newPageStack.Count == fragmentNavDestinations.Count)
			{
				int lastFragId = fragmentNavDestinations[newPageStack.Count - 1].Id;
				navController.PopBackStack();
				navController.Navigate(lastFragId);
			}
			// Our back stack has more entries on it then  
			else
			{
				int popToId = fragmentNavDestinations[newPageStack.Count - 1].Id;
				navController.PopBackStack(popToId, false);
			}

			// We only keep destinations around that are on the backstack
			// This iterates over the new backstack and removes any destinations
			// that are no longer apart of the back stack
			var iterateNewStack = NavHost.NavController.BackQueue.Iterator();
			int startId = -1;
			while (iterateNewStack.HasNext)
			{
				if (iterateNewStack.Next() is NavBackStackEntry nbse &&
					nbse.Destination is FragmentNavigator.Destination nvd)
				{
					fragmentNavDestinations.Remove(nvd);

					if (startId == -1)
						startId = nvd.Id;
				}
			}

			foreach (var activeDestinations in fragmentNavDestinations)
			{
				NavGraph.Remove(activeDestinations);
			}

			// If we end up removing the destination that was initially the StartDestination
			// The Navigation Graph can get really confused
			if (NavGraph.StartDestination != startId)
				NavGraph.StartDestination = startId;

			// The NavigationIcon on the toolbar gets set inside the Navigate call so this is the earliest
			// point in time that we can setup toolbar colors for the incoming page
			TriggerBackButtonVisibleUpdate();
		}

		void TriggerBackButtonVisibleUpdate()
		{
			if (NavigationView != null)
			{
				ToolbarElement?.Toolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
			}
		}

		public virtual FragmentNavigator.Destination AddFragmentDestination()
		{
			var destination = new FragmentNavigator.Destination(FragmentNavigator);
			var canonicalName = Java.Lang.Class.FromType(typeof(NavigationViewFragment)).CanonicalName;

			if (canonicalName != null)
				destination.SetClassName(canonicalName);

			destination.Id = AView.GenerateViewId();
			NavGraph.AddDestination(destination);
			return destination;
		}

		internal void NavigationFinished(IStackNavigation? navigationView)
		{
			IsInitialNavigation = false;
			IsPopping = null;
			ActiveRequestedArgs = null;
			navigationView?.NavigationFinished(NavigationStack);
		}

		// This occurs when the navigation page is first being renderer so we sync up the
		// Navigation Stack on the INavigationView to our platform stack
		List<int> Initialize(IReadOnlyList<IView> pages)
		{
			var navController = NavHost.NavController;

			// We are subtracting one because the navgraph itself is the first item on the stack
			int PlatformNavigationStackCount = navController.BackQueue.Size() - 1;

			// set this to one because when the graph is first attached to the controller
			// it will add the graph and the first destination
			if (PlatformNavigationStackCount < 0)
				PlatformNavigationStackCount = 1;

			List<int> destinations = new List<int>();

			NavDestination navDestination;

			foreach (var page in pages)
			{
				navDestination = AddFragmentDestination();
				destinations.Add(navDestination.Id);
			}

			NavGraph.StartDestination = destinations[0];
			navController.SetGraph(NavGraph, null);

			for (var i = PlatformNavigationStackCount; i < pages.Count; i++)
			{
				var dest = destinations[i];
				navController.Navigate(dest);
			}

			UpdateNavigationStack(pages);
			return destinations;
		}

		void UpdateNavigationStack(IReadOnlyList<IView> newPageStack)
		{
			NavigationStack = new List<IView>(newPageStack);
			_currentPage = NavigationStack[NavigationStack.Count - 1];
		}

		public virtual void Disconnect()
		{
			if (_fragmentLifecycleCallbacks != null)
			{
				if (_navHost?.NavController != null && _navHost.NavController.IsAlive())
					_navHost.NavController.RemoveOnDestinationChangedListener(_fragmentLifecycleCallbacks);

				ChildFragmentManager?.UnregisterFragmentLifecycleCallbacks(_fragmentLifecycleCallbacks);

				_fragmentLifecycleCallbacks.Disconnect();
				_fragmentLifecycleCallbacks = null;
			}

			VirtualView = null;
			NavigationView = null;
			_navHost = null;
			_fragmentNavigator = null;
		}

		public virtual void Connect(IView navigationView)
		{
			VirtualView = navigationView;
			NavigationView = (IStackNavigation)navigationView;

			var fragmentManager = MauiContext?.GetFragmentManager();
			_ = fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			var navHostFragment = fragmentManager.FindFragmentById(Resource.Id.nav_host);
			_navHost = navHostFragment as NavHostFragment;

			if (_navHost == null)
				throw new InvalidOperationException($"No NavHostFragment found");

			System.Diagnostics.Debug.WriteLine($"_navHost: {_navHost} {_navHost.GetHashCode()}");

			_fragmentNavigator =
				(FragmentNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));
		}

		public virtual void RequestNavigation(NavigationRequest e)
		{
			if (_navGraph == null)
			{
				var navGraphNavigator =
				   (NavGraphNavigator)NavHost
					   .NavController
					   .NavigatorProvider
					   .GetNavigator(Java.Lang.Class.FromType(typeof(NavGraphNavigator)));

				_navGraph = new NavGraph(navGraphNavigator);
			}

			if (_fragmentLifecycleCallbacks == null)
			{
				_fragmentLifecycleCallbacks = new Callbacks(this);
				NavHost.NavController.AddOnDestinationChangedListener(_fragmentLifecycleCallbacks);
				ChildFragmentManager?.RegisterFragmentLifecycleCallbacks(_fragmentLifecycleCallbacks, false);
			}

			ApplyNavigationRequest(e);
		}

		// Fragments are always destroyed if they aren't visible
		// The Handler/PlatformView associated with the visible IView remain intact
		// The performance hit of destroying/recreating fragments should be negligible
		// Hopefully this behavior survives implementation
		// This will need to be tested with Maps and WebViews to make sure they behave efficiently
		// being removed and then added back to a different Fragment
		// 
		// I'm firing NavigationFinished from here instead of FragmentAnimationFinished because
		// this event appears to fire slightly after `FragmentAnimationFinished` and it also fires
		// if we aren't using animations
		protected virtual void OnNavigationViewFragmentDestroyed(AndroidX.Fragment.App.FragmentManager fm, NavigationViewFragment navHostPageFragment)
		{
			_ = NavigationView ?? throw new InvalidOperationException($"NavigationView cannot be null");

			if (IsNavigating)
			{
				NavigationFinished(NavigationView);
			}
		}

		protected virtual void OnNavigationViewFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, NavigationViewFragment navHostPageFragment)
		{
			if (IsInitialNavigation)
			{
				NavigationFinished(NavigationView);
			}
		}

		protected virtual void OnDestinationChanged(NavController navController, NavDestination navDestination, Bundle? bundle)
		{
		}

		FragmentManager? ChildFragmentManager
		{
			get
			{
				// If you try to access `ChildFragmentManager` and the `NavHost`
				// isn't attached to a context then android will throw an IllegalStateException
				if (_navHost.IsAlive() &&
					_navHost?.Context != null &&
					_navHost.ChildFragmentManager.IsAlive())
				{
					return _navHost.ChildFragmentManager;
				}

				return null;
			}
		}

		internal class StackLayoutInflater : LayoutInflater
		{
			readonly LayoutInflater _original;

			public StackLayoutInflater(
				LayoutInflater original,
				Context? context,
				StackNavigationManager stackNavigationManager) :
				base(original, new StackContext(context, stackNavigationManager))
			{
				_original = original;
				StackNavigationManager = stackNavigationManager;
			}

			public StackNavigationManager StackNavigationManager { get; }

			public override LayoutInflater? CloneInContext(Context? newContext)
			{
				return new StackLayoutInflater(_original, newContext, StackNavigationManager);
			}
		}

		internal class StackContext : AndroidX.AppCompat.View.ContextThemeWrapper
		{
			public StackContext(
				Context? context,
				StackNavigationManager stackNavigationManager) : base(context, context?.Theme)
			{
				StackNavigationManager = stackNavigationManager;
			}

			public StackNavigationManager StackNavigationManager { get; }
		}

		class Callbacks :
			AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks,
			NavController.IOnDestinationChangedListener
		{
			StackNavigationManager? _stackNavigationManager;

			public Callbacks(StackNavigationManager navigationLayout)
			{
				_stackNavigationManager = navigationLayout;
			}

			#region IOnDestinationChangedListener

			void NavController.IOnDestinationChangedListener.OnDestinationChanged(
				NavController p0, NavDestination p1, Bundle? p2)
			{
				_stackNavigationManager?.OnDestinationChanged(p0, p1, p2);
			}

			#endregion

			#region FragmentLifecycleCallbacks
			public override void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f)
			{
				if (_stackNavigationManager?.VirtualView == null)
					return;

				if (f is NavigationViewFragment pf)
					_stackNavigationManager.OnNavigationViewFragmentResumed(fm, pf);

				AToolbar? platformToolbar = null;
				IToolbar? toolbar = null;

				if (_stackNavigationManager.ToolbarElement?.Toolbar is IToolbar tb &&
					tb?.Handler?.PlatformView is AToolbar ntb)
				{
					platformToolbar = ntb;
					toolbar = tb;
				}

				// Wire up the toolbar to the currently made visible Fragment
				var controller = NavHostFragment.FindNavController(f);
				_ = new AppBarConfiguration
						.Builder(_stackNavigationManager.NavGraph);

				if (platformToolbar != null && toolbar != null && toolbar.Handler?.MauiContext != null)
				{
					if (toolbar.Handler is ToolbarHandler th)
					{
						th.SetupWithNavController(controller, _stackNavigationManager);
					}
				}
			}

			public override void OnFragmentViewDestroyed(
				AndroidX.Fragment.App.FragmentManager fm,
				AndroidX.Fragment.App.Fragment f)
			{
				if (_stackNavigationManager?.VirtualView == null)
					return;

				if (f is NavigationViewFragment pf)
					_stackNavigationManager.OnNavigationViewFragmentDestroyed(fm, pf);

				base.OnFragmentViewDestroyed(fm, f);
			}
			#endregion

			internal void Disconnect()
			{
				_stackNavigationManager = null;
			}
		}
	}
}
