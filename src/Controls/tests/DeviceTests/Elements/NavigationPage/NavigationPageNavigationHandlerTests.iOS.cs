using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every NavigationPageTests fact/theory under NavigationViewHandler instead of the
	/// base class's NavigationRenderer. Same pattern as ShellHandlerSubclasses.Android.cs.
	/// TabbedPage stays on TabbedRenderer here; its handler migration is out of scope.
	/// </summary>
	[Category(TestCategory.NavigationPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class NavigationPageNavigationHandlerTests : NavigationPageTests
	{
		protected override void RegisterNavigationPageHandlers(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
			handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
			handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
		}

		[Fact]
		public Task ParentingControllerStopsForwardingAfterPageHandlerDisconnects()
		{
			SetupBuilder();

			return InvokeOnMainThreadAsync(() =>
			{
				var page = new ContentPage();
				// Keep the controller off-window so UIKit cannot follow a self-reference.
				using var parentingController = NavigationHandlerParentingViewController.CreateForPage(page, MauiContext);
				var pageController = Assert.IsAssignableFrom<UIViewController>((page.Handler as IPlatformViewHandler)?.ViewController);

				Assert.Same(pageController, parentingController.ChildViewControllerForStatusBarHidden());
				Assert.Same(pageController, parentingController.ChildViewControllerForHomeIndicatorAutoHidden);

				page.DisconnectHandlers();

				Assert.Null(parentingController.ChildViewControllerForStatusBarHidden());
				Assert.Null(parentingController.ChildViewControllerForHomeIndicatorAutoHidden);
			});
		}
	}
}
