using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

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

		public virtual void NavigateTo(NavigationRequest arg3)
		{
			bool push = true;
			if (NavigationStack.Count > arg3.NavigationStack.Count)
				push = false;

			var transition = new SlideNavigationTransitionInfo()
			{
				Effect = (!push) ? SlideNavigationTransitionEffect.FromLeft : SlideNavigationTransitionEffect.FromRight
			};

			NavigationStack = arg3.NavigationStack;
			_currentPage = NavigationStack[NavigationStack.Count - 1];
			NavigationFrame.Navigate(typeof(NavigationFramePage), null, transition);
		}

		// This is used to fire NavigationFinished back to the xplat view
		// Firing NavigationFinished from Loaded is the latest reliable point
		// in time that I know of for firing `NavigationFinished`
		// Ideally we could fire it when the `NavigationTransitionInfo` is done but
		// I haven't found a way to do that
		void OnNavigated(object sender, UI.Xaml.Navigation.NavigationEventArgs e)
		{
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
