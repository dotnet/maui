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
	public class StackNavigationManager
	{
		IView? _currentPage;
		IMauiContext _mauiContext;
		Frame? _navigationFrame;
		protected NavigationRootManager WindowManager => _mauiContext.GetNavigationRootManager();
		private protected INavigationView? NavigationView { get; private set; }
		public IReadOnlyList<IView> NavigationStack { get; set; } = new List<IView>();
		public IMauiContext MauiContext => _mauiContext;
		public IView CurrentPage
			=> _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");
		public Frame NavigationFrame =>
			_navigationFrame ?? throw new InvalidOperationException("NavigationFrame Null");

		public StackNavigationManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		public virtual void Connect(IView navigationView, Frame navigationFrame)
		{
			if (_navigationFrame != null)
				_navigationFrame.Navigated -= OnNavigated;

			navigationFrame.Navigated += OnNavigated;
			_navigationFrame = navigationFrame;
			NavigationView = (INavigationView)navigationView;

			if (WindowManager?.RootView is NavigationView nv)
				nv.IsPaneVisible = true;
		}

		public virtual void Disconnect(IView navigationView, Frame navigationFrame)
		{
			if (_navigationFrame != null)
				_navigationFrame.Navigated -= OnNavigated;

			_navigationFrame = null;
			NavigationView = null;
		}

		public virtual void NavigateTo(NavigationRequest args)
		{
			IReadOnlyList<IView> newPageStack = args.NavigationStack;
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

			NavigationTransitionInfo? transition = GetNavigationTransition(args);
			_currentPage = newPageStack[newPageStack.Count - 1];

			if (previousNavigationStack.Count < args.NavigationStack.Count)
			{
				Type destinationPageType = GetDestinationPageType();
				NavigationStack = new List<IView>(newPageStack);
				NavigationFrame.Navigate(destinationPageType, null, transition);
			}
			else
			{
				NavigationStack = new List<IView>(newPageStack);
				NavigationFrame.GoBack(transition);
			}
		}

		protected virtual Type GetDestinationPageType() =>
			typeof(Page);

		protected virtual NavigationTransitionInfo? GetNavigationTransition(NavigationRequest args)
		{
			if (!args.Animated)
				return null;

			if (NavigationStack.Count > args.NavigationStack.Count)
				return new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft };
						
			return new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
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
						0, new PageStackEntry(GetDestinationPageType(), null, null));
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

			if (e.Content is not Page page)
				return;


			ContentPresenter? presenter;

			if (page.Content == null)
			{
				presenter = new ContentPresenter()
				{
					HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
					VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
				};

				page.Content = presenter;
			}
			else
			{
				presenter = page.Content as ContentPresenter;
			}

			// At this point if the Content isn't a ContentPresenter the user has replaced
			// the conent so we just let them take control
			if (presenter == null || _currentPage == null)
				return;

			presenter.Content = _currentPage.ToNative(MauiContext);

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
