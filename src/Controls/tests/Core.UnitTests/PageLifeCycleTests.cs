using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class PageLifeCycleTests : BaseTestFixture
	{
		[Test]
		public void NavigationPageInitialPage()
		{
			var lcPage = new LCPage();
			NavigationPage navigationPage = new NavigationPage(lcPage);
			navigationPage.InitialNativeNavigationStackLoaded();
			Assert.IsNull(lcPage.NavigatingFromArgs);
			Assert.IsNull(lcPage.NavigatedFromArgs);
			Assert.NotNull(lcPage.NavigatedToArgs);
			Assert.IsNull(lcPage.NavigatedToArgs.PreviousPage);
		}

		[Test]
		public async Task NavigationPagePushPage()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			NavigationPage navigationPage = new NavigationPage(previousPage);
			await navigationPage.PushAsync(lcPage);

			Assert.IsNotNull(previousPage.NavigatingFromArgs);
			Assert.AreEqual(previousPage, lcPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(lcPage, previousPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task NavigationPagePopPage()
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			NavigationPage navigationPage = new NavigationPage(firstPage);
			await navigationPage.PushAsync(poppedPage);
			await navigationPage.PopAsync();

			Assert.IsNotNull(poppedPage.NavigatingFromArgs);
			Assert.AreEqual(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task NavigationPagePopToRoot()
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			NavigationPage navigationPage = new NavigationPage(firstPage);
			await navigationPage.PushAsync(new ContentPage());
			await navigationPage.PushAsync(new ContentPage());
			await navigationPage.PushAsync(poppedPage);
			await navigationPage.PopToRootAsync();

			Assert.IsNotNull(poppedPage.NavigatingFromArgs);
			Assert.AreEqual(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task TabbedPageBasicSelectionChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var tabbedPage = new TabbedPage() { Children = { firstPage, secondPage } };

			tabbedPage.CurrentPage = secondPage;
			Assert.IsNotNull(firstPage.NavigatingFromArgs);
			Assert.AreEqual(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public void TabbedPageInitialPage()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var tabbedPage = new TabbedPage() { Children = { firstPage, secondPage } };
			Assert.IsNull(firstPage.NavigatingFromArgs);
			Assert.IsNull(firstPage.NavigatedFromArgs);
			Assert.NotNull(firstPage.NavigatedToArgs);
			Assert.IsNull(firstPage.NavigatedToArgs.PreviousPage);
		}

		[Test]
		public async Task FlyoutPageFlyoutChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var flyoutPage = new FlyoutPage() { Flyout = firstPage };
			flyoutPage.Flyout = secondPage;

			Assert.IsNotNull(firstPage.NavigatingFromArgs);
			Assert.AreEqual(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task FlyoutPageDetailChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var flyoutPage = new FlyoutPage() { Detail = firstPage };
			flyoutPage.Detail = secondPage;

			Assert.IsNotNull(firstPage.NavigatingFromArgs);
			Assert.AreEqual(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}



		[Test]
		public async Task PushModalPage()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var window = new Window(previousPage);

			await window.Navigation.PushModalAsync(lcPage);

			Assert.IsNotNull(previousPage.NavigatingFromArgs);
			Assert.AreEqual(previousPage, lcPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(lcPage, previousPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task PopModalPage()
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			var window = new Window(firstPage);
			await window.Navigation.PushModalAsync(poppedPage);
			await window.Navigation.PopModalAsync();

			Assert.IsNotNull(poppedPage.NavigatingFromArgs);
			Assert.AreEqual(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task PopToAModalPage()
		{
			var firstPage = new LCPage();
			var firstModalPage = new LCPage();
			var secondModalPage = new LCPage();

			var window = new Window(firstPage);
			await window.Navigation.PushModalAsync(firstModalPage);
			await window.Navigation.PushModalAsync(secondModalPage);

			firstModalPage.ClearNavigationArgs();
			secondModalPage.ClearNavigationArgs();

			await window.Navigation.PopModalAsync();

			Assert.IsNotNull(secondModalPage.NavigatingFromArgs);
			Assert.AreEqual(secondModalPage, firstModalPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(firstModalPage, secondModalPage.NavigatedFromArgs.DestinationPage);
		}

		[Test]
		public async Task PushSecondModalPage()
		{
			var firstPage = new LCPage();
			var firstModalPage = new LCPage();
			var secondModalPage = new LCPage();

			var window = new Window(firstPage);
			await window.Navigation.PushModalAsync(firstModalPage);

			firstModalPage.ClearNavigationArgs();
			secondModalPage.ClearNavigationArgs();

			await window.Navigation.PushModalAsync(secondModalPage);

			Assert.IsNotNull(firstModalPage.NavigatingFromArgs);
			Assert.AreEqual(firstModalPage, secondModalPage.NavigatedToArgs.PreviousPage);
			Assert.AreEqual(secondModalPage, firstModalPage.NavigatedFromArgs.DestinationPage);
		}

		class LCPage : ContentPage
		{
			public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
			public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
			public NavigatedToEventArgs NavigatedToArgs { get; private set; }


			public void ClearNavigationArgs()
			{
				NavigatedFromArgs = null;
				NavigatingFromArgs = null;
				NavigatedToArgs = null;
			}

			protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
			{
				base.OnNavigatedFrom(args);
				NavigatedFromArgs = args;
			}

			protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
			{
				base.OnNavigatingFrom(args);
				NavigatingFromArgs = args;
			}

			protected override void OnNavigatedTo(NavigatedToEventArgs args)
			{
				base.OnNavigatedTo(args);
				NavigatedToArgs = args;
			}
		}
	}
}
