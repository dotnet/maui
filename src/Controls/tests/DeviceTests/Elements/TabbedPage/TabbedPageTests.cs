using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
				});
			});
		}

		[Fact(DisplayName = "TabbedPage always fills size regardless of WxH Request")]
		public async Task ToolbarVisibleWhenPushingToTabbedPage()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.HeightRequest = 10;
			tabbedPage.WidthRequest = 10;

			var tabbedPageHandler = await CreateHandlerAsync<TabbedViewHandler>(tabbedPage);

			var measure = await InvokeOnMainThreadAsync(() => tabbedPage.Measure(1000, 1000));

		}


		TabbedPage CreateBasicTabbedPage()
		{
			return new TabbedPage()
			{
				Title = "Tabbed Page",
				Children =
				{
					new ContentPage() { Title = "Page 1" }
				}
			};
		}
	}
}
