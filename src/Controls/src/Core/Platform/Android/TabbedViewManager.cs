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
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Handlers;

/// <summary>
/// Manages tabbed view UI on Android — handles ViewPager2, BottomNavigationView, TabLayout,
/// fragment placement, and tab appearance. Works against <see cref="ITabbedView"/> so it can be 
/// used by TabbedPageManager, ShellItemHandler (bottom tabs), and ShellSectionHandler (top tabs).
/// </summary>
internal class TabbedViewManager
{
    Fragment _tabLayoutFragment;
    ColorStateList _originalTabTextColors;
    ColorStateList _orignalTabIconColors;
    ColorStateList _newTabTextColors;
    ColorStateList _newTabIconColors;
    FragmentManager _fragmentManager;
    TabLayout _tabLayout;
    BottomNavigationView _bottomNavigationView;
    ColorStateList _originalBnvItemTextColors;
    ColorStateList _originalBnvItemIconTintColors;
    ViewPager2 _viewPager;
    int _previousTabIndex = -1;
    int[] _checkedStateSet = null;
    int[] _selectedStateSet = null;
    int[] _emptyStateSet = null;
    int _defaultARGBColor = Colors.Transparent.ToPlatform().ToArgb();
    AColor _defaultAndroidColor = Colors.Transparent.ToPlatform();
    readonly IMauiContext _context;
    readonly Listeners _listeners;
    protected ITabbedViewSource Element { get; set; }

    /// <summary>
    /// Gets or sets the TabLayout used for top tabs.
    /// Set this before calling <see cref="SetElement"/> to provide a pre-configured TabLayout.
    /// If not set, <see cref="SetElement"/> creates a default TabLayout for top tab placement.
    /// </summary>
    public TabLayout TabLayout
    {
        get => _tabLayout;
        set => _tabLayout = value;
    }

    public BottomNavigationView BottomNavigationView => _bottomNavigationView;
    public ViewPager2 ViewPager => _viewPager;
    int _tabplacementId;
    Brush _currentBarBackground;
    Color _currentBarItemColor;
    Color _currentBarTextColor;
    Color _currentBarSelectedItemColor;
    ColorStateList _currentBarTextColorStateList;
    bool _tabItemStyleLoaded;
    TabLayoutMediator _tabLayoutMediator;
    IDisposable _pendingFragment;

    /// <summary>
    /// Callback invoked when a tab is selected. The consumer (Shell, TabbedPageManager) 
    /// handles the actual navigation/selection.
    /// </summary>
    public Action<int> OnTabSelected { get; set; }

    /// <summary>
    /// Callback invoked when the ViewPager2 page changes (e.g., via swipe).
    /// </summary>
    public Action<int> OnPageSelected { get; set; }

    /// <summary>
    /// Callback invoked when the "More" overflow item is selected.
    /// </summary>
    public Action<int, BottomSheetDialog> OnMoreItemSelected { get; set; }

    /// <summary>
    /// Delegate for creating the ViewPager2 adapter. Consumers provide their own adapter 
    /// (e.g., MultiPageFragmentStateAdapter for TabbedPage, ShellSectionFragmentAdapter for Shell).
    /// </summary>
    public Func<FragmentManager, IMauiContext, RecyclerView.Adapter> CreateAdapter { get; set; }

    protected NavigationRootManager NavigationRootManager { get; }
    public static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppInfo.RequestedTheme) == AppTheme.Dark;

    #region Static Factory Methods

    static BottomNavigationView CreateBottomNavigationView(
        Context context,
        ViewGroup.LayoutParams layoutParams = null)
    {
        var bottomNav = new BottomNavigationView(context, null, Resource.Attribute.bottomNavigationViewStyle)
        {
            LayoutParameters = layoutParams ?? new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent),
            LabelVisibilityMode = Google.Android.Material.BottomNavigation.LabelVisibilityMode.LabelVisibilityLabeled
        };

        return bottomNav;
    }

    static ViewPager2 CreateViewPager2(
        Context context,
        ViewGroup.LayoutParams layoutParams = null)
    {
        return new ViewPager2(context)
        {
            OverScrollMode = OverScrollMode.Never,
            LayoutParameters = layoutParams ?? new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };
    }

    static int GetDefaultColorFromTheme(Context context)
    {
        if (context?.Theme is null)
        {
            return IsDarkTheme ? unchecked((int)0xB3FFFFFF) : unchecked((int)0x99000000);
        }

        var styledAttributes = context.Theme.ObtainStyledAttributes(
            null,
            Resource.Styleable.NavigationBarView,
            Resource.Attribute.bottomNavigationStyle,
            0);

        try
        {
            var defaultColors = styledAttributes.GetColorStateList(
                Resource.Styleable.NavigationBarView_itemIconTint);

            if (defaultColors is not null)
            {
                return defaultColors.DefaultColor;
            }

            if (IsDarkTheme)
            {
                return AndroidX.Core.Graphics.ColorUtils.SetAlphaComponent(
                    ContextCompat.GetColor(context, Resource.Color.primary_dark_material_light),
                    153);
            }
            else
            {
                return AndroidX.Core.Graphics.ColorUtils.SetAlphaComponent(
                    ContextCompat.GetColor(context, Resource.Color.primary_dark_material_dark),
                    153);
            }
        }
        finally
        {
            styledAttributes.Recycle();
        }
    }

    #endregion

    // When false, TabbedViewManager does not control the ViewPager2 adapter, position,
    // offscreen limit, or swipe paging. Used by Shell handlers that manage their own VP2.
    readonly bool _managesViewPager;

    public TabbedViewManager(IMauiContext context)
    {
        _context = context;
        _listeners = new Listeners(this);
        _managesViewPager = true;
        _viewPager = CreateViewPager2(context.Context);

        _viewPager.RegisterOnPageChangeCallback(_listeners);
    }

    /// <summary>
    /// Creates a TabbedViewManager that uses an external ViewPager2.
    /// The consumer manages the VP2 adapter, position, and lifecycle.
    /// TabbedViewManager manages BNV/TabLayout creation, fragment placement, and tab appearance.
    /// The VP2 reference is used for TabLayoutMediator (top tabs) only.
    /// </summary>
    public TabbedViewManager(IMauiContext context, ViewPager2 externalViewPager)
    {
        _context = context;
        _listeners = new Listeners(this);
        _viewPager = externalViewPager;
        _managesViewPager = false;
        // Do NOT register page change callback — consumer manages VP2 callbacks
    }

    internal IMauiContext MauiContext => _context;
    protected FragmentManager FragmentManager => _fragmentManager ??= _context.GetFragmentManager();
    public bool IsBottomTabPlacement => Element?.TabBarPlacement == TabBarPlacement.Bottom;

    public Color BarItemColor => Element?.UnselectedTabColor;

    public Color BarSelectedItemColor => Element?.SelectedTabColor;

    public virtual void SetElement(ITabbedViewSource tabbedView)
    {
        if (Element is not null)
        {
            Element.TabsChanged -= OnTabsCollectionChanged;
            RemoveTabs();

            if (_managesViewPager)
            {
                _viewPager.Adapter = null;
            }
        }

        Element = tabbedView;

        if (Element is not null)
        {
            // Let consumer provide the ViewPager2 adapter (only when managing VP2)
            if (_managesViewPager && CreateAdapter is not null)
            {
                _viewPager.Adapter = CreateAdapter(FragmentManager, _context);
            }

            if (IsBottomTabPlacement)
            {
                _bottomNavigationView = CreateBottomNavigationView(
                    _context.Context,
                    new CoordinatorLayout.LayoutParams(AppBarLayout.LayoutParams.MatchParent, AppBarLayout.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Bottom
                    });

                // Store original colors for restoration when custom colors are cleared
                _originalBnvItemTextColors = _bottomNavigationView.ItemTextColor;
                _originalBnvItemIconTintColors = _bottomNavigationView.ItemIconTintList;
            }
            else
            {
                if (_tabLayout is null)
                {
                    _tabLayout = new TabLayout(_context.Context)
                    {
                        TabMode = TabLayout.ModeFixed,
                        TabGravity = TabLayout.GravityFill,
                        LayoutParameters = new AppBarLayout.LayoutParams(AppBarLayout.LayoutParams.MatchParent, AppBarLayout.LayoutParams.WrapContent)
                    };
                }
            }

            OnTabsCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (_managesViewPager)
            {
                ScrollToCurrentTab();
            }

            if (Element.CurrentTab is not null)
            {
                _previousTabIndex = Element.Tabs.IndexOf(Element.CurrentTab);
            }

            Element.TabsChanged += OnTabsCollectionChanged;

            if (_managesViewPager)
            {
                SetTabLayout();
            }
        }
    }

    public void RemoveTabs()
    {
        _pendingFragment?.Dispose();
        _pendingFragment = null;

        if (_tabLayoutFragment is not null)
        {
            var fragment = _tabLayoutFragment;
            _tabLayoutFragment = null;

            var fragmentManager =
                _context
                    .GetNavigationRootManager()
                    .FragmentManager;

            if (!fragmentManager.IsDestroyed(_context?.Context))
            {
                SetContentBottomMargin(0);

                if (_context?.Context is Context c)
                {
                    _pendingFragment =
                        fragmentManager
                            .RunOrWaitForResume(c, fm =>
                            {
                                fm
                                    .BeginTransaction()
                                    .Remove(fragment)
                                    .SetReorderingAllowed(true)
                                    .Commit();
                            });
                }
            }

            _tabplacementId = 0;
        }
    }

    protected virtual void RootViewChanged(object sender, EventArgs e)
    {
        if (sender is NavigationRootManager rootManager)
        {
            rootManager.RootViewChanged -= RootViewChanged;
            SetTabLayout();
        }
    }

    public void SetTabLayout()
    {
        _pendingFragment?.Dispose();
        _pendingFragment = null;

        int id;
        var rootManager =
            _context.GetNavigationRootManager();

        _tabItemStyleLoaded = false;
        if (rootManager.RootView is null)
        {
            rootManager.RootViewChanged += RootViewChanged;
            return;
        }

        if (IsBottomTabPlacement)
        {
            id = Resource.Id.navigationlayout_bottomtabs;
            if (_tabplacementId == id)
                return;

            SetContentBottomMargin(_context.Context.Resources.GetDimensionPixelSize(Resource.Dimension.design_bottom_navigation_height));
        }
        else
        {
            id = Resource.Id.navigationlayout_toptabs;
            if (_tabplacementId == id)
                return;

            SetContentBottomMargin(0);
        }

        if (_context?.Context is Context c)
        {
            _pendingFragment =
                rootManager
                    .FragmentManager
                    .RunOrWaitForResume(c, fm =>
                    {
                        if (IsBottomTabPlacement)
                        {
                            _tabLayoutFragment = new ViewFragment(BottomNavigationView);
                        }
                        else
                        {
                            _tabLayoutFragment = new ViewFragment(TabLayout);
                        }

                        _tabplacementId = id;

                        fm
                            .BeginTransactionEx()
                            .ReplaceEx(id, _tabLayoutFragment)
                            .SetReorderingAllowed(true)
                            .Commit();
                    });
        }
    }

    public void SetContentBottomMargin(int bottomMargin)
    {
        var rootManager = _context.GetNavigationRootManager();
        var layoutContent = rootManager.RootView?.FindViewById(Resource.Id.navigationlayout_content);
        if (layoutContent is not null && layoutContent.LayoutParameters is ViewGroup.MarginLayoutParams cl)
        {
            cl.BottomMargin = bottomMargin;
        }
    }

    protected virtual void OnTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_managesViewPager)
        {
            ViewPager2 pager = _viewPager;
            pager.Adapter?.NotifyDataSetChanged();
        }

        if (IsBottomTabPlacement)
        {
            BottomNavigationView bottomNavigationView = _bottomNavigationView;

            if (_managesViewPager)
            {
                NotifyDataSetChanged();
            }

            if (Element.Tabs.Count == 0)
            {
                bottomNavigationView.Menu.Clear();
                _bottomNavigationView?.SetOnItemSelectedListener(null);
            }
            else
            {
                SetupBottomNavigationView();
            }
        }
        else
        {
            TabLayout tabs = _tabLayout;

            if (_managesViewPager)
            {
                NotifyDataSetChanged();
            }

            if (Element.Tabs.Count == 0)
            {
                tabs.RemoveAllTabs();
                tabs.SetupWithViewPager(null);
                _tabLayoutMediator?.Detach();
                _tabLayoutMediator = null;
            }
            else
            {
                if (_tabLayoutMediator is null && _viewPager is not null)
                {
                    _tabLayoutMediator = new TabLayoutMediator(tabs, _viewPager, _listeners);
                    _tabLayoutMediator.Attach();
                }

                UpdateTabIcons();
#pragma warning disable CS0618 // Type or member is obsolete
                tabs.AddOnTabSelectedListener(_listeners);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }

    internal void NotifyDataSetChanged()
    {
        if (!_managesViewPager)
        {
            return;
        }

        var adapter = _viewPager?.Adapter;
        if (adapter is not null)
        {
            var currentTab = Element.CurrentTab;
            var currentIndex = currentTab is not null ? Element.Tabs.IndexOf(currentTab) : -1;

            if (_viewPager.CurrentItem != currentIndex && currentIndex < Element.Tabs.Count && currentIndex >= 0)
                _viewPager.SetCurrentItem(currentIndex, false);

            adapter.NotifyDataSetChanged();
        }
    }

    /// <summary>
    /// Programmatically updates the selected tab in the BNV or TabLayout.
    /// Used by Shell handlers when switching sections/contents programmatically.
    /// </summary>
    public void SetSelectedTab(int index)
    {
        if (IsBottomTabPlacement)
        {
            if (_bottomNavigationView is not null && index >= 0 && _bottomNavigationView.SelectedItemId != index)
                _bottomNavigationView.SelectedItemId = index;
        }
        else if (_tabLayout is not null && index >= 0 && index < _tabLayout.TabCount)
        {
            _tabLayout.SelectTab(_tabLayout.GetTabAt(index));
        }
    }

    /// <summary>
    /// Triggers a full refresh of tab items from the <see cref="ITabbedView.Tabs"/> collection.
    /// Used when individual tab properties change (title, icon, isEnabled).
    /// </summary>
    public void RefreshTabs()
    {
        OnTabsCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected virtual void TabSelected(TabLayout.Tab tab)
    {
        if (Element is null)
        {
            return;
        }

        int selectedIndex = tab.Position;
        if (Element.Tabs.Count > selectedIndex && selectedIndex >= 0)
        {
            Element.CurrentTab = Element.Tabs[selectedIndex];
        }

        SetIconColorFilter(selectedIndex, tab, true);

        OnTabSelected?.Invoke(selectedIndex);
    }

    internal void ScrollToCurrentTab()
    {
        if (!_managesViewPager || Element?.CurrentTab is null)
        {
            return;
        }

        var currentIndex = Element.Tabs.IndexOf(Element.CurrentTab);
        if (currentIndex >= 0)
        {
            _viewPager.SetCurrentItem(currentIndex, Element.IsSmoothScrollEnabled);
        }
    }

    internal void UpdateOffscreenPageLimit()
    {
        if (!_managesViewPager)
        {
            return;
        }

        _viewPager.OffscreenPageLimit = Element.OffscreenPageLimit;
    }

    internal void UpdateSwipePaging()
    {
        if (!_managesViewPager)
        {
            return;
        }

        _viewPager.UserInputEnabled = Element.IsSwipePagingEnabled;
    }

    List<(string title, ImageSource icon, bool tabEnabled)> CreateTabList()
    {
        var items = new List<(string title, ImageSource icon, bool tabEnabled)>();

        for (int i = 0; i < Element.Tabs.Count; i++)
        {
            var tab = Element.Tabs[i];
            // ITab.Icon is IImageSource; convert to ImageSource if possible for BottomNavigationViewUtils
            var icon = tab.Icon as ImageSource;
            items.Add((tab.Title, icon, tab.IsEnabled));
        }

        return items;
    }

    internal virtual void SetupBottomNavigationView()
    {
        if (_bottomNavigationView is null)
        {
            return;
        }

        var menu = _bottomNavigationView.Menu;
        menu.Clear();

        var tabs = Element.Tabs;
        if (tabs is null || tabs.Count == 0)
        {
            return;
        }

        // Hide bottom navigation if only one tab
        if (tabs.Count == 1)
        {
            _bottomNavigationView.Visibility = ViewStates.Gone;
            return;
        }

        _bottomNavigationView.Visibility = ViewStates.Visible;

        _bottomNavigationView.SetOnItemSelectedListener(_listeners);

        int maxItems = _bottomNavigationView.MaxItemCount;
        bool showMore = tabs.Count > maxItems;
        int end = showMore ? maxItems - 1 : tabs.Count;

        for (int i = 0; i < end; i++)
        {
            var tab = tabs[i];
            var title = !string.IsNullOrWhiteSpace(tab.Title) ? tab.Title : $"Tab {i + 1}";
            var menuItem = menu.Add(0, i, i, title);

            if (menuItem is null)
            {
                continue;
            }

            if (!tab.IsEnabled)
            {
                menuItem.SetEnabled(false);
            }

            LoadBottomNavIconAsync(menuItem, tab);
        }

        // Add "More" overflow item if needed
        if (showMore)
        {
            var moreItem = menu.Add(0, BottomNavigationViewUtils.MoreTabId, maxItems - 1, "More");
            moreItem?.SetIcon(Resource.Drawable.abc_ic_menu_overflow_material);
        }

        // Set initial selection
        var currentTab = Element.CurrentTab;
        var currentIndex = currentTab is not null ? tabs.IndexOf(currentTab) : -1;
        if (currentIndex >= 0 && currentIndex < tabs.Count)
        {
            int targetId = currentIndex >= end ? BottomNavigationViewUtils.MoreTabId : currentIndex;
            _bottomNavigationView.SelectedItemId = targetId;
        }

        _bottomNavigationView.SetShiftMode(false, false);

        if (Element.CurrentTab is null && tabs.Count > 0)
            Element.CurrentTab = tabs[0];
    }

    async void LoadBottomNavIconAsync(IMenuItem menuItem, ITab tab)
    {
        try
        {
            if (tab.Icon is not ImageSource icon)
            {
                return;
            }

            var result = await icon.GetPlatformImageAsync(_context);
            if (result?.Value is not null && menuItem.IsAlive())
            {
                menuItem.SetIcon(result.Value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TabbedViewManager: Failed to load icon: {ex.Message}");
        }
    }

    internal virtual void UpdateTabIcons()
    {
        TabLayout tabs = _tabLayout;

        if (tabs.TabCount != Element.Tabs.Count)
            return;

        for (var i = 0; i < Element.Tabs.Count; i++)
        {
            ITab child = Element.Tabs[i];
            TabLayout.Tab tab = tabs.GetTabAt(i);
            SetTabIconImageSource(child, tab);
        }
    }

    internal virtual void SetTabIconImageSource(ITab tabItem, TabLayout.Tab tab, Drawable icon)
    {
        tab.SetIcon(icon);
        SetIconColorFilter(Element.Tabs.IndexOf(tabItem), tab);
    }

    void SetTabIconImageSource(ITab tabItem, TabLayout.Tab tab)
    {
        var icon = tabItem.Icon as ImageSource;
        icon?.LoadImage(
            _context,
            result =>
            {
                SetTabIconImageSource(tabItem, tab, result?.Value);
            });
    }

    public virtual void UpdateBarBackgroundColor()
    {
        if (Element.BarBackground is Brush)
        {
            return;
        }

        if (IsBottomTabPlacement)
        {
            if (_bottomNavigationView is not null)
            {
                if (Element.BarBackgroundColor is null)
                    _bottomNavigationView.SetBackground(null);
                else
                    _bottomNavigationView.SetBackgroundColor(Element.BarBackgroundColor.ToPlatform());
            }
        }
        else
        {
            Color tintColor = Element.BarBackgroundColor;

            if (tintColor is null)
            {
                _tabLayout.BackgroundTintMode = null;
            }
            else
            {
                _tabLayout.BackgroundTintMode = PorterDuff.Mode.Src;
                _tabLayout.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToPlatform());
            }
        }
    }

    public virtual void UpdateBarBackground()
    {
        var barBackground = Element.BarBackground as Brush;

        if (_currentBarBackground == barBackground)
        {
            return;
        }

        if (_currentBarBackground is GradientBrush oldGradientBrush)
        {
            oldGradientBrush.Parent = null;
            oldGradientBrush.InvalidateGradientBrushRequested -= OnBarBackgroundChanged;
        }

        _currentBarBackground = barBackground;

        if (_currentBarBackground is GradientBrush newGradientBrush)
        {
            newGradientBrush.Parent = Element as Element;
            newGradientBrush.InvalidateGradientBrushRequested += OnBarBackgroundChanged;
        }

        RefreshBarBackground();
    }

    void OnBarBackgroundChanged(object sender, EventArgs e)
    {
        RefreshBarBackground();
    }

    internal virtual void RefreshBarBackground()
    {
        if (IsBottomTabPlacement)
        {
            _bottomNavigationView?.UpdateBackground(_currentBarBackground);
        }
        else
        {
            _tabLayout.UpdateBackground(_currentBarBackground);
        }
    }

    internal virtual ColorStateList GetItemTextColorStates()
    {
        if (_originalTabTextColors is null)
        {
            _originalTabTextColors = IsBottomTabPlacement ? _bottomNavigationView.ItemTextColor : _tabLayout.TabTextColors;
        }

        Color barItemColor = BarItemColor;
        Color barTextColor = Element.BarTextColor;
        Color barSelectedItemColor = BarSelectedItemColor;

        if (barItemColor is null && barTextColor is null && barSelectedItemColor is null)
            return _originalTabTextColors;

        if (_newTabTextColors is not null)
            return _newTabTextColors;

        int checkedColor;
        int? defaultColor = null;

        if (barTextColor is not null)
        {
            checkedColor = barTextColor.ToPlatform().ToArgb();
            defaultColor = checkedColor;
        }
        else
        {
            defaultColor = GetItemTextColor(barItemColor, _originalTabTextColors);
            checkedColor = GetItemTextColor(barSelectedItemColor, _originalTabTextColors);
        }

        _newTabTextColors = GetColorStateList(defaultColor.Value, checkedColor);

        return _newTabTextColors;
    }

    int GetItemTextColor(Color customColor, ColorStateList originalColors)
    {
        return customColor?.ToPlatform().ToArgb() ?? originalColors?.DefaultColor ?? 0;
    }

    internal virtual ColorStateList GetItemIconTintColorState(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= Element.Tabs.Count)
            return null;

        var tab = Element.Tabs[tabIndex];

        // If the icon is a FontImageSource with a color, don't apply tint
        if (tab.Icon is FontImageSource fontImageSource && fontImageSource.Color is not null)
        {
            return null;
        }

        if (_orignalTabIconColors is null)
        {
            _orignalTabIconColors = IsBottomTabPlacement ? _bottomNavigationView.ItemIconTintList : _tabLayout.TabIconTint;
        }

        Color barItemColor = BarItemColor;
        Color barSelectedItemColor = BarSelectedItemColor;

        if (barItemColor is null && barSelectedItemColor is null)
        {
            return _orignalTabIconColors;
        }

        if (_newTabIconColors is not null)
        {
            return _newTabIconColors;
        }

        int defaultColor;
        int checkedColor;

        if (barItemColor is not null)
        {
            defaultColor = barItemColor.ToPlatform().ToArgb();
        }
        else
        {
            defaultColor = GetDefaultColor();
        }

        if (barSelectedItemColor is not null)
        {
            checkedColor = barSelectedItemColor.ToPlatform().ToArgb();
        }
        else
        {
            checkedColor = GetDefaultColor();
        }

        _newTabIconColors = GetColorStateList(defaultColor, checkedColor);
        return _newTabIconColors;
    }

    int GetDefaultColor()
    {
        return GetDefaultColorFromTheme(_context.Context);
    }

    void OnMoreSheetDismissedInternal(BottomSheetDialog dialog)
    {
        var currentTab = Element.CurrentTab;
        var index = currentTab is not null ? Element.Tabs.IndexOf(currentTab) : -1;

        var menu = _bottomNavigationView?.Menu;
        if (menu is null || index < 0)
            return;

        int targetIndex = Math.Min(index, menu.Size() - 1);
        if (targetIndex < 0)
            return;

        menu.GetItem(targetIndex)?.SetChecked(true);
    }

    void OnMoreItemSelectedInternal(int selectedIndex, BottomSheetDialog dialog)
    {
        if (selectedIndex >= 0 && _bottomNavigationView.SelectedItemId != selectedIndex && Element.Tabs.Count > selectedIndex)
        {
            Element.CurrentTab = Element.Tabs[selectedIndex];
        }

        OnMoreItemSelected?.Invoke(selectedIndex, dialog);

        dialog.Dismiss();
        dialog.Dispose();
    }

    void UpdateItemIconColor()
    {
        _newTabIconColors = null;

        if (IsBottomTabPlacement)
        {
            for (int i = 0; i < _bottomNavigationView.Menu.Size(); i++)
            {
                var menuItem = _bottomNavigationView.Menu.GetItem(i);
                SetupBottomNavigationViewIconColor(i, menuItem);
            }
        }
        else
        {
            for (int i = 0; i < _tabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = _tabLayout.GetTabAt(i);
                this.SetIconColorFilter(i, tab);
            }
        }
    }

    void SetupBottomNavigationViewIconColor(int tabIndex, IMenuItem menuItem)
    {
        ColorStateList colors = GetItemIconTintColorState(tabIndex);
#pragma warning disable XAOBS001 // Type or member is obsolete
        if (_bottomNavigationView?.GetChildAt(0) is BottomNavigationMenuView menuView)
        {
            if (tabIndex >= 0 && tabIndex < menuView.ChildCount)
            {
                var itemView = menuView.GetChildAt(tabIndex) as BottomNavigationItemView;
                itemView?.SetIconTintList(colors);
            }
        }
#pragma warning restore XAOBS001 // Type or member is obsolete
    }

    internal virtual void UpdateStyleForTabItem()
    {
        Color barItemColor = BarItemColor;
        Color barTextColor = Element.BarTextColor;
        Color barSelectedItemColor = BarSelectedItemColor;

        if (_tabItemStyleLoaded &&
            _currentBarItemColor == barItemColor &&
            _currentBarTextColor == barTextColor &&
            _currentBarSelectedItemColor == barSelectedItemColor)
        {
            return;
        }

        _tabItemStyleLoaded = true;
        _currentBarItemColor = BarItemColor;
        _currentBarTextColor = Element.BarTextColor;
        _currentBarSelectedItemColor = BarSelectedItemColor;

        if (IsBottomTabPlacement)
        {
            if (_bottomNavigationView is not null)
            {
                if (barItemColor is null && barSelectedItemColor is null)
                {
                    _bottomNavigationView.ItemTextColor = _originalBnvItemTextColors;
                    _bottomNavigationView.ItemIconTintList = _originalBnvItemIconTintColors;
                }
                else
                {
                    int unselectedArgb = barItemColor?.ToPlatform().ToArgb() ?? GetDefaultColorFromTheme(_context.Context);
                    int selectedArgb = barSelectedItemColor?.ToPlatform().ToArgb() ?? GetDefaultColorFromTheme(_context.Context);
                    var colorStateList = GetColorStateList(unselectedArgb, selectedArgb);
                    _bottomNavigationView.ItemTextColor = colorStateList;
                    _bottomNavigationView.ItemIconTintList = colorStateList;
                }
            }
        }
        else
        {
            UpdateBarTextColor();
            UpdateItemIconColor();
        }
    }

    internal void UpdateTabItemStyle()
    {
        UpdateStyleForTabItem();
    }

    void UpdateBarTextColor()
    {
        _newTabTextColors = null;

        _currentBarTextColorStateList = GetItemTextColorStates() ?? _originalTabTextColors;
        if (IsBottomTabPlacement)
        {
            _bottomNavigationView.ItemTextColor = _currentBarTextColorStateList;
        }
        else
        {
            _tabLayout.TabTextColors = _currentBarTextColorStateList;
        }
    }

    void SetIconColorFilter(int tabIndex, TabLayout.Tab tab)
    {
        SetIconColorFilter(tabIndex, tab, _tabLayout.GetTabAt(_tabLayout.SelectedTabPosition) == tab);
    }

    internal virtual void SetIconColorFilter(int tabIndex, TabLayout.Tab tab, bool selected)
    {
        var icon = tab.Icon;
        if (icon is null)
        {
            return;
        }

        ColorStateList colors = GetItemIconTintColorState(tabIndex);

        if (colors is null)
        {
            ADrawableCompat.SetTintList(icon, null);
        }
        else
        {
            int[] _stateSet = null;

            if (selected)
            {
                _stateSet = GetSelectedStateSet();
            }
            else
            {
                _stateSet = GetEmptyStateSet();
            }

            if (colors.GetColorForState(_stateSet, _defaultAndroidColor) == _defaultARGBColor)
            {
                ADrawableCompat.SetTintList(icon, null);
            }
            else
            {
                var wrappedIcon = ADrawableCompat.Wrap(icon);
                if (wrappedIcon != icon)
                {
                    icon = wrappedIcon;
                    tab.SetIcon(wrappedIcon);
                }

                icon.Mutate();
                icon.SetState(_stateSet);

                // FontImageSource has its own color — don't apply tint list
                if (tabIndex >= 0 && tabIndex < Element.Tabs.Count &&
                    Element.Tabs[tabIndex].Icon is not FontImageSource)
                {
                    _tabLayout.TabIconTint = colors;
                }

                ADrawableCompat.SetTintList(icon, colors);
            }
        }
        icon.InvalidateSelf();
    }

    int[] GetSelectedStateSet()
    {
        if (IsBottomTabPlacement)
        {
            if (_checkedStateSet is null)
            {
                _checkedStateSet = new int[] { global::Android.Resource.Attribute.StateChecked };
            }

            return _checkedStateSet;
        }
        else
        {
            if (_selectedStateSet is null)
            {
                _selectedStateSet = GetStateSet(new TempView(_context.Context).SelectedStateSet);
            }

            return _selectedStateSet;
        }
    }

    int[] GetEmptyStateSet()
    {
        if (_emptyStateSet is null)
        {
            _emptyStateSet = GetStateSet(new TempView(_context.Context).EmptyStateSet);
        }

        return _emptyStateSet;
    }

    class TempView : AView
    {
        public new IList<int> EmptyStateSet => AView.EmptyStateSet;
        public new IList<int> SelectedStateSet => AView.SelectedStateSet;
        public TempView(Context context) : base(context)
        {
        }
    }

    int[] GetStateSet(IList<int> stateSet)
    {
        var results = new int[stateSet.Count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = stateSet[i];
        }

        return results;
    }

    ColorStateList GetColorStateList(int defaultColor, int checkedColor)
    {
        int[][] states = new int[2][];
        int[] colors = new int[2];

        states[0] = GetSelectedStateSet();
        colors[0] = checkedColor;
        states[1] = GetEmptyStateSet();
        colors[1] = defaultColor;

#pragma warning disable RS0030
        return new ColorStateList(states, colors);
#pragma warning restore RS0030
    }

    class Listeners : ViewPager2.OnPageChangeCallback,
#pragma warning disable CS0618 // Type or member is obsolete
        TabLayout.IOnTabSelectedListener,
#pragma warning restore CS0618 // Type or member is obsolete
        NavigationBarView.IOnItemSelectedListener,
        TabLayoutMediator.ITabConfigurationStrategy
    {
        readonly TabbedViewManager _manager;

        public Listeners(TabbedViewManager manager)
        {
            _manager = manager;
        }

        public override void OnPageSelected(int position)
        {
            base.OnPageSelected(position);

            var element = _manager.Element;

            if (element is null)
            {
                return;
            }

            var IsBottomTabPlacement = _manager.IsBottomTabPlacement;
            var _bottomNavigationView = _manager._bottomNavigationView;

            if (element.Tabs.Count > 0 && position < element.Tabs.Count)
            {
                element.CurrentTab = element.Tabs[position];
            }

            if (IsBottomTabPlacement)
            {
                _bottomNavigationView.SelectedItemId = position;
            }

            _manager._previousTabIndex = position;
            _manager.OnPageSelected?.Invoke(position);
        }

        void TabLayoutMediator.ITabConfigurationStrategy.OnConfigureTab(TabLayout.Tab p0, int p1)
        {
            if (p1 < _manager.Element.Tabs.Count)
            {
                p0.SetText(_manager.Element.Tabs[p1].Title);
            }
        }

        bool NavigationBarView.IOnItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
        {
            if (_manager.Element is null)
            {
                return false;
            }

            var id = item.ItemId;
            if (id == BottomNavigationViewUtils.MoreTabId)
            {
                var items = _manager.CreateTabList();
                var bottomSheetDialog = BottomNavigationViewUtils.CreateMoreBottomSheet(
                    _manager.OnMoreItemSelectedInternal,
                    _manager._context,
                    items,
                    _manager._bottomNavigationView.MaxItemCount);
                bottomSheetDialog.DismissEvent += (s, e) => _manager.OnMoreSheetDismissedInternal(bottomSheetDialog);
                bottomSheetDialog.Show();
            }
            else
            {
                if (_manager._bottomNavigationView.SelectedItemId != item.ItemId && _manager.Element.Tabs.Count > item.ItemId)
                {
                    _manager.Element.CurrentTab = _manager.Element.Tabs[item.ItemId];
                    _manager.OnTabSelected?.Invoke(item.ItemId);
                }
            }

            return true;
        }

        void TabLayout.IOnTabSelectedListener.OnTabReselected(TabLayout.Tab tab)
        {
        }

        void TabLayout.IOnTabSelectedListener.OnTabSelected(TabLayout.Tab tab)
        {
            _manager.TabSelected(tab);
        }

        void TabLayout.IOnTabSelectedListener.OnTabUnselected(TabLayout.Tab tab)
        {
            if (_manager.Element?.CurrentTab is not null)
            {
                var currentIndex = _manager.Element.Tabs.IndexOf(_manager.Element.CurrentTab);
                _manager.SetIconColorFilter(currentIndex, tab, false);
            }
        }
    }
}
