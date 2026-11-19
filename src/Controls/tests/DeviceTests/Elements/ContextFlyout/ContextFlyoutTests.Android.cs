#nullable enable
using System;
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

				var menu = manager.Popup!.Menu;
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
				var menu = manager.Popup!.Menu;
				menu.PerformIdentifierAction(menu.GetItem(0)!.ItemId, 0);
				Assert.Equal("parameter", executedParameter);

				Assert.True(manager.Show());
				menu = manager.Popup!.Menu;
				menu.PerformIdentifierAction(menu.GetItem(1)!.ItemId, 0);
				Assert.False(disabledExecuted);

				Assert.True(manager.Show());
				menu = manager.Popup!.Menu;
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
				Assert.Equal("Before", manager.Popup!.Menu.GetItem(3)!.TitleFormatted?.ToString());

				viewModel.Title = "After";
				nested.Text = "Nested renamed";
				flyout.RemoveAt(0);

				Assert.True(manager.Show());
				var menu = manager.Popup!.Menu;
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
				Assert.NotNull(manager.Popup);
				manager.Dismiss();
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
				Assert.Null(manager.Popup);

				// Moving beyond the touch slop (scrolling) cancels the long press
				downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, x, y));
				Assert.False(Dispatch(window, downTime, MotionEventActions.Move, x, y + 200));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Null(manager.Popup);
				Dispatch(window, downTime, MotionEventActions.Up, x, y + 200);

				// Holding opens the menu where the view was pressed; the rest of the touch is consumed so nothing gets clicked
				var (pressX, pressY) = (x - 40, y + 10);
				downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, pressX, pressY));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Popup);
				AssertAnchoredAt(manager, pressX, pressY);
				Assert.True(Dispatch(window, downTime, MotionEventActions.Up, pressX, pressY));

				manager.Dismiss();
				Assert.Null(manager.Anchor?.Parent);
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

				Assert.NotNull(topManager.Popup);
				Assert.Null(bottomManager.Popup);
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

				Assert.NotNull(parentManager.Popup);
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
			{ "Entry", true },
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
			var pressed = behavior == "Entry" ? (Microsoft.Maui.Controls.View)new Entry { Text = "Text" } : new Label { Text = "Target" };
			pressed.WidthRequest = 200;
			pressed.HeightRequest = 100;
			var root = new Grid { Children = { pressed } };
			var withMenu = behavior is "ChildLongPressGesture" or "ReorderingWithMenu" or "ElevatedLongPressGesture" or "DisabledChildWithMenu" or "DisabledChildLongPressGesture"
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
				Assert.Equal(menuOpensOnLongPress, manager.Popup is not null);

				// Only one of them happens: the gesture doesn't fire after the menu opened, even on a parent
				if (behavior.EndsWith("LongPressGesture", StringComparison.Ordinal))
				{
					await Task.Delay(ViewConfiguration.LongPressTimeout);
					Assert.Equal(menuOpensOnLongPress ? 0 : 1, longPresses);
				}

				ContextFlyoutManager.SetInterceptsLongPress(rootView, false);
				Dispatch(window, downTime, MotionEventActions.Up, x, y);
				manager.Dismiss();

				// The accessibility long-click action acts on the view with the menu itself
				var sameViewHandler = behavior is "LongPressGesture" or "DragGesture";
				menuView.PerformAccessibilityAction(global::Android.Views.Accessibility.Action.LongClick, null);
				Assert.Equal(!sameViewHandler, manager.Popup is not null);
				manager.Dismiss();

				// A secondary click always opens the context menu
				downTime = SystemClock.UptimeMillis();
				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse);
				Assert.NotNull(manager.Popup);
				manager.Dismiss();
			}));
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
				SendKey(view, KeyEventActions.Down);
				SendKey(view, KeyEventActions.Up);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Equal(1, clicks);
				Assert.Null(manager.Popup);

				// Modifiers (e.g. Shift+Enter) don't open the menu
				SendKey(view, KeyEventActions.Down, MetaKeyStates.ShiftOn | MetaKeyStates.ShiftLeftOn);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.Null(manager.Popup);
				SendKey(view, KeyEventActions.Up, MetaKeyStates.ShiftOn | MetaKeyStates.ShiftLeftOn);

				// Holding opens the menu at the center of the view
				SendKey(view, KeyEventActions.Down);
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Popup);
				var (x, y) = GetCenterInWindow(view);
				AssertAnchoredAt(manager, x, y);

				// Releasing the key after the menu opened doesn't click, like a native long click
				SendKey(view, KeyEventActions.Up);
				Assert.Equal(1, clicks);
				Assert.False(view.Pressed);

				manager.Dismiss();
				view.FocusableInTouchMode = false;
			}));

			static void SendKey(AView view, KeyEventActions action, MetaKeyStates meta = 0)
			{
				var now = SystemClock.UptimeMillis();
				using var e = new KeyEvent(now, now, action, Keycode.Enter, 0, meta);
				view.DispatchKeyEvent(e);
			}
		}

		[Fact(DisplayName = "Items without an icon are laid out like a native popup menu")]
		public async Task ItemsWithoutIconHaveNoPlaceholder()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			FlyoutBase.SetContextFlyout(label, new MenuFlyout
			{
				new MenuFlyoutItem { Text = "With icon", IconImageSource = new FontImageSource { Glyph = "A", Color = Colors.Blue } },
				new MenuFlyoutItem { Text = "Without icon" },
			});

			await WithTouchTrackerAsync(() => AttachAndRun(label, async handler =>
			{
				var manager = ContextFlyoutManager.Get((AView)handler.PlatformView!)!;
				Assert.True(manager.Show());

				await AssertHelpers.AssertEventually(() => manager.Popup!.Menu.GetItem(0)!.Icon is not null);
				Assert.Null(manager.Popup!.Menu.GetItem(1)!.Icon);

				manager.Dismiss();
			}));
		}

		static void AssertAnchoredAt(ContextFlyoutManager manager, float x, float y)
		{
			var anchor = Assert.IsAssignableFrom<AView>(manager.Anchor);
			Assert.NotNull(anchor.Parent);
			var location = new int[2];
			anchor.GetLocationInWindow(location);
			Assert.Equal((int)x, location[0]);
			Assert.Equal((int)y, location[1]);
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
