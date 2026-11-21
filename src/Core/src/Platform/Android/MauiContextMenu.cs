using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.Views;
using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContextMenu : Java.Lang.Object, AView.IOnCreateContextMenuListener, IMenuItemOnMenuItemClickListener
	{
		static readonly ConditionalWeakTable<AView, MauiContextMenu> s_menus = new();

		readonly WeakReference<IViewHandler> _handler;
		readonly bool _wasLongClickable;
		readonly Dictionary<int, IMenuElement> _items = new();
		readonly List<IMenuItem> _platformItems = new();

		MauiContextMenu(AView view, IViewHandler handler)
		{
			_handler = new(handler);
			_wasLongClickable = view.LongClickable;
		}

		internal static void Attach(AView view, IViewHandler handler)
		{
			if (s_menus.TryGetValue(view, out var existing))
			{
				existing._handler.SetTarget(handler);
				return;
			}

			var listener = new MauiContextMenu(view, handler);
			s_menus.Add(view, listener);
			view.SetOnCreateContextMenuListener(listener);
			view.LongClickable = true;
		}

		internal static void Remove(AView view)
		{
			if (!s_menus.TryGetValue(view, out var listener))
				return;

			view.SetOnCreateContextMenuListener(null);
			view.LongClickable = listener._wasLongClickable;
			s_menus.Remove(view);
			listener.Dispose();
		}

		public void OnCreateContextMenu(IContextMenu? menu, AView? view, IContextMenuContextMenuInfo? menuInfo)
		{
			ClearItems();

			if (menu is null ||
				!_handler.TryGetTarget(out var handler) ||
				handler.PlatformView != view ||
				handler.VirtualView is not IView { IsEnabled: true, InputTransparent: false } virtualView ||
				virtualView is not IContextFlyoutElement { ContextFlyout: IMenuFlyout flyout })
				return;

			PopulateMenu(menu, flyout);
		}

		void PopulateMenu(IMenu menu, IList<IMenuElement> elements)
		{
			MenuCompat.SetGroupDividerEnabled(menu, true);
			int group = 0;

			for (int i = 0; i < elements.Count; i++)
			{
				var element = elements[i];
				if (element is IMenuFlyoutSeparator)
				{
					group++;
					continue;
				}

				int id = AView.GenerateViewId();
				if (element is IMenuFlyoutSubItem subItem)
				{
					var subMenu = menu.AddSubMenu(group, id, i, element.Text)
						?? throw new InvalidOperationException("Unable to create an Android context submenu.");
					subMenu.Item?.SetEnabled(element.IsEnabled);
					PopulateMenu(subMenu, subItem);
				}
				else
				{
					var item = menu.Add(group, id, i, element.Text)
						?? throw new InvalidOperationException("Unable to create an Android context menu item.");
					item.SetEnabled(element.IsEnabled);
					item.SetOnMenuItemClickListener(this);
					_items.Add(id, element);
					_platformItems.Add(item);
				}
			}
		}

		public bool OnMenuItemClick(IMenuItem? item)
		{
			if (item is null ||
				!_handler.TryGetTarget(out var handler) ||
				handler.VirtualView is not IView { IsEnabled: true, InputTransparent: false } ||
				!_items.TryGetValue(item.ItemId, out var element) ||
				!element.IsEnabled)
				return false;

			element.Clicked();
			return true;
		}

		void ClearItems()
		{
			for (int i = 0; i < _platformItems.Count; i++)
			{
				if (_platformItems[i].IsAlive())
					_platformItems[i].SetOnMenuItemClickListener(null);
			}

			_platformItems.Clear();
			_items.Clear();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				ClearItems();

			base.Dispose(disposing);
		}
	}
}
