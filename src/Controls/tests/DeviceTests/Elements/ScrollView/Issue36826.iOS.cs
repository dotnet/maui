using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue36826 : ControlsHandlerTestBase
	{
		const double MinimumKeyboardRangeIncrease = 200;

		[Fact]
		public async Task SoftInputIncreasesBottomScrollRange()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
				});
			});

			var entry = new Entry
			{
				HeightRequest = 56,
				Placeholder = "Tap to raise keyboard",
			};

			var content = new VerticalStackLayout
			{
				Spacing = 12,
				Children =
				{
					new Label { HeightRequest = 1200, Text = "Scrollable content" },
					entry,
					new Label { HeightRequest = 72, Text = "Trailing content" },
				},
			};

			var scrollView = new ScrollView
			{
				Content = content,
				SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput),
			};

			var page = new ContentPage
			{
				Content = scrollView,
				SafeAreaEdges = SafeAreaEdges.None,
			};

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				var nativeScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				var nativeEntry = ((EntryHandler)entry.Handler).PlatformView;
				var baselineRange = Math.Max(0, nativeScrollView.ContentSize.Height - nativeScrollView.Bounds.Height);
				var keyboardDidShow = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				NSObject keyboardObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIKeyboard.DidShowNotification,
					_ => keyboardDidShow.TrySetResult());

				try
				{
					nativeScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, baselineRange), false);
					nativeEntry.BecomeFirstResponder();
					await keyboardDidShow.Task;

					nativeScrollView.Superview?.LayoutIfNeeded();
					nativeScrollView.LayoutIfNeeded();

					var keyboardVisibleRange = Math.Max(0, nativeScrollView.ContentSize.Height - nativeScrollView.Bounds.Height);
					Assert.True(
						keyboardVisibleRange - baselineRange >= MinimumKeyboardRangeIncrease,
						"SoftInput should increase the ScrollView bottom scroll range when the keyboard appears.");
				}
				finally
				{
					nativeEntry.ResignFirstResponder();
					NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardObserver);
				}
			});
		}
	}
}
