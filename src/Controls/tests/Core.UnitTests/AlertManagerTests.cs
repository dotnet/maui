using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AlertManagerTests : BaseTestFixture
	{
		private static (Window, IAlertManagerSubscription) CreateStubbedWindow(Action<IServiceProvider> builder = null)
		{
			var stub = Substitute.For<IAlertManagerSubscription>();

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(IAlertManagerSubscription))).Returns(stub);
				builder?.Invoke(services);
			});

			return (window, stub);
		}

		private static Window CreateWindow(Action<IServiceProvider> builder = null)
		{
			var services = Substitute.For<IServiceProvider>();
			builder?.Invoke(services);

			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);

			var windowHandler = Substitute.For<IElementHandler>();
			windowHandler.MauiContext.Returns(mauiContext);

			var window = new Window();
			window.Handler = windowHandler;

			var app = Substitute.For<Element, IApplication>();
			window.Parent = app;

			return window;
		}

		[Fact]
		public void TestsAreSetUpCorrectly()
		{
			var window = CreateWindow();

			Assert.NotNull(window);
			Assert.NotNull(window.AlertManager);
			Assert.Null(((AlertManager)window.AlertManager).Subscription);
		}

		[Fact]
		public void SettingPageWithoutHandlerDoesNotSubscribe()
		{
			var (window, sub) = CreateStubbedWindow();

			window.Page = new ContentPage();

			Assert.Null(((AlertManager)window.AlertManager).Subscription);
			window.MauiContext.Services.DidNotReceive().GetService(Arg.Is<Type>(x => x == typeof(IAlertManagerSubscription)));
		}

		[Fact]
		public void SettingPageWithHandlerSubscribes()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.NotNull(((AlertManager)window.AlertManager).Subscription);
			Assert.Equal(sub, ((AlertManager)window.AlertManager).Subscription);
			window.MauiContext.Services.Received().GetService(Arg.Is<Type>(x => x == typeof(IAlertManagerSubscription)));
		}

		[Fact]
		public void BusyNotSentWhenNotVisible()
		{
			var (window, sub) = CreateStubbedWindow();

			var page = new ContentPage { IsBusy = true };
			window.Page = page;

			Assert.Null(((AlertManager)window.AlertManager).Subscription);
		}

		[Fact]
		public void BusySentWhenBusyPageAppears()
		{
			var (window, sub) = CreateStubbedWindow();

			var page = new ContentPage { IsBusy = true, Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null, NavigationType.Push));

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(true));
		}

		[Fact]
		public void BusySentWhenBusyPageDisappears()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage { IsBusy = true, Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null, NavigationType.Push));

			sub.ClearReceivedCalls();

			((IPageController)page).SendDisappearing();

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(false));
		}

		[Fact]
		public void BusySentWhenBusyPageIsNoLongerBusy()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage { IsBusy = true, Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null, NavigationType.Push));

			sub.ClearReceivedCalls();

			page.IsBusy = false;

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(false));
		}

		[Fact]
		public void BusySentWhenVisiblePageSetToBusy()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null, NavigationType.Push));

			sub.ClearReceivedCalls();

			page.IsBusy = true;

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(true));
		}

		[Fact]
		public void DisplayAlert()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			AlertArguments args = null;
			sub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>())).Do(x => args = x.Arg<AlertArguments>());

			var task = page.DisplayAlert("Title", "Message", "Accept", "Cancel");

			Assert.Equal("Title", args.Title);
			Assert.Equal("Message", args.Message);
			Assert.Equal("Accept", args.Accept);
			Assert.Equal("Cancel", args.Cancel);

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult(true);
			continueTask.Wait();
			sub.Received().OnAlertRequested(Arg.Is(page), Arg.Is(args));
			Assert.True(completed);
		}

		[Fact]
		public void DisplayActionSheet()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			ActionSheetArguments args = null;
			sub.When(sub => sub.OnActionSheetRequested(Arg.Any<Page>(), Arg.Any<ActionSheetArguments>())).Do(x => args = x.Arg<ActionSheetArguments>());

			var task = page.DisplayActionSheet("Title", "Cancel", "Destruction", "Other 1", "Other 2");

			Assert.Equal("Title", args.Title);
			Assert.Equal("Destruction", args.Destruction);
			Assert.Equal("Cancel", args.Cancel);
			Assert.Equal("Other 1", args.Buttons.First());
			Assert.Equal("Other 2", args.Buttons.Skip(1).First());

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult("Cancel");
			continueTask.Wait();
			sub.Received().OnActionSheetRequested(Arg.Is(page), Arg.Is(args));
			Assert.True(completed);
		}

		[Fact]
		public void CustomIAlertManagerResolvedFromDI()
		{
			var customAlertManager = Substitute.For<IAlertManager>();

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(IAlertManager))).Returns(customAlertManager);
			});

			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.Same(customAlertManager, window.AlertManager);
			customAlertManager.Received().Subscribe();
		}

		[Fact]
		public void DefaultAlertManagerUsedWhenNoDIRegistration()
		{
			var window = CreateWindow();

			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.IsType<AlertManager>(window.AlertManager);
		}

		[Fact]
		public void ReplacingPageWithHandledPageCallsUnsubscribeBeforeSubscribe()
		{
			// Arrange: custom IAlertManager so we can track call order
			var callOrder = new List<string>();
			var customAlertManager = Substitute.For<IAlertManager>();
			customAlertManager.When(x => x.Unsubscribe()).Do(_ => callOrder.Add("Unsubscribe"));
			customAlertManager.When(x => x.Subscribe()).Do(_ => callOrder.Add("Subscribe"));

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(IAlertManager))).Returns(customAlertManager);
			});

			// First page with handler to establish subscription
			var firstPage = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = firstPage;
			callOrder.Clear(); // reset after initial subscribe

			// Replace with a second page that already has a handler attached
			var secondPage = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = secondPage;

			// Unsubscribe must come before Subscribe
			Assert.Equal(2, callOrder.Count);
			Assert.Equal("Unsubscribe", callOrder[0]);
			Assert.Equal("Subscribe", callOrder[1]);
		}
	}
}
