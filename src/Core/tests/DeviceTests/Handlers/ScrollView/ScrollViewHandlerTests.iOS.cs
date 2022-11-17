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

				foreach (var platformView in scrollViewHandler.PlatformView.Subviews)
				{
					// ScrollView on iOS uses an intermediate ContentView to handle content measurement/arrangement
					if (platformView is Microsoft.Maui.Platform.ContentView contentView)
					{
						foreach (var content in contentView.Subviews)
						{
							if (content is MauiTextField)
							{
								return true;
							}
						}
					}
				}

				return false; // No MauiTextField
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(MauiTextField)} in the Subviews array");
		}

		[Fact]
		public async Task ScrollViewContentSizeSet() 
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			var scrollView = new ScrollViewStub();
			var entry = new EntryStub() { Text = "In a ScrollView", Height = 10000 };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				return CreateHandler(scrollView);
			});

			await InvokeOnMainThreadAsync(async () => {
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					// Simulate a bunch of things that would happen if this were a real app
					scrollViewHandler.UpdateValue(nameof(IScrollView.Content));
					scrollViewHandler.PlatformArrange(new Rect(0, 0, 50, 50));
					scrollViewHandler.PlatformView.SetNeedsLayout();
					scrollViewHandler.PlatformView.LayoutIfNeeded();

					Assert.Equal(10000, scrollViewHandler.PlatformView.ContentSize.Height);
				});
			});
		}
	}
}
