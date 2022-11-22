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
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler<Page, PageHandler>();

#if IOS || MACCATALYST
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
#else
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#endif
				});
			});
		}

		[Fact]
		public async Task PoppingTabbedPageDoesntCrash()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(CreateBasicTabbedPage());
				await navPage.PopAsync();
			});
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
