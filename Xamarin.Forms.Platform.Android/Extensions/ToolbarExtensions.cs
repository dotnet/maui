using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using ATextView = global::Android.Widget.TextView;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

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
			List<ToolbarItem> toolbarItemsCreated,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			if (sortedToolbarItems == null || menuItemsCreated == null)
				return;

			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var menuItem in menuItemsCreated)
				menuItem.Dispose();

			foreach (var toolbarItem in toolbarItemsCreated)
				toolbarItem.PropertyChanged -= toolbarItemChanged;

			menuItemsCreated.Clear();
			toolbarItemsCreated.Clear();

			foreach (var item in sortedToolbarItems)
			{
				UpdateMenuItem(toolbar, item, null, context, tintColor, toolbarItemChanged, menuItemsCreated, toolbarItemsCreated, updateMenuItemIcon);
			}
		}

		internal static void UpdateMenuItem(AToolbar toolbar,
			ToolbarItem item,
			int? menuItemIndex,
			Context context,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> menuItemsCreated,
			List<ToolbarItem> toolbarItemsCreated,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			IMenu menu = toolbar.Menu;
			item.PropertyChanged -= toolbarItemChanged;
			item.PropertyChanged += toolbarItemChanged;

			IMenuItem menuitem;

			Java.Lang.ICharSequence newTitle = null;

			if (!String.IsNullOrWhiteSpace(item.Text))
			{
				if (item.Order != ToolbarItemOrder.Secondary && tintColor != null && tintColor != Color.Default)
				{
					var color = item.IsEnabled ? tintColor.Value.ToAndroid() : tintColor.Value.MultiplyAlpha(0.302).ToAndroid();
					SpannableString titleTinted = new SpannableString(item.Text);
					titleTinted.SetSpan(new ForegroundColorSpan(color), 0, titleTinted.Length(), 0);
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

			if (menuItemIndex == null)
			{
				menuitem = menu.Add(0, Platform.GenerateViewId(), 0, newTitle);
				menuItemsCreated?.Add(menuitem);
				toolbarItemsCreated?.Add(item);
			}
			else
			{
				if (menuItemsCreated == null || menuItemsCreated.Count < menuItemIndex.Value)
					return;

				menuitem = menuItemsCreated[menuItemIndex.Value];

				if (!menuitem.IsAlive())
					return;

				menuitem.SetTitle(newTitle);
			}

			menuitem.SetEnabled(item.IsEnabled);
			menuitem.SetTitleOrContentDescription(item);

			if (updateMenuItemIcon != null)
				updateMenuItemIcon(context, menuitem, item);
			else
				UpdateMenuItemIcon(context, menuitem, item, tintColor);

			if (item.Order != ToolbarItemOrder.Secondary)
				menuitem.SetShowAsAction(ShowAsAction.Always);

			menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(((IMenuItemController)item).Activate));

			if (item.Order != ToolbarItemOrder.Secondary && !Forms.IsOreoOrNewer && (tintColor != null && tintColor != Color.Default))
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

		internal static void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem, Color? tintColor)
		{
			_ = context.ApplyDrawableAsync(toolBarItem, MenuItem.IconImageSourceProperty, baseDrawable =>
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
						if (tintColor != null)
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
			ICollection<ToolbarItem> toolbarItems,
			Context context,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged,
			List<IMenuItem> currentMenuItems,
			List<ToolbarItem> currentToolbarItems,
			Action<Context, IMenuItem, ToolbarItem> updateMenuItemIcon = null)
		{
			if (toolbarItems == null)
				return;

			if (!e.IsOneOf(MenuItem.TextProperty, MenuItem.IconImageSourceProperty, MenuItem.IsEnabledProperty))
				return;

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
				UpdateMenuItem(toolbar, toolbarItem, index, context, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
			else
				UpdateMenuItems(toolbar, toolbarItems, context, tintColor, toolbarItemChanged, currentMenuItems, currentToolbarItems, updateMenuItemIcon);
		}
	}
}