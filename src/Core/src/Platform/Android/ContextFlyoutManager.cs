using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AMenu = Android.Views.IMenu;
using AMenuItem = Android.Views.IMenuItem;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Shows the <see cref="IContextFlyoutElement.ContextFlyout"/> of a view as Android's own context menu.
	/// </summary>
	/// <remarks>
	/// The manager never changes how the view handles touches: long presses and secondary clicks are
	/// observed before the window dispatches them (see <see cref="ContextFlyoutTouchTracker"/>), so taps,
	/// selection, scrolling and accessibility focus behave exactly as they do without a context flyout.
	/// Only when the view isn't hosted by a <see cref="MauiAppCompatActivity"/> (for example when embedding)
	/// is a native long-click listener used instead.
	/// The menu is the window's native context menu, like <see cref="AView.ShowContextMenu()"/> opens: it's built by
	/// the view and its ancestors, and the flyout items are added by a listener on the window's root view that is
	/// only there while the menu is built (see <see cref="ShowNativeContextMenu"/>).
	/// </remarks>
	internal sealed class ContextFlyoutManager
	{
		// Above the ids menus use for their own items and groups (like text menus), so the flyout items get their own
		internal const int FirstMenuId = 1000;

		static readonly List<WeakReference<ContextFlyoutManager>> s_managers = new();
		static readonly ConditionalWeakTable<AView, ContextFlyoutManager> s_managersByView = new();

		// How MAUI's gesture recognizers handle touches on a view
		static readonly ConditionalWeakTable<AView, GestureState> s_gestureStates = new();

		// Views that take long presses over from their children, like a reorderable CollectionView
		static readonly ConditionalWeakTable<AView, object> s_longPressInterceptors = new();

		// A window reuses its context menu, so it belongs to whichever flyout showed it last
		static readonly ConditionalWeakTable<IContextMenu, WeakReference<ContextFlyoutManager>> s_menuOwners = new();

		readonly WeakReference<AView> _view;
		WeakReference<IViewHandler> _handler;
		readonly bool _usesLongClickFallback;
		readonly bool _wasLongClickable;
		LongClickListener? _longClickListener;
		AccessibilityDelegateCompatWrapper? _accessibilityDelegate;

		IContextMenu? _menu;
		List<ISubMenu>? _subMenus;
		(float X, float Y)? _menuPosition;

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

		/// <summary>The context menu this flyout opened, until it's dismissed or another one opens in its window.</summary>
		internal IContextMenu? Menu => IsShowing(this, _menu) ? _menu : null;

		/// <summary>Where the menu was asked to open, relative to the view; <see langword="null"/> when Android chose (a dialog, or a native long click).</summary>
		internal (float X, float Y)? MenuPosition => _menuPosition;

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

			// The view (or one of its children) may already have focus
			ContextFlyoutKeyTracker.Refresh(view);
			ContextFlyoutTextMenus.Prepare(view);
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

			return best;
		}

		/// <summary>
		/// Finds the context flyout that holding a confirm key (Enter, D-pad center, ...) opens for the focused
		/// view, following native long clicks: the focused view must be clickable (or have a context menu, which
		/// natively makes it long-clickable, or gesture recognizers that take its input), its own long-click handler
		/// comes first, and otherwise the menu of the view or of the closest ancestor with one opens.
		/// </summary>
		internal static ContextFlyoutManager? FindForFocusedView(AView? focused)
		{
			// Text inputs use the confirm keys themselves (new line, IME action); pickers and the like don't
			if (!HasAny || focused is null || focused is EditText { KeyListener: not null } || ContextFlyoutTextMenus.IsTextView(focused) ||
				!focused.Enabled || !(ReceivesTouches(focused) || Get(focused) is not null))
				return null;

			if (HasLongPressHandler(focused, Get(focused)))
				return null;

			for (var current = focused; current is not null; current = current.Parent as AView)
			{
				if (Get(current) is { } manager)
					return manager.CanShow() ? manager : null;
			}

			return null;
		}

		/// <summary>
		/// Shows the menu where a key long press on the given view opens it: at the view's center, like native.
		/// </summary>
		internal bool ShowForKey(AView focused)
		{
			var location = new int[2];
			focused.GetLocationInWindow(location);
			return Show(location[0] + focused.Width / 2f, location[1] + focused.Height / 2f);
		}

		/// <summary>
		/// Tells context flyouts how MAUI's gesture recognizers handle touches on a view.
		/// </summary>
		/// <param name="view">The platform view the gesture recognizers are attached to.</param>
		/// <param name="consumesTouches">Whether the view receives touches, so its children's views don't get them.</param>
		/// <param name="handlesLongPress">Whether the view does something on a long press (long-press or drag gestures).</param>
		/// <param name="cancelLongPress">Cancels a pending long press when a context menu takes the press over.</param>
		internal static void SetGestureState(AView view, bool consumesTouches, bool handlesLongPress, Action<MotionEvent>? cancelLongPress)
		{
			if (consumesTouches || handlesLongPress)
				s_gestureStates.AddOrUpdate(view, new GestureState(consumesTouches, handlesLongPress, cancelLongPress));
			else
				s_gestureStates.Remove(view);
		}

		/// <summary>
		/// Marks a view that takes long presses over from its children, like a reorderable CollectionView.
		/// </summary>
		internal static void SetInterceptsLongPress(AView view, bool interceptsLongPress)
		{
			if (interceptsLongPress)
				s_longPressInterceptors.AddOrUpdate(view, s_longPressInterceptors);
			else
				s_longPressInterceptors.Remove(view);
		}

		/// <summary>
		/// Whether a long press at the given point does something else instead of opening the context flyout.
		/// </summary>
		/// <remarks>
		/// This follows how Android handles long presses: they go to the innermost view under the finger that
		/// receives touches. If that view handles long presses itself (a long-press or drag gesture, or a
		/// long-click listener), that wins. Otherwise the context menu of that view or of its closest ancestor
		/// with one opens, which comes before built-in behavior like text selection and tooltips.
		/// A reorderable CollectionView keeps reordering its items on long press.
		/// </remarks>
		internal bool DefersLongPress(AView root, float x, float y)
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive())
				return true;

			var target = FindTouchTarget(view, root, x, y);

			// Reordering a collection the press is in, whether the menu is on an item, the collection or around it
			for (var current = target; current is not null; current = current.Parent as AView)
			{
				if (s_longPressInterceptors.TryGetValue(current, out _))
					return true;
			}

			// A disabled view still receives the touch, but nothing happens on a long press
			if (target != view && !target.Enabled)
				return true;

			// Text keeps selecting text on long press; its toolbars show the flyout items instead
			if (ContextFlyoutTextMenus.IsText(target))
			{
				ContextFlyoutTextMenus.Prepare(target);
				return true;
			}

			return HasLongPressHandler(target, Get(target));
		}

		/// <summary>
		/// The view that receives a touch at the given point in the view with this flyout.
		/// </summary>
		internal AView? FindTouchTarget(AView root, float x, float y) =>
			_view.TryGetTarget(out var view) && view.IsAlive() ? FindTouchTarget(view, root, x, y) : null;

		static AView FindTouchTarget(AView view, AView root, float x, float y) =>
			view is ViewGroup group ? FindTouchTargetAt(root, group, x, y) ?? view : view;

		// Used for the accessibility long-click action and the long-click fallback, which act on the view itself.
		// Text keeps its own long click (selecting text); its menus show the flyout items instead.
		bool DefersLongPress() =>
			!_view.TryGetTarget(out var view) || !view.IsAlive() || HasLongPressHandler(view, this) || ContextFlyoutTextMenus.IsText(view);

		// Android opens the context menu right after the long-click listener, so the items are added until then
		void PrepareNativeLongClick()
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive() || !_handler.TryGetTarget(out var handler) ||
				!TryGetFlyout(handler, out _) || NativeMenuScope.Create(view, CollectFlyouts(view)) is not NativeMenuScope scope)
				return;

			var posted = view.Post(() =>
			{
				SetMenu(scope.Menu, scope.SubMenus, null);
				scope.Dispose();
			});

			if (!posted)
				scope.Dispose();
		}

		// The innermost view under the point that receives touches, like ViewGroup dispatches them
		static AView? FindTouchTargetAt(AView root, ViewGroup group, float x, float y)
		{
			foreach (var child in GetChildrenInTouchOrder(group))
			{
				if (child.Visibility != ViewStates.Visible || !TryMapToLocal(root, child, x, y, out _, out _))
					continue;

				if (child is ViewGroup childGroup && FindTouchTargetAt(root, childGroup, x, y) is AView target)
					return target;

				if (ReceivesTouches(child))
					return child;
			}

			return null;
		}

		// Like ViewGroup.buildOrderedChildList: higher Z first, then the ones added later
		static List<AView> GetChildrenInTouchOrder(ViewGroup group)
		{
			var children = new List<AView>(group.ChildCount);
			var hasZ = false;
			for (int i = group.ChildCount - 1; i >= 0; i--)
			{
				if (group.GetChildAt(i) is AView child)
				{
					children.Add(child);
					hasZ |= ViewCompat.GetZ(child) != 0;
				}
			}

			if (hasZ)
			{
				// Stable, so children with the same Z keep the later ones first
				var ordered = new List<AView>(children.Count);
				foreach (var child in children)
				{
					var z = ViewCompat.GetZ(child);
					var index = ordered.FindIndex(c => ViewCompat.GetZ(c) < z);
					ordered.Insert(index < 0 ? ordered.Count : index, child);
				}

				return ordered;
			}

			return children;
		}

		// A context flyout doesn't make its view take touches: one that can't show (disabled, no items) lets them through
		static bool ReceivesTouches(AView view) =>
			view.Clickable || view.LongClickable ||
			(s_gestureStates.TryGetValue(view, out var state) && state.ConsumesTouches);

		static bool HasLongPressHandler(AView view, ContextFlyoutManager? manager)
		{
			if (s_gestureStates.TryGetValue(view, out var state) && state.HandlesLongPress)
				return true;

			// The long-click fallback of a context flyout is its own listener
			if (manager?._longClickListener is not null)
				return false;

			if (OperatingSystem.IsAndroidVersionAtLeast(30))
				return view.HasOnLongClickListeners;

			// Before Android 11 listeners can't be queried: text inputs, selectable text and tooltips are
			// long-clickable by themselves
			return view.Enabled && view.LongClickable && view is not EditText && view is not TextView { IsTextSelectable: true } &&
				!(OperatingSystem.IsAndroidVersionAtLeast(26) && view.TooltipTextFormatted is not null);
		}

		sealed record GestureState(bool ConsumesTouches, bool HandlesLongPress, Action<MotionEvent>? CancelLongPress);

		/// <summary>
		/// Cancels the pending long presses of gesture recognizers on the views under the point.
		/// </summary>
		internal static void CancelLongPresses(AView root, float x, float y, MotionEvent cancel)
		{
			if (root is ViewGroup group)
				CancelLongPresses(root, group, x, y, cancel);
		}

		static void CancelLongPresses(AView root, ViewGroup group, float x, float y, MotionEvent cancel)
		{
			for (int i = group.ChildCount - 1; i >= 0; i--)
			{
				if (group.GetChildAt(i) is not AView child || !TryMapToLocal(root, child, x, y, out _, out _))
					continue;

				if (s_gestureStates.TryGetValue(child, out var state))
					state.CancelLongPress?.Invoke(cancel);

				if (child is ViewGroup childGroup)
					CancelLongPresses(root, childGroup, x, y, cancel);
			}
		}

		internal void PerformHapticFeedback()
		{
			if (_view.TryGetTarget(out var view) && view.IsAlive())
				view.PerformHapticFeedback(FeedbackConstants.LongPress);
		}

		/// <summary>
		/// Builds the menu from the current state of the flyout and opens it, as Android's own context menu.
		/// </summary>
		/// <param name="windowX">Where the menu opens, in window coordinates, like native context menus that open
		/// where the view was pressed. Without a position the menu opens as a dialog, like native context menus that
		/// aren't opened by a touch (for example by an accessibility service).</param>
		/// <param name="windowY">See <paramref name="windowX"/>.</param>
		/// <returns><see langword="true"/> when the request to open a menu was handled.</returns>
		internal bool Show(float? windowX = null, float? windowY = null)
		{
			if (!_view.TryGetTarget(out var view) || !view.IsAlive() || !view.IsAttachedToWindow || !view.IsShown)
				return false;

			if (!_handler.TryGetTarget(out var handler) || !TryGetFlyout(handler, out _))
				return false;

			(float X, float Y)? position = null;
			if (windowX is float x && windowY is float y && view.RootView is AView root)
			{
				if (!TryMapToLocal(root, view, x, y, out var localX, out var localY))
				{
					var location = new int[2];
					view.GetLocationInWindow(location);
					localX = x - location[0];
					localY = y - location[1];
				}

				position = (localX, localY);
			}

			// Like native context menus, the menus of the view's ancestors are added after its own
			if (!ShowNativeContextMenuCore(view, position, CollectFlyouts(view), out var menu, out var subMenus))
				return false;

			SetMenu(menu, subMenus, position);
			return true;
		}

		void SetMenu(IContextMenu? menu, List<ISubMenu>? subMenus, (float X, float Y)? position)
		{
			_menu = menu;
			_subMenus = menu is null ? null : subMenus;
			_menuPosition = menu is null ? null : position;

			if (menu is not null)
				s_menuOwners.AddOrUpdate(menu, new WeakReference<ContextFlyoutManager>(this));
		}

		static bool IsShowing(ContextFlyoutManager manager, IContextMenu? menu) =>
			menu is not null && menu.IsAlive() && s_menuOwners.TryGetValue(menu, out var owner) &&
			owner.TryGetTarget(out var current) && current == manager;

		/// <summary>
		/// Opens the window's native context menu for a view, like <see cref="AView.ShowContextMenu()"/> does, with the
		/// items of the given flyouts.
		/// </summary>
		/// <remarks>
		/// Native context menus are built by the view and then its ancestors, up to the window's root view, so a
		/// listener there adds the items after the ones of the view and its ancestors (like the actions of text).
		/// It's only there while the menu is built. The root is the window's own decor view: apps don't register
		/// context menus on it, as that would make the whole window long-clickable, so no listener of the app is
		/// replaced.
		/// </remarks>
		/// <param name="view">The view the menu is for.</param>
		/// <param name="position">Where the menu opens, relative to the view. Without one it opens as a dialog.</param>
		/// <param name="flyouts">The flyouts whose items are added.</param>
		/// <param name="menu">The menu, when the flyout items were added to it.</param>
		/// <returns><see langword="true"/> when the request was handled: a menu opened, or an ancestor handled it
		/// instead (like a ListView, which runs its long-click listener first).</returns>
		internal static bool ShowNativeContextMenu(AView view, (float X, float Y)? position, IReadOnlyList<IMenuFlyout> flyouts, out IContextMenu? menu) =>
			ShowNativeContextMenuCore(view, position, flyouts, out menu, out _);

		static bool ShowNativeContextMenuCore(AView view, (float X, float Y)? position, IReadOnlyList<IMenuFlyout> flyouts, out IContextMenu? menu, out List<ISubMenu>? subMenus)
		{
			menu = null;
			subMenus = null;

			if (flyouts.Count == 0 || NativeMenuScope.Create(view, flyouts) is not NativeMenuScope scope)
				return false;

			using (scope)
			{
				var shown = position is (float x, float y) && OperatingSystem.IsAndroidVersionAtLeast(24)
					? view.ShowContextMenu(x, y)
					: view.ShowContextMenu();

				menu = shown ? scope.Menu : null;
				subMenus = menu is null ? null : scope.SubMenus;
				return shown;
			}
		}

		/// <summary>
		/// Adds the flyout items to the context menus the window builds while the scope is open.
		/// </summary>
		sealed class NativeMenuScope : IDisposable
		{
			readonly AView _root;
			readonly bool _wasLongClickable;
			ContextMenuListener? _listener;

			NativeMenuScope(AView root, ContextMenuListener listener)
			{
				_root = root;
				_wasLongClickable = root.LongClickable;
				_listener = listener;
				root.SetOnCreateContextMenuListener(listener);
			}

			public IContextMenu? Menu => _listener?.Menu;

			public List<ISubMenu>? SubMenus => _listener?.SubMenus;

			public static NativeMenuScope? Create(AView view, IReadOnlyList<IMenuFlyout> flyouts) =>
				view.RootView is AView root ? new NativeMenuScope(root, new ContextMenuListener(flyouts)) : null;

			public void Dispose()
			{
				if (_listener is null)
					return;

				// Setting a listener makes the view long-clickable
				if (_root.IsAlive())
				{
					_root.SetOnCreateContextMenuListener(null);
					_root.LongClickable = _wasLongClickable;
				}

				_listener.Dispose();
				_listener = null;
			}
		}

		sealed class ContextMenuListener : Java.Lang.Object, AView.IOnCreateContextMenuListener
		{
			readonly IReadOnlyList<IMenuFlyout> _flyouts;

			public ContextMenuListener(IReadOnlyList<IMenuFlyout> flyouts) =>
				_flyouts = flyouts;

			public IContextMenu? Menu { get; private set; }

			// On phones an open sub menu is a popup of its own, which closing the menu doesn't close
			public List<ISubMenu> SubMenus { get; } = new();

			public void OnCreateContextMenu(IContextMenu? menu, AView? v, IContextMenuContextMenuInfo? menuInfo)
			{
				if (menu is null)
					return;

				var writer = new MenuWriter((menuItem, element) => menuItem.SetOnMenuItemClickListener(new ItemClickListener(element)), SubMenus)
				{
					FirstGroupId = FirstMenuId,
					FirstItemId = FirstMenuId,
				};

				writer.Write(menu, _flyouts);
				MenuCompat.SetGroupDividerEnabled(menu, true);
				Menu = menu;
			}
		}

		sealed class ItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
		{
			// The window keeps its last context menu, which mustn't keep the flyout alive
			readonly WeakReference<IMenuElement> _element;

			public ItemClickListener(IMenuElement element) =>
				_element = new WeakReference<IMenuElement>(element);

			public bool OnMenuItemClick(AMenuItem item)
			{
				// MenuItem.Clicked is raised even when disabled, so check the current state first
				if (_element.TryGetTarget(out var element) && element.IsEnabled)
					element.Clicked();

				return true;
			}
		}

		/// <summary>
		/// The context flyouts that apply to a view, like the context menu Android builds for it: the flyout of the
		/// view itself, followed by the ones of its ancestors. Flyouts that can't show (disabled or input transparent
		/// views, no items) are left out, and a flyout shared by several of them is added once.
		/// </summary>
		internal static List<IMenuFlyout> CollectFlyouts(AView? view)
		{
			var flyouts = new List<IMenuFlyout>();

			for (var current = view; current is not null; current = current.Parent as AView)
			{
				if (Get(current) is not ContextFlyoutManager manager || !manager._handler.TryGetTarget(out var handler) ||
					!TryGetFlyout(handler, out var flyout) || flyouts.Contains(flyout))
					continue;

				flyouts.Add(flyout);
			}

			return flyouts;
		}

		/// <summary>
		/// Adds the items of context flyouts to a native menu. Separators start a new group, which Android draws a
		/// divider for; the menus of ancestors continue the list, like native context menus.
		/// </summary>
		internal sealed class MenuWriter
		{
			readonly Action<AMenuItem, IMenuElement> _onItem;
			readonly List<ISubMenu>? _subMenus;

			public MenuWriter(Action<AMenuItem, IMenuElement> onItem, List<ISubMenu>? subMenus = null)
			{
				_onItem = onItem;
				_subMenus = subMenus;
			}

			/// <summary>The first group id; menus that already have groups (like text menus) start after theirs.</summary>
			public int FirstGroupId { get; init; } = 1;

			/// <summary>The first item id; menus that already have items use ids that don't collide with theirs.</summary>
			public int FirstItemId
			{
				get => _nextItemId;
				init => _nextItemId = value;
			}

			/// <summary>The order of the items, which Android sorts menus by.</summary>
			public int Order { get; init; } = AMenu.None;

			/// <summary>The group ids that were used.</summary>
			public List<int> GroupIds { get; } = new();

			int _nextItemId = 1;

			public void Write(AMenu menu, IReadOnlyList<IMenuFlyout> flyouts)
			{
				if (flyouts.Count == 0)
					return;

				var groupId = NewGroup();
				foreach (var flyout in flyouts)
					groupId = Write(menu, flyout, groupId);
			}

			int NewGroup()
			{
				var groupId = FirstGroupId + GroupIds.Count;
				GroupIds.Add(groupId);
				return groupId;
			}

			int Write(AMenu menu, IList<IMenuElement> items, int groupId)
			{
				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];

					// Android draws a divider where the group changes
					if (item is IMenuFlyoutSeparator)
					{
						groupId = NewGroup();
						continue;
					}

					var itemId = _nextItemId++;
					var title = item.Text ?? string.Empty;
					AMenuItem? menuItem;

					if (item is IMenuFlyoutSubItem subItem)
					{
						var subMenu = menu.AddSubMenu(groupId, itemId, Order, title);
						if (subMenu is null)
							continue;

						_subMenus?.Add(subMenu);
						Write(subMenu, subItem, NewGroup());
						MenuCompat.SetGroupDividerEnabled(subMenu, true);
						menuItem = subMenu.Item;
					}
					else
					{
						menuItem = menu.Add(groupId, itemId, Order, title);
						if (menuItem is not null)
							_onItem(menuItem, item);
					}

					if (menuItem is null)
						continue;

					// Like native context menus, items have no icons
					menuItem.SetEnabled(item.IsEnabled);
				}

				return groupId;
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
			var menu = _menu;
			var subMenus = _subMenus;
			var wasShowing = IsShowing(this, menu);
			_menu = null;
			_subMenus = null;
			_menuPosition = null;

			if (!wasShowing)
				return;

			s_menuOwners.Remove(menu!);

			// On phones an open sub menu replaced the menu with a popup of its own, so it's closed separately
			if (subMenus is not null)
			{
				foreach (var subMenu in subMenus)
				{
					if (subMenu.IsAlive())
						subMenu.Close();
				}
			}

			menu!.Close();
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

					if (ReceivesTouchesAt(root, sibling, x, y))
						return true;
				}
			}

			return false;
		}

		static bool ReceivesTouchesAt(AView root, AView view, float x, float y)
		{
			if (view.Visibility != ViewStates.Visible || !TryMapToLocal(root, view, x, y, out _, out _))
				return false;

			if (ReceivesTouches(view))
				return true;

			if (view is ViewGroup group)
			{
				for (int i = group.ChildCount - 1; i >= 0; i--)
				{
					if (group.GetChildAt(i) is AView child && ReceivesTouchesAt(root, child, x, y))
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

			// Not handling the long click lets Android open the context menu where the view was pressed, like it does
			// for views with a native context menu; the flyout items are added while it's built.
			public bool OnLongClick(AView? v)
			{
				if (_manager.TryGetTarget(out var manager) && !manager.DefersLongPress())
					manager.PrepareNativeLongClick();

				return false;
			}
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
