using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using ColorStateList = Android.Content.Res.ColorStateList;

namespace Microsoft.Maui.Controls.Platform;
#pragma warning disable RS0016 // Add public types and members to the declared API
/// <summary>
/// Manages a BottomNavigationView with shared functionality for both TabbedPage and Shell.
/// This class extracts common bottom navigation patterns to enable code reuse.
/// </summary>
public class BottomNavigationManager
{
    private readonly IMauiContext _mauiContext;
    private readonly BottomNavigationView _bottomNavigationView;

    private ColorStateList? _originalItemTextColors;
    private ColorStateList? _originalItemIconTintColors;
    private ColorStateList? _currentItemTextColors;
    private ColorStateList? _currentItemIconTintColors;

    private readonly int[] _checkedStateSet = new int[] { global::Android.Resource.Attribute.StateChecked };
    private readonly int[] _emptyStateSet = Array.Empty<int>();

    /// <summary>
    /// Callback invoked when a tab item is selected.
    /// </summary>
    public Action<int>? OnTabSelected { get; set; }

    /// <summary>
    /// Callback invoked when the "More" button is clicked (for overflow scenarios).
    /// </summary>
    public Action? OnMoreClicked { get; set; }

    /// <summary>
    /// Gets the underlying BottomNavigationView.
    /// </summary>
    public BottomNavigationView BottomNavigationView => _bottomNavigationView;

    /// <summary>
    /// Gets the current dark theme state.
    /// </summary>
    public static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppInfo.RequestedTheme) == AppTheme.Dark;

    #region Factory Methods

    /// <summary>
    /// Creates a standard BottomNavigationView with common settings used by both Shell and TabbedPage.
    /// </summary>
    /// <param name="context">The Android context.</param>
    /// <param name="layoutParams">Optional layout parameters. If null, uses MatchParent width and WrapContent height.</param>
    /// <returns>A configured BottomNavigationView.</returns>
    public static BottomNavigationView CreateBottomNavigationView(
        Context context,
        ViewGroup.LayoutParams? layoutParams = null)
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

    /// <summary>
    /// Creates a standard ViewPager2 with common settings used by both Shell and TabbedPage.
    /// </summary>
    /// <param name="context">The Android context.</param>
    /// <param name="layoutParams">Optional layout parameters. If null, uses MatchParent for both width and height.</param>
    /// <returns>A configured ViewPager2.</returns>
    public static ViewPager2 CreateViewPager2(
        Context context,
        ViewGroup.LayoutParams? layoutParams = null)
    {
        return new ViewPager2(context)
        {
            OverScrollMode = OverScrollMode.Never,
            LayoutParameters = layoutParams ?? new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };
    }

    #endregion

    /// <summary>
    /// Creates a new BottomNavigationManager.
    /// </summary>
    /// <param name="mauiContext">The MAUI context.</param>
    /// <param name="bottomNavigationView">The BottomNavigationView to manage.</param>
    public BottomNavigationManager(
        IMauiContext mauiContext,
        BottomNavigationView bottomNavigationView)
    {
        _mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
        _bottomNavigationView = bottomNavigationView ?? throw new ArgumentNullException(nameof(bottomNavigationView));

        // Store original colors for restoration
        _originalItemTextColors = bottomNavigationView.ItemTextColor;
        _originalItemIconTintColors = bottomNavigationView.ItemIconTintList;
    }

    /// <summary>
    /// Sets up the bottom navigation with the provided tab items.
    /// </summary>
    /// <param name="items">The tab items to display.</param>
    /// <param name="selectedIndex">The initially selected index.</param>
    public void SetupTabs(IReadOnlyList<ITabItem> items, int selectedIndex = 0)
    {
        var menu = _bottomNavigationView.Menu;
        menu.Clear();

        if (items is null || items.Count == 0)
        {
            return;
        }

        // Hide if only one item
        if (items.Count == 1)
        {
            _bottomNavigationView.Visibility = ViewStates.Gone;
            return;
        }

        _bottomNavigationView.Visibility = ViewStates.Visible;

        // Setup listener
        _bottomNavigationView.SetOnItemSelectedListener(new ItemSelectedListener(this));

        // Add menu items
        int maxItems = _bottomNavigationView.MaxItemCount;
        bool showMore = items.Count > maxItems;
        int end = showMore ? maxItems - 1 : items.Count;

        for (int i = 0; i < end; i++)
        {
            var item = items[i];
            var title = !string.IsNullOrWhiteSpace(item.Title) ? item.Title : $"Tab {i + 1}";
            var menuItem = menu.Add(0, i, i, title);

            if (menuItem is null)
            {
                continue;
            }

            if (!item.IsEnabled)
            {
                menuItem.SetEnabled(false);
            }

            // Load icon asynchronously
            LoadMenuItemIconAsync(menuItem, item);
        }

        // Add "More" item if needed
        if (showMore)
        {
            var moreItem = menu.Add(0, BottomNavigationViewUtils.MoreTabId, maxItems - 1, "More");
            moreItem?.SetIcon(Resource.Drawable.abc_ic_menu_overflow_material);
        }

        // Set initial selection
        if (selectedIndex >= 0 && selectedIndex < items.Count)
        {
            int targetId = selectedIndex >= end ? BottomNavigationViewUtils.MoreTabId : selectedIndex;
            _bottomNavigationView.SelectedItemId = targetId;
        }

        // Apply shift mode fix
        _bottomNavigationView.SetShiftMode(false, false);
    }

    /// <summary>
    /// Updates a specific tab item's properties.
    /// </summary>
    /// <param name="index">The index of the tab to update.</param>
    /// <param name="item">The updated tab item data.</param>
    public void UpdateTab(int index, ITabItem item)
    {
        if (index < 0 || index >= _bottomNavigationView.Menu.Size())
        {
            return;
        }

        var menuItem = _bottomNavigationView.Menu.GetItem(index);
        if (menuItem is null)
        {
            return;
        }

        menuItem.SetTitle(item.Title ?? string.Empty);
        menuItem.SetEnabled(item.IsEnabled);
        LoadMenuItemIconAsync(menuItem, item);
    }

    /// <summary>
    /// Updates the selected item programmatically.
    /// </summary>
    /// <param name="index">The index to select.</param>
    public void SetSelectedItem(int index)
    {
        if (index >= 0 && _bottomNavigationView.SelectedItemId != index)
        {
            _bottomNavigationView.SelectedItemId = index;
        }
    }

    /// <summary>
    /// Updates the background color of the bottom navigation view.
    /// </summary>
    /// <param name="backgroundColor">The background color, or null to use default.</param>
    public void UpdateBackgroundColor(Color? backgroundColor)
    {
        if (backgroundColor is null)
        {
            _bottomNavigationView.SetBackground(null);
        }
        else
        {
            _bottomNavigationView.SetBackgroundColor(backgroundColor.ToPlatform());
        }
    }

    /// <summary>
    /// Updates the background with a brush.
    /// </summary>
    /// <param name="brush">The brush to apply as background.</param>
    public void UpdateBackground(Brush? brush)
    {
        _bottomNavigationView.UpdateBackground(brush);
    }

    /// <summary>
    /// Updates the item text and icon colors.
    /// </summary>
    /// <param name="unselectedColor">The color for unselected items.</param>
    /// <param name="selectedColor">The color for the selected item.</param>
    public void UpdateItemColors(Color? unselectedColor, Color? selectedColor)
    {
        _currentItemTextColors = null;
        _currentItemIconTintColors = null;

        if (unselectedColor is null && selectedColor is null)
        {
            // Restore original colors
            _bottomNavigationView.ItemTextColor = _originalItemTextColors;
            _bottomNavigationView.ItemIconTintList = _originalItemIconTintColors;
            return;
        }

        int unselectedArgb = unselectedColor?.ToPlatform().ToArgb() ?? GetDefaultUnselectedColor();
        int selectedArgb = selectedColor?.ToPlatform().ToArgb() ?? GetDefaultSelectedColor();

        var colorStateList = CreateColorStateList(unselectedArgb, selectedArgb);

        _currentItemTextColors = colorStateList;
        _currentItemIconTintColors = colorStateList;

        _bottomNavigationView.ItemTextColor = colorStateList;
        _bottomNavigationView.ItemIconTintList = colorStateList;
    }

    /// <summary>
    /// Clears the tab listener to prevent memory leaks.
    /// </summary>
    public void ClearListener()
    {
        _bottomNavigationView.SetOnItemSelectedListener(null);
    }

    /// <summary>
    /// Shows a "More" bottom sheet for overflow items.
    /// </summary>
    /// <param name="items">The list of all tab items (title, icon, enabled).</param>
    /// <param name="onItemSelected">Callback when an item is selected.</param>
    /// <param name="onDismissed">Optional callback when the sheet is dismissed.</param>
    public void ShowMoreSheet(
        IReadOnlyList<(string title, ImageSource icon, bool tabEnabled)> items,
        Action<int, BottomSheetDialog> onItemSelected,
        Action<BottomSheetDialog>? onDismissed = null)
    {
        var bottomSheetDialog = BottomNavigationViewUtils.CreateMoreBottomSheet(
            onItemSelected,
            _mauiContext,
            items as List<(string title, ImageSource icon, bool tabEnabled)> ?? new List<(string, ImageSource, bool)>(items),
            _bottomNavigationView.MaxItemCount);

        if (onDismissed is not null)
        {
            bottomSheetDialog.DismissEvent += (s, e) => onDismissed(bottomSheetDialog);
        }

        bottomSheetDialog.Show();
    }

    /// <summary>
    /// Sets the icon tint for a specific item at the given index.
    /// Uses the obsolete BottomNavigationItemView approach for per-item customization.
    /// </summary>
    /// <param name="index">The index of the item to update.</param>
    /// <param name="colors">The color state list to apply, or null to clear.</param>
    public void SetItemIconTint(int index, ColorStateList? colors)
    {
#pragma warning disable XAOBS001 // Type or member is obsolete
        if (_bottomNavigationView.GetChildAt(0) is BottomNavigationMenuView menuView)
        {
            if (index >= 0 && index < menuView.ChildCount)
            {
                var itemView = menuView.GetChildAt(index) as BottomNavigationItemView;
                itemView?.SetIconTintList(colors);
            }
        }
#pragma warning restore XAOBS001 // Type or member is obsolete
    }

    /// <summary>
    /// Marks an item as checked after sheet dismissal.
    /// </summary>
    /// <param name="index">The index to check.</param>
    public void SetItemChecked(int index)
    {
        var menu = _bottomNavigationView.Menu;
        int targetIndex = Math.Min(index, menu.Size() - 1);
        if (targetIndex < 0)
            return;

        var menuItem = menu.GetItem(targetIndex);
        menuItem?.SetChecked(true);
    }

    #region Public Helpers

    /// <summary>
    /// Gets the default color from the current theme for bottom navigation items.
    /// </summary>
    /// <param name="context">The Android context.</param>
    /// <returns>The default ARGB color value.</returns>
    public static int GetDefaultColorFromTheme(Context? context)
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

            // Fallback to hardcoded values
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

    /// <summary>
    /// Creates a ColorStateList for checked/unchecked states.
    /// </summary>
    /// <param name="unselectedColor">The ARGB color for unselected state.</param>
    /// <param name="selectedColor">The ARGB color for selected state.</param>
    /// <returns>A new ColorStateList.</returns>
    public static ColorStateList CreateCheckedColorStateList(int unselectedColor, int selectedColor)
    {
        int[][] states = new int[2][];
        int[] colors = new int[2];

        states[0] = new int[] { global::Android.Resource.Attribute.StateChecked };
        colors[0] = selectedColor;
        states[1] = Array.Empty<int>();
        colors[1] = unselectedColor;

#pragma warning disable RS0030 // Do not use banned APIs
        return new ColorStateList(states, colors);
#pragma warning restore RS0030
    }

    #endregion

    #region Private Helpers

    private async void LoadMenuItemIconAsync(IMenuItem menuItem, ITabItem item)
    {
        try
        {
            var drawable = await item.GetIconDrawableAsync(_mauiContext);
            if (drawable is not null && menuItem.IsAlive())
            {
                menuItem.SetIcon(drawable);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BottomNavigationManager: Failed to load icon: {ex.Message}");
        }
    }

    private ColorStateList CreateColorStateList(int unselectedColor, int selectedColor)
    {
        int[][] states = new int[2][];
        int[] colors = new int[2];

        states[0] = _checkedStateSet;
        colors[0] = selectedColor;
        states[1] = _emptyStateSet;
        colors[1] = unselectedColor;

#pragma warning disable RS0030 // Do not use banned APIs
        return new ColorStateList(states, colors);
#pragma warning restore RS0030
    }

    private int GetDefaultUnselectedColor()
    {
        return GetDefaultColorFromTheme(_mauiContext.Context);
    }

    private int GetDefaultSelectedColor()
    {
        return GetDefaultColorFromTheme(_mauiContext.Context);
    }

    #endregion

    #region Item Selected Listener

    private class ItemSelectedListener : Java.Lang.Object, Google.Android.Material.Navigation.NavigationBarView.IOnItemSelectedListener
    {
        private readonly BottomNavigationManager _manager;

        public ItemSelectedListener(BottomNavigationManager manager)
        {
            _manager = manager;
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            if (item.ItemId == BottomNavigationViewUtils.MoreTabId)
            {
                _manager.OnMoreClicked?.Invoke();
                return true;
            }

            _manager.OnTabSelected?.Invoke(item.ItemId);
            return true;
        }
    }

    #endregion
}
