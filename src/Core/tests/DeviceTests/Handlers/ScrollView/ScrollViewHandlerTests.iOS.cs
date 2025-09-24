using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests : CoreHandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		[Theory]
		[InlineData(ScrollOrientation.Horizontal, true, false)]
		[InlineData(ScrollOrientation.Vertical, false, true)]
		[InlineData(ScrollOrientation.Both, true, true)]
		[InlineData(ScrollOrientation.Neither, false, false)]
		public async Task ScrollViewOrientationSetsBounceBehaviorCorrectly(ScrollOrientation orientation, bool expectedHorizontalBounce, bool expectedVerticalBounce)
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });
					
			var scrollView = new ScrollViewStub()
			{
				Orientation = orientation,
				Content = new EntryStub
				{
					Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
				} // Add some content
			};
			
			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(scrollView);

				return handler;
			});
			
			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					Assert.Equal(expectedHorizontalBounce, scrollViewHandler.PlatformView.AlwaysBounceHorizontal);
					Assert.Equal(expectedVerticalBounce, scrollViewHandler.PlatformView.AlwaysBounceVertical);
					
					// ScrollEnabled should be false only for Neither orientation
					Assert.Equal(orientation != ScrollOrientation.Neither, scrollViewHandler.PlatformView.ScrollEnabled);
				});
			});
		}
		
		[Fact]
		public async Task ContentInitializesCorrectly()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var entry = new EntryStub() { Text = "In a ScrollView" };

				var scrollView = new ScrollViewStub()
				{
					Content = entry
				};

				var scrollViewHandler = CreateHandler(scrollView);
				return scrollViewHandler.PlatformView.FindDescendantView<MauiTextField>() is not null;
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(MauiTextField)} in the Subviews array");
		}

		[Fact]
		public async Task ScrollViewContentSizeSet()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			var scrollView = new ScrollViewStub();
			var entry = new EntryStub() { Text = "In a ScrollView" };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(scrollView);

				// Setting an arbitrary value so we can verify that the handler is setting
				// the UIScrollView's ContentSize property during AttachAndRun
				handler.PlatformView.ContentSize = new CoreGraphics.CGSize(100, 100);
				return handler;
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					// Verify that the ContentSize values have been modified
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Height);
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Width);
				});
			});
		}
	}
}
