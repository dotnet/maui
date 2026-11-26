using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		static ContentPage CreateBottomSheet(params BottomSheetDetent[] detents)
		{
			var sheet = new ContentPage { Content = new Label { Text = "Bottom sheet" } };
			BottomSheet.SetIsEnabled(sheet, true);
			if (detents.Length > 0)
				BottomSheet.SetDetents(sheet, detents);
			return sheet;
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BottomSheetPushAndPopKeepsModalStackInSync(bool useShell)
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(useShell ? new Shell { CurrentItem = page } : page);
			var sheet = CreateBottomSheet(BottomSheetDetent.Medium, BottomSheetDetent.Large);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);

					Assert.Same(sheet, Assert.Single(page.Navigation.ModalStack));
					AssertPresentedAsBottomSheet(sheet);

					await page.Navigation.PopModalAsync();
					await OnUnloadedAsync(sheet.Content);

					Assert.Empty(page.Navigation.ModalStack);
				});
		}

		[Fact]
		public async Task RapidBottomSheetPushAndPopLeavesNoSheetBehind()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					for (int i = 0; i < 3; i++)
					{
						var sheet = CreateBottomSheet(BottomSheetDetent.Medium);
						await page.Navigation.PushModalAsync(sheet, animated: i % 2 == 0);
						await page.Navigation.PopModalAsync(animated: i % 2 == 1);
					}

					Assert.Empty(page.Navigation.ModalStack);
					await AssertEventually(() => !IsBottomSheetOnScreen(window));

					// A sheet still presents after the rapid sequence.
					var last = CreateBottomSheet(BottomSheetDetent.Medium);
					await page.Navigation.PushModalAsync(last);
					await OnLoadedAsync(last.Content);
					AssertPresentedAsBottomSheet(last);
					await page.Navigation.PopModalAsync();
				});
		}

#if ANDROID || IOS || MACCATALYST
		[Fact]
		public async Task UserDismissalOfBottomSheetPopsItAndNextSheetPresents()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);
			var sheet = CreateBottomSheet(BottomSheetDetent.Medium, BottomSheetDetent.Large);
			int popped = 0;
			window.ModalPopped += (_, e) => { if (e.Modal == sheet) popped++; };

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);

					await DismissBottomSheetAsUser(sheet);

					await AssertEventually(() => page.Navigation.ModalStack.Count == 0);
					Assert.Equal(1, popped);

					// The platform modal stack is in sync: the next modal presents.
					var next = CreateBottomSheet(BottomSheetDetent.Large);
					await page.Navigation.PushModalAsync(next);
					await OnLoadedAsync(next.Content);
					AssertPresentedAsBottomSheet(next);
					await page.Navigation.PopModalAsync();
				});
		}

		[Fact]
		public async Task BottomSheetPropertiesMapToTheNativeSheet()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);
			var sheet = CreateBottomSheet(BottomSheetDetent.Fraction(0.25), BottomSheetDetent.Medium, BottomSheetDetent.Large);
			BottomSheet.SetIsDismissible(sheet, false);
			BottomSheet.SetHasDragHandle(sheet, false);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);

					await AssertNativeSheetState(sheet, expectedStops: 3, expectedIndex: 0, dismissible: false);

					BottomSheet.SetSelectedDetent(sheet, BottomSheetDetent.Large);
					await AssertNativeSheetState(sheet, expectedStops: 3, expectedIndex: 2, dismissible: false);

					// A programmatic selection is not echoed back as a different value.
					Assert.Equal(BottomSheetDetent.Large, BottomSheet.GetSelectedDetent(sheet));

					BottomSheet.SetIsDismissible(sheet, true);
					BottomSheet.SetHasDragHandle(sheet, true);
					BottomSheet.SetDetents(sheet, new[] { BottomSheetDetent.Medium, BottomSheetDetent.Large });
					await AssertNativeSheetState(sheet, expectedStops: 2, expectedIndex: 1, dismissible: true);

					await page.Navigation.PopModalAsync();
					Assert.Empty(page.Navigation.ModalStack);
				});
		}

		[Fact]
		public async Task UserDetentChangeUpdatesSelectedDetent()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);
			var sheet = CreateBottomSheet(BottomSheetDetent.Medium, BottomSheetDetent.Large);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);
					Assert.Null(BottomSheet.GetSelectedDetent(sheet));

					await SelectDetentAsUser(sheet, index: 1);
					await AssertEventually(() => BottomSheet.GetSelectedDetent(sheet) == BottomSheetDetent.Large);

					await SelectDetentAsUser(sheet, index: 0);
					await AssertEventually(() => BottomSheet.GetSelectedDetent(sheet) == BottomSheetDetent.Medium);

					await page.Navigation.PopModalAsync();
				});
		}

		[Fact]
		public async Task BackButtonOnlyDismissesDismissibleSheet()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);
			var sheet = CreateBottomSheet(BottomSheetDetent.Medium);
			BottomSheet.SetIsDismissible(sheet, false);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async _ =>
				{
					await page.Navigation.PushModalAsync(sheet);
					await OnLoadedAsync(sheet.Content);

					Assert.True(((IWindow)window).BackButtonClicked());
					await Task.Delay(100);
					Assert.Same(sheet, Assert.Single(page.Navigation.ModalStack));

					BottomSheet.SetIsDismissible(sheet, true);
					Assert.True(((IWindow)window).BackButtonClicked());
					await AssertEventually(() => page.Navigation.ModalStack.Count == 0);
				});
		}
#endif

#if WINDOWS
		static void AssertPresentedAsBottomSheet(Page sheet) =>
			Assert.NotNull(sheet.Handler);

		static bool IsBottomSheetOnScreen(Window window) => false;
#endif
	}
}
