using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;

namespace Microsoft.Maui.Platform
{

	public interface IToolbarContainer
	{

		public void SetToolbar(MauiToolbar? toolbar);

	}

	public class NavigationView : Box, IToolbarContainer
	{

		MauiToolbar? _toolbar;
		Widget? _pageWidget;
		IMauiContext? _mauiContext;
		IStackNavigation? _virtualView;
		IReadOnlyList<IView> _navigationStack = new List<IView>();

		public NavigationView() : base(Orientation.Vertical, 0)
		{ }

		public void Connect(IStackNavigationView virtualView)
		{
			_mauiContext = virtualView.Handler?.MauiContext;
			_virtualView = virtualView;
		}

		public void Disconnect(IStackNavigationView virtualView)
		{
			_mauiContext = null;
			_virtualView = null;
		}

		public void SetToolbar(MauiToolbar? toolbar)
		{
			if (toolbar is null)
				return;

			if (_toolbar is not null)
			{
				_toolbar.BackButtonClicked -= NavigateBack;
				Remove(_toolbar);
			}

			toolbar.BackButtonClicked += NavigateBack;
			_toolbar = toolbar;
			PackStart(_toolbar, false, true, 0);
		}

		void NavigateBack(object? sender)
		{
			if (_navigationStack.Count <= 1)
			{
				return;
			}

			var request = new NavigationRequest(_navigationStack.SkipLast(1).ToList(), false);
			_virtualView?.RequestNavigation(request);
		}

		public void RequestNavigation(NavigationRequest request)
		{
			// stack top is last
			var page = request.NavigationStack.Last();
			_navigationStack = request.NavigationStack;
			var newPageWidget = page.ToPlatform(_mauiContext!);

			if (_pageWidget is null)
			{
				PackEnd(newPageWidget, true, true, 0);
			}
			else
			{
				Remove(_pageWidget);
				Add(newPageWidget);
				SetChildPacking(newPageWidget, true, true, 0, PackType.End);
			}

			_pageWidget = newPageWidget;
		}

	}

}