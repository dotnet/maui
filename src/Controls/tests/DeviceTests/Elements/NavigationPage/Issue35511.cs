#if MACCATALYST
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35511 : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});
		}

		static bool ContainsVisibleText(UIView view, string text)
		{
			if (view.Hidden ||
				view.Alpha <= 0 ||
				view.Bounds.Width <= 0 ||
				view.Bounds.Height <= 0)
			{
				return false;
			}

			if (view is UILabel label &&
				label.Window is not null &&
				label.Text == text)
			{
				return true;
			}

			foreach (var subview in view.Subviews)
			{
				if (ContainsVisibleText(subview, text))
					return true;
			}

			return false;
		}

		[Fact]
		public async Task BackButtonTitleRemainsVisibleWithCustomTitleView()
		{
			SetupBuilder();

			var rootPage = new ContentPage();
			NavigationPage.SetBackButtonTitle(rootPage, "Main");

			var detailPage = new ContentPage();
			NavigationPage.SetTitleView(detailPage, new HorizontalStackLayout
			{
				Children =
				{
					new Label { Text = "Custom title" }
				}
			});

			var navigationPage = new NavigationPage(rootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navigationPage), async handler =>
			{
				await navigationPage.PushAsync(detailPage);
				await OnLoadedAsync(detailPage);

				var navigationBar = GetPlatformToolbar(handler);
				var isBackButtonTitleVisible = ContainsVisibleText(navigationBar, "Main");

				Assert.True(
					isBackButtonTitleVisible,
					"Back button title 'Main' should remain visible when TitleView is set.");
			});
		}
	}
}
#endif
