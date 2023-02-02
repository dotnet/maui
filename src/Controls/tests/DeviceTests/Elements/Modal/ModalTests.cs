using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
using TabbedViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Modal)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class ModalTests : ControlsHandlerTestBase
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

					handlers.AddHandler(typeof(Controls.Shell), typeof(ShellHandler));
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
#if WINDOWS
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
#endif
				});
			});
		}

		[Fact]
		public async Task LoadModalPagesBeforeWindowHasLoaded()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnNavigatedToAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);
				});
		}

		[Fact]
		public async Task ChangePageOnWindowRemovesModalStack()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnLoadedAsync(modalPage.Content);
					var nextPage = new ContentPage();
					window.Page = nextPage;
					await OnUnloadedAsync(modalPage.Content);
					Assert.Equal(0, window.Navigation.ModalStack.Count);
				});
		}

		[Fact]
		public async Task RecreatingStackCorrectlyRecreatesModalStack()
		{
			SetupBuilder();

			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			var mauiContextStub1 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnNavigatedToAsync(modalPage);
				await OnLoadedAsync(modalPage.Content);
			}, mauiContextStub1);

			var mauiContextStub2 = ContextStub.CreateNew(MauiContext);
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnNavigatedToAsync(modalPage);
				await OnLoadedAsync(modalPage.Content);
			}, mauiContextStub2);
		}

		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task BasicPushAndPop(Page rootPage, Page modalPage)
		{
			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var currentPage = (rootPage as IPageContainer<Page>).CurrentPage;
					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Equal(1, currentPage.Navigation.ModalStack.Count);
					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});


			Assert.Equal(0, (rootPage as IPageContainer<Page>).CurrentPage.Navigation.ModalStack.Count);
		}

		class PageTypes : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				for (int i = 0; i < 2; i++)
				{
					Func<Page> rootPage;

					if (i == 0)
						rootPage = () => new NavigationPage(new ContentPage());
					else
						rootPage = () => new Shell() { CurrentItem = new ContentPage() };

					yield return new object[] {
						rootPage(), new NavigationPage(new ContentPage())
					};

					yield return new object[] {
						rootPage(), new ContentPage()
					};

					yield return new object[] {
						rootPage(), new TabbedPage()
						{
							Children =
							{
								new ContentPage(),
								new NavigationPage(new ContentPage())
							}
						}
					};

					yield return new object[] {
						rootPage(), new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Flyout" },
							Detail = new ContentPage() { Title = "Detail" },
						}
					};

				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
