using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Page)]
	public partial class PageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
				});
			});
		}

		[Fact("SafeAreaInset Property is Set")]
		public async Task SafeAreaInsetIsSet()
		{
			SetupBuilder();

			var page = new ContentPage { Background = Colors.Blue };

			var shell = new Shell() { CurrentItem = page };

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, (handler) =>
			{
				if (handler.VirtualView is Window window && window.Page is Shell shellPage)
				{
					Assert.NotEqual(shellPage.CurrentPage.On<iOS>().SafeAreaInsets(), Thickness.Zero);
				}
			});
		}

		[Theory("SafeArea behavior works with both old and new APIs")]
		[InlineData(true)] // Use new SafeArea API
		[InlineData(false)] // Use old UseSafeArea API
		public async Task SafeAreaBehaviorConsistency(bool useNewApi)
		{
			SetupBuilder();

			var page = new ContentPage { Background = Colors.Blue };

			if (useNewApi)
			{
				page.SafeAreaEdges = SafeAreaEdges.All;
			}
			else
			{
#pragma warning disable CS0618 // Type or member is obsolete
				page.On<iOS>().SetUseSafeArea(true);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			var shell = new Shell() { CurrentItem = page };

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, (handler) =>
			{
				if (handler.VirtualView is Window window && window.Page is Shell shellPage)
				{
					// Both APIs should result in safe area being respected (non-zero insets)
					Assert.NotEqual(shellPage.CurrentPage.On<iOS>().SafeAreaInsets(), Thickness.Zero);
				}
			});
		}

		//src/Compatibility/Core/tests/iOS/EmbeddingTests.cs
		[Fact(DisplayName = "Can Create Platform View From ContentPage")]
		public async Task CanCreateViewControllerFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			await contentPage.Dispatcher.DispatchAsync(async () =>
			{
				var handler = CreateHandler<PageHandler>(contentPage);
				var mauiContext = handler.MauiContext;

				await contentPage.Dispatcher.DispatchAsync(() =>
				{
					UIViewController viewController = contentPage.ToUIViewController(mauiContext);
					Assert.NotNull(viewController);
				});
			});
		}
	}
}
