using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class NavigationPageLifecycleTests : BaseTestFixture
	{
		[Test]
		public async Task AppearingFiresForInitialPage()
		{
			ContentPage contentPage = new ContentPage();
			ContentPage resultPage = null;

			contentPage.Appearing += (sender, _)
				=> resultPage = (ContentPage)sender;

			NavigationPage nav = new NavigationPage(contentPage);
			Assert.IsNull(resultPage);
			nav.SendAppearing();
			Assert.AreEqual(resultPage, contentPage);
		}

		[Test]
		public async Task PushLifeCycle()
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageDisappear = null;
			ContentPage pageAppearing = null;

			initialPage.Disappearing += (sender, _)
				=> initialPageDisappear = (ContentPage)sender;

			pushedPage.Appearing += (sender, _)
				=> pageAppearing = (ContentPage)sender;

			NavigationPage nav = new NavigationPage(initialPage);
			nav.SendAppearing();

			await nav.PushAsync(pushedPage);

			Assert.AreEqual(initialPageDisappear, initialPage);
			Assert.AreEqual(pageAppearing, pushedPage);
		}

		[Test]
		public async Task PopLifeCycle()
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageAppearing = null;
			ContentPage pageDisappeared = null;

			NavigationPage nav = new NavigationPage(initialPage);
			nav.SendAppearing();

			initialPage.Appearing += (sender, _)
				=> initialPageAppearing = (ContentPage)sender;

			pushedPage.Disappearing += (sender, _)
				=> pageDisappeared = (ContentPage)sender;

			await nav.PushAsync(pushedPage);
			Assert.IsNull(initialPageAppearing);
			Assert.IsNull(pageDisappeared);

			await nav.PopAsync();

			Assert.AreEqual(initialPageAppearing, initialPage);
			Assert.AreEqual(pageDisappeared, pushedPage);
		}

		[Test]
		public async Task RemoveLastPage()
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageAppearing = null;
			ContentPage pageDisappeared = null;

			NavigationPage nav = new NavigationPage(initialPage);
			nav.SendAppearing();

			initialPage.Appearing += (sender, _)
				=> initialPageAppearing = (ContentPage)sender;

			pushedPage.Disappearing += (sender, _)
				=> pageDisappeared = (ContentPage)sender;

			await nav.PushAsync(pushedPage);
			Assert.IsNull(initialPageAppearing);
			Assert.IsNull(pageDisappeared);

			nav.Navigation.RemovePage(pushedPage);

			Assert.AreEqual(initialPageAppearing, initialPage);
			Assert.AreEqual(pageDisappeared, pushedPage);
		}


		[Test]
		public async Task RemoveInnerPage()
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageAppearing = null;
			ContentPage pageDisappeared = null;

			NavigationPage nav = new NavigationPage(initialPage);
			nav.SendAppearing();

			var pageToRemove = new ContentPage();
			await nav.PushAsync(pageToRemove);
			await nav.PushAsync(pushedPage);


			initialPage.Appearing += (__, _)
				=> Assert.Fail("Appearing Fired Incorrectly");

			pushedPage.Disappearing += (__, _)
				=> Assert.Fail("Appearing Fired Incorrectly");

			nav.Navigation.RemovePage(pageToRemove);
		}
	}
}
