#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ALabelVisibilityMode = Google.Android.Material.BottomNavigation.LabelVisibilityMode;
using ColorStateList = Android.Content.Res.ColorStateList;
using IMenu = Android.Views.IMenu;
using LP = Android.Views.ViewGroup.LayoutParams;
using Orientation = Android.Widget.Orientation;
using Typeface = Android.Graphics.Typeface;
using TypefaceStyle = Android.Graphics.TypefaceStyle;

namespace Microsoft.Maui.Controls.Platform
{
	public static class BottomNavigationViewUtils
	{
		internal const int MoreTabId = 99;

		public static Drawable CreateItemBackgroundDrawable()
		{
			var stateList = ColorStateList.ValueOf(Colors.Black.MultiplyAlpha(0.2f).ToPlatform());
			var colorDrawable = new ColorDrawable(AColor.White);
			return new RippleDrawable(stateList, colorDrawable, null);
		}

		internal static void UpdateEnabled(bool tabEnabled, IMenuItem menuItem)
		{
			if (menuItem.IsEnabled != tabEnabled)
				menuItem.SetEnabled(tabEnabled);
		}

		internal static Task SetupMenuItem(
			(string title, ImageSource icon, bool tabEnabled) item,
			IMenu menu,
			int index,
			int currentIndex,
			BottomNavigationView bottomView,
			IMauiContext mauiContext,
			out IMenuItem menuItem)
		{
			Task returnValue;
			using var title = new Java.Lang.String(item.title);
			menuItem = menu.Add(0, index, 0, title);
			returnValue = SetMenuItemIcon(menuItem, item.icon, mauiContext);
			UpdateEnabled(item.tabEnabled, menuItem);
			if (index == currentIndex)
			{
				menuItem.SetChecked(true);
				bottomView.SelectedItemId = index;
			}

			return returnValue;
		}

		internal static async void SetupMenu(
			IMenu menu,
			int maxBottomItems,
			List<(string title, ImageSource icon, bool tabEnabled)> items,
			int currentIndex,
			BottomNavigationView bottomView,
			IMauiContext mauiContext)
		{
			Context context = mauiContext.Context;

			while (items.Count < menu.Size())
			{
				menu.RemoveItem(menu.GetItem(menu.Size() - 1).ItemId);
			}

			int numberOfMenuItems = items.Count;
			bool showMore = numberOfMenuItems > maxBottomItems;
			int end = showMore ? maxBottomItems - 1 : numberOfMenuItems;

			List<IMenuItem> menuItems = new List<IMenuItem>();
			List<Task> loadTasks = new List<Task>();
			for (int i = 0; i < end; i++)
			{
				var item = items[i];

				IMenuItem menuItem;
				if (i >= menu.Size())
					loadTasks.Add(SetupMenuItem(item, menu, i, currentIndex, bottomView, mauiContext, out menuItem));
				else
				{
					menuItem = menu.GetItem(i);
					if (menuItem.ItemId != i)
					{
						menu.RemoveItem(menuItem.ItemId);
						loadTasks.Add(SetupMenuItem(item, menu, i, currentIndex, bottomView, mauiContext, out menuItem));
					}
					else
					{
						SetMenuItemTitle(menuItem, item.title);
						loadTasks.Add(SetMenuItemIcon(menuItem, item.icon, mauiContext));
					}
				}

				menuItems.Add(menuItem);
			}

			var menuSize = menu.Size();
			if (showMore && menu.GetItem(menuSize - 1).ItemId != MoreTabId)
			{
				var moreString = context.Resources.GetText(Resource.String.overflow_tab_title);
				if (menuSize == maxBottomItems)
					menu.RemoveItem(menu.GetItem(menuSize - 1).ItemId);
				var menuItem = menu.Add(0, MoreTabId, 0, moreString);
				menuItems.Add(menuItem);

				menuItem.SetIcon(Resource.Drawable.abc_ic_menu_overflow_material);
				if (currentIndex >= maxBottomItems - 1)
					menuItem.SetChecked(true);
			}

			bottomView.SetShiftMode(false, false);

			if (loadTasks.Count > 0)
				await Task.WhenAll(loadTasks);
		}

		internal static void SetMenuItemTitle(IMenuItem menuItem, string title)
		{
			using var jTitle = new Java.Lang.String(title);
			menuItem.SetTitle(jTitle);
		}

		internal static async Task SetMenuItemIcon(IMenuItem menuItem, ImageSource source, IMauiContext context)
		{
			if (!menuItem.IsAlive())
				return;

			if (source is null)
				return;

			var services = context.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(source);

			var result = await imageSourceService.GetDrawableAsync(
				source,
				context.Context);

			if (menuItem.IsAlive())
			{
				menuItem.SetIcon(result?.Value);
			}
		}

		public static BottomSheetDialog CreateMoreBottomSheet(
			Action<int, BottomSheetDialog> selectCallback,
			IMauiContext mauiContext,
			List<(string title, ImageSource icon, bool tabEnabled)> items)
		{
			return CreateMoreBottomSheet(selectCallback, mauiContext, items, 5);
		}

		internal static BottomSheetDialog CreateMoreBottomSheet(
			Action<int, BottomSheetDialog> selectCallback,
			IMauiContext mauiContext,
			List<(string title, ImageSource icon, bool tabEnabled)> items,
			int maxItemCount)
		{
			var context = mauiContext.Context;
			var bottomSheetDialog = new BottomSheetDialog(context);
			var bottomSheetLayout = new LinearLayout(context);
			using (var bottomShellLP = new LP(LP.MatchParent, LP.WrapContent))
				bottomSheetLayout.LayoutParameters = bottomShellLP;
			bottomSheetLayout.Orientation = Orientation.Vertical;

			// handle the more tab
			for (int i = maxItemCount - 1; i < items.Count; i++)
			{
				var i_local = i;
				var shellContent = items[i];

				using (var innerLayout = new LinearLayout(context))
				{
					innerLayout.ClipToOutline = true;
					innerLayout.SetBackground(CreateItemBackgroundDrawable());
					innerLayout.SetPadding(0, (int)context.ToPixels(6), 0, (int)context.ToPixels(6));
					innerLayout.Orientation = Orientation.Horizontal;
					using (var param = new LP(LP.MatchParent, LP.WrapContent))
						innerLayout.LayoutParameters = param;

					// technically the unhook isn't needed
					// we dont even unhook the events that dont fire
					void clickCallback(object s, EventArgs e)
					{
						selectCallback(i_local, bottomSheetDialog);
						if (!innerLayout.IsDisposed())
							innerLayout.Click -= clickCallback;
					}
					innerLayout.Click += clickCallback;

					var image = new ImageView(context);
					var lp = new LinearLayout.LayoutParams((int)context.ToPixels(32), (int)context.ToPixels(32))
					{
						LeftMargin = (int)context.ToPixels(20),
						RightMargin = (int)context.ToPixels(20),
						TopMargin = (int)context.ToPixels(6),
						BottomMargin = (int)context.ToPixels(6),
						Gravity = GravityFlags.Center
					};
					image.LayoutParameters = lp;
					lp.Dispose();

					image.ImageTintList = ColorStateList.ValueOf(Colors.Black.MultiplyAlpha(0.6f).ToPlatform());

					shellContent.icon.LoadImage(mauiContext, result =>
					{
						image.SetImageDrawable(result?.Value);
					});

					innerLayout.AddView(image);

					using (var text = new TextView(context))
					{
						text.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal), TypefaceStyle.Normal);
						text.SetTextColor(AColor.Black);
						text.Text = shellContent.title;
						lp = new LinearLayout.LayoutParams(0, LP.WrapContent)
						{
							Gravity = GravityFlags.Center,
							Weight = 1
						};
						text.LayoutParameters = lp;
						lp.Dispose();

						innerLayout.AddView(text);
					}

					bottomSheetLayout.AddView(innerLayout);
				}
			}

			bottomSheetDialog.SetContentView(bottomSheetLayout);
			bottomSheetLayout.Dispose();

			return bottomSheetDialog;
		}


		public static void SetShiftMode(this BottomNavigationView bottomNavigationView, bool enableShiftMode, bool enableItemShiftMode)
		{
			try
			{
#pragma warning disable XAOBS001 // Obsolete
				var menuView = bottomNavigationView.GetChildAt(0) as BottomNavigationMenuView;
				if (menuView is null)
				{
					System.Diagnostics.Debug.WriteLine("Unable to find BottomNavigationMenuView");
					return;
				}
#pragma warning restore XAOBS001 // Obsolete

#if __ANDROID_28__
				if (enableShiftMode)
					bottomNavigationView.LabelVisibilityMode = ALabelVisibilityMode.LabelVisibilityAuto;
				else
					bottomNavigationView.LabelVisibilityMode = ALabelVisibilityMode.LabelVisibilityLabeled;
#else
				var shiftMode = menuView.Class.GetDeclaredField("mShiftingMode");
				shiftMode.Accessible = true;
				shiftMode.SetBoolean(menuView, enableShiftMode);
				shiftMode.Accessible = false;
				shiftMode.Dispose();
#endif
				for (int i = 0; i < menuView.ChildCount; i++)
				{
					var child = menuView.GetChildAt(i);
#pragma warning disable XAOBS001 // Obsolete
					var item = child as BottomNavigationItemView;
#pragma warning restore XAOBS001 // Obsolete
					if (item != null)
					{
#if __ANDROID_28__
						item.SetShifting(enableItemShiftMode);
#else
						item.SetShiftingMode(enableItemShiftMode);
#endif
						item.SetChecked(item.ItemData.IsChecked);
					}
				}
				menuView.UpdateMenuView();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Unable to set shift mode: {ex}");
			}
		}
	}
}
