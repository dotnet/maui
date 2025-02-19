using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Microsoft.Maui.Platform
{
	public class StackNavigationManager
	{
		IView? _currentPage;
		IMauiContext _mauiContext;
		Frame? _navigationFrame;
		Action? _pendingNavigationFinished;
		ContentPresenter? _previousContent;
		bool _connected;

		protected NavigationRootManager WindowManager => _mauiContext.GetNavigationRootManager();
		internal IStackNavigation? NavigationView { get; private set; }
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

		public virtual void Connect(IStackNavigation navigationView, Frame navigationFrame)
		{
			_connected = true;
			if (_navigationFrame != null)
			{
				_navigationFrame.Navigating -= OnNavigating;
				_navigationFrame.Navigated -= OnNavigated;
			}

			FirePendingNavigationFinished();

			navigationFrame.Navigating += OnNavigating;
			navigationFrame.Navigated += OnNavigated;

			_navigationFrame = navigationFrame;
			NavigationView = (IStackNavigation)navigationView;

			if (WindowManager?.RootView is NavigationView nv)
				nv.IsPaneVisible = true;
		}

		public virtual void Disconnect(IStackNavigation navigationView, Frame navigationFrame)
		{
			_connected = false;
			if (_navigationFrame != null)
			{
				_navigationFrame.Navigating -= OnNavigating;
				_navigationFrame.Navigated -= OnNavigated;
			}

			FirePendingNavigationFinished();
			_navigationFrame = null;
			NavigationView = null;

			if (_previousContent is not null)
			{
				_previousContent.Content = null;
				_previousContent = null;
			}
		}

		public virtual void NavigateTo(NavigationRequest args)
		{
			var newPageStack = new List<IView>(args.NavigationStack);
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
				NavigationStack = newPageStack;
				FireNavigationFinished();
				return;
			}

			NavigationTransitionInfo? transition = GetNavigationTransition(args);
			_currentPage = newPageStack[newPageStack.Count - 1];

			_ = _currentPage ?? throw new InvalidOperationException("Navigation Request Contains Null Elements");
			if (previousNavigationStack.Count < args.NavigationStack.Count)
			{
				Type destinationPageType = GetDestinationPageType();
				NavigationStack = newPageStack;
				NavigationFrame.Navigate(destinationPageType, null, transition);
			}
			else if (previousNavigationStack.Count == args.NavigationStack.Count)
			{
				Type destinationPageType = GetDestinationPageType();
				NavigationStack = newPageStack;
				NavigationFrame.Navigate(destinationPageType, null, transition);
			}
			else
			{
				NavigationStack = newPageStack;
				NavigationFrame.GoBack(transition);
			}
		}

		protected virtual Type GetDestinationPageType() =>
			typeof(Page);

		protected virtual NavigationTransitionInfo? GetNavigationTransition(NavigationRequest args)
		{
			if (!args.Animated)
				return null;

			// GoBack just plays the animation in reverse so we always just return the same animation
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

		private void OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			// We can navigate to pages that do not exist in the shell as sections, so we 
			// end up keeping the previous page loaded and in the (MAUI) visual tree.
			// This means we need to manually clear the WinUI content from the presenter so we don't
			// get a crash when the page is navigated to again due to the content already being the
			// child of another parent
			if (NavigationFrame.Content is Page p)
			{
				p.Unloaded += PageUnloaded;
				void PageUnloaded(object s, RoutedEventArgs e)
				{
					p.Unloaded -= PageUnloaded;
					if (p.Content is ContentPresenter presenter)
					{
						presenter.Content = null;
						_previousContent = null;
					}
				}
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

			var nv = NavigationView;
			ContentPresenter? presenter;

			if (page.Content == null)
			{
				presenter = new ContentPresenter()
				{
					HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
					VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
				};

				// Clear the content just in case the previous page didn't unload
				if (_previousContent is not null)
				{
					_previousContent.Content = null;
					_previousContent = null;
				}

				page.Content = presenter;
			}
			else
			{
				presenter = page.Content as ContentPresenter;
			}

			// At this point if the Content isn't a ContentPresenter the user has replaced
			// the content so we just let them take control
			if (presenter == null || _currentPage == null)
				return;

			var platformPage = _currentPage.ToPlatform(MauiContext);

			try
			{
				presenter.Content = platformPage;
			}
			catch (Exception)
			{
				FireNavigationFinished();
				throw;
			}

			_pendingNavigationFinished = () =>
			{
				_previousContent = presenter;

				if (presenter?.Content is not FrameworkElement pc)
				{
					FireNavigationFinished();
				}
				else
				{
					pc.OnLoaded(FireNavigationFinished);
				}

				if (NavigationView is IView view && _connected)
				{
					view.Arrange(fe);
				}
			};

			fe.OnLoaded(FirePendingNavigationFinished);
		}

		void FireNavigationFinished()
		{
			_pendingNavigationFinished = null;
			NavigationView?.NavigationFinished(NavigationStack);
		}

		void FirePendingNavigationFinished()
		{
			Interlocked.Exchange(ref _pendingNavigationFinished, null)?.Invoke();
		}
	}
}
