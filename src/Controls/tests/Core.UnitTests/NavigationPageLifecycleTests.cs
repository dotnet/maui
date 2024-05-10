using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NavigationPageLifecycleTests : BaseTestFixture
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AppearingFiresForInitialPage(bool useMaui)
		{
			ContentPage contentPage = new ContentPage();
			ContentPage resultPage = null;

			contentPage.Appearing += (sender, _)
				=> resultPage = (ContentPage)sender;

			NavigationPage nav = new TestNavigationPage(useMaui, contentPage);

			Assert.Null(resultPage);
			_ = new TestWindow(nav);
			Assert.Equal(resultPage, contentPage);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PushLifeCycle(bool useMaui)
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageDisappear = null;
			ContentPage pageAppearing = null;

			initialPage.Disappearing += (sender, _)
				=> initialPageDisappear = (ContentPage)sender;

			pushedPage.Appearing += (sender, _)
				=> pageAppearing = (ContentPage)sender;

			NavigationPage nav = new TestNavigationPage(useMaui, initialPage);
			_ = new TestWindow(nav);
			nav.SendAppearing();

			await nav.PushAsync(pushedPage);

			Assert.Equal(initialPageDisappear, initialPage);
			Assert.Equal(pageAppearing, pushedPage);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PopLifeCycle(bool useMaui)
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage rootPageFiresAppearingAfterPop = null;
			ContentPage pageDisappeared = null;

			NavigationPage nav = new TestNavigationPage(useMaui, initialPage);

			// Because of queued change propagation on BPs
			// sometimes the appearing will fire a bit later than we expect.
			// This ensures the first one fires before we move on
			TaskCompletionSource waitForFirstAppearing = new TaskCompletionSource();
			initialPage.Appearing += OnInitialPageAppearing;
			void OnInitialPageAppearing(object sender, EventArgs e)
			{
				waitForFirstAppearing.SetResult();
				initialPage.Appearing -= OnInitialPageAppearing;
			}

			_ = new TestWindow(nav);

			nav.SendAppearing();
			await waitForFirstAppearing.Task;
			initialPage.Appearing += (sender, _)
				=> rootPageFiresAppearingAfterPop = (ContentPage)sender;

			pushedPage.Disappearing += (sender, _)
				=> pageDisappeared = (ContentPage)sender;

			await nav.PushAsync(pushedPage);
			Assert.Null(rootPageFiresAppearingAfterPop);
			Assert.Null(pageDisappeared);

			await nav.PopAsync();

			Assert.Equal(initialPage, rootPageFiresAppearingAfterPop);
			Assert.Equal(pushedPage, pageDisappeared);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RemoveLastPage(bool useMaui)
		{
			var initialPage = new PageLifeCycleTests.LCPage();
			var pushedPage = new PageLifeCycleTests.LCPage();

			var nav = new TestNavigationPage(useMaui, initialPage);
			_ = new TestWindow(nav);

			await nav.PushAsync(pushedPage);
			Assert.Equal(1, initialPage.AppearingCount);
			Assert.Equal(1, pushedPage.AppearingCount);
			Assert.Equal(0, pushedPage.DisappearingCount);

			nav.Navigation.RemovePage(pushedPage);
			await nav.NavigatingTask;

			Assert.Equal(2, initialPage.AppearingCount);
			Assert.Equal(1, pushedPage.AppearingCount);
			Assert.Equal(1, pushedPage.DisappearingCount);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RemoveInnerPage(bool useMaui)
		{
			ContentPage initialPage = new ContentPage();
			ContentPage pushedPage = new ContentPage();

			ContentPage initialPageAppearing = null;
			ContentPage pageDisappeared = null;

			NavigationPage nav = new TestNavigationPage(useMaui, initialPage);
			_ = new Window(nav);
			nav.SendAppearing();

			var pageToRemove = new ContentPage();
			await nav.PushAsync(pageToRemove);
			await nav.PushAsync(pushedPage);


			initialPage.Appearing += (__, _)
				=> throw new XunitException("Appearing Fired Incorrectly");

			pushedPage.Disappearing += (__, _)
				=> throw new XunitException("Appearing Fired Incorrectly");

			nav.Navigation.RemovePage(pageToRemove);
		}
	}
}
