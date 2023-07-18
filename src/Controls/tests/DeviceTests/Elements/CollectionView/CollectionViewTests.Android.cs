using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task PushAndPopPageWithCollectionView()
		{
			NavigationPage rootPage = new NavigationPage(new ContentPage());
			ContentPage modalPage = new ContentPage();

			var collectionView = new CollectionView
			{
				ItemsSource = new string[]
				{
				  "Item 1",
				  "Item 2",
				  "Item 3",
				}
			};

			modalPage.Content = collectionView;

			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var currentPage = (rootPage as IPageContainer<Page>).CurrentPage;

					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// Navigate a second time
					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});


			// Without Exceptions here, the test has passed.
			Assert.Equal(0, (rootPage as IPageContainer<Page>).CurrentPage.Navigation.ModalStack.Count);
		}

		[Fact]
		public async Task NullItemsSourceDisplaysHeaderFooterAndEmptyView()
		{
			SetupBuilder();

			var emptyView = new Label { Text = "Empty" };
			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };

			var collectionView = new CollectionView
			{
				ItemsSource = null,
				EmptyView = emptyView,
				Header = header,
				Footer = footer
			};

			ContentPage contentPage = new ContentPage() { Content = collectionView };

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<IWindowHandler>(contentPage,
				async (_) =>
				{
					await WaitForUIUpdate(frame, collectionView);

					Assert.True(emptyView.Height > 0, "EmptyView should be laid out");
					Assert.True(header.Height > 0, "Header should be laid out");
					Assert.True(footer.Height > 0, "Footer should be laid out");
				});
		}
	}
}