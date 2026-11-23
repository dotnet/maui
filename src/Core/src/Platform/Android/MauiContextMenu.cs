using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.Views;
using AndroidX.Core.View;
using AView = Android.Views.View;
using APopupMenu = AndroidX.AppCompat.Widget.PopupMenu;
using Looper = Android.OS.Looper;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContextMenu : Java.Lang.Object, IMenuItemOnMenuItemClickListener, APopupMenu.IOnDismissListener
	{
		static readonly ConditionalWeakTable<AView, MauiContextMenu> s_menus = new();

		readonly WeakReference<IViewHandler> _handler;
		readonly WeakReference<AView> _view;
		readonly IMenuFlyout _flyout;
		readonly Dictionary<int, IMenuElement> _items = new();
		readonly List<IMenuItem> _platformItems = new();
		APopupMenu? _popup;
		IDisposable? _unloaded;
		bool _closing;

		MauiContextMenu(AView view, IViewHandler handler, IMenuFlyout flyout, APopupMenu popup)
		{
			_handler = new(handler);
			_view = new(view);
			_flyout = flyout;
			_popup = popup;
		}

		internal static void Remove(AView view)
		{
			if (!s_menus.TryGetValue(view, out var listener))
				return;

			listener.Close();
		}

		internal static void Show(IViewHandler handler, IView view)
		{
			if (Looper.MyLooper() != Looper.MainLooper)
				throw new InvalidOperationException("Context flyouts must be presented on the UI thread.");

			if (handler.VirtualView != view || handler.PlatformView is not AView target ||
				!target.IsAlive() || !target.IsAttachedToWindow || target.WindowToken is null ||
				!target.IsShown || !target.Enabled || !view.IsEnabled ||
				view.InputTransparent || view.Visibility != Visibility.Visible)
				throw new InvalidOperationException("The context-flyout target must be loaded, visible, enabled, and accept input.");

			if (view is not IContextFlyoutElement { ContextFlyout: IMenuFlyout { Count: > 0 } flyout })
				throw new InvalidOperationException("A nonempty menu flyout must be attached to the target.");

			if (s_menus.TryGetValue(target, out _))
				return;

			var context = target.Context
				?? throw new InvalidOperationException("The native target must have an Android context.");
			var popup = new APopupMenu(context, target);
			var presenter = new MauiContextMenu(target, handler, flyout, popup);
			s_menus.Add(target, presenter);
			bool shown = false;
			try
			{
				presenter.PopulateMenu(popup.Menu, flyout);
				popup.SetOnDismissListener(presenter);
				presenter._unloaded = view.OnUnloaded(presenter.Close);
				popup.Show();
				shown = true;
			}
			finally
			{
				if (!shown)
					presenter.Close();
			}
		}

		public void OnDismiss(APopupMenu? menu) => Close();

		void Close()
		{
			if (_closing)
				return;

			_closing = true;
			_unloaded?.Dispose();
			_unloaded = null;
			if (_view.TryGetTarget(out var view))
				s_menus.Remove(view);
			var popup = _popup;
			_popup = null;
			if (popup is not null)
			{
				popup.SetOnDismissListener(null);
				popup.Dismiss();
				ClearItems();
				popup.Dispose();
			}
			Dispose();
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
			if (_closing || item is null ||
				!_handler.TryGetTarget(out var handler) ||
				!_view.TryGetTarget(out var view) || handler.PlatformView != view ||
				!view.IsAttachedToWindow || !view.IsShown ||
				handler.VirtualView is not IView { IsEnabled: true, InputTransparent: false, Visibility: Visibility.Visible } virtualView ||
				virtualView is not IContextFlyoutElement contextFlyoutElement || contextFlyoutElement.ContextFlyout != _flyout ||
				!_items.TryGetValue(item.ItemId, out var element) ||
				!element.IsEnabled || !ContainsEnabledItem(_flyout, element))
				return false;

			element.Clicked();
			return true;
		}

		static bool ContainsEnabledItem(IList<IMenuElement> elements, IMenuElement item)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				var element = elements[i];
				if (!element.IsEnabled)
					continue;
				if (ReferenceEquals(element, item) ||
					element is IMenuFlyoutSubItem submenu && ContainsEnabledItem(submenu, item))
					return true;
			}
			return false;
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
