using System;
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
		private static (Window, AlertManager.IAlertManagerSubscription) CreateSubbedWindow(Action<IServiceProvider> builder = null)
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
			var (window, sub) = CreateSubbedWindow();

			window.Page = new ContentPage();

			Assert.Null(window.AlertManager.Subscription);
			window.MauiContext.Services.DidNotReceive().GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription)));
		}

		[Fact]
		public void SettingPageWithHandlerSubscribes()
		{
			var (window, sub) = CreateSubbedWindow();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			Assert.NotNull(window.AlertManager.Subscription);
			Assert.Equal(sub, window.AlertManager.Subscription);
			window.MauiContext.Services.Received().GetService(Arg.Is<Type>(x => x == typeof(AlertManager.IAlertManagerSubscription)));
		}

		[Fact]
		public void BusyNotSentWhenNotVisible()
		{
			var (window, sub) = CreateSubbedWindow();

			var page = new ContentPage { IsBusy = true };
			window.Page = page;

			Assert.Null(window.AlertManager.Subscription);
		}

		[Fact]
		public void BusySentWhenBusyPageAppears()
		{
			var (window, sub) = CreateSubbedWindow();

			var page = new ContentPage { IsBusy = true };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null));

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(true));
		}

		[Fact]
		public void BusySentWhenBusyPageDisappears()
		{
			var (window, sub) = CreateSubbedWindow();
			var page = new ContentPage { IsBusy = true, Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null));

			sub.ClearReceivedCalls();

			((IPageController)page).SendDisappearing();

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(false));
		}

		[Fact]
		public void BusySentWhenBusyPageIsNoLongerBusy()
		{
			var (window, sub) = CreateSubbedWindow();
			var page = new ContentPage { IsBusy = true, Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null));

			sub.ClearReceivedCalls();

			page.IsBusy = false;

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(false));
		}

		[Fact]
		public void BusySentWhenVisiblePageSetToBusy()
		{
			var (window, sub) = CreateSubbedWindow();
			var page = new ContentPage { Handler = Substitute.For<IViewHandler>() };
			window.Page = page;

			((IPageController)page).SendAppearing();
			page.SendNavigatedTo(new NavigatedToEventArgs(null));

			sub.ClearReceivedCalls();

			page.IsBusy = true;

			sub.Received().OnPageBusy(Arg.Is(page), Arg.Is(true));
		}

		[Fact]
		public void DisplayAlert()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			AlertArguments args = null;
			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments e) => args = e);

			var task = page.DisplayAlert("Title", "Message", "Accept", "Cancel");

			Assert.Equal("Title", args.Title);
			Assert.Equal("Message", args.Message);
			Assert.Equal("Accept", args.Accept);
			Assert.Equal("Cancel", args.Cancel);

			bool completed = false;
			var continueTask = task.ContinueWith(t => completed = true);

			args.SetResult(true);
			continueTask.Wait();
			Assert.True(completed);
		}

		[Fact]
		public void DisplayActionSheet()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };

			ActionSheetArguments args = null;
			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments e) => args = e);

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
			Assert.True(completed);
		}
	}
}
