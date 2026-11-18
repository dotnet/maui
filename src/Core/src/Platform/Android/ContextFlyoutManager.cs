using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AMenu = Android.Views.IMenu;
using AMenuItem = Android.Views.IMenuItem;
using AView = Android.Views.View;
using PopupMenu = AndroidX.AppCompat.Widget.PopupMenu;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Shows the <see cref="IContextFlyoutElement.ContextFlyout"/> of a view as a native popup menu.
	/// </summary>
	/// <remarks>
	/// The manager never changes how the view handles touches: long presses and secondary clicks are
	/// observed before the window dispatches them (see <see cref="ContextFlyoutTouchTracker"/>), so taps,
	/// selection, scrolling and accessibility focus behave exactly as they do without a context flyout.
	/// Only when the view isn't hosted by a <see cref="MauiAppCompatActivity"/> (for example when embedding)
	/// is a native long-click listener used instead.
	/// </remarks>
	internal sealed class ContextFlyoutManager
	{
		const int IconLoadTimeoutMilliseconds = 300;
		const int IconSizeDp = 24;

		static readonly List<WeakReference<ContextFlyoutManager>> s_managers = new();
		static readonly ConditionalWeakTable<AView, ContextFlyoutManager> s_managersByView = new();

		// Views whose long press is handled by MAUI (gesture recognizers, CollectionView reordering)
		static readonly ConditionalWeakTable<AView, object> s_longPressHandlers = new();

		readonly WeakReference<AView> _view;
		WeakReference<IViewHandler> _handler;
		readonly bool _usesLongClickFallback;
		readonly bool _wasLongClickable;
		LongClickListener? _longClickListener;
		AccessibilityDelegateCompatWrapper? _accessibilityDelegate;

		PopupMenu? _popup;
		Dictionary<int, IMenuElement>? _leafItems;
		List<ISubMenu>? _subMenus;
		List<IDisposable>? _imageResults;
		int _menuVersion;

		ContextFlyoutManager(IViewHandler handler, AView view)
		{
			_view = new WeakReference<AView>(view);
			_handler = new WeakReference<IViewHandler>(handler);

			// Accessibility services can't long press: expose the menu through the standard long-click action.
			// Without a delegate AndroidX would install a plain one, which stops MAUI from adding its own
			// (e.g. for tap gestures), so start from MAUI's wrapper that other delegates can wrap.
			if (ViewCompat.GetAccessibilityDelegate(view) is null)
			{
				_accessibilityDelegate = new AccessibilityDelegateCompatWrapper(null);
				ViewCompat.SetAccessibilityDelegate(view, _accessibilityDelegate);
			}

			ViewCompat.ReplaceAccessibilityAction(view, AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionLongClick!, (string?)null, new AccessibilityCommand(this));

			if (!(UseTouchTrackerOverride ?? view.Context?.GetActivity() is MauiAppCompatActivity))
			{
				_usesLongClickFallback = true;
				_wasLongClickable = view.LongClickable;

				// A view that already handles long press (e.g. text selection) keeps doing so
				if (!_wasLongClickable)
				{
					_longClickListener = new LongClickListener(this);
					view.SetOnLongClickListener(_longClickListener);
				}
			}
		}

		internal static bool HasAny => s_managers.Count > 0;

		// Lets device tests, whose activity isn't a MauiAppCompatActivity, choose the touch mechanism
		internal static bool? UseTouchTrackerOverride { get; set; }

		internal bool UsesLongClickFallback => _usesLongClickFallback;

		internal PopupMenu? Popup => _popup;

		internal static ContextFlyoutManager? Get(AView view) =>
			s_managersByView.TryGetValue(view, out var manager) ? manager : null;

		internal static void Attach(IViewHandler handler, AView view)
		{
			if (s_managersByView.TryGetValue(view, out var existing))
			{
				existing._handler = new WeakReference<IViewHandler>(handler);
				return;
			}

			var manager = new ContextFlyoutManager(handler, view);
			s_managersByView.Add(view, manager);
			s_managers.Add(new WeakReference<ContextFlyoutManager>(manager));
		}

		internal static void Detach(AView view)
		{
			if (!s_managersByView.TryGetValue(view, out var manager))
				return;

			s_managersByView.Remove(view);

			for (int i = s_managers.Count - 1; i >= 0; i--)
			{
				if (!s_managers[i].TryGetTarget(out var m) || m == manager)
					s_managers.RemoveAt(i);
			}

			manager.Dismiss();

			if (!view.IsAlive())
				return;

			ViewCompat.RemoveAccessibilityAction(view, AccessibilityNodeInfoCompat.ActionLongClick);

			if (manager._accessibilityDelegate is not null && ViewCompat.GetAccessibilityDelegate(view) == manager._accessibilityDelegate)
				ViewCompat.SetAccessibilityDelegate(view, null);

			if (manager._longClickListener is not null)
			{
				// SetOnLongClickListener makes the view long-clickable, so restore the original value afterwards
				view.SetOnLongClickListener(null);
				view.LongClickable = manager._wasLongClickable;
				manager._longClickListener?.Dispose();
				manager._longClickListener = null;
			}
		}

		/// <summary>
		/// Finds the view whose context flyout should open for a touch at the given window coordinates.
		/// </summary>
		internal static ContextFlyoutManager? FindTarget(AView root, float x, float y)
		{
			ContextFlyoutManager? best = null;
			AView? bestView = null;

			for (int i = s_managers.Count - 1; i >= 0; i--)
			{
				if (!s_managers[i].TryGetTarget(out var manager) || !manager._view.TryGetTarget(out var view) || !view.IsAlive())
				{
					s_managers.RemoveAt(i);
					continue;
				}

				if (manager._usesLongClickFallback || !view.IsAttachedToWindow || !view.IsShown || !view.Enabled || view.RootView != root || !manager.CanShow())
					continue;

				if (!TryMapToLocal(root, view, x, y, out _, out _))
					continue;

				// Something interactive drawn on top of the view receives this touch instead
				if (IsOccluded(root, view, x, y))
					continue;

				// The innermost view wins, like routed context requests on other platforms; otherwise the
				// one drawn on top, which is the first view Android offers the touch to
				if (bestView is null || IsDescendantOf(view, bestView) || (!IsDescendantOf(bestView, view) && IsDrawnAbove(view, bestView)))
				{
					best = manager;
					bestView = view;
				}
			}

			if (best is null || bestView is null)
				return null;

			// A descendant that handles long presses itself (e.g. text selection) takes precedence
			if (bestView is ViewGroup group && HasLongClickableDescendantAt(root, group, x, y))
				return null;

			return best;
		}

		/// <summary>
		/// Opens the context flyout of the focused view, or of the closest ancestor that has one.
		/// </summary>
		internal static bool TryShowForFocusedView(AView? focused)
		{
			if (!HasAny)
				return false;

			for (var current = focused; current is not null; current = current.Parent as AView)
			{
				if (Get(current) is { } manager && manager.Show())
					return true;
			}

			return false;
		}

		// Shift+F10 opens on key down, like Windows. The menu key isn't used: it opens the options menu.
		internal static bool IsContextMenuKeyDown(Keycode keyCode, KeyEvent? e) =>
			keyCode == Keycode.F10 && e is not null && e.HasModifiers(MetaKeyStates.ShiftOn);

		/// <summary>
		/// Marks a view whose long press is handled by MAUI, so that a context flyout doesn't take it over.
		/// </summary>
		internal static void SetHandlesLongPress(AView view, bool handlesLongPress)
		{
			if (handlesLongPress)
				s_longPressHandlers.AddOrUpdate(view, s_longPressHandlers);
			else
				s_longPressHandlers.Remove(view);
		}

		/// <summary>
		/// Whether a long press at the given point already does something else, which then keeps priority over
		/// the context flyout: a long-press or drag gesture, text selection, a tooltip, reordering, etc. on the
		/// view, an ancestor, or the view under the finger. Secondary clicks and the keyboard still open the menu.
		/// </summary>
		internal bool DefersLongPress(AView root, float x, float y)
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive())
				return true;

			for (AView? current = view; current is not null; current = current.Parent as AView)
			{
				if (HandlesLongPress(current, current == view && _longClickListener is not null))
					return true;

				if (current == root)
					break;
			}

			return view is ViewGroup group && HasLongPressHandlerAt(root, group, x, y);
		}

		// Used for the accessibility long-click action, which has no position
		bool DefersLongPress()
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive())
				return true;

			for (AView? current = view; current is not null; current = current.Parent as AView)
			{
				if (HandlesLongPress(current, current == view && _longClickListener is not null))
					return true;
			}

			return false;
		}

		// The fallback long-click listener makes the view long-clickable itself, which doesn't count
		static bool HandlesLongPress(AView view, bool ignoreLongClickable = false) =>
			(!ignoreLongClickable && view.Enabled && view.LongClickable) ||
			s_longPressHandlers.TryGetValue(view, out _) ||
			(OperatingSystem.IsAndroidVersionAtLeast(26) && view.TooltipTextFormatted is not null);

		static bool HasLongPressHandlerAt(AView root, ViewGroup group, float x, float y)
		{
			for (int i = group.ChildCount - 1; i >= 0; i--)
			{
				if (group.GetChildAt(i) is not AView child || child.Visibility != ViewStates.Visible || !TryMapToLocal(root, child, x, y, out _, out _))
					continue;

				if (HandlesLongPress(child) || (child is ViewGroup childGroup && HasLongPressHandlerAt(root, childGroup, x, y)))
					return true;
			}

			return false;
		}

		internal void PerformHapticFeedback()
		{
			if (_view.TryGetTarget(out var view) && view.IsAlive())
				view.PerformHapticFeedback(FeedbackConstants.LongPress);
		}

		/// <summary>
		/// Builds the menu from the current state of the flyout and shows it.
		/// </summary>
		/// <returns><see langword="true"/> when a menu is (about to be) shown.</returns>
		internal bool Show()
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive() || !view.IsAttachedToWindow || !view.IsShown || view.Context is not Context context)
				return false;

			if (!_handler.TryGetTarget(out var handler) || handler.MauiContext is not IMauiContext mauiContext ||
				!TryGetFlyout(handler, out var flyout))
				return false;

			Dismiss();

			var version = ++_menuVersion;
			var popup = new PopupMenu(context, view);
			var leafItems = new Dictionary<int, IMenuElement>();
			var subMenus = new List<ISubMenu>();
			var iconLoads = new List<Task>();
			int nextItemId = 1;
			int nextGroupId = 1;

			AddItems(popup.Menu, flyout);

			if (iconLoads.Count > 0)
				popup.SetForceShowIcon(true);

			// No DismissEvent cleanup: on small screens the root popup is dismissed to show a sub menu,
			// and the sub menu's items are still routed through this popup. State is released when the
			// next menu is shown or the flyout is detached.
			popup.MenuItemClick += OnMenuItemClick;

			_popup = popup;
			_leafItems = leafItems;
			_subMenus = subMenus;

			// The popup measures its width once, so icons are loaded before it's shown
			if (iconLoads.Count == 0)
				ShowPopup(popup, version);
			else
				_ = ShowWhenIconsLoadedAsync(popup, version, iconLoads);

			return true;

			void AddItems(AMenu menu, IList<IMenuElement> items)
			{
				var groupId = nextGroupId++;
				List<AMenuItem>? itemsWithoutIcon = null;
				var hasIcons = false;

				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];

					// Android draws a divider where the group changes
					if (item is IMenuFlyoutSeparator)
					{
						groupId = nextGroupId++;
						continue;
					}

					var itemId = nextItemId++;
					var title = item.Text ?? string.Empty;
					AMenuItem? menuItem;

					if (item is IMenuFlyoutSubItem subItem)
					{
						var subMenu = menu.AddSubMenu(groupId, itemId, AMenu.None, title);
						if (subMenu is null)
							continue;

						subMenus.Add(subMenu);
						AddItems(subMenu, subItem);
						menuItem = subMenu.Item;
					}
					else
					{
						menuItem = menu.Add(groupId, itemId, AMenu.None, title);
						leafItems[itemId] = item;
					}

					if (menuItem is null)
						continue;

					menuItem.SetEnabled(item.IsEnabled);

					if (item.Source is IImageSource { IsEmpty: false } source)
					{
						hasIcons = true;
						iconLoads.Add(LoadIconAsync(menuItem, source, version, context, mauiContext));
					}
					else
					{
						(itemsWithoutIcon ??= new List<AMenuItem>()).Add(menuItem);
					}
				}

				// Keep the text of all items aligned when only some of them have an icon
				if (hasIcons && itemsWithoutIcon is not null)
				{
					foreach (var menuItem in itemsWithoutIcon)
						menuItem.SetIcon(CreateIconPlaceholder(context));
				}

				MenuCompat.SetGroupDividerEnabled(menu, true);
			}
		}

		bool CanShow() =>
			_handler.TryGetTarget(out var handler) && TryGetFlyout(handler, out _);

		// Disabled or input transparent views don't receive input, so they don't open a context menu either
		static bool TryGetFlyout(IViewHandler handler, [NotNullWhen(true)] out IMenuFlyout? flyout)
		{
			flyout = null;

			if (handler.VirtualView is not IView { IsEnabled: true, InputTransparent: false } virtualView ||
				virtualView is not IContextFlyoutElement { ContextFlyout: IMenuFlyout menuFlyout } || !HasItems(menuFlyout))
				return false;

			flyout = menuFlyout;
			return true;
		}

		internal void Dismiss()
		{
			var popup = _popup;
			if (popup is null)
				return;

			_menuVersion++;

			// On small screens an open sub menu is a separate popup that replaced the root one
			if (_subMenus is not null)
			{
				foreach (var subMenu in _subMenus)
				{
					if (subMenu.IsAlive())
						subMenu.Close();
				}
			}

			Cleanup(popup);

			if (popup.IsAlive())
				popup.Dismiss();
		}

		static bool HasItems(IMenuFlyout flyout)
		{
			for (int i = 0; i < flyout.Count; i++)
			{
				if (flyout[i] is not IMenuFlyoutSeparator)
					return true;
			}

			return false;
		}

		void ShowPopup(PopupMenu popup, int version)
		{
			if (version != _menuVersion || _popup != popup)
				return;

			if (!_view.TryGetTarget(out var view) || !view.IsAlive() || !view.IsAttachedToWindow || !view.IsShown)
			{
				Dismiss();
				return;
			}

			popup.Show();
		}

		async Task ShowWhenIconsLoadedAsync(PopupMenu popup, int version, List<Task> iconLoads)
		{
			try
			{
				await Task.WhenAny(Task.WhenAll(iconLoads), Task.Delay(IconLoadTimeoutMilliseconds));
			}
			catch
			{
				// An icon that fails to load is left out; the menu is still shown
			}

			ShowPopup(popup, version);
		}

		async Task LoadIconAsync(AMenuItem menuItem, IImageSource source, int version, Context context, IMauiContext mauiContext)
		{
			IImageSourceServiceResult<Drawable>? result;

			try
			{
				result = await source.GetPlatformImageAsync(mauiContext);
			}
			catch
			{
				return;
			}

			if (result is null)
				return;

			if (version != _menuVersion || !menuItem.IsAlive())
			{
				result.Dispose();
				return;
			}

			(_imageResults ??= new List<IDisposable>()).Add(result);

			if (result.Value is Drawable drawable)
				menuItem.SetIcon(ToMenuIcon(drawable, context));
		}

		static GradientDrawable CreateIconPlaceholder(Context context)
		{
			var size = (int)context.ToPixels(IconSizeDp);
			var placeholder = new GradientDrawable();
			placeholder.SetColor(global::Android.Graphics.Color.Transparent);
			placeholder.SetSize(size, size);
			return placeholder;
		}

		// Menu icons are 24dp; larger images (e.g. a full-size png) are scaled down to fit
		static Drawable ToMenuIcon(Drawable drawable, Context context)
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(23))
				return drawable;

			var maxSize = (int)context.ToPixels(IconSizeDp);
			var width = drawable.IntrinsicWidth;
			var height = drawable.IntrinsicHeight;

			if (width > 0 && height > 0 && width <= maxSize && height <= maxSize)
				return drawable;

			if (width <= 0 || height <= 0)
			{
				width = maxSize;
				height = maxSize;
			}

			var scale = Math.Min((double)maxSize / width, (double)maxSize / height);
			var layer = new LayerDrawable(new[] { drawable });
			layer.SetLayerSize(0, Math.Max(1, (int)(width * scale)), Math.Max(1, (int)(height * scale)));
			return layer;
		}

		void OnMenuItemClick(object? sender, PopupMenu.MenuItemClickEventArgs e)
		{
			if (e.Item is not AMenuItem item || _leafItems is null || !_leafItems.TryGetValue(item.ItemId, out var element))
			{
				// Sub menu headers open their sub menu
				e.Handled = false;
				return;
			}

			e.Handled = true;

			// MenuItem.Clicked is raised even when disabled, so check the current state first
			if (element.IsEnabled)
				element.Clicked();
		}

		void Cleanup(PopupMenu popup)
		{
			popup.MenuItemClick -= OnMenuItemClick;

			if (_popup == popup)
			{
				_popup = null;
				_leafItems = null;
				_subMenus = null;
			}

			if (_imageResults is not null)
			{
				foreach (var result in _imageResults)
					result.Dispose();

				_imageResults = null;
			}
		}

		internal static bool TryMapToLocal(AView root, AView view, float x, float y, out float localX, out float localY)
		{
			localX = x;
			localY = y;

			var chain = new List<AView>();
			AView? current = view;
			while (current is not null && current != root)
			{
				chain.Add(current);
				current = current.Parent as AView;
			}

			if (current != root || x < 0 || y < 0 || x >= root.Width || y >= root.Height)
				return false;

			// Mirror how ViewGroup dispatches touches: the point must be inside every view along the way
			AView parent = root;
			for (int i = chain.Count - 1; i >= 0; i--)
			{
				var child = chain[i];
				localX += parent.ScrollX - child.Left;
				localY += parent.ScrollY - child.Top;

				var matrix = child.Matrix;
				if (matrix is not null && !matrix.IsIdentity)
				{
					using var inverse = new global::Android.Graphics.Matrix();
					if (!matrix.Invert(inverse))
						return false;

					var point = new[] { localX, localY };
					inverse.MapPoints(point);
					localX = point[0];
					localY = point[1];
				}

				if (child.Visibility != ViewStates.Visible || localX < 0 || localY < 0 || localX >= child.Width || localY >= child.Height)
					return false;

				// Input transparent wrappers don't pass touches on to their content
				if (child is WrapperView { InputTransparent: true })
					return false;

				parent = child;
			}

			return true;
		}

		static bool IsDescendantOf(AView view, AView ancestor)
		{
			for (var parent = view.Parent; parent is not null; parent = parent.Parent)
			{
				if (parent == ancestor)
					return true;
			}

			return false;
		}

		static bool IsDrawnAbove(AView view, AView other)
		{
			// Compare the two branches below the closest common ancestor, like ViewGroup orders touch targets
			for (AView child = view; child.Parent is ViewGroup parent; child = parent)
			{
				for (AView otherChild = other; otherChild.Parent is ViewGroup otherParent; otherChild = otherParent)
				{
					if (otherParent != parent)
						continue;

					var z = ViewCompat.GetZ(child);
					var otherZ = ViewCompat.GetZ(otherChild);
					return z != otherZ ? z > otherZ : parent.IndexOfChild(child) > parent.IndexOfChild(otherChild);
				}
			}

			return false;
		}

		static bool IsOccluded(AView root, AView target, float x, float y)
		{
			for (AView child = target; child != root && child.Parent is ViewGroup parent; child = parent)
			{
				var index = parent.IndexOfChild(child);
				var z = ViewCompat.GetZ(child);

				for (int i = 0; i < parent.ChildCount; i++)
				{
					if (i == index || parent.GetChildAt(i) is not AView sibling || sibling.Visibility != ViewStates.Visible)
						continue;

					// Siblings drawn above this branch: higher Z, or same Z and added later
					var siblingZ = ViewCompat.GetZ(sibling);
					if (siblingZ < z || (siblingZ == z && i < index))
						continue;

					if (HandlesTouchAt(root, sibling, x, y, longPressOnly: false))
						return true;
				}
			}

			return false;
		}

		static bool HasLongClickableDescendantAt(AView root, ViewGroup group, float x, float y)
		{
			for (int i = group.ChildCount - 1; i >= 0; i--)
			{
				if (group.GetChildAt(i) is AView child && HandlesTouchAt(root, child, x, y, longPressOnly: true))
					return true;
			}

			return false;
		}

		static bool HandlesTouchAt(AView root, AView view, float x, float y, bool longPressOnly)
		{
			if (view.Visibility != ViewStates.Visible || !TryMapToLocal(root, view, x, y, out _, out _))
				return false;

			// Disabled views don't long press, but a disabled clickable view still consumes the touch
			if ((view.Enabled && view.LongClickable) || (!longPressOnly && view.Clickable))
				return true;

			if (view is ViewGroup group)
			{
				for (int i = group.ChildCount - 1; i >= 0; i--)
				{
					if (group.GetChildAt(i) is AView child && HandlesTouchAt(root, child, x, y, longPressOnly))
						return true;
				}
			}

			return false;
		}

		sealed class LongClickListener : Java.Lang.Object, AView.IOnLongClickListener
		{
			readonly WeakReference<ContextFlyoutManager> _manager;

			public LongClickListener(ContextFlyoutManager manager) =>
				_manager = new WeakReference<ContextFlyoutManager>(manager);

			public bool OnLongClick(AView? v) =>
				_manager.TryGetTarget(out var manager) && !manager.DefersLongPress() && manager.Show();
		}

		sealed class AccessibilityCommand : Java.Lang.Object, IAccessibilityViewCommand
		{
			readonly WeakReference<ContextFlyoutManager> _manager;

			public AccessibilityCommand(ContextFlyoutManager manager) =>
				_manager = new WeakReference<ContextFlyoutManager>(manager);

			// Returning false lets the view's own long-click behavior run, as it does without a context flyout
			public bool Perform(AView? view, AccessibilityViewCommandCommandArguments? arguments) =>
				_manager.TryGetTarget(out var manager) && !manager.DefersLongPress() && manager.Show();
		}
	}
}
