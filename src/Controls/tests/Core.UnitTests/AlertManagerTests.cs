using System;
using System.Linq;
using System.Threading;
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
			// default (the Standard no-op AlertRequestHelper in the test environment) and must NOT
			// invoke the alert delegate. We assert that deterministically by tracking invocation
			// of the alert delegate rather than racing a wall-clock timer against the prompt task.
			bool alertDelegateInvoked = false;
			Func<Page, AlertArguments, Task> alertFunc = (page, args) =>
			{
				alertDelegateInvoked = true;
				args.SetResult(true);
				return Task.CompletedTask;
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			// Sanity: the alert delegate IS invoked for alert requests.
			var alertResult = await page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel");
			Assert.True(alertResult);
			Assert.True(alertDelegateInvoked);

			alertDelegateInvoked = false;

			// Prompt has no delegate registered, so the alert delegate must NOT fire when a prompt
			// is requested. The platform default no-op fallback is invoked instead, leaving the
			// returned task pending forever (we don't await it).
			_ = page.DisplayPromptAsync("Title", "Message");
			Assert.False(alertDelegateInvoked);
		}

		[Fact]
		public async Task DelegateFuncInterceptsDisplayActionSheet()
		{
			Func<Page, ActionSheetArguments, Task> actionSheetFunc = (page, args) =>
			{
				args.SetResult("Other 1");
				return Task.CompletedTask;
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, ActionSheetArguments, Task>))).Returns(actionSheetFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			Assert.NotNull(window.AlertManager.Subscription);

			var result = await page.DisplayActionSheet("Title", "Cancel", "Destruction", "Other 1", "Other 2");

			Assert.Equal("Other 1", result);
		}

		[Fact]
		public async Task DelegateFuncInterceptsDisplayPrompt()
		{
			Func<Page, PromptArguments, Task> promptFunc = (page, args) =>
			{
				args.SetResult("user input");
				return Task.CompletedTask;
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, PromptArguments, Task>))).Returns(promptFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			Assert.NotNull(window.AlertManager.Subscription);

			var result = await page.DisplayPromptAsync("Title", "Message");

			Assert.Equal("user input", result);
		}

		[Fact]
		public async Task ExplicitSubscriptionWinsOverDelegateFuncs()
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

			AlertArguments args = null;
			stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>())).Do(x => args = x.Arg<AlertArguments>());

			var resultTask = page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel");

			stub.Received().OnAlertRequested(Arg.Is(page), Arg.Is(args));
			args.SetResult(true);
			Assert.True(await resultTask);
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

		[Fact]
		public async Task DelegateFuncAsynchronousFaultPropagatesToCaller()
		{
			// Delegate returns a Task that faults asynchronously (after a yield), exercising the
			// ContinueWith path in DelegateAlertSubscription.Invoke rather than the synchronous
			// throw path covered by DelegateFuncExceptionPropagatesToCaller.
			Func<Page, AlertArguments, Task> alertFunc = async (page, args) =>
			{
				await Task.Yield();
				throw new InvalidOperationException("async boom");
			};

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel"));
			Assert.Equal("async boom", ex.Message);
		}

		[Fact]
		public async Task DelegateFuncCancellationPropagatesToCaller()
		{
			Func<Page, AlertArguments, Task> alertFunc = (page, args) => Task.FromCanceled(new CancellationToken(canceled: true));

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			await Assert.ThrowsAsync<TaskCanceledException>(() =>
				page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel"));
		}

		[Fact]
		public async Task DelegateFuncReturningNullTaskFaultsTheCaller()
		{
			// A delegate that returns null is a contract violation; the caller must observe it as
			// an InvalidOperationException rather than hang forever waiting on the TCS.
			Func<Page, AlertArguments, Task> alertFunc = (page, args) => null;

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel"));
			Assert.Contains("null Task", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public async Task DelegateFuncCompletingWithoutSetResultFaultsTheCaller()
		{
			// A delegate whose Task completes successfully without ever calling SetResult is also
			// a contract violation; the caller must observe an InvalidOperationException rather
			// than hang forever waiting on the TCS.
			Func<Page, AlertArguments, Task> alertFunc = (page, args) => Task.CompletedTask;

			var window = CreateWindow(services =>
			{
				services.GetService(Arg.Is<Type>(x => x == typeof(Func<Page, AlertArguments, Task>))).Returns(alertFunc);
			});
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
			window.Page = page;

			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				page.DisplayAlertAsync("Title", "Message", "Accept", "Cancel"));
			Assert.Contains("SetResult", ex.Message, StringComparison.Ordinal);
		}
	}
}
