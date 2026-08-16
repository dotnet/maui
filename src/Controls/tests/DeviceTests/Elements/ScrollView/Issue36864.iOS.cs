using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue36864 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task OpeningKeyboardDoesNotChangeScrollViewOffset()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
				});
			});

			var entry = new Entry
			{
				Placeholder = "Keyboard target"
			};

			var bottomLabel = new Label
			{
				HeightRequest = 44,
				Text = "BOTTOM LABEL"
			};

			var scrollContent = new Grid
			{
				HeightRequest = 650,
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto)
				}
			};
			scrollContent.Add(new Label { Text = "ScrollView content" });
			scrollContent.Add(entry, 0, 2);
			scrollContent.Add(bottomLabel, 0, 3);

			var scrollView = new ScrollView
			{
				SafeAreaEdges = SafeAreaEdges.Container,
				Content = scrollContent
			};

			var pageContent = new Grid
			{
				Padding = new Thickness(16, 12),
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star)
				}
			};
			pageContent.Add(new Label
			{
				HeightRequest = 120,
				Text = "Issue 36864 ScrollView keyboard reproduction"
			});
			pageContent.Add(scrollView, 0, 1);

			var page = new ContentPage { Content = pageContent };

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				await AssertEventually(
					() => scrollView.Height > 0 && scrollView.ContentSize.Height > scrollView.Height,
					timeout: 5000,
					message: "ScrollView did not complete its initial layout");

				var platformScrollView = (UIScrollView)scrollView.ToPlatform();
				var platformEntry = entry.ToPlatform();
				var baselineOffset = platformScrollView.ContentOffset.Y;
				var keyboardShown = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

				using (UIKeyboard.Notifications.ObserveDidShow((_, _) => keyboardShown.TrySetResult()))
				{
					Assert.True(platformEntry.BecomeFirstResponder(), "Entry did not become first responder");
					await keyboardShown.Task.WaitAsync(TimeSpan.FromSeconds(5));
				}

				var offsetAfterKeyboardOpened = platformScrollView.ContentOffset.Y;

				platformEntry.ResignFirstResponder();
				await platformEntry.WaitForKeyboardToHide(5000);

				Assert.True(
					Math.Abs(offsetAfterKeyboardOpened - baselineOffset) <= 2,
					$"ScrollView offset changed after keyboard opened: baseline {baselineOffset:F1}, actual {offsetAfterKeyboardOpened:F1}");
			});
		}
	}
}
