#if ANDROID || IOS || MACCATALYST || WINDOWS
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	// TabbedPage installs its platform view factory lazily, the first time a TabbedPage is attached to a handler.
	// The install is a one-time action per process, so these tests also call InstallPlatformViewFactory directly:
	// other tests may already have attached a TabbedPage.
	[Category(TestCategory.TabbedPage)]
	public class TabbedPagePlatformViewFactoryTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
				});
			});
		}

		static TabbedPage CreateTabbedPage() => new()
		{
			Children =
			{
				new ContentPage { Title = "Page 1" },
				new ContentPage { Title = "Page 2" },
			}
		};

		[Fact(DisplayName = "Controls Installs Its Platform View Factory When The App Has None")]
		public async Task ControlsInstallsItsPlatformViewFactoryWhenTheAppHasNone()
		{
			// Restoring to null would leave the factory unset if this test performed the one-time remap.
			TabbedPage.InstallPlatformViewFactory();
			var originalFactory = TabbedViewHandler.PlatformViewFactory;
			try
			{
				TabbedViewHandler.PlatformViewFactory = null;

				TabbedPage.InstallPlatformViewFactory();

				Assert.NotNull(TabbedViewHandler.PlatformViewFactory);

				// The Core TabbedViewHandler throws from CreatePlatformView, so attaching only succeeds through
				// the Controls factory.
				SetupBuilder();
				var tabbedPage = CreateTabbedPage();
				await CreateHandlerAndAddToWindow<IElementHandler>(tabbedPage, handler =>
				{
					Assert.NotNull(handler.PlatformView);
					return Task.CompletedTask;
				});
			}
			finally
			{
				TabbedViewHandler.PlatformViewFactory = originalFactory;
			}
		}

		[Fact(DisplayName = "Controls Keeps A Platform View Factory The App Installed Before Attaching")]
		public async Task ControlsKeepsAPlatformViewFactoryTheAppInstalledBeforeAttaching()
		{
			// Restoring to null would leave the factory unset if this test performed the one-time remap.
			TabbedPage.InstallPlatformViewFactory();
			var originalFactory = TabbedViewHandler.PlatformViewFactory;
			try
			{
				TabbedViewHandler.PlatformViewFactory = null;
				TabbedPage.InstallPlatformViewFactory();
				var controlsFactory = TabbedViewHandler.PlatformViewFactory;

				var appFactoryCalls = 0;
				object appPlatformView = null;
				TabbedViewHandler.PlatformViewFactory = handler =>
				{
					appFactoryCalls++;
					var platformView = controlsFactory(handler);
					appPlatformView = platformView;
					return platformView;
				};
				var appFactory = TabbedViewHandler.PlatformViewFactory;

				TabbedPage.InstallPlatformViewFactory();

				Assert.Same(appFactory, TabbedViewHandler.PlatformViewFactory);

				SetupBuilder();
				var tabbedPage = CreateTabbedPage();
				await CreateHandlerAndAddToWindow<IElementHandler>(tabbedPage, _ =>
				{
					Assert.True(ReferenceEquals(appFactory, TabbedViewHandler.PlatformViewFactory), "The app factory was replaced.");
					Assert.Equal(1, appFactoryCalls);
					Assert.True(ReferenceEquals(appPlatformView, tabbedPage.Handler.PlatformView), "The handler does not use the app factory's platform view.");
					return Task.CompletedTask;
				});
			}
			finally
			{
				TabbedViewHandler.PlatformViewFactory = originalFactory;
			}
		}
	}
}
#endif
