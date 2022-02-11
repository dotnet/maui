using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.FlyoutPage)]
	public partial class FlyoutPageTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Controls.Window, WindowHandlerStub>();
				});
			});
		}
#if !IOS
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
				var detailView2 = details2.ToPlatform();

				await detailView2.OnLoadedAsync();
				dl = FindPlatformFlyoutView(detailView);
				Assert.Equal(flyoutView, dl);
			});
		}
#endif
	}
}
