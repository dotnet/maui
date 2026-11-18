#nullable enable
using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
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

				// Holding opens the menu; the rest of the touch is consumed so nothing gets clicked
				downTime = SystemClock.UptimeMillis();
				Assert.False(Dispatch(window, downTime, MotionEventActions.Down, x, y));
				await Task.Delay(ViewConfiguration.LongPressTimeout + 300);
				Assert.NotNull(manager.Popup);
				Assert.True(Dispatch(window, downTime, MotionEventActions.Up, x, y));

				manager.Dismiss();
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

		[Fact(DisplayName = "Secondary mouse click opens the context menu immediately")]
		public async Task SecondaryClickOpensContextMenu()
		{
			SetupBuilder();
			var label = new Label { Text = "Target", WidthRequest = 200, HeightRequest = 100 };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));

			await WithTouchTrackerAsync(() => AttachAndRun(label, handler =>
			{
				var view = (AView)handler.PlatformView!;
				var manager = ContextFlyoutManager.Get(view)!;
				var window = view.Context!.GetActivity()!.Window!;
				var (x, y) = GetCenterInWindow(view);
				var downTime = SystemClock.UptimeMillis();

				Assert.True(Dispatch(window, downTime, MotionEventActions.Down, x, y, InputSourceType.Mouse, MotionEventButtonState.Secondary));
				Assert.NotNull(manager.Popup);
				Assert.True(Dispatch(window, downTime, MotionEventActions.Up, x, y, InputSourceType.Mouse));

				manager.Dismiss();
			}));
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
			return ContextFlyoutTouchTracker.OnDispatchTouchEvent(window, e);
		}
	}
}
