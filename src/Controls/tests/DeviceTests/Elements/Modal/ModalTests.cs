using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

#if IOS
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
using TabbedViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Modal)]
#if ANDROID || IOS
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class ModalTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});
		}

		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task BasicPushAndPop(Page modalPage)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (_) =>
				{
					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Equal(1, navPage.Navigation.ModalStack.Count);
					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});


			Assert.Equal(0, navPage.Navigation.ModalStack.Count);
		}

		class PageTypes : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] {
					new ContentPage()
				};

				yield return new object[] {
					new TabbedPage()
					{
						Children =
						{
							new ContentPage(),
							new NavigationPage(new ContentPage())
						}
					}
				};

				yield return new object[] {
					new FlyoutPage()
					{
						Flyout = new ContentPage() { Title = "Flyout" },
						Detail = new ContentPage() { Title = "Detail" },
					}
				};
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
