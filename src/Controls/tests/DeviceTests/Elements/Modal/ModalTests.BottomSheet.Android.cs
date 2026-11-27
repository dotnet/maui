#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Google.Android.Material.BottomSheet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests
	{
		static BottomSheetDialog? GetShowingSheetDialog(Window window) =>
			window.Handler?.MauiContext?.GetFragmentManager().Fragments
				.OfType<ModalNavigationManager.BottomSheetModalFragment>()
				.Select(fragment => fragment.Dialog)
				.OfType<BottomSheetDialog>()
				.LastOrDefault(dialog => dialog.IsShowing);

		[Fact]
		public async Task SheetRestoredWhileItsDismissalIsDeferredIsPresentedOnce()
		{
			SetupBuilder();
			var page = new ContentPage();
			var shell = new Shell { CurrentItem = page };
			var window = new Window(shell);
			var sheet = CreateBottomSheet(BottomSheetDetent.Medium);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);

					ShellNavigatingEventArgs? navigating = null;
					ShellNavigatingDeferral? deferral = null;
					shell.Navigating += (_, e) =>
					{
						if (deferral is null)
						{
							navigating = e;
							deferral = e.GetDeferral();
						}
					};

					await DismissBottomSheetAsUser(sheet);
					await AssertEventually(() => deferral is not null);

					// Resuming the app reconciles the stacks, which presents the sheet again while its dismissal waits.
					((IWindow)window).Deactivated();
					((IWindow)window).Activated();
					await AssertEventually(() => IsBottomSheetOnScreen(window));

					navigating!.Cancel();
					deferral!.Complete();
					await Task.Delay(500);

					Assert.Same(sheet, Assert.Single(page.Navigation.ModalStack));
					Assert.Equal(1, CountShowingSheetDialogs(window));

					await page.Navigation.PopModalAsync();
					await AssertEventually(() => CountShowingSheetDialogs(window) == 0);
				});
		}

		static int CountShowingSheetDialogs(Window window) =>
			window.Handler?.MauiContext?.GetFragmentManager().Fragments
				.OfType<ModalNavigationManager.BottomSheetModalFragment>()
				.Select(fragment => fragment.Dialog)
				.OfType<BottomSheetDialog>()
				.Count(dialog => dialog.IsShowing) ?? 0;

		static BottomSheetDialog RequireSheetDialog(Page sheet)
		{
			var dialog = GetShowingSheetDialog(sheet.Window);
			Assert.NotNull(dialog);
			return dialog!;
		}

		static void AssertPresentedAsBottomSheet(Page sheet)
		{
			Assert.True(BottomSheet.IsSupported);
			RequireSheetDialog(sheet);
		}

		static bool IsBottomSheetOnScreen(Window window) =>
			GetShowingSheetDialog(window) is not null;

		// Cancelling the dialog is what Material does when the user swipes the sheet away or taps outside it.
		static Task DismissBottomSheetAsUser(Page sheet)
		{
			RequireSheetDialog(sheet).Cancel();
			return Task.CompletedTask;
		}

		static async Task AssertNativeSheetState(Page sheet, int expectedStops, int expectedIndex, bool dismissible)
		{
			var behavior = RequireSheetDialog(sheet).Behavior;

			int expectedState = expectedStops switch
			{
				1 => BottomSheetBehavior.StateExpanded,
				2 => expectedIndex == 0 ? BottomSheetBehavior.StateCollapsed : BottomSheetBehavior.StateExpanded,
				_ => expectedIndex switch
				{
					0 => BottomSheetBehavior.StateCollapsed,
					1 => BottomSheetBehavior.StateHalfExpanded,
					_ => BottomSheetBehavior.StateExpanded,
				},
			};

			// Three resting positions need the half-expanded state, which Material only has when it doesn't fit to contents.
			await AssertEventually(() => behavior.FitToContents == (expectedStops < 3) && behavior.State == expectedState);
			Assert.Equal(expectedStops == 1, behavior.SkipCollapsed);
			Assert.Equal(dismissible, behavior.Hideable);

			var dialog = RequireSheetDialog(sheet);
			var handle = FindDescendant<BottomSheetDragHandleView>(dialog.Window?.DecorView);
			Assert.Equal(BottomSheet.GetHasDragHandle(sheet), handle?.Visibility == global::Android.Views.ViewStates.Visible);
		}

		// The user dragging the sheet to another detent: the behavior settles in another state without MAUI asking for it.
		static Task SelectDetentAsUser(Page sheet, int index)
		{
			var behavior = RequireSheetDialog(sheet).Behavior;
			behavior.State = index == 0 ? BottomSheetBehavior.StateCollapsed : BottomSheetBehavior.StateExpanded;
			return Task.CompletedTask;
		}

		static T? FindDescendant<T>(global::Android.Views.View? view) where T : global::Android.Views.View
		{
			if (view is T match)
				return match;

			if (view is global::Android.Views.ViewGroup group)
			{
				for (int i = 0; i < group.ChildCount; i++)
				{
					if (FindDescendant<T>(group.GetChildAt(i)) is T found)
						return found;
				}
			}

			return null;
		}
	}
}
