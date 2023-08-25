using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WindowSoftInputModeAdjust = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ChangeModalStackWhileDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(modalPage);
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(new ContentPage());
					iWindow.Deactivated();
					await page.Navigation.PopModalAsync();
					await page.Navigation.PopModalAsync();
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}

		[Fact]
		public async Task DontPushModalPagesWhenWindowIsDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					iWindow.Deactivated();
					await page.Navigation.PushModalAsync(modalPage);
					Assert.False(modalPage.IsLoaded);
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}

		[Theory]
		[InlineData(WindowSoftInputModeAdjust.Resize)]
		[InlineData(WindowSoftInputModeAdjust.Pan)]
		public async Task ModalPageMarginCorrectAfterKeyboardOpens(WindowSoftInputModeAdjust panSize)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage);
			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					Entry testEntry = null;
					try
					{
						window.UpdateWindowSoftInputModeAdjust(panSize.ToPlatform());
						VerticalStackLayout layout = new VerticalStackLayout();
						List<Entry> entries = new List<Entry>();
						ContentPage modalPage = new ContentPage()
						{
							Content = layout
						};

						// Add enough entries into the stack layout so that we can
						// guarantee we'll have entries that would be covered by the keyboard
						for (int i = 0; i < 30; i++)
						{
							var entry = new Entry();

							if (i == 0)
							{
								// This just lets us visually verify where
								// the first entry is located
								entry.Text = "First Entry";
							}

							entries.Add(entry);
							layout.Add(entry);
						}

						await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
						await OnNavigatedToAsync(modalPage);

						// Locate the lowest visible entry
						var pageBoundingBox = modalPage.GetBoundingBox();
						testEntry = entries[0];

						// Ensure that the keyboard is closed before we start
						await AssertionExtensions.HideKeyboardForView(testEntry, message: "Ensure that the keyboard is closed before we start");

						foreach (var entry in entries)
						{
							var entryLocation = entry.GetLocationOnScreen();
							var entryBox = entry.GetBoundingBox();

							// Locate the lowest visible entry
							if ((entryLocation.Value.Y + (entryBox.Height * 2)) > pageBoundingBox.Height)
								break;

							testEntry = entry;
						}

						// determine the screen dimensions with no keyboard open
						var rootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
						var modalOffsetY = modalPage.GetLocationOnScreen().Value.Y;
						var originalModalPageSize = modalPage.GetBoundingBox();

						await AssertionExtensions.ShowKeyboardForView(testEntry, message: "Show keyboard for entry");

						// Type text into the entries
						testEntry.Text = "Typing";

						// Wait for the size of the screen to settle after the keyboard has opened
						bool offsetMatchesWhenKeyboardOpened = await AssertionExtensions.Wait(() =>
						{
							var keyboardOpenRootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
							var keyboardOpenModalOffsetY = modalPage.GetLocationOnScreen().Value.Y;

							var originalDiff = Math.Abs(rootPageOffsetY - modalOffsetY);
							var openDiff = Math.Abs(keyboardOpenRootPageOffsetY - keyboardOpenModalOffsetY);

							return Math.Abs(originalDiff - openDiff) <= 0.2;
						});

						Assert.True(offsetMatchesWhenKeyboardOpened, "Modal page has an invalid offset when open");

						foreach (var entry in entries)
						{
							var entryBox = entry.GetLocationOnScreen();

							if (entryBox.Value.Y > 0)
							{
								await AssertionExtensions.HideKeyboardForView(testEntry, message: "Close Keyboard to see if sizes adjust back");
								break;
							}
						}

						// Wait for the size of the screen to settle after the keyboard has closed
						bool offsetMatchesWhenKeyboardClosed = await AssertionExtensions.Wait(() =>
						{
							var keyboardClosedRootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
							var keyboardClosedModalOffsetY = modalPage.GetLocationOnScreen().Value.Y;

							return rootPageOffsetY == keyboardClosedRootPageOffsetY &&
									modalOffsetY == keyboardClosedModalOffsetY;
						});

						Assert.True(offsetMatchesWhenKeyboardClosed, "Modal page failed to return to expected offset");

						// Make sure that everything has returned to the initial size once the keyboard has closed
						var finalModalPageSize = modalPage.GetBoundingBox();
						Assert.Equal(originalModalPageSize, finalModalPageSize);
					}
					finally
					{
						window.UpdateWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize.ToPlatform());

						if (testEntry?.Handler is IPlatformViewHandler testEntryHandler)
							testEntryHandler.PlatformView.HideSoftInput();
					}
				});
		}
	}
}
