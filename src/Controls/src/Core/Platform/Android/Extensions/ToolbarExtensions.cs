using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using AGraphics = Android.Graphics;
using ATextView = global::Android.Widget.TextView;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = global::Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		static ColorStateList? _defaultTitleTextColor;
		static int? _defaultNavigationIconColor;

		public static void UpdateIsVisible(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			_ = nativeToolbar.Context ?? throw new ArgumentNullException(nameof(nativeToolbar.Context));

			bool showNavBar = toolbar.IsVisible;
			var lp = nativeToolbar.LayoutParameters;
			if (lp == null)
				return;

			if (!showNavBar)
			{
				lp.Height = 0;
			}
			else
			{
				if (toolbar.BarHeight != null)
					lp.Height = (int)nativeToolbar.Context.ToPixels(toolbar.BarHeight.Value);
				else
					lp.Height = nativeToolbar.Context?.GetActionBarHeight() ?? 0;
			}

			nativeToolbar.LayoutParameters = lp;
		}

		public static void UpdateTitleIcon(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			_ = nativeToolbar.Context ?? throw new ArgumentNullException(nameof(nativeToolbar.Context));
			_ = toolbar?.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			ImageSource source = toolbar.TitleIcon;

			if (source == null || source.IsEmpty)
			{
				if (nativeToolbar.GetChildAt(0) is ToolbarTitleIconImageView existingImageView)
					nativeToolbar.RemoveView(existingImageView);

				return;
			}

			var iconView = new ToolbarTitleIconImageView(nativeToolbar.Context);
			nativeToolbar.AddView(iconView, 0);
			iconView.SetImageResource(global::Android.Resource.Color.Transparent);

			source.LoadImage(toolbar.Handler.MauiContext, (result) =>
			{
				iconView.SetImageDrawable(result?.Value);
				AutomationPropertiesProvider.AccessibilitySettingsChanged(iconView, source);
			});
		}

		public static void UpdateBackButton(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			if (toolbar.BackButtonVisible)
			{
				var context =
					nativeToolbar.Context?.GetThemedContext() ??
					nativeToolbar.Context ??
					toolbar.Handler?.MauiContext?.Context;

				nativeToolbar.NavigationIcon ??= new DrawerArrowDrawable(context!);
				if (nativeToolbar.NavigationIcon is DrawerArrowDrawable iconDrawable)
					iconDrawable.Progress = 1;

				var backButtonTitle = toolbar.BackButtonTitle;
				ImageSource image = toolbar.TitleIcon;

				if (!string.IsNullOrEmpty(backButtonTitle))
				{
					nativeToolbar.NavigationContentDescription = backButtonTitle;
				}
				else if (image == null ||
					nativeToolbar.SetNavigationContentDescription(image) == null)
				{
					nativeToolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_navigate_up_description);
				}
			}
			else
			{
				if (!toolbar.DrawerToggleVisible)
				{
					nativeToolbar.NavigationIcon = null;
				}
				else
				{
					if (nativeToolbar.NavigationIcon is DrawerArrowDrawable iconDrawable)
						iconDrawable.Progress = 0;

					nativeToolbar.SetNavigationContentDescription(Resource.String.nav_app_bar_open_drawer_description);
				}
			}

			nativeToolbar.UpdateIconColor(toolbar);
			nativeToolbar.UpdateBarTextColor(toolbar);
		}

		public static void UpdateBarBackground(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			Brush barBackground = toolbar.BarBackground;

			if (barBackground is SolidColorBrush solidColor)
			{
				var tintColor = solidColor.Color;
				if (tintColor == null)
				{
					nativeToolbar.BackgroundTintMode = null;
				}
				else
				{
					nativeToolbar.BackgroundTintMode = PorterDuff.Mode.Src;
					nativeToolbar.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToPlatform());
				}
			}
			else
			{
				nativeToolbar.UpdateBackground(barBackground);

				if (Brush.IsNullOrEmpty(barBackground))
					nativeToolbar.BackgroundTintMode = null;
			}
		}

		public static void UpdateIconColor(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			var navIconColor = toolbar.IconColor;
			if (navIconColor is null)
				return;

			var platformColor = navIconColor.ToPlatform();
			if (nativeToolbar.NavigationIcon is Drawable navigationIcon)
			{
				if (navigationIcon is DrawerArrowDrawable dad)
					dad.Color = AGraphics.Color.White;

				navigationIcon.SetColorFilter(platformColor, FilterMode.SrcAtop);
			}

			if (nativeToolbar.OverflowIcon is Drawable overflowIcon)
			{
				overflowIcon.SetColorFilter(platformColor, FilterMode.SrcAtop);
			}
		}

		public static void UpdateBarTextColor(this AToolbar nativeToolbar, Toolbar toolbar)
		{
			var textColor = toolbar.BarTextColor;

			// Because we use the same toolbar across multiple navigation pages (think tabbed page with nested NavigationPage)
			// We need to reset the toolbar text color to the default color when it's unset
			if (_defaultTitleTextColor == null)
			{
				var context = nativeToolbar.Context?.GetThemedContext();
				_defaultTitleTextColor = PlatformInterop.GetColorStateListForToolbarStyleableAttribute(context,
					Resource.Attribute.toolbarStyle, Resource.Styleable.Toolbar_titleTextColor);
			}

			if (textColor != null)
			{
				nativeToolbar.SetTitleTextColor(textColor.ToPlatform().ToArgb());
			}
			else if (_defaultTitleTextColor != null)
			{
				nativeToolbar.SetTitleTextColor(_defaultTitleTextColor);
			}

			if (nativeToolbar.NavigationIcon is DrawerArrowDrawable icon)
			{
				if (textColor != null)
				{
					_defaultNavigationIconColor = icon.Color;
					icon.Color = textColor.ToPlatform().ToArgb();
				}
				else if (_defaultNavigationIconColor != null)
				{
					icon.Color = _defaultNavigationIconColor.Value;
				}
			}
		}

		class ToolbarTitleIconImageView : AppCompatImageView
		{
			public ToolbarTitleIconImageView(Context context) : base(context)
			{
			}
		}

		const int DefaultDisabledToolbarAlpha = 127;
		public static void DisposeMenuItems(this AToolbar? toolbar, IEnumerable<ToolbarItem> toolbarItems, PropertyChangedEventHandler toolbarItemChanged)
		{
			if (toolbarItems == null)
				return;

			foreach (var item in toolbarItems)
				item.PropertyChanged -= toolbarItemChanged;
		}

		public static void UpdateMenuItems(this AToolbar toolbar,
			IEnumerable<ToolbarItem> sortedToolbarItems,
			IMauiContext mauiContext,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> previousMenuItems,
			List<ToolbarItem> previousToolBarItems,
			Action<Context, IMenuItem, ToolbarItem>? updateMenuItemIcon = null)
		{
			if (sortedToolbarItems == null || previousMenuItems == null)
				return;

			var context = mauiContext.Context;
			var menu = toolbar.Menu;

			foreach (var toolbarItem in previousToolBarItems)
				toolbarItem.PropertyChanged -= toolbarItemChanged;

			int i = 0;
			foreach (var item in sortedToolbarItems)
			{
				UpdateMenuItem(toolbar, item, i, mauiContext, tintColor, toolbarItemChanged, previousMenuItems, previousToolBarItems, updateMenuItemIcon);
				i++;
			}

			int toolBarItemCount = i;
			while (toolBarItemCount < previousMenuItems.Count)
			{
				if (menu != null)
				{
					menu.RemoveItem(previousMenuItems[toolBarItemCount].ItemId);
				}
				previousMenuItems[toolBarItemCount].Dispose();
				previousMenuItems.RemoveAt(toolBarItemCount);
			}

			previousToolBarItems.Clear();
			previousToolBarItems.AddRange(sortedToolbarItems);
		}

		static void UpdateMenuItem(AToolbar toolbar,
			ToolbarItem item,
			int? menuItemIndex,
			IMauiContext mauiContext,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> previousMenuItems,
			List<ToolbarItem> previousToolBarItems,
			Action<Context, IMenuItem, ToolbarItem>? updateMenuItemIcon = null)
		{
			var context = mauiContext?.Context ??
					throw new ArgumentNullException($"{nameof(mauiContext.Context)}");

			IMenu? menu = toolbar.Menu;

			item.PropertyChanged -= toolbarItemChanged;
			item.PropertyChanged += toolbarItemChanged;

			IMenuItem menuitem;

			Java.Lang.ICharSequence? newTitle = null;

			if (!String.IsNullOrWhiteSpace(item.Text))
			{
				if (item.Order != ToolbarItemOrder.Secondary && tintColor != null && tintColor != null)
				{
					var color = item.IsEnabled ? tintColor.ToPlatform() : tintColor.MultiplyAlpha(0.302f).ToPlatform();
					SpannableString titleTinted = new SpannableString(item.Text);
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
					titleTinted.SetSpan(new ForegroundColorSpan(color), 0, titleTinted.Length(), 0);
#pragma warning restore CA1416
					newTitle = titleTinted;
				}
				else
				{
					newTitle = new Java.Lang.String(item.Text);
				}
			}
			else
			{
				newTitle = new Java.Lang.String();
			}

			if (menuItemIndex == null || menuItemIndex >= previousMenuItems?.Count)
			{
				menuitem = menu?.Add(0, AView.GenerateViewId(), 0, newTitle) ??
					throw new InvalidOperationException($"Failed to create menuitem: {newTitle}");
				previousMenuItems?.Add(menuitem);
			}
			else
			{
				if (previousMenuItems == null || previousMenuItems.Count < menuItemIndex.Value)
					return;

				menuitem = previousMenuItems[menuItemIndex.Value];

				if (!menuitem.IsAlive())
					return;

				menuitem.SetTitle(newTitle);
			}

			menuitem.SetEnabled(item.IsEnabled);
			menuitem.SetTitleOrContentDescription(item);

			if (updateMenuItemIcon != null)
				updateMenuItemIcon(context, menuitem, item);
			else
				UpdateMenuItemIcon(mauiContext, menuitem, item, tintColor);

			if (item.Order != ToolbarItemOrder.Secondary)
				menuitem.SetShowAsAction(ShowAsAction.Always);
			else
				menuitem.SetShowAsAction(ShowAsAction.Never);

			menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(((IMenuItemController)item).Activate));

			if (item.Order != ToolbarItemOrder.Secondary && !OperatingSystem.IsAndroidVersionAtLeast(26) && (tintColor != null && tintColor != null))
			{
				var view = toolbar.FindViewById(menuitem.ItemId);
				if (view is ATextView textView)
				{
					if (item.IsEnabled)
						textView.SetTextColor(tintColor.ToPlatform());
					else
						textView.SetTextColor(tintColor.MultiplyAlpha(0.302f).ToPlatform());
				}
			}
		}

		internal static void UpdateMenuItemIcon(this IMauiContext mauiContext, IMenuItem menuItem, ToolbarItem toolBarItem, Color? tintColor)
		{
			toolBarItem.IconImageSource.LoadImage(mauiContext, result =>
			{
				var baseDrawable = result?.Value;
				if (menuItem == null || !menuItem.IsAlive())
				{
					return;
				}

				if (baseDrawable != null)
				{
					using (var constant = baseDrawable.GetConstantState())
					using (var newDrawable = constant!.NewDrawable())
					using (var iconDrawable = newDrawable.Mutate())
					{
						if (tintColor != null)
							iconDrawable.SetColorFilter(tintColor.ToPlatform(Colors.White), FilterMode.SrcAtop);

						if (!menuItem.IsEnabled)
						{
							iconDrawable.Mutate().SetAlpha(DefaultDisabledToolbarAlpha);
						}

						menuItem.SetIcon(iconDrawable);
					}
				}
			});
		}

		public static void OnToolbarItemPropertyChanged(
			this AToolbar toolbar,
			PropertyChangedEventArgs e,
			ToolbarItem toolbarItem,
			ICollection<ToolbarItem> toolbarItems,
			IMauiContext mauiContext,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> currentMenuItems,
			List<ToolbarItem> currentToolbarItems,
			Action<Context, IMenuItem, ToolbarItem>? updateMenuItemIcon = null)
		{
			if (toolbarItems == null)
				return;

			if (!e.IsOneOf(MenuItem.TextProperty, MenuItem.IconImageSourceProperty, MenuItem.IsEnabledProperty))
				return;
			var context = mauiContext.Context;
			int index = 0;

			foreach (var item in toolbarItems)
			{
				if (item == toolbarItem)
				{
					break;
				}

				index++;
			}

			if (index >= currentMenuItems.Count)
				return;

			if (currentMenuItems[index].IsAlive())
				UpdateMenuItem(toolbar, toolbarItem, index, mauiContext, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
			else
				UpdateMenuItems(toolbar, toolbarItems, mauiContext, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
		}
	}
}
