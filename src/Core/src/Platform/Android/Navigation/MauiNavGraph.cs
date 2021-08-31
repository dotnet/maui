using System;
using System.Collections.Generic;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class MauiNavGraph : NavGraph
	{
		IView? _currentPage;
		public IView CurrentPage
		{
			get => _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");
			private set => _currentPage = value;
		}

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		public MauiNavGraph(Navigator navGraphNavigator) : base(navGraphNavigator)
		{
			Id = AView.GenerateViewId();
		}

		internal void NavigationFinished(INavigationView? navigationView)
		{
			IsPopping = null;
			ActiveRequestedArgs = null;
			navigationView?.NavigationFinished(NavigationStack);
		}

		internal bool IsNavigating => ActiveRequestedArgs != null;
		internal bool? IsPopping { get; private set; }
		internal bool IsAnimated { get; set; } = true;
		internal MauiNavigationRequestedEventArgs? ActiveRequestedArgs { get; private set; }

		/*
		 * The important thing to know going into reading this method is that it's not possible to
		 * modify the backstack. You can only push and pop to and from the top of the stack.
		 * So if a user uses an API like `RemovePage` or `InsertPage` we will typically ignore processing those natively
		 * unless it requires changes to the NavBar (i.e removing the first page with only 2 pages on the stack).
		 * Once the user performs an operation that changes the currently visible page then we process any stack changes
		 * that have occurred.
		 * Let's say the user has pages A,B,C,D on the stack 
		 * If they remove Page B and Page C then we don't do anything. Then if the user pushes E onto the stack
		 * we just transform A,B,C,D into A,D,E.
		 * Natively that's a "pop" but we use the correct animation for a "push" so visually it looks like a push.
		 * This is also the reason why we aren't using the custom animation features on the navigation component itself.
		 * Because we might be natively popping but visually pushing.
		 * 
		 * The Fragments that are on the stack also do not have a hard connection to the page they originally rendereded.
		 * Whenever a fragment is the "visible" fragment it just figures out what the current page is and displays that.
		 * Fragments are recreated everytime they are pushed on the stack but the handler renderer is not.
		 * It's just attached to a new fragment
		 * */
		internal void ApplyNavigationRequest(
			MauiNavigationRequestedEventArgs args,
			NavigationLayout navigationLayout)
		{
			if (IsNavigating)
			{
				// This should really never fire for the developer. Our xplat code should be handling waiting for navigation to
				// complete before requesting another navigation from Core
				// Maybe some day we'll put a navigation queue into Core? For now we won't
				throw new InvalidOperationException("Previous Navigation Request is still Processing");
			}

			ActiveRequestedArgs = args;
			IReadOnlyList<IView> newPageStack = args.NavigationStack;
			bool animated = args.Animated;
			var navController = navigationLayout.NavHost.NavController;
			var previousNavigationStack = NavigationStack;
			var previousNavigationStackCount = previousNavigationStack.Count;

			// This updates the graphs public navigation stack property so it's outwardly correct
			// But we've saved off the previous stack so we can correctly interpret navigation
			UpdateNavigationStack(newPageStack);

			// If the new stack isn't changing the visible page or the app bar then we just ignore
			// the changes because there's no point to applying these to the native back stack
			// We only apply changes when the currently visible page changes and/or the appbar
			// will change (gain a back button)
			if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1] &&
				newPageStack.Count > 1 &&
				previousNavigationStackCount > 1)
			{

				NavigationFinished(navigationLayout.NavigationView);
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

			var iterator = navigationLayout.NavHost.NavController.BackStack.Iterator();
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
				for (int i = 0; i < newPageStack.Count; i++)
				{
					if (fragmentNavDestinations.Count > i)
					{
					}
					else
					{
						var dest = AddDestination(navigationLayout.FragmentNavigator);
						navController.Navigate(dest.Id);
					}
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
			var iterateNewStack = navigationLayout.NavHost.NavController.BackStack.Iterator();

			while (iterateNewStack.HasNext)
			{
				if (iterateNewStack.Next() is NavBackStackEntry nbse &&
					nbse.Destination is FragmentNavigator.Destination nvd)
				{
					fragmentNavDestinations.Remove(nvd);
				}
			}

			foreach (var activeDestinations in fragmentNavDestinations)
			{
				this.Remove(activeDestinations);
			}
		}

		public FragmentNavigator.Destination AddDestination(Navigator fragmentNavigator)
		{
			var destination = new FragmentNavigator.Destination(fragmentNavigator);
			destination.SetClassName(Java.Lang.Class.FromType(typeof(NavHostPageFragment)).CanonicalName);
			destination.Id = AView.GenerateViewId();
			AddDestination(destination);
			return destination;
		}

		// This occurs when the navigation page is first being renderer so we sync up the
		// Navigation Stack on the INavigationView to our native stack
		internal List<int> Initialize(
			IReadOnlyList<IView> pages,
			NavigationLayout navigationLayout)
		{
			var navController = navigationLayout.NavHost.NavController;

			// We are subtracting one because the navgraph itself is the first item on the stack
			int NativeNavigationStackCount = navController.BackStack.Size() - 1;

			// set this to one because when the graph is first attached to the controller
			// it will add the graph and the first destination
			if (NativeNavigationStackCount < 0)
				NativeNavigationStackCount = 1;

			List<int> destinations = new List<int>();

			NavDestination navDestination;

			foreach (var page in pages)
			{
				navDestination =
						AddDestination(navigationLayout.FragmentNavigator);

				destinations.Add(navDestination.Id);
			}

			StartDestination = destinations[0];
			navController.SetGraph(this, null);

			for (var i = NativeNavigationStackCount; i < pages.Count; i++)
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
			CurrentPage = NavigationStack[NavigationStack.Count - 1];
		}
	}
}
