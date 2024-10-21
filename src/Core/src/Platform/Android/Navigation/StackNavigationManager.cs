﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Java.Interop;
using Java.Lang;
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
		FragmentManager? _fragmentManager;
		FragmentContainerView? _fragmentContainerView;

		internal IView? VirtualView { get; private set; }
		internal IStackNavigation? NavigationView { get; private set; }
		internal bool IsNavigating => ActiveRequestedArgs != null;
		internal bool IsInitialNavigation { get; private set; }
		internal bool? IsPopping { get; private set; }
		internal bool IsAnimated { get; set; } = true;
		internal NavigationRequest? ActiveRequestedArgs { get; private set; }
		internal NavigationRequest? OnResumeRequestedArgs { get; private set; }
		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		internal NavHostFragment NavHost =>
			_navHost ?? throw new InvalidOperationException($"NavHost cannot be null");

		internal NavController NavController =>
			NavHost.NavController ?? throw new InvalidOperationException($"NavHost cannot be null");

		internal FragmentNavigator FragmentNavigator =>
			_fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");

		internal NavGraph NavGraph => _navGraph ??
			throw new InvalidOperationException($"NavGraph cannot be null");

		internal bool HasNavHost => _navHost is not null;

		public IView CurrentPage
			=> _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");

		public IMauiContext MauiContext { get; }

		internal IToolbarElement? ToolbarElement =>
			MauiContext.GetNavigationRootManager().ToolbarElement;

		public StackNavigationManager(IMauiContext mauiContext)
		{
			MauiContext = mauiContext;
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
			if (IsNavigating && OnResumeRequestedArgs is null)
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

			if (_fragmentManager?.IsStateSaved == true)
			{
				OnResumeRequestedArgs = args;
				return;
			}

			OnResumeRequestedArgs = null;

			IReadOnlyList<IView> newPageStack = args.NavigationStack;
			bool animated = args.Animated;
			var navController = NavController;
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

			var fragmentNavDestinations = new List<FragmentNavigator.Destination>();
			navController.IterateBackStack(d => fragmentNavDestinations.Add(d));

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

			var iterateNewStack = NavController.Graph.Iterator();
			int startId = -1;

			navController.IterateBackStack(nvd =>
			{
				if (startId == -1)
					startId = nvd.Id;
				fragmentNavDestinations.Remove(nvd);
			});

			foreach (var activeDestinations in fragmentNavDestinations)
			{
				NavGraph.Remove(activeDestinations);
			}

			// If we end up removing the destination that was initially the StartDestination
			// The Navigation Graph can get really confused
#pragma warning disable CS0618 // Obsolete
			if (NavGraph.StartDestination != startId)
				NavGraph.StartDestination = startId;
#pragma warning restore CS0618 // Obsolete

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
			var navController = NavController;

			List<int> destinations = new List<int>();

			NavDestination navDestination;

			foreach (var page in pages)
			{
				navDestination = AddFragmentDestination();
				destinations.Add(navDestination.Id);
			}

			NavGraph.StartDestination = destinations[0];
			navController.SetGraph(NavGraph, null);

			int PlatformNavigationStackCount = 0;

			navController.IterateBackStack(_ => PlatformNavigationStackCount++);

			// set this to one because when the graph is first attached to the controller
			// it will add the graph and the first destination
			if (PlatformNavigationStackCount < 0)
				PlatformNavigationStackCount = 1;

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
			if (IsNavigating)
				NavigationFinished(NavigationView);

			if (_fragmentContainerView is not null)
			{
				_fragmentContainerView.ViewAttachedToWindow -= OnNavigationPlatformViewAttachedToWindow;
				_fragmentContainerView.ChildViewAdded -= OnNavigationHostViewAdded;
			}

			_fragmentLifecycleCallbacks?.Disconnect();
			_fragmentLifecycleCallbacks = null;

			VirtualView = null;
			NavigationView = null;
			SetNavHost(null);
			_fragmentNavigator = null;
		}

		public virtual void Connect(IView navigationView)
		{
			VirtualView = navigationView;
			NavigationView = (IStackNavigation)navigationView;

			_fragmentContainerView = navigationView.Handler?.PlatformView as FragmentContainerView;

			_fragmentManager = MauiContext?.GetFragmentManager();

			_ = _fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			var navHostFragment = _fragmentManager.FindFragmentById(Resource.Id.nav_host);
			SetNavHost(navHostFragment as NavHostFragment);

			if (_navHost == null)
				throw new InvalidOperationException($"No NavHostFragment found");

			if (_fragmentContainerView is not null)
			{
				_fragmentContainerView.ViewAttachedToWindow += OnNavigationPlatformViewAttachedToWindow;
				_fragmentContainerView.ChildViewAdded += OnNavigationHostViewAdded;
			}
		}

		void OnNavigationPlatformViewAttachedToWindow(object? sender, AView.ViewAttachedToWindowEventArgs e)
		{
			// If the previous Navigation Host Fragment was destroyed then we need to add a new one
			if (_fragmentManager.IsDestroyed(MauiContext.Context) &&
				_fragmentContainerView is not null &&
				_fragmentContainerView.Fragment is null)
			{
				var fragmentManager = MauiContext.GetFragmentManager();

				if (fragmentManager.IsDestroyed(MauiContext.Context))
					return;

				var navHostFragment = new MauiNavHostFragment()
				{
					StackNavigationManager = this
				};

				// We can't call CheckForFragmentChange right away. The Fragment has to finish attaching
				// before we can start interacting with the Navigation Host.
				// OnNavigationHostViewAdded takes care of calling CheckForFragmentChange once the
				// view has been added
				fragmentManager
					.BeginTransactionEx()
					.AddEx(_fragmentContainerView.Id, navHostFragment)
					.Commit();
			}
		}

		void OnNavigationHostViewAdded(object? sender, ViewGroup.ChildViewAddedEventArgs e)
		{
			CheckForFragmentChange();
		}

		internal void CheckForFragmentChange()
		{
			if (_fragmentContainerView?.Fragment is null)
				return;

			var fragmentManager = MauiContext.GetFragmentManager();
			var navHostFragment = _fragmentContainerView?.Fragment;

			if ((navHostFragment != null && _navHost != navHostFragment) || (fragmentManager != _fragmentManager))
			{
				System.Diagnostics.Debug.WriteLine($"CheckForFragmentChange: {_fragmentContainerView}");

				_fragmentManager = fragmentManager;
				_ = _fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");

				navHostFragment = navHostFragment ?? _fragmentManager.FindFragmentById(Resource.Id.nav_host);

				_fragmentManager = MauiContext.GetFragmentManager();
				_fragmentLifecycleCallbacks?.Disconnect();
				_fragmentLifecycleCallbacks = null;
				SetNavHost(navHostFragment as NavHostFragment);

				if (_navHost == null)
					throw new InvalidOperationException($"No NavHostFragment found");

				_fragmentNavigator =
					(FragmentNavigator)NavController
						.NavigatorProvider
						.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));

				NavController.SetGraph(NavGraph, null);
				_fragmentLifecycleCallbacks = new Callbacks(this, NavController, ChildFragmentManager);
			}
		}

		public virtual void RequestNavigation(NavigationRequest e)
		{
			if (MauiContext == null)
				return;

			CheckForFragmentChange();

			if (_navGraph == null)
			{
				var navGraphNavigator =
				   (NavGraphNavigator)NavController
					   .NavigatorProvider
					   .GetNavigator(Java.Lang.Class.FromType(typeof(NavGraphNavigator)));

				_navGraph = new NavGraph(navGraphNavigator);
			}

			if (_fragmentLifecycleCallbacks == null)
			{
				_fragmentLifecycleCallbacks = new Callbacks(this, NavController, ChildFragmentManager);
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
					_navHost.Context is not null &&
					_navHost.ChildFragmentManager.IsAlive())
				{
					return _navHost.ChildFragmentManager;
				}

				return null;
			}
		}

		void SetNavHost(NavHostFragment? navHost)
		{
			if (_navHost == navHost)
				return;

			if (_navHost is MauiNavHostFragment oldHost)
				oldHost.StackNavigationManager = null;

			if (navHost is MauiNavHostFragment newHost)
				newHost.StackNavigationManager = this;

			_navHost = navHost;

			if (_navHost != null)
			{
				_fragmentNavigator =
					(FragmentNavigator)NavController
						.NavigatorProvider
						.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));

				foreach (var fragment in _navHost.ChildFragmentManager.Fragments)
				{
					if (fragment is NavigationViewFragment nvf)
					{
						nvf.NavigationManager = this;
					}
				}
			}
			else
			{
				_fragmentNavigator = null;
			}
		}

		class Callbacks :
			AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks,
			NavController.IOnDestinationChangedListener
		{
			StackNavigationManager? _stackNavigationManager;
			NavController _navController;
			FragmentManager? _childFragmentManager;

			public Callbacks(StackNavigationManager navigationLayout, NavController navController, FragmentManager? childFragmentManager)
			{
				_stackNavigationManager = navigationLayout;
				_navController = navController;
				_childFragmentManager = childFragmentManager;

				_navController.AddOnDestinationChangedListener(this);
				_childFragmentManager?.RegisterFragmentLifecycleCallbacks(this, false);
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

				if (_stackNavigationManager.OnResumeRequestedArgs is not null)
				{
					_stackNavigationManager.ApplyNavigationRequest(_stackNavigationManager.OnResumeRequestedArgs);
					return;
				}

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

			public override void OnFragmentCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState)
			{
				if (f is NavigationViewFragment pf && _stackNavigationManager != null)
					pf.NavigationManager = _stackNavigationManager;

				base.OnFragmentCreated(fm, f, savedInstanceState);
			}

			public override void OnFragmentPreCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState)
			{
				if (f is NavigationViewFragment pf && _stackNavigationManager != null)
					pf.NavigationManager = _stackNavigationManager;

				base.OnFragmentPreCreated(fm, f, savedInstanceState);
			}

			public override void OnFragmentPreAttached(FragmentManager fm, Fragment f, Context context)
			{
				base.OnFragmentPreAttached(fm, f, context);
			}

			public override void OnFragmentStarted(FragmentManager fm, Fragment f)
			{
				base.OnFragmentStarted(fm, f);
			}

			public override void OnFragmentAttached(FragmentManager fm, Fragment f, Context context)
			{
				base.OnFragmentAttached(fm, f, context);
			}

			public override void OnFragmentSaveInstanceState(FragmentManager fm, Fragment f, Bundle outState)
			{
				base.OnFragmentSaveInstanceState(fm, f, outState);
			}

			public override void OnFragmentViewCreated(FragmentManager fm, Fragment f, AView v, Bundle? savedInstanceState)
			{
				base.OnFragmentViewCreated(fm, f, v, savedInstanceState);
			}

			#endregion

			internal void Disconnect()
			{
				_stackNavigationManager = null;

				if (_navController != null && _navController.IsAlive())
					_navController.RemoveOnDestinationChangedListener(this);

				_childFragmentManager?.UnregisterFragmentLifecycleCallbacks(this);
			}
		}
	}
}