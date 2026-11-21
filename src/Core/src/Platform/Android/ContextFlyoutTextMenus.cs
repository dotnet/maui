using System;
using System.Collections.Generic;
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
		const int FirstGroupId = ContextFlyoutManager.FirstMenuId;

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

			// Like the items apps add to native context menus, they come before the text's own actions
			var flyouts = ContextFlyoutManager.CollectFlyouts(textView);
			if (flyouts.Count == 0)
				return false;

			return ContextFlyoutManager.ShowNativeContextMenu(textView, (x, y), flyouts, out _) || fallback();
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
