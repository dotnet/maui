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
			IEnumerable<ToolbarItem> toolbarItems, 
			Context context, 
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged
			)
		{
			if (toolbarItems == null)
				return;

			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var item in toolbarItems)
			{
				item.PropertyChanged -= toolbarItemChanged;
				item.PropertyChanged += toolbarItemChanged;

				using (var title = new Java.Lang.String(item.Text))
				{
					var menuitem = menu.Add(global::Android.Views.Menu.None, 0, item.Priority, title);
					menuitem.SetEnabled(item.IsEnabled);
					menuitem.SetTitleOrContentDescription(item);
					UpdateMenuItemIcon(context, menuitem, item, tintColor);

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

					menuitem.Dispose();
				}
			}
		}

		static void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem, Color? tintColor)
		{
			_ = context.ApplyDrawableAsync(toolBarItem, ToolbarItem.IconImageSourceProperty, baseDrawable =>
			{
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
			IEnumerable<ToolbarItem> toolbarItems,
			Context context,
			Color? tintColor,
			PropertyChangedEventHandler toolbarItemChanged)
		{
			if (toolbarItems == null)
				return;

			if (e.IsOneOf(MenuItem.TextProperty, MenuItem.IconImageSourceProperty, MenuItem.IsEnabledProperty))
			{
				toolbar.UpdateMenuItems(toolbarItems, context, tintColor, toolbarItemChanged);
			}
		}
	}
}
