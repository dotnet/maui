#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
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
	TabbedPageTabbedViewSourceAdapter _adapter;

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
			Element.InternalChildren.ForEach(page => TeardownPage(page as Page));
			((IPageController)Element).InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;
			Element.Appearing -= OnTabbedPageAppearing;
			Element.Disappearing -= OnTabbedPageDisappearing;
			ViewPager.LayoutChange -= OnLayoutChanged;
		}

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
	}

	protected virtual void OnTabbedPageDisappearing(object sender, EventArgs e)
	{
		_tabbedViewManager.RemoveTabs();
	}

	protected virtual void OnTabbedPageAppearing(object sender, EventArgs e)
	{
		_tabbedViewManager.SetTabLayout();
	}

	protected virtual void RootViewChanged(object sender, EventArgs e)
	{
		if (sender is NavigationRootManager rootManager)
		{
			rootManager.RootViewChanged -= RootViewChanged;
			_tabbedViewManager.SetTabLayout();
		}
	}

	#endregion

	#region Collection & Page Lifecycle

	protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		e.Apply((o, i, c) => SetupPage((Page)o), (o, i) => TeardownPage((Page)o), Reset);

		if (ViewPager.Adapter is MultiPageFragmentStateAdapter<Page> adapter)
		{
			adapter.CountOverride = Element.Children.Count;
		}

		// TabbedViewManager handles the tab UI refresh via TabsChanged event on the adapter
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
		if (e.PropertyName == Page.TitleProperty.PropertyName ||
			e.PropertyName == Page.IconImageSourceProperty.PropertyName)
		{
			// Refresh the tab display via TabbedViewManager
			_tabbedViewManager.RefreshTabs();
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

	#endregion

	#region Tab Appearance

	protected virtual void SetupBottomNavigationView()
	{
		_tabbedViewManager.SetupBottomNavigationView();
	}

	protected virtual void UpdateTabIcons()
	{
		_tabbedViewManager.UpdateTabIcons();
	}

	protected virtual void SetTabIconImageSource(Page page, TabLayout.Tab tab, Drawable icon)
	{
		var tabIndex = Element.Children.IndexOf(page);
		var tabs = ((ITabbedView)Element).Tabs;

		if (tabIndex >= 0 && tabIndex < tabs.Count)
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
		var index = Element.Children.IndexOf(Element.CurrentPage);
		if (BottomNavigationView is not null)
		{
			_tabbedViewManager.SetSelectedTab(index);
		}

		if (sender is BottomSheetDialog bsd)
		{
			bsd.DismissEvent -= OnMoreSheetDismissed;
		}
	}

	protected virtual void OnMoreItemSelected(int selectedIndex, BottomSheetDialog dialog)
	{
		if (selectedIndex >= 0 && BottomNavigationView?.SelectedItemId != selectedIndex && Element.Children.Count > selectedIndex)
		{
			Element.CurrentPage = Element.Children[selectedIndex];
		}

		dialog.Dismiss();
		dialog.DismissEvent -= OnMoreSheetDismissed;
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

		public event NotifyCollectionChangedEventHandler TabsChanged
		{
			add => ((IPageController)_tabbedPage).InternalChildren.CollectionChanged += value;
			remove => ((IPageController)_tabbedPage).InternalChildren.CollectionChanged -= value;
		}
	}

	#endregion
}