using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NLinearLayout = Tizen.NUI.LinearLayout;
#pragma warning disable CA1859 // Use concrete types when possible for improved performance

namespace Microsoft.Maui.Platform
{
	public interface IToolbarContainer
	{
		public void SetToolbar(MauiToolbar toolbar);
	}

	public class StackNavigationManager : View, IToolbarContainer
	{
		Dictionary<IView, NaviPage> _pageMap = new Dictionary<IView, NaviPage>();
		Dictionary<IView, IViewHandler?> _handlerMap = new Dictionary<IView, IViewHandler?>();

		MauiToolbar? _toolbar;

		List<IView> NavigationStack { get; set; } = new List<IView>();

		protected IMauiContext? MauiContext { get; set; }

		protected IStackNavigation? NavigationView { get; set; }

		protected NavigationStack PlatformNavigation { get; }

		public StackNavigationManager()
		{
			HeightSpecification = LayoutParamPolicies.MatchParent;
			WidthSpecification = LayoutParamPolicies.MatchParent;

			Layout = new NLinearLayout
			{
				LinearOrientation = NLinearLayout.Orientation.Vertical
			};
			PlatformNavigation = new NavigationStack();

			Add(PlatformNavigation);
		}

		public void SetToolbar(MauiToolbar toolbar)
		{
			if (_toolbar != null)
			{
				Remove(_toolbar);
				_toolbar.Dispose();
				_toolbar = null;
			}

			_toolbar = toolbar;
			Add(toolbar);
			(toolbar.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
		}

		public virtual void Connect(IView navigationView)
		{
			NavigationView = (IStackNavigation)navigationView;
			MauiContext = navigationView.Handler?.MauiContext;
		}

		public virtual void Disconnect()
		{
			NavigationView = null;
			MauiContext = null;
		}

		public virtual async void RequestNavigation(NavigationRequest e)
		{
			var newPageStack = new List<IView>(e.NavigationStack);
			var previousNavigationStack = NavigationStack;
			var previousNavigationStackCount = previousNavigationStack.Count;
			bool initialNavigation = previousNavigationStackCount == 0;

			if (initialNavigation)
			{
				await InitializeStack((IReadOnlyList<IView>)newPageStack, e.Animated);
				NavigationStack = newPageStack;
				NavigationFinished(NavigationStack);
				return;
			}

			if (newPageStack.Count > 0 && previousNavigationStackCount > 0 &&
				newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
			{
				SyncBackStackToNavigationStack(newPageStack);
				NavigationStack = newPageStack;
				NavigationFinished(NavigationStack);
				return;
			}

			if (newPageStack.Count > previousNavigationStackCount)
			{
				//Push to sync
				await PushToSync(newPageStack, e.Animated);
				NavigationStack = newPageStack;
				NavigationFinished(NavigationStack);
			}
			else
			{
				// top to sync
				await PopToSync(newPageStack, e.Animated);
				NavigationStack = newPageStack;
				NavigationFinished(NavigationStack);
			}
		}

		protected virtual async Task InitializeStack(IReadOnlyList<IView> newStack, bool animated)
		{
			var navigationStack = newStack;
			if (navigationStack.Count == 0)
				return;

			var top = navigationStack[navigationStack.Count - 1];
			foreach (var page in navigationStack)
			{
				await PlatformNavigation.Push(GetNavigationItem(page), page == top && animated);
			}
		}

		void SyncBackStackToNavigationStack(List<IView> newStack)
		{
			if (newStack.Count > NavigationStack.Count)
			{
				for (int i = 0; i < newStack.Count; i++)
				{
					if (NavigationStack.IndexOf(newStack[i]) == -1)
					{
						PlatformNavigation.Insert(GetNavigationItem(NavigationStack[i]), GetNavigationItem(newStack[i]));
					}
				}
			}
			else
			{
				foreach (var page in NavigationStack)
				{
					if (newStack.IndexOf(page) == -1)
					{
						PlatformNavigation.Pop(GetNavigationItem(page));
						_pageMap.Remove(page);
						if (_handlerMap.TryGetValue(page, out var handler))
						{
							(handler as IPlatformViewHandler)?.Dispose();
							_handlerMap.Remove(page);
						}
					}
				}
			}
		}

		async Task PushToSync(List<IView> newStack, bool animated)
		{
			int start = NavigationStack.Count;
			for (int i = start; i < newStack.Count; i++)
			{
				var isTop = i + 1 == newStack.Count;
				await PlatformNavigation.Push(GetNavigationItem(newStack[i]), isTop && animated);
			}
		}

		async Task PopToSync(List<IView> newStack, bool animated)
		{
			int start = newStack.Count;
			for (int i = start; i < NavigationStack.Count; i++)
			{
				var isLast = i + 1 == NavigationStack.Count;
				var page = NavigationStack[i];

				if (isLast)
				{
					await PlatformNavigation.Pop(animated);
				}
				else
				{
					PlatformNavigation.Pop(GetNavigationItem(NavigationStack[i]));
				}
				_pageMap.Remove(page);
				if (_handlerMap.TryGetValue(page, out var handler))
				{
					(handler as IPlatformViewHandler)?.Dispose();
					_handlerMap.Remove(page);
				}
			}
		}

		void NavigationFinished(List<IView> stack)
		{
			NavigationView?.NavigationFinished(stack);
		}

		NaviPage GetNavigationItem(IView page)
		{
			if (_pageMap.ContainsKey(page))
			{
				return _pageMap[page];
			}

			var content = page.ToPlatform(MauiContext!);
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightSpecification = LayoutParamPolicies.MatchParent;

			var naviPage = new NaviPage
			{
				Content = content,
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.MatchParent,
			};
			_pageMap[page] = naviPage;
			_handlerMap[page] = page.Handler;
			return naviPage;
		}
	}
}
#pragma warning restore CA1859 // Use concrete types when possible for improved performance