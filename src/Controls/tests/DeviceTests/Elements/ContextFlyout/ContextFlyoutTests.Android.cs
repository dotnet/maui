#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Fact(DisplayName = "Context flyout manager is only attached when a ContextFlyout is set and doesn't change touch handling")]
		public async Task ManagerIsAttachedOnlyWithContextFlyout()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() => WithTouchTracker(() =>
			{
				var label = new Label { Text = "Target" };
				var handler = CreateHandler<LabelHandler>(label);
				var view = handler.PlatformView;
				var wasClickable = view.Clickable;
				var wasLongClickable = view.LongClickable;

				Assert.Null(ContextFlyoutManager.Get(view));

				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				var manager = ContextFlyoutManager.Get(view);
				Assert.NotNull(manager);
				Assert.False(manager!.UsesLongClickFallback);
				Assert.Equal(wasClickable, view.Clickable);
				Assert.Equal(wasLongClickable, view.LongClickable);

				FlyoutBase.SetContextFlyout(label, null);
				Assert.Null(ContextFlyoutManager.Get(view));
			}));
		}

		[Fact(DisplayName = "Context menu contains items, sub menus, separators and disabled state")]
		public async Task ContextMenuIsBuiltFromFlyout()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;
				Assert.True(manager.Show());

				var menu = manager.Menu!;
				Assert.Equal(3, menu.Size());

				var copy = menu.GetItem(0)!;
				var disabled = menu.GetItem(1)!;
				var edit = menu.GetItem(2)!;

				Assert.Equal("Copy", copy.TitleFormatted?.ToString());
				Assert.True(copy.IsEnabled);
				Assert.Equal("Disabled", disabled.TitleFormatted?.ToString());
				Assert.False(disabled.IsEnabled);

				// Items after a separator are in another group, which Android draws a divider for
				Assert.Equal(copy.GroupId, disabled.GroupId);
				Assert.NotEqual(copy.GroupId, edit.GroupId);

				Assert.Equal("Edit", edit.TitleFormatted?.ToString());
				Assert.True(edit.HasSubMenu);
				Assert.Equal("Nested", edit.SubMenu!.GetItem(0)!.TitleFormatted?.ToString());

				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Choosing a context menu item runs its command with the bound parameter")]
		public async Task ChoosingMenuItemRunsCommand()
		{
			SetupBuilder();
			object? executedParameter = null;
			var disabledExecuted = false;
			var nestedClicked = false;

			var flyout = CreateFlyout(out var nested);
			var copy = (MenuFlyoutItem)flyout[0];
			copy.Command = new Command(p => executedParameter = p);
			copy.SetBinding(MenuItem.CommandParameterProperty, new Binding(nameof(ContextMenuViewModel.Title)));
			((MenuFlyoutItem)flyout[1]).Clicked += (_, _) => disabledExecuted = true;
			nested.Clicked += (_, _) => nestedClicked = true;

			var label = new Label { Text = "Target", BindingContext = new ContextMenuViewModel { Title = "parameter" } };
			FlyoutBase.SetContextFlyout(label, flyout);

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;

				Assert.True(manager.Show());
				var menu = manager.Menu!;
				menu.PerformIdentifierAction(menu.GetItem(0)!.ItemId, 0);
				Assert.Equal("parameter", executedParameter);

				Assert.True(manager.Show());
				menu = manager.Menu!;
				menu.PerformIdentifierAction(menu.GetItem(1)!.ItemId, 0);
				Assert.False(disabledExecuted);

				Assert.True(manager.Show());
				menu = manager.Menu!;
				var subMenu = menu.GetItem(2)!.SubMenu!;
				subMenu.PerformIdentifierAction(subMenu.GetItem(0)!.ItemId, 0);
				Assert.True(nestedClicked);

				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Context menu reflects the current state of the items every time it opens")]
		public async Task ContextMenuIsRebuiltWithCurrentState()
		{
			SetupBuilder();
			var flyout = CreateFlyout(out var nested);
			var bound = new MenuFlyoutItem();
			bound.SetBinding(MenuFlyoutItem.TextProperty, new Binding(nameof(ContextMenuViewModel.Title)));
			flyout.Add(bound);
			var viewModel = new ContextMenuViewModel { Title = "Before" };
			var label = new Label { Text = "Target", BindingContext = viewModel };
			FlyoutBase.SetContextFlyout(label, flyout);

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;

				Assert.True(manager.Show());
				Assert.Equal("Before", manager.Menu!.GetItem(3)!.TitleFormatted?.ToString());

				viewModel.Title = "After";
				nested.Text = "Nested renamed";
				flyout.RemoveAt(0);

				Assert.True(manager.Show());
				var menu = manager.Menu!;
				Assert.Equal(3, menu.Size());
				Assert.Equal("After", menu.GetItem(2)!.TitleFormatted?.ToString());
				Assert.Equal("Nested renamed", menu.GetItem(1)!.SubMenu!.GetItem(0)!.TitleFormatted?.ToString());

				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Disabled or input transparent views don't open a context menu")]
		public async Task DisabledViewHasNoContextMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;

				label.IsEnabled = false;
				Assert.False(manager.Show());

				label.IsEnabled = true;
				label.InputTransparent = true;
				Assert.False(manager.Show());

				label.InputTransparent = false;
				Assert.True(manager.Show());
				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Accessibility long-click action opens the context menu")]
		public async Task AccessibilityActionOpensContextMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var view = (AView)handler.PlatformView!;
				var manager = ContextFlyoutManager.Get(view)!;

				using var node = view.CreateAccessibilityNodeInfo()!;
				Assert.Contains(node.ActionList!, a => a.Id == AccessibilityNodeInfoCompat.ActionLongClick);

				Assert.True(view.PerformAccessibilityAction(global::Android.Views.Accessibility.Action.LongClick, null));
				Assert.NotNull(manager.Menu);

				// Like a native long click that isn't a touch, the menu opens as a dialog rather than at a position
				Assert.Null(manager.MenuPosition);
				manager.Dismiss();
				Assert.Null(manager.Menu);
			}));
		}

		[Fact(DisplayName = "Long press opens the context menu, a tap doesn't, and touches aren't consumed until it opens")]
		public async Task LongPressOpensContextMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target", WidthRequest = 200, HeightRequest = 100 };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, async handler =>
			{
				var view = (AView)handler.PlatformView!;
				var manager = ContextFlyoutManager.Get(view)!;
				var window = view.Context!.GetActivity()!.Window!;
				var (x, y) = GetCenterInWindow(view);

				// A tap is passed on and doesn't open the menu
				var downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, x, y));
				Assert.False(Dispatch(window, downTime, MotionEventActions.Up, x, y));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Null(manager.Menu);

				// Moving beyond the touch slop (scrolling) cancels the long press
				downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, x, y));
				Assert.False(Dispatch(window, downTime, MotionEventActions.Move, x, y + 200));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Null(manager.Menu);
				Dispatch(window, downTime, MotionEventActions.Up, x, y + 200);

				// Holding opens the menu where the view was pressed; the rest of the touch is consumed so nothing gets clicked
				var (pressX, pressY) = (x - 40, y + 10);
				downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, pressX, pressY));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Menu);
				AssertOpenedAt(manager, view, pressX, pressY);
				Assert.True(Dispatch(window, downTime, MotionEventActions.Up, pressX, pressY));

				manager.Dismiss();
				Assert.Null(manager.Menu);
			}));
		}

		[Fact(DisplayName = "Overlapping views open the menu of the view drawn on top, regardless of registration order")]
		public async Task TopmostOverlappingViewOpensItsMenu()
		{
			SetupBuilder();
			var bottom = new Label { Text = "Bottom" };
			var top = new Label { Text = "Top" };
			var grid = new Grid { WidthRequest = 200, HeightRequest = 100, Children = { bottom, top } };

			// The view drawn below registers last
			FlyoutBase.SetContextFlyout(top, new MenuFlyout { new MenuFlyoutItem { Text = "Top item" } });
			FlyoutBase.SetContextFlyout(bottom, new MenuFlyout { new MenuFlyoutItem { Text = "Bottom item" } });

			await WithTouchTrackerAsync(() => AttachAndRun(grid, async handler =>
			{
				var topView = (AView)top.Handler!.PlatformView!;
				var bottomManager = ContextFlyoutManager.Get((AView)bottom.Handler!.PlatformView!)!;
				var topManager = ContextFlyoutManager.Get(topView)!;
				var window = topView.Context!.GetActivity()!.Window!;
				var (x, y) = GetCenterInWindow(topView);

				var downTime = SystemClock.UptimeMillis();
				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse);

				Assert.NotNull(topManager.Menu);
				Assert.Null(bottomManager.Menu);
				topManager.Dismiss();
				await Task.Yield();
			}));
		}

		[Fact(DisplayName = "Tap gesture accessibility keeps working on a view with a context flyout")]
		public async Task TapGestureAccessibilityDelegateIsKept()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() => WithTouchTracker(() =>
			{
				var label = new Label { Text = "Target" };
				label.GestureRecognizers.Add(new TapGestureRecognizer());
				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				var handler = CreateHandler<LabelHandler>(label);
				var view = handler.PlatformView;

				Assert.IsType<ControlsAccessibilityDelegate>(AndroidX.Core.View.ViewCompat.GetAccessibilityDelegate(view));

				using var node = view.CreateAccessibilityNodeInfo()!;
				Assert.Contains(node.ActionList!, a => a.Id == AccessibilityNodeInfoCompat.ActionLongClick);
				Assert.Contains(node.ActionList!, a => a.Id == AccessibilityNodeInfoCompat.ActionClick);
			}));
		}

		[Fact(DisplayName = "A view that can't open its context menu doesn't block the context menu of its parent")]
		public async Task NonShowableChildDoesNotBlockParentMenu()
		{
			SetupBuilder();
			var child = new Label { Text = "Child", InputTransparent = true };
			var parent = new Grid { WidthRequest = 200, HeightRequest = 100, Children = { child } };
			FlyoutBase.SetContextFlyout(child, new MenuFlyout { new MenuFlyoutItem { Text = "Child item" } });
			FlyoutBase.SetContextFlyout(parent, new MenuFlyout { new MenuFlyoutItem { Text = "Parent item" } });

			await WithTouchTrackerAsync(() => AttachAndRun(parent, async handler =>
			{
				var childView = (AView)child.Handler!.PlatformView!;
				var parentManager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;
				var window = childView.Context!.GetActivity()!.Window!;
				var (x, y) = GetCenterInWindow(childView);

				var downTime = SystemClock.UptimeMillis();
				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse);

				Assert.NotNull(parentManager.Menu);
				parentManager.Dismiss();
				await Task.Yield();
			}));
		}

		// What a long press does natively: the innermost view under the finger that receives touches handles it.
		// Its own long-press handler comes first, then the context menu, then text selection and tooltips.
		public static TheoryData<string, bool> LongPressPriorities => new()
		{
			{ "LongPressGesture", false },
			{ "DragGesture", false },
			{ "ChildLongPressGesture", false },
			{ "ParentLongPressGesture", true },
			{ "Entry", false },
			{ "EntryInParent", false },
			{ "ReadOnlyEntry", true },
			{ "DatePicker", true },
			{ "Picker", true },
			{ "ToolTip", true },
			{ "ReorderingParent", false },
			{ "ReorderingWithMenu", false },
			{ "DropTargetAbove", true },
			{ "ElevatedLongPressGesture", false },
			{ "DisabledChildWithMenu", true },
			{ "DisabledChildLongPressGesture", true },
		};

		[Theory(DisplayName = "Long press follows native priority; secondary click always opens the menu")]
		[MemberData(nameof(LongPressPriorities))]
		public async Task LongPressFollowsNativePriority(string behavior, bool menuOpensOnLongPress)
		{
			SetupBuilder();
			// Text keeps its own long press (selecting text) and shows the flyout items in its toolbars instead
			var isText = behavior is "Entry" or "EntryInParent";
			Microsoft.Maui.Controls.View pressed = behavior switch
			{
				"Entry" or "EntryInParent" => new Entry { Text = "Text" },
				// Read-only text can't be selected, so it opens the context menu like other views
				"ReadOnlyEntry" => new Entry { Text = "Text", IsReadOnly = true },
				"Picker" => new Picker { Title = "Picker" },
				// Text-based, but it doesn't select text
				"DatePicker" => new DatePicker(),
				_ => new Label { Text = "Target" },
			};
			pressed.WidthRequest = 200;
			pressed.HeightRequest = 100;
			var root = new Grid { Children = { pressed } };
			var withMenu = behavior is "ChildLongPressGesture" or "ReorderingWithMenu" or "ElevatedLongPressGesture" or "DisabledChildWithMenu" or "DisabledChildLongPressGesture" or "EntryInParent"
				? (Microsoft.Maui.Controls.View)root
				: pressed;
			var longPresses = 0;
			var longPress = new LongPressGestureRecognizer();
			longPress.LongPressed += (_, _) => longPresses++;

			switch (behavior)
			{
				case "LongPressGesture":
				case "ChildLongPressGesture":
					pressed.GestureRecognizers.Add(longPress);
					break;
				case "DisabledChildWithMenu":
					// Disabled views don't take touches, so the press goes to the parent's menu
					pressed.IsEnabled = false;
					FlyoutBase.SetContextFlyout(pressed, CreateFlyout(out _));
					break;
				case "DisabledChildLongPressGesture":
					pressed.IsEnabled = false;
					pressed.GestureRecognizers.Add(longPress);
					break;
				case "DragGesture":
					pressed.GestureRecognizers.Add(new DragGestureRecognizer());
					break;
				case "ToolTip":
					ToolTipProperties.SetText(pressed, "Tooltip");
					break;
				case "ParentLongPressGesture":
					root.GestureRecognizers.Add(longPress);
					break;
				case "DropTargetAbove":
					// Drop targets only watch drags, so the press goes through to the view below
					var dropTarget = new Label { WidthRequest = 200, HeightRequest = 100 };
					dropTarget.GestureRecognizers.Add(new DropGestureRecognizer());
					root.Add(dropTarget);
					break;
				case "ElevatedLongPressGesture":
					// The elevated view gets the press first, even though the tappable one was added later
					pressed.GestureRecognizers.Add(longPress);
					var tappable = new Label { WidthRequest = 200, HeightRequest = 100 };
					tappable.GestureRecognizers.Add(new TapGestureRecognizer());
					root.Add(tappable);
					break;
			}

			FlyoutBase.SetContextFlyout(withMenu, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(root, async handler =>
			{
				var view = (AView)pressed.Handler!.PlatformView!;
				var menuView = (AView)withMenu.Handler!.PlatformView!;
				var manager = ContextFlyoutManager.Get(menuView)!;
				var window = view.Context!.GetActivity()!.Window!;
				var (x, y) = GetCenterInWindow(view);

				// Like a reorderable CollectionView, which reorders its items on long press
				var rootView = (AView)root.Handler!.PlatformView!;
				if (behavior is "ReorderingParent" or "ReorderingWithMenu")
					ContextFlyoutManager.SetInterceptsLongPress(rootView, true);

				if (behavior == "ElevatedLongPressGesture")
					view.Elevation = 10;

				var downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, x, y));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Equal(menuOpensOnLongPress, manager.Menu is not null);

				if (isText)
				{
					var editText = Assert.IsAssignableFrom<global::Android.Widget.EditText>(view);
					Assert.NotNull(editText.CustomSelectionActionModeCallback);
					Assert.NotNull(editText.CustomInsertionActionModeCallback);
				}

				// Only one of them happens: the gesture doesn't fire after the menu opened, even on a parent
				if (behavior.EndsWith("LongPressGesture", StringComparison.Ordinal))
				{
					await Task.Delay(ViewConfiguration.LongPressTimeout);
					Assert.Equal(menuOpensOnLongPress ? 0 : 1, longPresses);
				}

				ContextFlyoutManager.SetInterceptsLongPress(rootView, false);
				Dispatch(window, downTime, MotionEventActions.Up, x, y);
				manager.Dismiss();

				// The accessibility long-click action acts on the view with the menu itself; text keeps its own
				var sameViewHandler = behavior is "LongPressGesture" or "DragGesture" or "Entry";
				menuView.PerformAccessibilityAction(global::Android.Views.Accessibility.Action.LongClick, null);
				Assert.Equal(!sameViewHandler, manager.Menu is not null);
				manager.Dismiss();

				// A secondary click always opens a context menu; on text it's the text's own, with the flyout items
				downTime = SystemClock.UptimeMillis();
				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse);
				Assert.Equal(!isText, manager.Menu is not null);
				manager.Dismiss();
				view.Context!.GetActivity()!.CloseContextMenu();
			}));
		}

		[Fact(DisplayName = "With mobile context flyouts turned off, views keep their behavior from before")]
		public async Task MobileContextFlyoutsCanBeTurnedOff()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			var entry = new Entry { Text = "Text" };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
			FlyoutBase.SetContextFlyout(entry, CreateFlyout(out _));
			var root = new VerticalStackLayout { label, entry };

			await WithMobileContextFlyoutsDisabled(() => AttachAndRun(root, handler =>
			{
				var labelView = (AView)label.Handler!.PlatformView!;
				var editText = (global::Android.Widget.EditText)entry.Handler!.PlatformView!;

				Assert.Null(ContextFlyoutManager.Get(labelView));
				Assert.Null(ContextFlyoutManager.Get(editText));
				Assert.False(labelView.LongClickable);
				Assert.Null(editText.CustomSelectionActionModeCallback);
			}));
		}

		[Fact(DisplayName = "The menus of ancestors are added after the view's own, like native context menus")]
		public async Task AncestorMenusAreMerged()
		{
			SetupBuilder();
			var label = new Label { Text = "Target", WidthRequest = 200, HeightRequest = 100 };
			var parentClicks = 0;
			var parentItem = new MenuFlyoutItem { Text = "Parent" };
			parentItem.Clicked += (_, _) => parentClicks++;
			var root = new Grid { Children = { label } };
			var sharedFlyout = CreateFlyout(out _);
			FlyoutBase.SetContextFlyout(label, sharedFlyout);
			FlyoutBase.SetContextFlyout(root, new MenuFlyout { parentItem });

			await WithTouchTrackerAsync(() => AttachAndRun(root, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)label.Handler!.PlatformView!)!;
				Assert.True(manager.Show());

				var menu = manager.Menu!;
				Assert.Equal(new[] { "Copy", "Disabled", "Edit", "Parent" }, GetTitles(menu));

				// Natively the items of the menus follow each other, without a divider in between
				Assert.Equal(menu.GetItem(2)!.GroupId, menu.GetItem(3)!.GroupId);

				Assert.True(menu.PerformIdentifierAction(menu.GetItem(3)!.ItemId, 0));
				Assert.Equal(1, parentClicks);
				manager.Dismiss();

				// A flyout shared by the view and an ancestor is added once
				FlyoutBase.SetContextFlyout(root, sharedFlyout);
				Assert.True(manager.Show());
				Assert.Equal(new[] { "Copy", "Disabled", "Edit" }, GetTitles(manager.Menu!));
				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Text toolbars show the flyout items; the app's own toolbar callbacks keep working")]
		public async Task TextToolbarsShowFlyoutItems()
		{
			SetupBuilder();
			var entry = new Entry { Text = "Text", WidthRequest = 200, HeightRequest = 100 };
			var copyClicks = 0;
			var flyout = CreateFlyout(out _);
			((MenuFlyoutItem)flyout[0]).Clicked += (_, _) => copyClicks++;
			var root = new Grid { Children = { entry } };
			FlyoutBase.SetContextFlyout(entry, flyout);
			FlyoutBase.SetContextFlyout(root, new MenuFlyout { new MenuFlyoutItem { Text = "Parent" } });

			await WithTouchTrackerAsync(() => AttachAndRun(root, handler =>
			{
				var editText = (global::Android.Widget.EditText)entry.Handler!.PlatformView!;
				var appCallback = new ToolbarCallbackStub();
				editText.CustomSelectionActionModeCallback = appCallback;
				ContextFlyoutTextMenus.Prepare(editText);

				var callback = editText.CustomSelectionActionModeCallback!;
				Assert.NotSame(appCallback, callback);
				Assert.NotNull(editText.CustomInsertionActionModeCallback);

				var menu = new global::AndroidX.AppCompat.Widget.PopupMenu(editText.Context!, editText).Menu;
				Assert.True(callback.OnCreateActionMode(null, menu));
				Assert.Equal(new[] { "App", "Copy", "Disabled", "Edit", "Parent" }, GetTitles(menu));

				// The flyouts are read again when the toolbar updates, without adding the items twice
				Assert.True(callback.OnPrepareActionMode(null, menu));
				Assert.Equal(new[] { "App", "Copy", "Disabled", "Edit", "Parent" }, GetTitles(menu));

				Assert.True(callback.OnActionItemClicked(null, FindItem(menu, "Copy")));
				Assert.Equal(1, copyClicks);

				// Disabled items do nothing
				Assert.True(callback.OnActionItemClicked(null, FindItem(menu, "Disabled")));
				Assert.Equal(1, copyClicks);

				// The app's own items still go to the app's callback
				callback.OnActionItemClicked(null, FindItem(menu, "App"));
				Assert.Equal(1, appCallback.Clicks);

				callback.OnDestroyActionMode(null);
				Assert.True(appCallback.Destroyed);

				// An app that wraps the callback in its own after the flyout was added still gets the items once
				editText.CustomSelectionActionModeCallback = new DelegatingCallback(callback);
				ContextFlyoutTextMenus.Prepare(editText);
				var outer = editText.CustomSelectionActionModeCallback!;
				Assert.IsNotType<DelegatingCallback>(outer);
				menu.Clear();
				Assert.True(outer.OnCreateActionMode(null, menu));
				Assert.Equal(new[] { "App", "Copy", "Disabled", "Edit", "Parent" }, GetTitles(menu));
				callback = outer;

				// Without flyouts nothing is added
				FlyoutBase.SetContextFlyout(entry, null);
				FlyoutBase.SetContextFlyout(root, null);
				menu.Clear();
				Assert.True(callback.OnCreateActionMode(null, menu));
				Assert.Equal(new[] { "App" }, GetTitles(menu));
			}));
		}

		[Fact(DisplayName = "A secondary click on text opens its native context menu with the flyout items")]
		public async Task SecondaryClickOnTextShowsNativeTextMenu()
		{
			SetupBuilder();
			var entry = new Entry { Text = "Text", WidthRequest = 200, HeightRequest = 100 };
			var copyClicks = 0;
			var flyout = CreateFlyout(out _);
			((MenuFlyoutItem)flyout[0]).Clicked += (_, _) => copyClicks++;
			var root = new Grid { Children = { entry } };
			FlyoutBase.SetContextFlyout(entry, flyout);
			FlyoutBase.SetContextFlyout(root, new MenuFlyout { new MenuFlyoutItem { Text = "Parent" } });

			await WithTouchTrackerAsync(() => AttachAndRun(root, handler =>
			{
				var editText = (global::Android.Widget.EditText)entry.Handler!.PlatformView!;
				var window = editText.Context!.GetActivity()!.Window!;
				var decor = window.DecorView;
				var decorWasLongClickable = decor.LongClickable;

				// An app's own native context menu listener keeps working next to the flyout
				IContextMenu? created = null;
				editText.ContextMenuCreated += (_, e) => created = e.Menu;

				var (x, y) = GetCenterInWindow(editText);
				var downTime = SystemClock.UptimeMillis();
				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse);

				Assert.NotNull(created);
				var titles = GetTitles(created!);

				// Like the items apps add to native context menus, the flyout items come before the text's own actions
				Assert.Equal(new[] { "Copy", "Disabled", "Edit", "Parent" }, titles.Take(4));
				Assert.True(titles.Length > 4, "The text's own actions follow the flyout items");

				Assert.True(created!.PerformIdentifierAction(FindItem(created, "Copy").ItemId, 0));
				Assert.Equal(1, copyClicks);

				// The listener that added the items is only there while the menu is built
				Assert.Equal(decorWasLongClickable, decor.LongClickable);
				editText.Context!.GetActivity()!.CloseContextMenu();
			}));
		}

		static string[] GetTitles(IMenu menu)
		{
			var titles = new List<string>();
			for (int i = 0; i < menu.Size(); i++)
				titles.Add(menu.GetItem(i)!.TitleFormatted?.ToString() ?? string.Empty);

			return titles.ToArray();
		}

		static IMenuItem FindItem(IMenu menu, string title)
		{
			for (int i = 0; i < menu.Size(); i++)
			{
				if (menu.GetItem(i)!.TitleFormatted?.ToString() == title)
					return menu.GetItem(i)!;
			}

			throw new InvalidOperationException($"No menu item {title}");
		}

		sealed class DelegatingCallback : Java.Lang.Object, ActionMode.ICallback
		{
			readonly ActionMode.ICallback _inner;

			public DelegatingCallback(ActionMode.ICallback inner) => _inner = inner;

			public bool OnCreateActionMode(ActionMode? mode, IMenu? menu) => _inner.OnCreateActionMode(mode, menu);

			public bool OnPrepareActionMode(ActionMode? mode, IMenu? menu) => _inner.OnPrepareActionMode(mode, menu);

			public bool OnActionItemClicked(ActionMode? mode, IMenuItem? item) => _inner.OnActionItemClicked(mode, item);

			public void OnDestroyActionMode(ActionMode? mode) => _inner.OnDestroyActionMode(mode);
		}

		sealed class ToolbarCallbackStub : Java.Lang.Object, ActionMode.ICallback
		{
			public int Clicks { get; private set; }

			public bool Destroyed { get; private set; }

			public bool OnCreateActionMode(ActionMode? mode, IMenu? menu)
			{
				menu?.Add(0, 1, 0, "App");
				return true;
			}

			public bool OnPrepareActionMode(ActionMode? mode, IMenu? menu) => false;

			public bool OnActionItemClicked(ActionMode? mode, IMenuItem? item)
			{
				Clicks++;
				return true;
			}

			public void OnDestroyActionMode(ActionMode? mode) => Destroyed = true;
		}

		[Theory(DisplayName = "Holding a confirm key on a focused control opens its context menu at its center; a short press clicks")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task HoldingConfirmKeyOpensContextMenu(bool tapGesture)
		{
			SetupBuilder();
			var clicks = 0;
			Microsoft.Maui.Controls.View target;
			if (tapGesture)
			{
				// Not clickable natively: the tap gesture handles the confirm key
				var tap = new TapGestureRecognizer();
				tap.Tapped += (_, _) => clicks++;
				target = new Label { Text = "Target", WidthRequest = 200, HeightRequest = 100, GestureRecognizers = { tap } };
			}
			else
			{
				var button = new Button { Text = "Target", WidthRequest = 200, HeightRequest = 100 };
				button.Clicked += (_, _) => clicks++;
				target = button;
			}

			FlyoutBase.SetContextFlyout(target, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(target, async handler =>
			{
				var view = (AView)handler.PlatformView!;
				var manager = ContextFlyoutManager.Get(view)!;
				view.FocusableInTouchMode = true;
				ContextFlyoutKeyTracker.Observe(view.RootView);
				Assert.True(view.RequestFocus());

				// A short press clicks and doesn't open the menu
				var downTime = SendKey(view, KeyEventActions.Down);
				SendKey(view, KeyEventActions.Up, downTime);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Equal(1, clicks);
				Assert.Null(manager.Menu);

				// From Android 13, keys with modifiers (e.g. Shift+Enter) don't long click views, so they don't open the menu
				if (OperatingSystem.IsAndroidVersionAtLeast(33))
				{
					downTime = SendKey(view, KeyEventActions.Down, meta: MetaKeyStates.ShiftOn | MetaKeyStates.ShiftLeftOn);
					await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
					Assert.Null(manager.Menu);
					SendKey(view, KeyEventActions.Up, downTime, MetaKeyStates.ShiftOn | MetaKeyStates.ShiftLeftOn);
				}

				// Holding opens the menu at the center of the view
				downTime = SendKey(view, KeyEventActions.Down);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Menu);
				var (x, y) = GetCenterInWindow(view);
				AssertOpenedAt(manager, view, x, y);

				// Releasing the key after the menu opened doesn't click, like a native long click
				SendKey(view, KeyEventActions.Up, downTime);
				Assert.Equal(1, clicks);
				Assert.False(view.Pressed);
				manager.Dismiss();

				// When the release goes to the open menu instead, later presses (e.g. on a tap gesture elsewhere) still count
				downTime = SendKey(view, KeyEventActions.Down);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Menu);
				manager.Dismiss();
				var now = SystemClock.UptimeMillis();
				using (var release = new KeyEvent(downTime, now, KeyEventActions.Up, Keycode.Enter, 0))
					Assert.True(ContextFlyoutKeyTracker.IsConsumedKeyUp(release));
				using (var laterRelease = new KeyEvent(now, now, KeyEventActions.Up, Keycode.Enter, 0))
					Assert.False(ContextFlyoutKeyTracker.IsConsumedKeyUp(laterRelease));

				view.FocusableInTouchMode = false;
			}));

			static long SendKey(AView view, KeyEventActions action, long? downTime = null, MetaKeyStates meta = 0)
			{
				var now = SystemClock.UptimeMillis();
				using var e = new KeyEvent(downTime ?? now, now, action, Keycode.Enter, 0, meta);
				view.DispatchKeyEvent(e);
				return downTime ?? now;
			}
		}

		[Fact(DisplayName = "Items have no icons, like native context menus")]
		public async Task ContextMenuItemsHaveNoIcons()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			FlyoutBase.SetContextFlyout(label, new MenuFlyout
			{
				new MenuFlyoutItem { Text = "With icon", IconImageSource = new FontImageSource { Glyph = "A", Color = Colors.Blue } },
				new MenuFlyoutSubItem { Text = "Sub menu with icon", IconImageSource = new FontImageSource { Glyph = "B", Color = Colors.Blue } },
			});

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;
				Assert.True(manager.Show());

				var menu = manager.Menu!;
				Assert.Equal(new[] { "With icon", "Sub menu with icon" }, GetTitles(menu));
				Assert.Null(menu.GetItem(0)!.Icon);
				Assert.Null(menu.GetItem(1)!.Icon);

				manager.Dismiss();
			}));
		}

		[Fact(DisplayName = "Showing another view's context menu replaces the open one")]
		public async Task OnlyOneContextMenuIsOpen()
		{
			SetupBuilder();
			var first = new Label { Text = "First" };
			var second = new Label { Text = "Second" };
			FlyoutBase.SetContextFlyout(first, new MenuFlyout { new MenuFlyoutItem { Text = "First item" } });
			FlyoutBase.SetContextFlyout(second, new MenuFlyout { new MenuFlyoutItem { Text = "Second item" } });
			var root = new VerticalStackLayout { first, second };

			await WithTouchTrackerAsync(() => AttachAndRun(root, handler =>
			{
				var firstManager = ContextFlyoutManager.Get((AView)first.Handler!.PlatformView!)!;
				var secondManager = ContextFlyoutManager.Get((AView)second.Handler!.PlatformView!)!;

				Assert.True(firstManager.Show());
				Assert.True(secondManager.Show());

				// The window has one context menu: dismissing the replaced flyout doesn't close the open one
				Assert.Null(firstManager.Menu);
				Assert.Equal(new[] { "Second item" }, GetTitles(secondManager.Menu!));
				firstManager.Dismiss();
				Assert.NotNull(secondManager.Menu);

				secondManager.Dismiss();
				Assert.Null(secondManager.Menu);
			}));
		}

		[Fact(DisplayName = "Removing the flyout closes its open sub menu too")]
		public async Task DismissClosesOpenSubMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target", WidthRequest = 200, HeightRequest = 100 };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, async handler =>
			{
				var view = (AView)handler.PlatformView!;
				var manager = ContextFlyoutManager.Get(view)!;
				var decor = view.RootView!;
				await AssertHelpers.AssertEventually(() => decor.HasWindowFocus, message: "The window has focus before the menu opens");

				// Opened at a position, so it's a popup: on phones the sub menu replaces it with a popup of its own
				var (x, y) = GetCenterInWindow(view);
				Assert.True(manager.Show(x, y));
				await AssertHelpers.AssertEventually(() => !decor.HasWindowFocus, message: "The menu takes focus");
				var menu = manager.Menu!;
				Assert.True(menu.PerformIdentifierAction(FindItem(menu, "Edit").ItemId, 0));
				await Task.Delay(300);

				FlyoutBase.SetContextFlyout(label, null);

				// Nothing is left open on top of the window
				await AssertHelpers.AssertEventually(() => decor.HasWindowFocus, message: "The sub menu was closed");
			}));
		}

		static void AssertOpenedAt(ContextFlyoutManager manager, AView view, float windowX, float windowY)
		{
			Assert.True(manager.MenuPosition.HasValue);
			var position = manager.MenuPosition!.Value;
			var location = new int[2];
			view.GetLocationInWindow(location);
			Assert.Equal(windowX - location[0], position.X, 1f);
			Assert.Equal(windowY - location[1], position.Y, 1f);
		}

		[Fact(DisplayName = "Outside a MauiAppCompatActivity a long-click listener is used and removed again")]
		public async Task LongClickFallbackIsRestored()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var previous = ContextFlyoutManager.UseTouchTrackerOverride;
				ContextFlyoutManager.UseTouchTrackerOverride = false;

				try
				{
					var label = new Label { Text = "Target" };
					var handler = CreateHandler<LabelHandler>(label);
					var view = handler.PlatformView;
					var wasLongClickable = view.LongClickable;

					FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
					Assert.True(ContextFlyoutManager.Get(view)!.UsesLongClickFallback);
					Assert.True(view.LongClickable);

					FlyoutBase.SetContextFlyout(label, null);
					Assert.Equal(wasLongClickable, view.LongClickable);
				}
				finally
				{
					ContextFlyoutManager.UseTouchTrackerOverride = previous;
				}
			});
		}

		[Fact(DisplayName = "Outside a MauiAppCompatActivity a long click opens the native context menu with the flyout items")]
		public async Task LongClickFallbackOpensNativeContextMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			var copyClicks = 0;
			var flyout = CreateFlyout(out _);
			((MenuFlyoutItem)flyout[0]).Clicked += (_, _) => copyClicks++;
			FlyoutBase.SetContextFlyout(label, flyout);

			var previous = ContextFlyoutManager.UseTouchTrackerOverride;
			ContextFlyoutManager.UseTouchTrackerOverride = false;

			try
			{
				await AttachAndRun(label, async handler =>
				{
					var view = (AView)handler.PlatformView!;
					var manager = ContextFlyoutManager.Get(view)!;
					Assert.True(manager.UsesLongClickFallback);
					var decor = view.RootView!;
					var decorWasLongClickable = decor.LongClickable;

					// Android opens the menu itself when the long-click listener doesn't handle the long click
					Assert.True(view.PerformLongClick());
					await AssertHelpers.AssertEventually(() => manager.Menu is not null);

					var menu = manager.Menu!;
					Assert.Equal(new[] { "Copy", "Disabled", "Edit" }, GetTitles(menu));
					Assert.True(menu.PerformIdentifierAction(menu.GetItem(0)!.ItemId, 0));
					Assert.Equal(1, copyClicks);

					// The listener that added the items is only there while the menu is built
					Assert.Equal(decorWasLongClickable, decor.LongClickable);
					manager.Dismiss();
				});
			}
			finally
			{
				ContextFlyoutManager.UseTouchTrackerOverride = previous;
			}
		}

		static void WithTouchTracker(Action action)
		{
			var previous = ContextFlyoutManager.UseTouchTrackerOverride;
			ContextFlyoutManager.UseTouchTrackerOverride = true;

			try
			{
				action();
			}
			finally
			{
				ContextFlyoutManager.UseTouchTrackerOverride = previous;
			}
		}

		static async Task WithTouchTrackerAsync(Func<Task> action)
		{
			var previous = ContextFlyoutManager.UseTouchTrackerOverride;
			ContextFlyoutManager.UseTouchTrackerOverride = true;

			try
			{
				await action();
			}
			finally
			{
				ContextFlyoutManager.UseTouchTrackerOverride = previous;
			}
		}

		static (float X, float Y) GetCenterInWindow(AView view)
		{
			var location = new int[2];
			view.GetLocationInWindow(location);
			return (location[0] + view.Width / 2f, location[1] + view.Height / 2f);
		}

		static bool Dispatch(global::Android.Views.Window window, long downTime, MotionEventActions action, float x, float y,
			InputSourceType source = InputSourceType.Touchscreen, MotionEventButtonState buttonState = 0)
		{
			var properties = new[] { new MotionEvent.PointerProperties { Id = 0, ToolType = source == InputSourceType.Mouse ? MotionEventToolType.Mouse : MotionEventToolType.Finger } };
			var coords = new[] { new MotionEvent.PointerCoords { X = x, Y = y, Pressure = 1, Size = 1 } };
			using var e = MotionEvent.Obtain(downTime, SystemClock.UptimeMillis(), action, 1, properties, coords, 0, buttonState, 1, 1, 0, 0, source, 0)!;
			// Like the activity: the views get the event unless the tracker consumed it
			if (ContextFlyoutTouchTracker.OnDispatchTouchEvent(window, e))
				return true;

			window.SuperDispatchTouchEvent(e);
			return false;
		}
	}
}
