#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using UIKit;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using TabbedPageConfiguration = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TabbedPage;
using TranslucencyMode = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TranslucencyMode;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		// Instance state for tracking tab bar defaults (used by appearance mappers)
		bool _barBackgroundColorWasSet;
		bool _barTextColorWasSet;
		UIColor _defaultBarTextColor;
		bool _defaultBarTextColorSet;
		UIColor _defaultBarColor;
		bool _defaultBarColorSet;
		bool? _defaultBarTranslucent;
		UITabBarAppearance _tabBarAppearance;
		Brush _currentBarBackground;

		// Tracks pages from the previous MapItemsSource call so we can
		// disconnect handlers for pages that were removed.
		HashSet<Page> _previousPages;

		// Per-page generation counter to detect stale async icon loads in SetTabBarItem
		Dictionary<Page, int> _tabBarItemGeneration;

		static UIView OnCreatePlatformView(ViewHandler<ITabbedView, UIView> arg)
		{
			if (arg.VirtualView is TabbedPage tabbedPage && arg is TabbedViewHandler handler)
			{
				return tabbedPage.CreatePlatformView(handler);
			}

			throw new InvalidOperationException("TabbedViewHandler.PlatformViewFactory requires a TabbedPage and TabbedViewHandler.");
		}

		partial void OnHandlerChangingPartial(HandlerChangingEventArgs args)
		{
			if (args.OldHandler is null)
			{
				return;
			}

			// Handler is being removed or replaced — clean up old resources

			// Unsubscribe manager events to prevent leaks
			if (args.OldHandler is TabbedViewHandler oldHandler)
			{
				var manager = oldHandler.Manager;

				if (manager is not null)
				{
					manager.ViewDidAppear -= OnManagerViewDidAppear;
					manager.ViewDidDisappear -= OnManagerViewDidDisappear;
					manager.TabsReordered -= OnManagerTabsReordered;
					manager.GetCurrentPageViewControllerFunc = null;
					manager.Dispose();
				}
			}

			// Dispose UITabBarAppearance
			_tabBarAppearance?.Dispose();
			_tabBarAppearance = null;

			// Reset native defaults so they're recaptured from the new tab bar on reconnect
			_defaultBarColorSet = false;
			_defaultBarTextColorSet = false;
			_defaultBarTranslucent = null;
			_barBackgroundColorWasSet = false;
			_barTextColorWasSet = false;
			_defaultBarColor = null;
			_defaultBarTextColor = null;

			// Clean up gradient brush subscription
			if (_currentBarBackground is GradientBrush gradientBrush)
			{
				gradientBrush.Parent = null;
				gradientBrush.InvalidateGradientBrushRequested -= OnBarBackgroundChanged;
			}
			_currentBarBackground = null;

			// Clear previous pages tracking
			_previousPages = null;
			_tabBarItemGeneration = null;
		}

		UIView CreatePlatformView(TabbedViewHandler handler)
		{
			var manager = new TabBarControllerManager(handler);
			handler.SetManager(manager);

			// Subscribe to lifecycle events from the manager
			manager.ViewDidAppear += OnManagerViewDidAppear;
			manager.ViewDidDisappear += OnManagerViewDidDisappear;

			// Subscribe to tab reorder events
			manager.TabsReordered += OnManagerTabsReordered;

			// Provide current page VC callback for status bar / home indicator delegation
			manager.GetCurrentPageViewControllerFunc = () => GetViewController(CurrentPage);

			return manager.View;
		}

		void OnManagerViewDidAppear(object sender, EventArgs e)
		{
			SendAppearing();
		}

		void OnManagerViewDidDisappear(object sender, EventArgs e)
		{
			SendDisappearing();
		}

		void OnManagerTabsReordered(UIViewController[] viewControllers)
		{
			UpdateChildrenOrderIndex(viewControllers);
		}

		static TabBarControllerManager GetManager(ITabbedViewHandler handler)
		{
			if (handler is TabbedViewHandler tvh)
			{
				return tvh.Manager;
			}

			return null;
		}

		static UITabBar GetTabBar(ITabbedViewHandler handler)
		{
			return GetManager(handler)?.TabBar;
		}

		static UIViewController GetViewController(Page page)
		{
			if (page?.Handler is not IPlatformViewHandler nvh)
			{
				return null;
			}

			return nvh.ViewController;
		}

		internal static void MapFlowDirection(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);

			if (manager is null)
			{
				return;
			}

			// Apply FlowDirection to the controller's View (enables child MatchParent resolution)
			manager.View.UpdateFlowDirection(view);

			// Prevent tab item reversal — the handler's extracted view propagates
			// SemanticContentAttribute to the TabBar unlike the renderer
			manager.TabBar.SemanticContentAttribute = UISemanticContentAttribute.Unspecified;

			foreach (var child in view.InternalChildren)
			{
				if (child is Page page && page.Handler?.PlatformView is UIView childView)
				{
					childView.UpdateFlowDirection(page);
				}
			}
		}

		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);

			if (tabBar is null)
			{
				return;
			}

			if (view._currentBarBackground is GradientBrush oldGradientBrush)
			{
				oldGradientBrush.Parent = null;
				oldGradientBrush.InvalidateGradientBrushRequested -= view.OnBarBackgroundChanged;
			}

			view._currentBarBackground = view.BarBackground;

			if (view._currentBarBackground is GradientBrush newGradientBrush)
			{
				newGradientBrush.Parent = view;
				newGradientBrush.InvalidateGradientBrushRequested += view.OnBarBackgroundChanged;
			}

			tabBar.UpdateBackground(view._currentBarBackground);
		}

		void OnBarBackgroundChanged(object sender, EventArgs e)
		{
			var tabBar = GetTabBar(Handler as ITabbedViewHandler);
			tabBar?.UpdateBackground(_currentBarBackground);
		}

		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);

			if (tabBar is null)
			{
				return;
			}

			var barBackgroundColor = view.BarBackgroundColor;
			var isDefaultColor = barBackgroundColor is null;

			if (isDefaultColor && !view._barBackgroundColorWasSet)
			{
				return;
			}

			if (!view._defaultBarColorSet)
			{
				view._defaultBarColor = tabBar.BarTintColor;
				view._defaultBarColorSet = true;
			}

			if (!isDefaultColor)
			{
				view._barBackgroundColorWasSet = true;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
			{
				view.UpdateiOS15TabBarAppearance(tabBar);
			}
			else
			{
				tabBar.BarTintColor = isDefaultColor ? view._defaultBarColor : barBackgroundColor.ToPlatform();
			}
		}

		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);
			if (tabBar is null || tabBar.Items is null)
			{
				return;
			}

			var barTextColor = view.BarTextColor;
			var isDefaultColor = barTextColor is null;

			if (isDefaultColor && !view._barTextColorWasSet)
			{
				return;
			}

			if (!view._defaultBarTextColorSet)
			{
				view._defaultBarTextColor = tabBar.TintColor;
				view._defaultBarTextColorSet = true;
			}

			if (!isDefaultColor)
			{
				view._barTextColorWasSet = true;
			}

			UIColor tabBarTextColor;

			if (isDefaultColor)
			{
				tabBarTextColor = view._defaultBarTextColor;
			}
			else
			{
				tabBarTextColor = barTextColor.ToPlatform();
			}

			foreach (UITabBarItem item in tabBar.Items)
			{
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Normal);
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Selected);
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Disabled);
			}

			// Set TintColor for selected icon
			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
			{
				view.UpdateiOS15TabBarAppearance(tabBar);
			}
			else
			{
				tabBar.TintColor = isDefaultColor ? view._defaultBarTextColor : barTextColor.ToPlatform();
			}
		}

		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);
			if (tabBar is null || tabBar.Items is null)
			{
				return;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
			{
				view.UpdateiOS15TabBarAppearance(tabBar);
			}
			else
			{
				if (view.IsSet(UnselectedTabColorProperty) && view.UnselectedTabColor is not null)
				{
					tabBar.UnselectedItemTintColor = view.UnselectedTabColor.ToPlatform();
				}
				else
				{
					tabBar.UnselectedItemTintColor = UITabBar.Appearance.TintColor;
				}
			}
		}

		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);

			if (tabBar is null || tabBar.Items is null)
			{
				return;
			}

			if (view.IsSet(SelectedTabColorProperty) && view.SelectedTabColor is not null)
			{
				tabBar.TintColor = view.SelectedTabColor.ToPlatform();
			}
			else
			{
				tabBar.TintColor = UITabBar.Appearance.TintColor;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
			{
				view.UpdateiOS15TabBarAppearance(tabBar);
			}
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);

			if (manager is null)
			{
				view._pendingPagesChangedArgs = null;
				return;
			}

			var mauiContext = handler.MauiContext;
			if (mauiContext is null)
			{
				view._pendingPagesChangedArgs = null;
				return;
			}

			// Consume the pending args (if any)
			var args = view._pendingPagesChangedArgs;
			view._pendingPagesChangedArgs = null;

			// Try incremental update for simple Add/Remove
			if (args is not null && view._previousPages is not null)
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add when args.NewItems is not null:
						HandleIncrementalAdd(handler, view, manager, mauiContext, args);
						return;

					case NotifyCollectionChangedAction.Remove when args.OldItems is not null:
						HandleIncrementalRemove(handler, view, manager, args);
						return;
				}
			}

			// No pending args + pages already set up = Title/Icon change only.
			// Refresh tab bar items without full rebuild (matches renderer's UpdateTabBarItem approach).
			if (args is null && view._previousPages is not null)
			{
				HandleTabBarItemRefresh(view, manager);
				return;
			}

			// Full rebuild for Reset, Replace, Move, or initial load
			HandleFullRebuild(handler, view, manager, mauiContext);
		}

		static void HandleIncrementalAdd(ITabbedViewHandler handler, TabbedPage view,
			TabBarControllerManager manager, IMauiContext mauiContext, NotifyCollectionChangedEventArgs args)
		{
			// Setup only the new pages
			foreach (var item in args.NewItems)
			{
				if (item is not Page page)
				{
					continue;
				}

				var pageHandler = (IPlatformViewHandler)page.ToHandler(mauiContext);
				view.SetTabBarItem(pageHandler, manager);
				view._previousPages?.Add(page);
			}

			// Rebuild the VC array from current children
			var list = new List<UIViewController>();
			foreach (var child in view.InternalChildren)
			{
				if (child is Page p)
				{
					var vc = GetViewController(p);

					if (vc is not null)
					{
						list.Add(vc);
					}
				}
			}

			var controllersArray = list.ToArray();
			manager.ViewControllers = controllersArray;
			manager.UpdateTabBarVisibility();

			// Refresh Tags so UpdateChildrenOrderIndex maps to correct pages
			RefreshTabBarItemTags(view);

			// Restore SelectedViewController from CurrentPage (UIKit can reset selection on VC reassignment)
			UIViewController controller = null;
			if (view.CurrentPage is Page currentPage)
			{
				controller = GetViewController(currentPage);
			}
			if (controller is not null && controller != manager.SelectedViewController
				&& Array.IndexOf(controllersArray, controller) >= 0)
			{
				manager.SelectedViewController = controller;
			}

			// Re-apply appearance for new items
			MapBarBackgroundColor(handler, view);
			MapBarTextColor(handler, view);
			MapSelectedTabColor(handler, view);
			MapUnselectedTabColor(handler, view);
		}

		static void HandleIncrementalRemove(ITabbedViewHandler handler, TabbedPage view,
			TabBarControllerManager manager, NotifyCollectionChangedEventArgs args)
		{
			// Teardown only the removed pages
			foreach (var item in args.OldItems)
			{
				if (item is not Page page)
				{
					continue;
				}

				view._previousPages?.Remove(page);
				page.Handler?.DisconnectHandler();
			}

			// Rebuild the VC array from current children
			var list = new List<UIViewController>();
			foreach (var child in view.InternalChildren)
			{
				if (child is Page p)
				{
					var vc = GetViewController(p);

					if (vc is not null)
					{
						list.Add(vc);
					}
				}
			}

			var controllersArray = list.ToArray();
			manager.ViewControllers = controllersArray;
			manager.UpdateTabBarVisibility();

			// Refresh Tags so UpdateChildrenOrderIndex maps to correct pages
			RefreshTabBarItemTags(view);

			// Ensure selected VC is still valid
			UIViewController controller = null;
			if (view.CurrentPage is Page currentPage)
			{
				controller = GetViewController(currentPage);
			}
			if (controller is not null && controller != manager.SelectedViewController
				&& Array.IndexOf(controllersArray, controller) >= 0)
			{
				manager.SelectedViewController = controller;
			}

			MapBarBackgroundColor(handler, view);
			MapBarTextColor(handler, view);
			MapSelectedTabColor(handler, view);
			MapUnselectedTabColor(handler, view);
		}

		static void HandleFullRebuild(ITabbedViewHandler handler, TabbedPage view,
			TabBarControllerManager manager, IMauiContext mauiContext)
		{
			var currentPages = new HashSet<Page>();
			var list = new List<UIViewController>();
			var pages = view.InternalChildren;
			for (var i = 0; i < pages.Count; i++)
			{
				var child = pages[i];

				if (child is not Page page)
				{
					continue;
				}

				currentPages.Add(page);
				var pageHandler = (IPlatformViewHandler)page.ToHandler(mauiContext);
				view.SetTabBarItem(pageHandler, manager);

				var vc = GetViewController(page);

				if (vc is not null)
				{
					list.Add(vc);
				}
			}

			// Disconnect handlers for pages that were removed since last rebuild
			if (view._previousPages is not null)
			{
				foreach (var oldPage in view._previousPages)
				{
					if (!currentPages.Contains(oldPage))
					{
						oldPage.Handler?.DisconnectHandler();
					}
				}
			}

			view._previousPages = currentPages;

			var controllersArray = list.ToArray();
			manager.ViewControllers = controllersArray;

			manager.UpdateTabBarVisibility();

			UIViewController controller = null;

			if (view.CurrentPage is Page currentPage)
			{
				controller = GetViewController(currentPage);
			}
			if (controller is not null && controller != manager.SelectedViewController
				&& Array.IndexOf(controllersArray, controller) >= 0)
			{
				manager.SelectedViewController = controller;
			}

			MapBarBackgroundColor(handler, view);
			MapBarTextColor(handler, view);
			MapSelectedTabColor(handler, view);
			MapUnselectedTabColor(handler, view);
		}

		static void HandleTabBarItemRefresh(TabbedPage view, TabBarControllerManager manager)
		{
			// If a specific page changed, update only that page's tab bar item
			var changedPage = view._pendingPropertyChangedPage;
			if (changedPage is not null && changedPage.Handler is IPlatformViewHandler changedHandler)
			{
				view.SetTabBarItem(changedHandler, manager);
				return;
			}

			// Fallback: update all tab bar items (e.g. trait collection change)
			foreach (var child in view.InternalChildren)
			{
				if (child is Page page && page.Handler is IPlatformViewHandler pageHandler)
				{
					view.SetTabBarItem(pageHandler, manager);
				}
			}
		}

		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{
			// ItemTemplate changes trigger a full rebuild via MapItemsSource
			MapItemsSource(handler, view);
		}

		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{
			// SelectedItem is synced via CurrentPage
			MapCurrentPage(handler, view);
		}

		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);
			if (manager is null)
			{
				return;
			}

			// Determine sync direction using the handler's flag.
			// When a native tab tap fires OnTabSelected, NativeSelectionInProgress is true
			// and we sync native→virtual. When CurrentPage is set programmatically,
			// the flag is false and we sync virtual→native.
			bool isNativeSelection = handler is TabbedViewHandler tvh && tvh.NativeSelectionInProgress;

			if (isNativeSelection)
			{
				// Native → virtual: user tapped a tab, update CurrentPage to match
				var nativeIndex = (int)manager.SelectedIndex;
				var count = view.InternalChildren.Count;

				if (nativeIndex >= 0 && nativeIndex < count)
				{
					var nativeCurrentPage = view.GetPageByIndex(nativeIndex);

					if (nativeCurrentPage is not null && nativeCurrentPage != view.CurrentPage)
					{
						view.CurrentPage = nativeCurrentPage;
					}
				}
			}
			else
			{
				// Virtual → native: CurrentPage set programmatically, update SelectedViewController
				var current = view.CurrentPage;
				if (current is null)
				{
					return;
				}

				// Don't set SelectedViewController if CurrentPage is no longer in Children
				if (view.Children.IndexOf(current) < 0)
				{
					return;
				}

				var controller = GetViewController(current);
				if (controller is null)
				{
					return;
				}

				// Verify the controller is in ViewControllers before setting
				var viewControllers = manager.ViewControllers;
				if (viewControllers is null || Array.IndexOf(viewControllers, controller) < 0)
				{
					return;
				}

				// Update status bar / home indicator for the new current page
				var tabBarController = manager.TabBarController;
				tabBarController?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
				tabBarController?.SetNeedsStatusBarAppearanceUpdate();

				if (controller != manager.SelectedViewController)
				{
					manager.SelectedViewController = controller;
				}
			}
		}

		internal static void MapPrefersHomeIndicatorAutoHiddenProperty(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);
			if (manager is null)
			{
				return;
			}

			manager.TabBarController?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		internal static void MapPrefersPrefersStatusBarHiddenProperty(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);
			if (manager is null)
			{
				return;
			}

			// Propagate status bar hidden preference to all child pages (matching renderer)
			var viewControllers = manager.ViewControllers;

			if (viewControllers is not null)
			{
				for (var i = 0; i < viewControllers.Length; i++)
				{
					view.GetPageByIndex(i).OnThisPlatform().SetPrefersStatusBarHidden(
						view.OnThisPlatform().PrefersStatusBarHidden());
				}
			}

			// Propagate preferred status bar update animation to current page
			PageUIStatusBarAnimation animation = view.OnThisPlatform().PreferredStatusBarUpdateAnimation();
			view.CurrentPage?.OnThisPlatform().SetPreferredStatusBarUpdateAnimation(animation);

			manager.TabBarController?.SetNeedsStatusBarAppearanceUpdate();
		}

		internal static void MapPreferredStatusBarUpdateAnimation(ITabbedViewHandler handler, TabbedPage view)
		{
			var manager = GetManager(handler);
			if (manager is null)
				return;

			PageUIStatusBarAnimation animation = view.OnThisPlatform().PreferredStatusBarUpdateAnimation();
			view.CurrentPage?.OnThisPlatform().SetPreferredStatusBarUpdateAnimation(animation);
			manager.TabBarController?.SetNeedsStatusBarAppearanceUpdate();
		}

		internal static void MapTranslucencyMode(ITabbedViewHandler handler, TabbedPage view)
		{
			var tabBar = GetTabBar(handler);

			if (tabBar is null)
			{
				return;
			}

			view._defaultBarTranslucent = view._defaultBarTranslucent ?? tabBar.Translucent;
			switch (TabbedPageConfiguration.GetTranslucencyMode(view))
			{
				case TranslucencyMode.Translucent:
					tabBar.Translucent = true;
					return;
				case TranslucencyMode.Opaque:
					tabBar.Translucent = false;
					return;
				default:
					tabBar.Translucent = view._defaultBarTranslucent.GetValueOrDefault();
					return;
			}
		}

		// Tab bar item creation — ported from TabbedRenderer.SetTabBarItem
		async void SetTabBarItem(IPlatformViewHandler renderer, TabBarControllerManager manager)
		{
			var page = renderer.VirtualView as Page;

			if (page is null)
			{
				return;
			}

			// Increment generation to invalidate any in-flight icon loads for this page
			_tabBarItemGeneration ??= new Dictionary<Page, int>();
			_tabBarItemGeneration.TryGetValue(page, out var previousGen);
			var currentGen = previousGen + 1;
			_tabBarItemGeneration[page] = currentGen;

			var icons = await GetIcon(page);

			// Post-await guard: page or TabbedPage handler may have been removed during icon load
			if (page.Handler is null || renderer.ViewController is null || Children.IndexOf(page) < 0
				|| Handler is not TabbedViewHandler tvh || tvh.Manager is not TabBarControllerManager currentManager
				|| currentManager != manager)
			{
				icons?.Item1?.Dispose();
				icons?.Item2?.Dispose();
				return;
			}

			// Stale icon guard: a newer SetTabBarItem call superseded this one
			if (_tabBarItemGeneration.TryGetValue(page, out var latestGen) && latestGen != currentGen)
			{
				icons?.Item1?.Dispose();
				icons?.Item2?.Dispose();
				return;
			}

			var resizedImage = TabbedViewExtensions.AutoResizeTabBarImage(currentManager.TraitCollection, icons?.Item1);
			var resizedSelectedImage = TabbedViewExtensions.AutoResizeTabBarImage(currentManager.TraitCollection, icons?.Item2);

			renderer.ViewController.TabBarItem = new UITabBarItem(page.Title, resizedImage, resizedSelectedImage)
			{
				Tag = Children.IndexOf(page),
				AccessibilityIdentifier = page.AutomationId
			};

			resizedImage?.Dispose();
			resizedSelectedImage?.Dispose();
			icons?.Item1?.Dispose();
			icons?.Item2?.Dispose();
		}

		Task<Tuple<UIImage, UIImage>> GetIcon(Page page)
		{
			var source = new TaskCompletionSource<Tuple<UIImage, UIImage>>();

			var mauiContext = Handler?.MauiContext;
			if (mauiContext is null || page.IconImageSource is null)
			{
				source.SetResult(null);
				return source.Task;
			}

			try
			{
				page.IconImageSource.LoadImage(mauiContext, result =>
				{
					if (result?.Value is null)
					{
						source.TrySetResult(null);
					}
					else
					{
						source.TrySetResult(Tuple.Create(result.Value, (UIImage)null));
					}
				});
			}
			catch
			{
				source.TrySetResult(null);
			}

			return source.Task;
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		void UpdateiOS15TabBarAppearance(UITabBar tabBar)
		{
			tabBar.UpdateiOS15TabBarAppearance(
				ref _tabBarAppearance,
				_defaultBarColor,
				_defaultBarTextColor,
				IsSet(SelectedTabColorProperty) ? SelectedTabColor : null,
				IsSet(UnselectedTabColorProperty) ? UnselectedTabColor : null,
				IsSet(BarBackgroundColorProperty) ? BarBackgroundColor : null,
				IsSet(BarTextColorProperty) ? BarTextColor : null,
				IsSet(BarTextColorProperty) ? BarTextColor : null);
		}

		void UpdateChildrenOrderIndex(UIViewController[] viewControllers)
		{
			for (var i = 0; i < viewControllers.Length; i++)
			{
				var tabBarItem = viewControllers[i]?.TabBarItem;

				if (tabBarItem is null)
				{
					continue;
				}

				var originalIndex = (int)tabBarItem.Tag;

				if (originalIndex < 0 || originalIndex >= InternalChildren.Count)
				{
					continue;
				}

				var page = InternalChildren[originalIndex] as Page;

				if (page is not null)
				{
					SetIndex(page, i);
				}
			}
		}

		static void RefreshTabBarItemTags(TabbedPage view)
		{
			foreach (var child in view.InternalChildren)
			{
				if (child is Page page && page.Handler is IPlatformViewHandler pvh
					&& pvh.ViewController?.TabBarItem is UITabBarItem tabBarItem)
				{
					tabBarItem.Tag = view.Children.IndexOf(page);
				}
			}
		}
	}
}
