using System.ComponentModel;
using Android.Views;
#if __ANDROID_29__
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
#else
using AToolbar = Android.Support.V7.Widget.Toolbar;
#endif
using ATextView = global::Android.Widget.TextView;
using Android.Content;
using Android.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ToolbarExtensions
	{
		const int DefaultDisabledToolbarAlpha = 127;
		public static void DisposeMenuItems(this AToolbar toolbar, IEnumerable<ToolbarItem> toolbarItems, PropertyChangedEventHandler toolbarItemChanged)
		{
			if (toolbarItems == null)
				return;

			foreach (var item in toolbarItems)
				item.PropertyChanged -= toolbarItemChanged;
		}

		public static void UpdateMenuItems(this AToolbar toolbar,
			IEnumerable<ToolbarItem> sortedToolbarItems, 
			Context context, 
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> menuItemsCreated,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null
			)
		{
			if (sortedToolbarItems == null || menuItemsCreated == null)
				return;

			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var menuItem in menuItemsCreated)
				menuItem.Dispose();

			menuItemsCreated.Clear();

			int i = 0;
			foreach (var item in sortedToolbarItems)
			{
				UpdateMenuItem(toolbar, context, menuItemsCreated, item, tintColor, toolbarItemChanged, null, updateMenuItemIcon);
				i++;
			}
		}

		internal static void UpdateMenuItem(
			AToolbar toolbar,
			Context context, 
			List<IMenuItem> menuItemsCreated, 
			ToolbarItem item, 
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			int? menuItemIndex,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			IMenu menu = toolbar.Menu;
			item.PropertyChanged -= toolbarItemChanged;
			item.PropertyChanged += toolbarItemChanged;

			IMenuItem menuitem = null;

			if (menuItemIndex == null)
			{
				menuitem = menu.Add(new Java.Lang.String(item.Text));
				if (menuItemsCreated != null)
					menuItemsCreated.Add(menuitem);
			}
			else
			{
				if (menuItemsCreated == null || menuItemsCreated.Count < menuItemIndex.Value)
					return;

				menuitem = menuItemsCreated[menuItemIndex.Value];

				if (!menuitem.IsAlive())
					return;

				menuitem.SetTitle(new Java.Lang.String(item.Text));
			}

			menuitem.SetEnabled(item.IsEnabled);
			menuitem.SetTitleOrContentDescription(item);

			if (updateMenuItemIcon != null)
				updateMenuItemIcon(context, menuitem, item);
			else
				UpdateMenuItemIcon(context, menu, menuItemsCreated, menuitem, item, tintColor);

			if (item.Order != ToolbarItemOrder.Secondary)
				menuitem.SetShowAsAction(ShowAsAction.Always);

			menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(((IMenuItemController)item).Activate));

			if (tintColor != null && tintColor != Color.Default)
			{
				var view = toolbar.FindViewById(menuitem.ItemId);
				if (view is ATextView textView)
				{
					if (item.IsEnabled)
						textView.SetTextColor(tintColor.Value.ToAndroid());
					else
						textView.SetTextColor(tintColor.Value.MultiplyAlpha(0.302).ToAndroid());
				}
			}
		}

		internal static void UpdateMenuItemIcon(Context context, IMenu menu, List<IMenuItem> menuItemsCreated, IMenuItem menuItem, ToolbarItem toolBarItem, Color? tintColor)
		{
			_ = context.ApplyDrawableAsync(toolBarItem, ToolbarItem.IconImageSourceProperty, baseDrawable =>
			{
				if (menuItem == null || !menuItem.IsAlive())
				{
					return;
				}

				if (baseDrawable != null)
				{
					using (var constant = baseDrawable.GetConstantState())
					using (var newDrawable = constant.NewDrawable())
					using (var iconDrawable = newDrawable.Mutate())
					{
						if(tintColor != null)
							iconDrawable.SetColorFilter(tintColor.Value.ToAndroid(Color.White), FilterMode.SrcAtop);

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
			IEnumerable<ToolbarItem> toolbarItems,
			Context context,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> currentMenuItems,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			if (toolbarItems == null)
				return;

			if (e.IsOneOf(MenuItem.TextProperty, MenuItem.IconImageSourceProperty, MenuItem.IsEnabledProperty))
			{
				int index = 0;
				foreach (var item in toolbarItems)
				{
					if(item == toolbarItem)
					{
						break;
					}

					index++;
				}

				if (index >= currentMenuItems.Count)
					return;

				if (currentMenuItems[index].IsAlive())
					UpdateMenuItem(toolbar, context, currentMenuItems, toolbarItem, tintColor, toolbarItemChanged, index, updateMenuItemIcon);
				else
					UpdateMenuItems(toolbar, toolbarItems, context, tintColor, toolbarItemChanged, currentMenuItems, updateMenuItemIcon);
			}
		}
	}
}
