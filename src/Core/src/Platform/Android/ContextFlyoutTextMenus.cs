using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AMenu = Android.Views.IMenu;
using AMenuItem = Android.Views.IMenuItem;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Context flyouts on text, following how native apps extend text menus: text keeps its own long press
	/// (selecting text, the insertion handle) and the flyout items are added to the text's menus, the
	/// selection and insertion toolbars and the context menu a secondary click opens.
	/// </summary>
	static class ContextFlyoutTextMenus
	{
		// Above the groups text menus use for their own items, so the flyout items get their own section
		const int FirstGroupId = 1000;
		const int IconLoadTimeoutMilliseconds = 300;

		// A window keeps its native context menu until the next one, so the icons it shows are kept with the window
		static readonly ConditionalWeakTable<AView, List<IDisposable>> s_iconResults = new();
		static int s_menuVersion;

		/// <summary>
		/// Whether the view is text that handles long presses and secondary clicks itself: editable text and
		/// selectable text, but not text-based controls that don't select text (like pickers).
		/// </summary>
		internal static bool IsText(AView? view) =>
			view is { Enabled: true } && IsTextView(view);

		/// <inheritdoc cref="IsText(AView?)"/>
		/// <remarks>
		/// Also when the text is currently disabled. Like Android decides whether text can be selected: it must be
		/// long-clickable, take focus and move a selection. Text that can't (a read-only Entry, a DatePicker) opens
		/// the context flyout like other views.
		/// </remarks>
		internal static bool IsTextView(AView? view) =>
			view is TextView { Focusable: true, LongClickable: true } textView &&
			textView.MovementMethod is { } movement && movement.CanSelectArbitrarily() &&
			(textView is EditText || textView.IsTextSelectable);

		/// <summary>
		/// Makes the selection and insertion toolbars of the text show the items of the context flyouts that apply
		/// to it. The app's own toolbar callbacks keep working.
		/// </summary>
		internal static void Prepare(AView? view)
		{
			if (!IsText(view) || view is not TextView textView || ContextFlyoutManager.CollectFlyouts(textView).Count == 0)
				return;

			if (textView.CustomSelectionActionModeCallback is not ToolbarCallback)
				textView.CustomSelectionActionModeCallback = new ToolbarCallback(textView, textView.CustomSelectionActionModeCallback);

			if (textView.CustomInsertionActionModeCallback is not ToolbarCallback)
				textView.CustomInsertionActionModeCallback = new ToolbarCallback(textView, textView.CustomInsertionActionModeCallback);
		}

		/// <summary>
		/// Opens the text's native context menu, with its text actions, plus the items of the context flyouts that
		/// apply to it, like a secondary click on text does natively.
		/// </summary>
		/// <param name="view">The text that was clicked.</param>
		/// <param name="x">Where it was clicked, relative to the text.</param>
		/// <param name="y">See <paramref name="x"/>.</param>
		/// <param name="fallback">Shows the flyout's own menu if the native one can't be shown.</param>
		/// <returns><see langword="true"/> when a menu is (about to be) shown.</returns>
		internal static bool ShowContextMenu(AView view, float x, float y, Func<bool> fallback)
		{
			if (!IsText(view) || view is not TextView textView)
				return false;

			var flyouts = ContextFlyoutManager.CollectFlyouts(textView, out var mauiContext);
			if (flyouts.Count == 0 || mauiContext is null)
				return false;

			var sources = new List<IImageSource>();
			foreach (var flyout in flyouts)
				CollectIcons(flyout, sources);

			if (textView.Context is null)
				return false;

			if (sources.Count == 0)
			{
				s_menuVersion++;
				return Show(textView, x, y, flyouts, null) || fallback();
			}

			_ = ShowWhenIconsLoadedAsync(textView, x, y, flyouts, sources, mauiContext, fallback);
			return true;
		}

		static async Task ShowWhenIconsLoadedAsync(TextView textView, float x, float y, List<IMenuFlyout> flyouts, List<IImageSource> sources, IMauiContext mauiContext, Func<bool> fallback)
		{
			// A newer secondary click replaces this one, like it replaces the menu that's open
			var version = ++s_menuVersion;
			var context = textView.Context!;
			var icons = new Dictionary<IImageSource, Drawable>();
			var results = new List<IDisposable>();
			var tooLate = false;

			var loads = new List<Task>(sources.Count);
			foreach (var source in sources)
				loads.Add(LoadAsync(source));

			try
			{
				await Task.WhenAny(Task.WhenAll(loads), Task.Delay(IconLoadTimeoutMilliseconds));
			}
			catch
			{
				// An icon that fails to load is left out; the menu is still shown
			}

			// Icons that finish loading later are released right away
			Dictionary<IImageSource, Drawable> loadedIcons;
			lock (results)
			{
				tooLate = true;
				loadedIcons = new Dictionary<IImageSource, Drawable>(icons);
			}

			if (version != s_menuVersion || !textView.IsAlive() || !textView.IsAttachedToWindow || !textView.IsShown ||
				!IsText(textView) || textView.RootView is not AView root)
			{
				Release(results);
				return;
			}

			if (!Show(textView, x, y, flyouts, loadedIcons))
			{
				Release(results);
				fallback();
				return;
			}

			// The window keeps showing the icons of its last context menu, so they're released with the next one
			if (s_iconResults.TryGetValue(root, out var previous))
				Release(previous);

			s_iconResults.AddOrUpdate(root, results);

			async Task LoadAsync(IImageSource source)
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

				lock (results)
				{
					if (tooLate)
					{
						result.Dispose();
						return;
					}

					results.Add(result);

					if (result.Value is Drawable drawable)
						icons[source] = ContextFlyoutManager.ToMenuIcon(drawable, context);
				}
			}
		}

		static void Release(List<IDisposable> results)
		{
			foreach (var result in results)
				result.Dispose();

			results.Clear();
		}

		static void CollectIcons(IList<IMenuElement> items, List<IImageSource> sources)
		{
			foreach (var item in items)
			{
				if (item is IMenuFlyoutSeparator)
					continue;

				if (item.Source is IImageSource { IsEmpty: false } source && !sources.Contains(source))
					sources.Add(source);

				if (item is IMenuFlyoutSubItem subItem)
					CollectIcons(subItem, sources);
			}
		}

		static bool Show(TextView textView, float x, float y, List<IMenuFlyout> flyouts, Dictionary<IImageSource, Drawable>? icons)
		{
			// Native context menus are built by the pressed view and then its ancestors, up to the window's root
			// view, so a listener there adds the items after the text's own. It's only there while the menu is built.
			// The root is the window's own decor view: apps don't register context menus on it, as that would make
			// the whole window long-clickable, so no listener of the app is replaced.
			if (textView.RootView is not AView root)
				return false;

			var wasLongClickable = root.LongClickable;
			using var listener = new ContextMenuListener(flyouts, icons);
			root.SetOnCreateContextMenuListener(listener);

			try
			{
				return textView.ShowContextMenu(x, y);
			}
			finally
			{
				// Setting a listener makes the view long-clickable
				root.SetOnCreateContextMenuListener(null);
				root.LongClickable = wasLongClickable;
			}
		}

		sealed class ContextMenuListener : Java.Lang.Object, AView.IOnCreateContextMenuListener
		{
			readonly List<IMenuFlyout> _flyouts;
			readonly Dictionary<IImageSource, Drawable>? _icons;

			public ContextMenuListener(List<IMenuFlyout> flyouts, Dictionary<IImageSource, Drawable>? icons)
			{
				_flyouts = flyouts;
				_icons = icons;
			}

			public void OnCreateContextMenu(IContextMenu? menu, AView? v, IContextMenuContextMenuInfo? menuInfo)
			{
				if (menu is null)
					return;

				// Like the items apps add to native context menus, they come before the text's own actions
				var writer = new ContextFlyoutManager.MenuWriter(
					(menuItem, element) => menuItem.SetOnMenuItemClickListener(new ItemClickListener(element)),
					(menuItem, source) =>
					{
						if (_icons is not null && _icons.TryGetValue(source, out var icon))
							menuItem.SetIcon(icon);
					})
				{
					FirstGroupId = FirstGroupId,
					FirstItemId = FirstGroupId,
				};

				writer.Write(menu, _flyouts);
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
		/// Adds the flyout items to the selection or insertion toolbar of text, like the custom action mode
		/// callbacks native apps use for their own text actions.
		/// </summary>
		sealed class ToolbarCallback : Java.Lang.Object, ActionMode.ICallback
		{
			readonly WeakReference<TextView> _textView;
			readonly ActionMode.ICallback? _inner;
			Dictionary<int, WeakReference<IMenuElement>>? _items;
			List<int>? _groupIds;

			public ToolbarCallback(TextView textView, ActionMode.ICallback? inner)
			{
				_textView = new WeakReference<TextView>(textView);
				_inner = inner;
			}

			public bool OnCreateActionMode(ActionMode? mode, AMenu? menu)
			{
				if (_inner is not null && !_inner.OnCreateActionMode(mode, menu))
					return false;

				Update(menu);
				return true;
			}

			// The flyouts are read again whenever the toolbar updates, so they reflect the current state
			public bool OnPrepareActionMode(ActionMode? mode, AMenu? menu)
			{
				var result = _inner?.OnPrepareActionMode(mode, menu) ?? true;
				return Update(menu) || result;
			}

			public bool OnActionItemClicked(ActionMode? mode, AMenuItem? item)
			{
				if (item is not null && _groupIds is not null && _groupIds.Contains(item.GroupId) &&
					_items is not null && _items.TryGetValue(item.ItemId, out var reference))
				{
					if (reference.TryGetTarget(out var element) && element.IsEnabled)
						element.Clicked();

					mode?.Finish();
					return true;
				}

				return _inner?.OnActionItemClicked(mode, item) ?? false;
			}

			public void OnDestroyActionMode(ActionMode? mode)
			{
				_items = null;
				_groupIds = null;
				_inner?.OnDestroyActionMode(mode);
			}

			bool Update(AMenu? menu)
			{
				// An app can wrap this callback in its own, which is then wrapped again: only the outer one adds items
				if (menu is null || !_textView.TryGetTarget(out var textView) || !textView.IsAlive() ||
					(textView.CustomSelectionActionModeCallback != this && textView.CustomInsertionActionModeCallback != this))
					return false;

				var changed = false;
				if (_groupIds is not null)
				{
					foreach (var groupId in _groupIds)
						menu.RemoveGroup(groupId);

					changed = _groupIds.Count > 0;
				}

				var items = new Dictionary<int, WeakReference<IMenuElement>>();
				var flyouts = ContextFlyoutManager.CollectFlyouts(textView);

				var writer = new ContextFlyoutManager.MenuWriter((menuItem, element) => items[menuItem.ItemId] = new WeakReference<IMenuElement>(element))
				{
					FirstGroupId = FirstGroupId,
					FirstItemId = FirstGroupId,
				};

				writer.Write(menu, flyouts);

				_items = items;
				_groupIds = writer.GroupIds;
				return changed || items.Count > 0;
			}
		}
	}
}
