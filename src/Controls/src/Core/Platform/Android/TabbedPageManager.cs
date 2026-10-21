#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Tabs;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Handlers;

/// <summary>
/// Thin wrapper around <see cref="TabbedViewManager"/> for TabbedPage tab management on Android.
/// Bridges TabbedPage-specific concerns (Page lifecycle, per-page PropertyChanged) while delegating
/// all tab UI logic (BNV, TabLayout, fragment placement, colors, icons) to TabbedViewManager.
/// </summary>
public class TabbedPageManager
{
	#region Properties & Constructor

	readonly TabbedViewManager _tabbedViewManager;
	readonly IMauiContext _context;
	readonly Dictionary<int, Page> _bottomBadgePages = new();
	readonly Dictionary<int, Page> _topBadgePages = new();
	readonly NativeElementRegistrationSet _nativeTabRegistrations = new();
	readonly NativeElementRegistrationSet _nativeMoreRegistrations = new();
	readonly List<IMenuItem> _registeredMenuItems = new();
	readonly List<AView> _moreItemViews = new();
	TabbedPageTabbedViewSourceAdapter _adapter;
	bool _badgesNeedUpdate;
	BottomSheetDialog _moreDialog;
	int _tabRegistrationGeneration;

	protected TabbedPage Element { get; set; }
	protected Page previousPage;

	public TabLayout TabLayout => _tabbedViewManager.TabLayout;
	public BottomNavigationView BottomNavigationView => _tabbedViewManager.BottomNavigationView;
	public ViewPager2 ViewPager => _tabbedViewManager.ViewPager;
	public bool IsBottomTabPlacement => _tabbedViewManager.IsBottomTabPlacement;
	public Color BarItemColor => _tabbedViewManager.BarItemColor;
	public Color BarSelectedItemColor => _tabbedViewManager.BarSelectedItemColor;
	protected NavigationRootManager NavigationRootManager => _context.GetNavigationRootManager();
	protected FragmentManager FragmentManager => _context.GetFragmentManager();
	public static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppInfo.RequestedTheme) == AppTheme.Dark;

	public TabbedPageManager(IMauiContext context)
	{
		_context = context;
		_tabbedViewManager = new TabbedViewManager(context)
		{
			// Wire TabbedViewManager callbacks to TabbedPageManager methods
			OnPageSelected = OnPageSelectedInternal,
			OnMoreItemSelected = OnMoreItemSelectedInternal,
			CreateMoreBottomSheet = CreateMoreBottomSheet,

			// Consumer provides the ViewPager2 adapter
			CreateAdapter = (fm, ctx) =>
					new MultiPageFragmentStateAdapter<Page>(Element, fm, ctx) { CountOverride = Element.Children.Count }
		};
	}

	internal IMauiContext MauiContext => _context;

	#endregion

	#region Element Lifecycle

	public virtual void SetElement(TabbedPage tabbedPage)
	{
		if (Element is not null)
		{
			_tabRegistrationGeneration++;
			_nativeTabRegistrations.Clear();
			CloseMoreDialog();
			_registeredMenuItems.Clear();
			Element.InternalChildren.ForEach(page => TeardownPage(page as Page));
			((IPageController)Element).InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;
			Element.Appearing -= OnTabbedPageAppearing;
			Element.Disappearing -= OnTabbedPageDisappearing;
			ViewPager.LayoutChange -= OnLayoutChanged;
			previousPage = null;
		}

		_bottomBadgePages.Clear();
		_topBadgePages.Clear();
		Element = tabbedPage;

		if (Element is not null)
		{
			ViewPager.LayoutChange += OnLayoutChanged;
			Element.Appearing += OnTabbedPageAppearing;
			Element.Disappearing += OnTabbedPageDisappearing;

			// Wire per-page property tracking and collection change for page lifecycle
			// Subscribe BEFORE SetElement so CountOverride is updated before NotifyDataSetChanged
			foreach (var page in Element.Children)
			{
				SetupPage(page);
			}

			((IPageController)tabbedPage).InternalChildren.CollectionChanged += OnChildrenCollectionChanged;

			// Create adapter and delegate to TabbedViewManager
			_adapter = new TabbedPageTabbedViewSourceAdapter(Element);
			_tabbedViewManager.SetElement(_adapter);
			ScheduleBadgeUpdate();
			ScheduleNativeTabRegistration();

			previousPage = tabbedPage.CurrentPage;
		}
		else
		{
			_tabbedViewManager.SetElement(null);
			_adapter = null;
		}
	}

	protected virtual void OnLayoutChanged(object sender, AView.LayoutChangeEventArgs e)
	{
		Element.Arrange(e);
		if (_badgesNeedUpdate)
		{
			TryUpdateAllBadges();
		}
	}

	protected virtual void OnTabbedPageDisappearing(object sender, EventArgs e)
	{
		// Element.Navigation.NavigationStack is resolved through the
		// NavigationProxy, which already walks the parent chain to find
		// the nearest NavigationPage ancestor.
		var navStack = Element?.Navigation?.NavigationStack;
		if (navStack is not null && navStack.Count > 0)
		{
			// If the TabbedPage is no longer the top page in the nav stack,
			// a page was pushed over it — remove tabs.
			if (navStack[navStack.Count - 1] != Element)
			{
				_tabbedViewManager.RemoveTabs();
				return;
			}

			// TabbedPage is still the top page in the nav stack, so this
			// Disappearing was triggered by a modal overlay or app lifecycle.
			// Keep tabs visible.
			return;
		}

		// No NavigationPage ancestor — original behavior applies.
		// This branch covers two cases (see PR #32878):
		// 1. A modal page is pushed over a root TabbedPage — ModalStack contains
		//    the modal, so we keep tabs alive and restore them on modal dismiss.
		// 2. The TabbedPage itself was pushed as a modal — ModalStack includes the
		//    TabbedPage, so tabs also stay. Disappearing only fires when something
		//    is later shown over it, and the guard still holds.
		// Do NOT simplify this check; removing it re-introduces the regression
		// where tabs are destroyed on modal overlay.
		if (Element?.Navigation?.ModalStack?.Count > 0)
		{
			return;
		}

		_tabbedViewManager.RemoveTabs();
	}

	protected virtual void OnTabbedPageAppearing(object sender, EventArgs e)
	{
		SetTabLayoutAndUpdateBadges();
	}

	protected virtual void RootViewChanged(object sender, EventArgs e)
	{
		if (sender is NavigationRootManager rootManager)
		{
			rootManager.RootViewChanged -= RootViewChanged;
			SetTabLayoutAndUpdateBadges();
		}
	}

	void SetTabLayoutAndUpdateBadges()
	{
		_tabbedViewManager.SetTabLayout();
		ScheduleBadgeUpdate();
		ScheduleNativeTabRegistration();
	}

	#endregion

	#region Collection & Page Lifecycle

	protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		_tabRegistrationGeneration++;
		CloseMoreDialog();
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			_nativeTabRegistrations.Clear();
			_registeredMenuItems.Clear();
		}

		e.Apply((o, i, c) => SetupPage((Page)o), (o, i) => TeardownPage((Page)o), Reset);

		if (ViewPager.Adapter is MultiPageFragmentStateAdapter<Page> adapter)
		{
			adapter.CountOverride = Element.Children.Count;
		}

		// TabbedViewManager handles the tab UI refresh via TabsChanged event on the adapter
		RemoveBadgePageMappings(
			_bottomBadgePages,
			Element.Children.Count,
			index => BottomNavigationView?.RemoveBadge(index));
		RemoveBadgePageMappings(
			_topBadgePages,
			Element.Children.Count,
			index => TabLayout?.GetTabAt(index)?.RemoveBadge());
		ScheduleBadgeUpdate();
		ScheduleNativeTabRegistration();
		UpdateIgnoreContainerAreas();
	}

	protected void NotifyDataSetChanged()
	{
		_tabbedViewManager.NotifyDataSetChanged();
	}

	#endregion

	#region Tab Selection & Navigation

	protected virtual void TabSelected(TabLayout.Tab tab)
	{
		if (Element is null)
		{
			return;
		}

		int selectedIndex = tab.Position;

		if (Element.Children.Count > selectedIndex && selectedIndex >= 0)
		{
			Element.CurrentPage = Element.Children[selectedIndex];
		}

		SetIconColorFilter(Element.CurrentPage, tab, true);
	}

	#endregion

	#region Per-Page Lifecycle

	void TeardownPage(Page page)
	{
		_nativeTabRegistrations.UnregisterOwner(page);
		page.PropertyChanged -= OnPagePropertyChangedInternal;
	}

	void SetupPage(Page page)
	{
		page.PropertyChanged += OnPagePropertyChangedInternal;
	}

	void Reset()
	{
		foreach (var page in Element.Children)
		{
			SetupPage(page);
		}
	}

	protected virtual void OnPagePropertyChanged(Page page, PropertyChangedEventArgs e)
	{
		if (Element is null)
		{
			return;
		}

		var index = Element.Children.IndexOf(page);
		if (index < 0)
		{
			return;
		}

		if (e.PropertyName == Page.TitleProperty.PropertyName)
		{
			_tabbedViewManager.UpdateTabTitle(index, page.Title);
		}
		else if (e.PropertyName == Page.IconImageSourceProperty.PropertyName)
		{
			_tabbedViewManager.UpdateTabIcon(index);
		}
		else if (e.PropertyName == TabbedPage.BadgeTextProperty.PropertyName ||
			e.PropertyName == TabbedPage.BadgeColorProperty.PropertyName ||
			e.PropertyName == TabbedPage.BadgeTextColorProperty.PropertyName)
		{
			UpdateBadge(page, index, e.PropertyName);
		}
	}

	void OnPagePropertyChangedInternal(object sender, PropertyChangedEventArgs e)
	{
		OnPagePropertyChanged((Page)sender, e);
	}

	internal void ScrollToCurrentPage()
	{
		_tabbedViewManager.ScrollToCurrentTab();
	}

	void UpdateIgnoreContainerAreas()
	{
		foreach (IPageController child in Element.Children)
		{
			child.IgnoresContainerArea = child is NavigationPage;
		}
	}

	[Obsolete]
	internal void UpdateOffscreenPageLimit()
	{
		_tabbedViewManager.UpdateOffscreenPageLimit();
	}

	internal void UpdateSwipePaging()
	{
		_tabbedViewManager.UpdateSwipePaging();
	}

	void TryUpdateAllBadges()
	{
		if (AreBadgeViewsReady() && UpdateAllBadges())
		{
			_badgesNeedUpdate = false;
		}
	}

	void ScheduleBadgeUpdate()
	{
		_badgesNeedUpdate = true;
		ViewPager?.Post(TryUpdateAllBadges);
	}

	void ScheduleNativeTabRegistration()
	{
		var registrationGeneration = ++_tabRegistrationGeneration;
		ViewPager?.Post(() =>
		{
			if (registrationGeneration != _tabRegistrationGeneration || Element is null)
				return;

			if (IsBottomTabPlacement)
			{
				var bottomNavigationView = BottomNavigationView;
				if (bottomNavigationView is null || !bottomNavigationView.IsAlive())
					return;

				_nativeTabRegistrations.Register(
					Element,
					bottomNavigationView,
					NativeElementRoles.ShellTab,
					NativeElementDiscriminators.TabBar);

				var menuItems = new List<IMenuItem>();
				for (var index = 0; index < bottomNavigationView.Menu.Size(); index++)
					menuItems.Add(bottomNavigationView.Menu.GetItem(index));

				RegisterBottomMenuItems(menuItems, Element.Children.ToList());
				return;
			}

			var tabLayout = TabLayout;
			if (tabLayout is null || !tabLayout.IsAlive())
				return;

			_nativeTabRegistrations.Clear();
			_nativeTabRegistrations.Register(
				Element,
				tabLayout,
				NativeElementRoles.ShellTab,
				NativeElementDiscriminators.TabBar);

			var retainedElements = new List<object> { tabLayout };
			var count = Math.Min(tabLayout.TabCount, Element.Children.Count);
			for (var index = 0; index < count; index++)
			{
				var tab = tabLayout.GetTabAt(index);
				RegisterTopTab(tab, index);
				if (tab?.View is AView tabView)
					retainedElements.Add(tabView);
			}

			_nativeTabRegistrations.Retain(retainedElements);
		});
	}

	bool AreBadgeViewsReady()
	{
		if (Element is null)
		{
			return false;
		}

		if (!IsBottomTabPlacement)
		{
			return TabLayout is { } tabLayout && tabLayout.TabCount >= Element.Children.Count;
		}

		var bottomNavigationView = BottomNavigationView;
		var maxItems = Math.Min(
			bottomNavigationView?.MaxItemCount ?? 0,
			BottomNavigationViewUtils.MaxBottomNavigationItems);
		var expectedItemCount = Math.Min(Element.Children.Count, maxItems);
		return maxItems > 0 &&
			bottomNavigationView is not null &&
			bottomNavigationView.Menu.Size() >= expectedItemCount;
	}

	internal bool UpdateAllBadges()
	{
		if (Element is null)
		{
			return false;
		}

		if (Element.Children.Count == 0)
		{
			_bottomBadgePages.Clear();
			_topBadgePages.Clear();
			return true;
		}

		if (IsBottomTabPlacement)
		{
			var bottomNavigationView = BottomNavigationView;
			var maxItems = Math.Min(
				bottomNavigationView?.MaxItemCount ?? 0,
				BottomNavigationViewUtils.MaxBottomNavigationItems);
			if (maxItems <= 0 || bottomNavigationView.Menu.Size() == 0)
			{
				return false;
			}

			var hasOverflow = Element.Children.Count > maxItems;
			var lastIndexToUpdate = hasOverflow
				? maxItems - 2
				: Math.Min(Element.Children.Count, maxItems) - 1;

			for (var i = 0; i <= lastIndexToUpdate; i++)
			{
				UpdateBottomBadge(Element.Children[i], i);
			}

			if (hasOverflow)
			{
				// SetupMenu uses each child index as its menu item ID; Material badge APIs take IDs, not positions.
				bottomNavigationView.RemoveBadge(maxItems - 1);
			}

			RemoveBadgePageMappings(
				_bottomBadgePages,
				lastIndexToUpdate + 1,
				index => bottomNavigationView.RemoveBadge(index));
		}
		else
		{
			var tabLayout = TabLayout;
			if (tabLayout is null)
			{
				return false;
			}

			if (tabLayout.TabCount < Element.Children.Count)
			{
				return false;
			}

			for (var i = 0; i < Element.Children.Count && i < tabLayout.TabCount; i++)
			{
				UpdateTopBadge(Element.Children[i], i);
			}
		}

		return true;
	}

	void UpdateBadge(Page page, int index, string propertyName)
	{
		if (IsBottomTabPlacement)
		{
			var bottomNavigationView = BottomNavigationView;
			if (bottomNavigationView is null)
			{
				ScheduleBadgeUpdate();
				return;
			}

			var maxItems = Math.Min(
				bottomNavigationView.MaxItemCount,
				BottomNavigationViewUtils.MaxBottomNavigationItems);
			if (Element.Children.Count > maxItems && index >= maxItems - 1)
			{
				return;
			}

			UpdateBottomBadge(page, index, propertyName);
		}
		else
		{
			UpdateTopBadge(page, index, propertyName);
		}
	}

	void UpdateBottomBadge(Page page, int index, string propertyName = null)
	{
		var bottomNavigationView = BottomNavigationView;
		if (bottomNavigationView is null)
		{
			ScheduleBadgeUpdate();
			return;
		}

		var badgeText = TabbedPage.GetBadgeText(page);
		if (_bottomBadgePages.TryGetValue(index, out var previousBadgePage) && previousBadgePage != page)
		{
			bottomNavigationView.RemoveBadge(index);
		}
		_bottomBadgePages[index] = page;

		if (badgeText is null)
		{
			bottomNavigationView.RemoveBadge(index);
			return;
		}

		var badgeColor = TabbedPage.GetBadgeColor(page);
		var badgeTextColor = TabbedPage.GetBadgeTextColor(page);
		if ((propertyName == TabbedPage.BadgeColorProperty.PropertyName && badgeColor is null) ||
			(propertyName == TabbedPage.BadgeTextColorProperty.PropertyName && badgeTextColor is null))
		{
			bottomNavigationView.RemoveBadge(index);
		}

		var badge = bottomNavigationView.GetOrCreateBadge(index);
		if (badgeText.Length > 0)
		{
			badge.Text = badgeText;
		}
		else
		{
			badge.Text = null;
			badge.ClearNumber();
		}

		if (badgeColor is not null)
		{
			badge.BackgroundColor = badgeColor.ToPlatform();
		}

		if (badgeTextColor is not null)
		{
			badge.BadgeTextColor = badgeTextColor.ToPlatform();
		}
	}

	void UpdateTopBadge(Page page, int index, string propertyName = null)
	{
		var tab = TabLayout?.GetTabAt(index);
		if (tab is null)
		{
			ScheduleBadgeUpdate();
			return;
		}

		var badgeText = TabbedPage.GetBadgeText(page);
		if (_topBadgePages.TryGetValue(index, out var previousBadgePage) && previousBadgePage != page)
		{
			tab.RemoveBadge();
		}
		_topBadgePages[index] = page;

		if (badgeText is null)
		{
			tab.RemoveBadge();
			return;
		}

		var badgeColor = TabbedPage.GetBadgeColor(page);
		var badgeTextColor = TabbedPage.GetBadgeTextColor(page);
		if ((propertyName == TabbedPage.BadgeColorProperty.PropertyName && badgeColor is null) ||
			(propertyName == TabbedPage.BadgeTextColorProperty.PropertyName && badgeTextColor is null))
		{
			tab.RemoveBadge();
		}

		var badge = tab.OrCreateBadge;
		if (badgeText.Length > 0)
		{
			badge.Text = badgeText;
		}
		else
		{
			badge.Text = null;
			badge.ClearNumber();
		}

		if (badgeColor is not null)
		{
			badge.BackgroundColor = badgeColor.ToPlatform();
		}

		if (badgeTextColor is not null)
		{
			badge.BadgeTextColor = badgeTextColor.ToPlatform();
		}
	}

	static void RemoveBadgePageMappings(
		Dictionary<int, Page> badgePages,
		int firstIndexToRemove,
		Action<int> removeNativeBadge)
	{
		var indicesToRemove = badgePages.Keys
			.Where(index => index >= firstIndexToRemove)
			.ToArray();

		foreach (var index in indicesToRemove)
		{
			removeNativeBadge(index);
			badgePages.Remove(index);
		}
	}

	#endregion

	#region Tab Appearance

	protected virtual void SetupBottomNavigationView()
	{
		_tabbedViewManager.SetupBottomNavigationView();
		ScheduleNativeTabRegistration();
	}

	void RegisterBottomMenuItems(
		IReadOnlyList<IMenuItem> menuItems,
		IReadOnlyList<Page> pages)
	{
		var registrationItems = new List<(IMenuItem MenuItem, object Owner, bool IsMoreItem)>();
		foreach (var previousMenuItem in _registeredMenuItems)
		{
			var retained = false;
			foreach (var currentMenuItem in menuItems)
			{
				if (ReferenceEquals(currentMenuItem, previousMenuItem))
				{
					retained = true;
					break;
				}
			}

			if (!retained)
				_nativeTabRegistrations.Unregister(previousMenuItem);
		}

		_registeredMenuItems.Clear();
		_registeredMenuItems.AddRange(menuItems);

		foreach (var menuItem in menuItems)
		{
			var isMoreItem = menuItem.ItemId == BottomNavigationViewUtils.MoreTabId;
			if (!isMoreItem && (menuItem.ItemId < 0 || menuItem.ItemId >= pages.Count))
				continue;

			object owner = isMoreItem ? Element : pages[menuItem.ItemId];
			registrationItems.Add((menuItem, owner, isMoreItem));
			_nativeTabRegistrations.Register(
				owner,
				menuItem,
				isMoreItem ? NativeElementRoles.ShellTabOverflow : NativeElementRoles.ShellTab,
				NativeElementDiscriminators.LogicalModel);
		}

		var registrationGeneration = ++_tabRegistrationGeneration;
		BottomNavigationView.Post(() =>
		{
			if (registrationGeneration != _tabRegistrationGeneration ||
				BottomNavigationView is null ||
				!BottomNavigationView.IsAlive())
				return;

			var retainedElements = new List<object> { BottomNavigationView };
			foreach (var registrationItem in registrationItems)
				retainedElements.Add(registrationItem.MenuItem);

			if (BottomNavigationView?.GetChildAt(0) is ViewGroup menuView)
			{
				var count = Math.Min(menuView.ChildCount, registrationItems.Count);
				for (int index = 0; index < count; index++)
				{
					var registrationItem = registrationItems[index];
					if (menuView.GetChildAt(index) is AView itemView)
					{
						retainedElements.Add(itemView);
						_nativeTabRegistrations.RegisterExclusive(
							registrationItem.Owner,
							itemView,
							registrationItem.IsMoreItem ? NativeElementRoles.ShellTabOverflow : NativeElementRoles.ShellTab,
							NativeElementDiscriminators.RealizedView);
					}
				}
			}

			_nativeTabRegistrations.Retain(retainedElements);
		});
	}

	void RegisterTopTab(TabLayout.Tab tab, int position)
	{
		if (position < 0 || position >= Element.Children.Count || tab.View is not AView tabView)
			return;

		_nativeTabRegistrations.RegisterExclusive(
			Element.Children[position],
			tabView,
			NativeElementRoles.ShellTab,
			NativeElementDiscriminators.RealizedView);
	}

	protected virtual void UpdateTabIcons()
	{
		_tabbedViewManager.UpdateTabIcons();
	}

	protected virtual void SetTabIconImageSource(Page page, TabLayout.Tab tab, Drawable icon)
	{
		var tabIndex = Element.Children.IndexOf(page);
		var tabs = _adapter?.Tabs;

		if (tabs is not null && tabIndex >= 0 && tabIndex < tabs.Count)
		{
			_tabbedViewManager.SetTabIconImageSource(tabs[tabIndex], tab, icon);
		}
	}

	public virtual void UpdateBarBackgroundColor()
	{
		_tabbedViewManager.UpdateBarBackgroundColor();
	}

	public virtual void UpdateBarBackground()
	{
		_tabbedViewManager.UpdateBarBackground();
	}

	protected virtual void RefreshBarBackground()
	{
		_tabbedViewManager.RefreshBarBackground();
	}

	protected virtual ColorStateList GetItemTextColorStates()
	{
		return _tabbedViewManager.GetItemTextColorStates();
	}

	protected virtual ColorStateList GetItemIconTintColorState(Page page)
	{
		var tabIndex = Element.Children.IndexOf(page);
		return _tabbedViewManager.GetItemIconTintColorState(tabIndex);
	}

	protected virtual void OnMoreSheetDismissed(object sender, EventArgs e)
	{
		ClearMoreRegistrations();
		var index = Element.Children.IndexOf(Element.CurrentPage);

		if (BottomNavigationView is not null)
		{
			_tabbedViewManager.SetSelectedTab(index);
		}

		if (sender is BottomSheetDialog bsd)
		{
			bsd.DismissEvent -= OnMoreSheetDismissed;
			if (ReferenceEquals(_moreDialog, bsd))
				_moreDialog = null;
		}
	}

	protected virtual void OnMoreItemSelected(int selectedIndex, BottomSheetDialog dialog)
	{
		if (selectedIndex >= 0 && BottomNavigationView?.SelectedItemId != selectedIndex && Element.Children.Count > selectedIndex)
		{
			Element.CurrentPage = Element.Children[selectedIndex];
		}
	}

	BottomSheetDialog CreateMoreBottomSheet(
		Action<int, BottomSheetDialog> selectCallback,
		List<(string title, ImageSource icon, bool tabEnabled)> items)
	{
		PrepareMoreRegistrations();
		var dialog = BottomNavigationViewUtils.CreateMoreBottomSheet(
			selectCallback,
			_context,
			items,
			Math.Min(BottomNavigationView.MaxItemCount, BottomNavigationViewUtils.MaxBottomNavigationItems),
			RegisterMoreRow);
		_moreDialog = dialog;
		dialog.DismissEvent += OnMoreSheetDismissed;
		BottomNavigationView.Post(() =>
		{
			if (ReferenceEquals(_moreDialog, dialog) && dialog.IsShowing)
				RegisterMoreDialog(dialog);
		});
		return dialog;
	}

	void PrepareMoreRegistrations()
	{
		ClearMoreRegistrations();
	}

	void RegisterMoreRow(int pageIndex, AView view)
	{
		if (pageIndex < 0 || pageIndex >= Element.Children.Count)
		{
			DisposeMoreItemView(view);
			return;
		}

		_moreItemViews.Add(view);
		_nativeMoreRegistrations.Register(
			Element.Children[pageIndex],
			view,
			NativeElementRoles.ShellTabOverflow,
			NativeElementDiscriminators.OverflowRow);
	}

	void RegisterMoreDialog(BottomSheetDialog dialog)
	{
		if (dialog.Window?.DecorView is AView dialogView)
		{
			_nativeMoreRegistrations.Register(
				Element,
				dialogView,
				NativeElementRoles.ShellTabOverflow,
				NativeElementDiscriminators.RealizedView);
		}
	}

	void ClearMoreRegistrations()
	{
		_nativeMoreRegistrations.Clear();
		foreach (var view in _moreItemViews)
			DisposeMoreItemView(view);
		_moreItemViews.Clear();
	}

	static void DisposeMoreItemView(AView view)
	{
		if (view.Parent is ViewGroup parent)
			parent.RemoveView(view);
		view.Dispose();
	}

	void CloseMoreDialog()
	{
		var dialog = _moreDialog;
		_moreDialog = null;
		ClearMoreRegistrations();
		if (dialog is null)
			return;

		dialog.DismissEvent -= OnMoreSheetDismissed;
		if (dialog.IsShowing)
			dialog.Dismiss();
		dialog.Dispose();
	}

	protected virtual void UpdateStyleForTabItem()
	{
		_tabbedViewManager.UpdateStyleForTabItem();
	}

	internal void UpdateTabItemStyle()
	{
		UpdateStyleForTabItem();
	}

	protected virtual void SetIconColorFilter(Page page, TabLayout.Tab tab, bool selected)
	{
		var tabIndex = Element.Children.IndexOf(page);
		_tabbedViewManager.SetIconColorFilter(tabIndex, tab, selected);
	}

	#endregion

	#region VP2 Page Change Callbacks

	void OnPageSelectedInternal(int position)
	{
		if (Element is null)
		{
			return;
		}

		if (previousPage != Element.CurrentPage)
		{
			previousPage?.SendDisappearing();
			previousPage = Element.CurrentPage;
		}

		if (Element.Children.Count > 0 && position < Element.Children.Count)
		{
			Element.CurrentPage = Element.Children[position];
			Element.CurrentPage.SendAppearing();
		}
	}

	void OnMoreItemSelectedInternal(int selectedIndex, BottomSheetDialog dialog)
	{
		OnMoreItemSelected(selectedIndex, dialog);
	}

	#endregion

	#region TabbedPageTabbedViewSourceAdapter

	/// <summary>
	/// Adapter that bridges TabbedPage to ITabbedViewSource for TabbedViewManager consumption.
	/// </summary>
	sealed class TabbedPageTabbedViewSourceAdapter : ITabbedViewSource
	{
		readonly TabbedPage _tabbedPage;

		public TabbedPageTabbedViewSourceAdapter(TabbedPage tabbedPage)
		{
			_tabbedPage = tabbedPage;
		}

		public IReadOnlyList<ITab> Tabs =>
			_tabbedPage.Children.Select(p => (ITab)new TabbedPage.PageTabAdapter(p)).ToList();

		public ITab CurrentTab
		{
			get => _tabbedPage.CurrentPage is not null ? new TabbedPage.PageTabAdapter(_tabbedPage.CurrentPage) : null;
			set
			{
				if (value is TabbedPage.PageTabAdapter adapter)
					_tabbedPage.CurrentPage = adapter.Page;
			}
		}

		public int CurrentTabIndex =>
			_tabbedPage.CurrentPage is not null
				? _tabbedPage.Children.IndexOf(_tabbedPage.CurrentPage)
				: -1;

		public Color BarBackgroundColor => _tabbedPage.BarBackgroundColor;
		public object BarBackground => _tabbedPage.BarBackground;
		public Color BarTextColor => _tabbedPage.BarTextColor;

		public Color UnselectedTabColor =>
			_tabbedPage.IsSet(TabbedPage.UnselectedTabColorProperty)
				? _tabbedPage.UnselectedTabColor
				: null;

		public Color SelectedTabColor =>
			_tabbedPage.IsSet(TabbedPage.SelectedTabColorProperty)
				? _tabbedPage.SelectedTabColor
				: null;

		public TabBarPlacement TabBarPlacement =>
			_tabbedPage.OnThisPlatform().GetToolbarPlacement() == ToolbarPlacement.Bottom
				? TabBarPlacement.Bottom
				: TabBarPlacement.Top;

		public int OffscreenPageLimit =>
#pragma warning disable CS0618 // Type or member is obsolete
			_tabbedPage.OnThisPlatform().OffscreenPageLimit();
#pragma warning restore CS0618

		public bool IsSwipePagingEnabled => _tabbedPage.OnThisPlatform().IsSwipePagingEnabled();
		public bool IsSmoothScrollEnabled => _tabbedPage.OnThisPlatform().IsSmoothScrollEnabled();

		public Element Owner => _tabbedPage;

		public event NotifyCollectionChangedEventHandler TabsChanged
		{
			add => ((IPageController)_tabbedPage).InternalChildren.CollectionChanged += value;
			remove => ((IPageController)_tabbedPage).InternalChildren.CollectionChanged -= value;
		}
	}

	#endregion
}
