using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;


#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
#endif


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.FlyoutPage)]
	public partial class FlyoutPageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.Label), typeof(LabelHandler));
					handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
#if IOS || MACCATALYST
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
#else
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#endif
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Frame, FrameRenderer>();
					handlers.AddHandler<Controls.Window, WindowHandlerStub>();
				});
			});
		}

		[Fact]
		public async Task PoppingFlyoutPageDoesntCrash()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var flyoutPage = new FlyoutPage()
				{
					Detail = new NavigationPage(new ContentPage() { Content = new Frame(), Title = "Detail" }),
					Flyout = new ContentPage() { Title = "Flyout" }
				};
				await navPage.PushAsync(flyoutPage);
				await navPage.PopAsync();
			});
		}


#if !IOS && !MACCATALYST

		[Fact(DisplayName = "FlyoutPage With Toolbar")]
		public async Task FlyoutPageWithToolbar()
		{
			SetupBuilder();
			var flyoutPage = new FlyoutPage()
			{
				Detail = new NavigationPage(new ContentPage() { Title = "Detail" }),
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, (handler) =>
			{
				// validate that nothing crashes

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Details View Updates w/NavigationPage")]
		public async Task DetailsViewUpdatesWithNavigationPage()
		{
			SetupBuilder();
			var flyoutPage = new FlyoutPage()
			{
				Detail = new NavigationPage(new ContentPage() { Title = "Detail" }),
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var details2 = new NavigationPage(new ContentPage() { Title = "Detail" });

				flyoutPage.Detail = details2;
				await OnLoadedAsync(details2.CurrentPage);
				var detailView2 = (details2.CurrentPage.Handler as IPlatformViewHandler)?.PlatformView;
				Assert.NotNull(detailView2);
			});
		}

		[Fact(DisplayName = "Details View Updates")]
		public async Task DetailsViewUpdates()
		{
			SetupBuilder();
			var flyoutPage = new FlyoutPage()
			{
				Detail = new ContentPage() { Title = "Detail" },
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var details2 = new ContentPage() { Title = "Detail" };
				var flyoutView = flyoutPage.ToPlatform();
				var detailView = flyoutPage.Detail.ToPlatform();
				var dl = FindPlatformFlyoutView(detailView);
				Assert.Equal(flyoutView, dl);

				flyoutPage.Detail = details2;

				await OnLoadedAsync(details2);
				await detailView.OnUnloadedAsync();
				dl = FindPlatformFlyoutView(details2.ToPlatform());
				Assert.Equal(flyoutView, dl);
				Assert.Null(FindPlatformFlyoutView(detailView));
			});
		}
#endif
	}
}
