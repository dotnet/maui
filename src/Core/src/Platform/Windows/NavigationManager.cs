using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Microsoft.Maui
{
	public class NavigationManager
	{
		IView? _currentPage;
		IMauiContext _mauiContext;
		protected WindowManager WindowManager => _mauiContext.GetWindowManager();
		internal INavigationView? NavigationView { get; private set; }
		NavigationFrame NavigationFrame => _navigationFrame ?? throw new InvalidOperationException("NavigationFrame Null");
		NavigationFrame? _navigationFrame;
		public IReadOnlyList<IView> NavigationStack { get; set; } = new List<IView>();
		public IMauiContext MauiContext => _mauiContext;
		public IView CurrentPage
			=> _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");

		public NavigationManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		public virtual void Connect(IView navigationView, NavigationFrame navigationFrame)
		{
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
			WindowManager.SetVisibleContent(NavigationStack.Last());
			NavigationView?.NavigationFinished(NavigationStack);
		}
	}
}
