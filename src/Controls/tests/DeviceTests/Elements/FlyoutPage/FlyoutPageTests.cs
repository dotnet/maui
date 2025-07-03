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
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Controls.Window, WindowHandlerStub>();
				});
			});
		}

		[Theory]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task SwappingDetailPageWorksForSplitFlyoutBehavior(Type flyoutPageType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var flyoutPage = CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Content = new Border(), Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

				await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(flyoutPage), async (handler) =>
				{
					var currentDetailPage = flyoutPage.Detail;

					// Set with new page
					var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };
					flyoutPage.Detail = navPage;
					await OnNavigatedToAsync(navPage);

					// Set back to previous page
					flyoutPage.Detail = currentDetailPage;
					await OnNavigatedToAsync(currentDetailPage);
				});
			});
		}


		[Theory(DisplayName = "FlyoutPage With Toolbar")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task FlyoutPageWithToolbar(Type flyoutPageType)
		{
			SetupBuilder();

			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, (handler) =>
			{
				// validate that nothing crashes

				return Task.CompletedTask;
			});
		}

		[Theory(DisplayName = "Details View Updates w/NavigationPage")]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task DetailsViewUpdatesWithNavigationPage(Type flyoutPageType)
		{
			SetupBuilder();

			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new NavigationPage(new ContentPage() { Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var details2 = new NavigationPage(new ContentPage() { Title = "Detail" });

				flyoutPage.Detail = details2;
				await OnLoadedAsync(details2.CurrentPage);
				var detailView2 = (details2.CurrentPage.Handler as IPlatformViewHandler)?.PlatformView;
				Assert.NotNull(detailView2);
			});
		}

		[Theory(DisplayName = "Details View Updates"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
		)]
		[ClassData(typeof(FlyoutPageLayoutBehaviorTestCases))]
		public async Task DetailsViewUpdates(Type flyoutPageType)
		{
			SetupBuilder();
			var flyoutPage =
				CreateFlyoutPage(
					flyoutPageType,
					new ContentPage() { Title = "Detail" },
					new ContentPage() { Title = "Flyout" });

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

		FlyoutPage CreateFlyoutPage(Type type, Page detail, Page flyout)
		{
			var flyoutPage = (FlyoutPage)Activator.CreateInstance(type);
			flyoutPage.Detail = detail;
			flyoutPage.Flyout = flyout;
			return flyoutPage;
		}

		[Theory]
		[InlineData(false
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		[InlineData(true)]
		public async Task DetailsPageMeasuresCorrectlyInSplitMode(bool isRtl)
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label()
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLabel
				},
				FlowDirection = (isRtl) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				if (!CanDeviceDoSplitMode(flyoutPage))
					return;

				await AssertEventually(() => flyoutPage.Flyout.GetBoundingBox().Width > 0);

				var detailBounds = flyoutPage.Detail.GetBoundingBox();
				var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();
				var windowBounds = flyoutPage.GetBoundingBox();

				Assert.True(detailBounds.Height <= windowBounds.Height, $"Details is measuring too high. Details - {detailBounds} Window - {windowBounds}");
				Assert.True(flyoutBounds.Height <= windowBounds.Height, $"Flyout is measuring too high Flyout - {flyoutBounds} Window - {windowBounds}");
				Assert.True(flyoutBounds.Width + detailBounds.Width <= windowBounds.Width,
					$"Flyout and Details width exceed the width of the window. Details - {detailBounds}  Flyout - {flyoutBounds} Window - {windowBounds}");

				Assert.True(detailBounds.X + detailBounds.Width <= windowBounds.Width,
					$"Right edge of Details View is off the screen. Details - {detailBounds} Window - {windowBounds}");

				if (isRtl)
				{
					Assert.Equal(flyoutBounds.X, detailBounds.Width);
				}
				else
				{
					Assert.Equal(flyoutBounds.Width, detailBounds.X);
				}

				Assert.Equal(detailBounds.Width, windowBounds.Width - flyoutBounds.Width);
			});
		}

		[Fact(DisplayName = "Back Button Enabled Changes with push/pop + page change")]
		public async Task BackButtonEnabledChangesWithPushPopAndPageChanges()
		{
			SetupBuilder();

			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Flyout = new ContentPage() { Title = "Hello world" }
			});

			var first = new NavigationPage(new ContentPage());
			var second = new NavigationPage(new ContentPage());

			flyoutPage.Detail = first;

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler));

				await first.PushAsync(new ContentPage());
				await AssertEventually(() => IsBackButtonVisible(handler));
				Assert.True(IsBackButtonVisible(handler));

				flyoutPage.Detail = second;
				Assert.False(IsBackButtonVisible(handler));

				await second.PushAsync(new ContentPage());
				await AssertEventually(() => IsBackButtonVisible(handler));
				Assert.True(IsBackButtonVisible(handler));
			});
		}

		[Fact(DisplayName = "FlyoutPage as Modal Does Not Leak")]
		public async Task DoesNotLeakAsModal()
		{
			SetupBuilder();

			var references = new List<WeakReference>();
			var launcherPage = new ContentPage();
			var window = new Window(launcherPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var flyoutPage = new FlyoutPage
				{
					Flyout = new ContentPage
					{
						Title = "Flyout",
						IconImageSource = "icon.png"
					},
					Detail = new ContentPage { Title = "Detail" }
				};

				await launcherPage.Navigation.PushModalAsync(flyoutPage, true);

				references.Add(new WeakReference(flyoutPage));
				references.Add(new WeakReference(flyoutPage.Flyout));
				references.Add(new WeakReference(flyoutPage.Detail));

				await launcherPage.Navigation.PopModalAsync();
			});

			await AssertionExtensions.WaitForGC(references.ToArray());
		}

		bool CanDeviceDoSplitMode(FlyoutPage page)
		{
			return ((IFlyoutPageController)page).ShouldShowSplitMode;
		}
	}
}
