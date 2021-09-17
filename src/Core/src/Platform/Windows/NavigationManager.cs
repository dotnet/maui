using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Microsoft.Maui
{
	public class NavigationManager
	{
		IView? _currentPage;
		IMauiContext _mauiContext;
		NavigationFrame? _navigationFrame;
		protected WindowManager WindowManager => _mauiContext.GetWindowManager();
		private protected INavigationView? NavigationView { get; private set; }
		public IReadOnlyList<IView> NavigationStack { get; set; } = new List<IView>();
		public IMauiContext MauiContext => _mauiContext;
		public IView CurrentPage
			=> _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");
		public NavigationFrame NavigationFrame =>
			_navigationFrame ?? throw new InvalidOperationException("NavigationFrame Null");

		public NavigationManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		public virtual void Connect(IView navigationView, NavigationFrame navigationFrame)
		{
			if (_navigationFrame != null)
				_navigationFrame.Navigated -= OnNavigated;

			navigationFrame.Navigated += OnNavigated;
			_navigationFrame = navigationFrame;
			NavigationView = (INavigationView)navigationView;
		}

		public virtual void NavigateTo(NavigationRequest args)
		{
			bool push = true;
			if (NavigationStack.Count > args.NavigationStack.Count)
				push = false;

			IReadOnlyList<IView> newPageStack = args.NavigationStack;
			bool animated = args.Animated;
			var previousNavigationStack = NavigationStack;
			var previousNavigationStackCount = previousNavigationStack.Count;
			bool initialNavigation = NavigationStack.Count == 0;

			// User has modified navigation stack but not the currently visible page
			// So we just sync the elements in the stack
			if (!initialNavigation &&
				newPageStack[newPageStack.Count - 1] == 
				previousNavigationStack[previousNavigationStackCount - 1])
			{
				SyncBackStackToNavigationStack(newPageStack);
				NavigationStack = new List<IView>(newPageStack);
				NavigationView?.NavigationFinished(NavigationStack);
				return;
			}


			NavigationTransitionInfo? transition = null;

			if (animated)
			{
				transition = new SlideNavigationTransitionInfo()
				{
					Effect = (!push) ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight
				};
			}


			NavigationStack = new List<IView>(newPageStack);
			_currentPage = NavigationStack[NavigationStack.Count - 1];

			if (push)
				NavigationFrame.Navigate(typeof(NavigationFramePage), null, transition);
			else
				NavigationFrame.GoBack(transition);
		}

		void SyncBackStackToNavigationStack(IReadOnlyList<IView> pageStack)
		{
			// Back stack depth doesn't count the currently visible page
			var nativeStackCount = NavigationFrame.BackStackDepth + 1;

			// BackStack entries have no hard relationship with a specific IView
			// Everytime an entry is about to become visible it just grabs whatever
			// IView is going to be the visible so all we're doing here is syncing
			// up the number of things on the stack
			while (nativeStackCount != pageStack.Count)
			{
				if (nativeStackCount > pageStack.Count)
				{
					NavigationFrame.BackStack.RemoveAt(0);
				}
				else
				{
					NavigationFrame.BackStack.Insert(
						0,
						new PageStackEntry(typeof(NavigationFramePage), null, null));
				}

				nativeStackCount = NavigationFrame.BackStackDepth + 1;
			}
		}

		// This is used to fire NavigationFinished back to the xplat view
		// Firing NavigationFinished from Loaded is the latest reliable point
		// in time that I know of for firing `NavigationFinished`
		// Ideally we could fire it when the `NavigationTransitionInfo` is done but
		// I haven't found a way to do that
		void OnNavigated(object sender, UI.Xaml.Navigation.NavigationEventArgs e)
		{
			// If the user has inserted or removed any extra pages
			SyncBackStackToNavigationStack(NavigationStack);

			if (e.Content is not FrameworkElement fe)
				return;

			if (fe.IsLoaded)
			{
				NavigationView?.NavigationFinished(NavigationStack);
				return;
			}

			fe.Loaded += OnLoaded;
			void OnLoaded(object sender, RoutedEventArgs e)
			{
				fe.Loaded -= OnLoaded;
				NavigationView?.NavigationFinished(NavigationStack);
			}
		}
	}
}
