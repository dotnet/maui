using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AlertManagerTests : BaseTestFixture
	{
		private static (Window, AlertManager.IAlertManagerSubscription) CreateStubbedWindow(Action<IServiceProvider> builder = null)
		{
			var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription))).Returns(stub);
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
			Assert.Null(window.AlertManager.Subscription);
		}

		[Fact]
		public void SettingPageWithoutHandlerDoesNotSubscribe()
		{
			var (window, sub) = CreateStubbedWindow();

			window.Page = new ContentPage();

			Assert.Null(window.AlertManager.Subscription);
			window.MauiContext.Services.DidNotReceive().GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription)));
		}

		[Fact]
		public void SettingPageWithHandlerSubscribes()
		{
			var (window, sub) = CreateStubbedWindow();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.NotNull(window.AlertManager.Subscription);
			Assert.Equal(sub, window.AlertManager.Subscription);
			window.MauiContext.Services.Received().GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription)));
		}

		[Fact]
		public void BusyNotSentWhenNotVisible()
		{
			var (window, sub) = CreateStubbedWindow();

			var page = new ContentPage { IsBusy = true };
			window.Page = page;

			Assert.Null(window.AlertManager.Subscription);
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
		public async Task DelegateFuncInterceptsDisplayAlert()
		{
			Func<Page, AlertArguments, Task> alertFunc = (page, args) =>
			{
				args.SetResult(true);
				return Task.CompletedTask;
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			// No explicit IAlertManagerSubscription -> delegate convention kicks in.
			Assert.NotNull(window.AlertManager.Subscription);

			var result = await page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel");

			Assert.True(result);
		}

		[Fact]
		public async Task DelegateFuncFallsThroughForUnregisteredOperation()
		{
			// Register ONLY the alert delegate. Prompt requests should fall through to the platform
			// default (which, in the test environment, is the Standard no-op AlertRequestHelper).
			Func<Page, AlertArguments, Task> alertFunc = (page, args) =>
			{
				args.SetResult(true);
				return Task.CompletedTask;
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			var alertResult = await page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel");
			Assert.True(alertResult);

			// Prompt has no delegate registered; the test's Standard fallback is a no-op so the
			// task should simply never complete. Verify that by racing it against a short delay.
			var promptTask = page.DisplayPromptAsync("Title", "Message");
			var winner = await Task.WhenAny(promptTask, Task.Delay(100));
			Assert.NotSame(promptTask, winner);
		}

		[Fact]
		public void ExplicitSubscriptionWinsOverDelegateFuncs()
		{
			Func<Page, AlertArguments, Task> alertFunc = (page, args) =>
			{
				args.SetResult(true);
				return Task.CompletedTask;
			};

			var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription))).Returns(stub);
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			Assert.Same(stub, window.AlertManager.Subscription);

			page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel");

			stub.Received().OnAlertRequested(Arg.Is(page), Arg.Any<AlertArguments>());
		}

		[Fact]
		public async Task DelegateFuncExceptionPropagatesToCaller()
		{
			Func<Page, AlertArguments, Task> alertFunc = (page, args) => throw new InvalidOperationException("boom");

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel"));
			Assert.Equal("boom", ex.Message);
		}
	}
}
