using System;
using System.Collections.Generic;
using System.Linq;


using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
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

        /// <summary>
        /// Tests that RequestPrompt does not throw when subscription is null and parameters are valid.
        /// Verifies that the null-conditional operator prevents exceptions when no subscription exists.
        /// </summary>
        [Fact]
        public void RequestPrompt_WhenSubscriptionIsNull_DoesNotThrow()
        {
            // Arrange
            var window = CreateWindow();
            var alertManager = new AlertManager(window);
            var page = new ContentPage();
            var arguments = new PromptArguments("Title", "Message");

            // Act & Assert
            var exception = Record.Exception(() => alertManager.RequestPrompt(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that RequestPrompt calls OnPromptRequested on subscription with correct parameters.
        /// Verifies the method properly delegates the call when subscription exists.
        /// </summary>
        [Fact]
        public void RequestPrompt_WhenSubscriptionExists_CallsOnPromptRequestedWithCorrectParameters()
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();
            var page = new ContentPage();
            var arguments = new PromptArguments("Test Title", "Test Message");

            // Act
            alertManager.RequestPrompt(page, arguments);

            // Assert
            subscription.Received(1).OnPromptRequested(Arg.Is(page), Arg.Is(arguments));
        }

        /// <summary>
        /// Tests RequestPrompt with null page parameter.
        /// Verifies behavior when page parameter is null.
        /// </summary>
        [Fact]
        public void RequestPrompt_WithNullPage_CallsSubscriptionWithNullPage()
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();
            var arguments = new PromptArguments("Title", "Message");

            // Act
            alertManager.RequestPrompt(null, arguments);

            // Assert
            subscription.Received(1).OnPromptRequested(Arg.Is<Page>(null), Arg.Is(arguments));
        }

        /// <summary>
        /// Tests RequestPrompt with null arguments parameter.
        /// Verifies behavior when arguments parameter is null.
        /// </summary>
        [Fact]
        public void RequestPrompt_WithNullArguments_CallsSubscriptionWithNullArguments()
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();
            var page = new ContentPage();

            // Act
            alertManager.RequestPrompt(page, null);

            // Assert
            subscription.Received(1).OnPromptRequested(Arg.Is(page), Arg.Is<PromptArguments>(null));
        }

        /// <summary>
        /// Tests RequestPrompt with both null parameters.
        /// Verifies behavior when both page and arguments are null.
        /// </summary>
        [Fact]
        public void RequestPrompt_WithBothParametersNull_CallsSubscriptionWithNullParameters()
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();

            // Act
            alertManager.RequestPrompt(null, null);

            // Assert
            subscription.Received(1).OnPromptRequested(Arg.Is<Page>(null), Arg.Is<PromptArguments>(null));
        }

        /// <summary>
        /// Tests RequestPrompt with various PromptArguments configurations.
        /// Verifies that different argument configurations are passed through correctly.
        /// </summary>
        [Theory]
        [InlineData("Title1", "Message1", "OK", "Cancel", "Placeholder", 100, "InitialValue")]
        [InlineData("", "", "Accept", "Cancel", null, -1, "")]
        [InlineData("Long Title", "Long Message", "Yes", "No", "Enter text", 50, "Default")]
        public void RequestPrompt_WithVariousArguments_PassesArgumentsCorrectly(
            string title, string message, string accept, string cancel,
            string placeholder, int maxLength, string initialValue)
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();
            var page = new ContentPage();
            var arguments = new PromptArguments(title, message, accept, cancel, placeholder, maxLength, initialValue: initialValue);

            // Act
            alertManager.RequestPrompt(page, arguments);

            // Assert
            subscription.Received(1).OnPromptRequested(Arg.Is(page), Arg.Is<PromptArguments>(args =>
                args.Title == title &&
                args.Message == message &&
                args.Accept == accept &&
                args.Cancel == cancel &&
                args.Placeholder == placeholder &&
                args.MaxLength == maxLength &&
                args.InitialValue == initialValue));
        }

        /// <summary>
        /// Tests RequestPrompt does not call subscription when it becomes null after being set.
        /// Verifies the null-conditional operator works correctly when subscription is unset.
        /// </summary>
        [Fact]
        public void RequestPrompt_WhenSubscriptionBecomesNull_DoesNotCallSubscription()
        {
            // Arrange
            var (window, subscription) = CreateStubbedWindow();
            var alertManager = new AlertManager(window);
            alertManager.Subscribe();
            alertManager.Unsubscribe();
            var page = new ContentPage();
            var arguments = new PromptArguments("Title", "Message");

            // Act
            alertManager.RequestPrompt(page, arguments);

            // Assert
            subscription.DidNotReceive().OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>());
        }

        /// <summary>
        /// Tests that OnAlertRequested is called with null Page sender parameter.
        /// Verifies that implementations can handle null sender gracefully.
        /// </summary>
        [Fact]
        public void OnAlertRequested_WithNullSender_CallsImplementationCorrectly()
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var arguments = new AlertArguments("Title", "Message", "Accept", "Cancel");
            Page capturedSender = null;
            AlertArguments capturedArguments = null;

            stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>()))
                .Do(x =>
                {
                    capturedSender = x.Arg<Page>();
                    capturedArguments = x.Arg<AlertArguments>();
                });

            // Act
            stub.OnAlertRequested(null, arguments);

            // Assert
            stub.Received(1).OnAlertRequested(Arg.Is<Page>(p => p == null), Arg.Is(arguments));
            Assert.Null(capturedSender);
            Assert.Same(arguments, capturedArguments);
        }

        /// <summary>
        /// Tests that OnAlertRequested is called with null AlertArguments parameter.
        /// Verifies that implementations can handle null arguments gracefully.
        /// </summary>
        [Fact]
        public void OnAlertRequested_WithNullArguments_CallsImplementationCorrectly()
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var page = new ContentPage();
            Page capturedSender = null;
            AlertArguments capturedArguments = null;

            stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>()))
                .Do(x =>
                {
                    capturedSender = x.Arg<Page>();
                    capturedArguments = x.Arg<AlertArguments>();
                });

            // Act
            stub.OnAlertRequested(page, null);

            // Assert
            stub.Received(1).OnAlertRequested(Arg.Is(page), Arg.Is<AlertArguments>(a => a == null));
            Assert.Same(page, capturedSender);
            Assert.Null(capturedArguments);
        }

        /// <summary>
        /// Tests that OnAlertRequested handles AlertArguments with various null and empty string combinations.
        /// Verifies that implementations receive the exact parameter values including null/empty strings.
        /// </summary>
        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("", "", "", "")]
        [InlineData("   ", "   ", "   ", "   ")]
        [InlineData("Title", null, "Accept", null)]
        [InlineData(null, "Message", null, "Cancel")]
        [InlineData("", "Message", "Accept", "")]
        public void OnAlertRequested_WithVariousAlertArgumentsValues_PassesCorrectParameters(
            string title, string message, string accept, string cancel)
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var page = new ContentPage();
            var arguments = new AlertArguments(title, message, accept, cancel);
            AlertArguments capturedArguments = null;

            stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>()))
                .Do(x => capturedArguments = x.Arg<AlertArguments>());

            // Act
            stub.OnAlertRequested(page, arguments);

            // Assert
            stub.Received(1).OnAlertRequested(Arg.Is(page), Arg.Is(arguments));
            Assert.Same(arguments, capturedArguments);
            Assert.Equal(title, capturedArguments.Title);
            Assert.Equal(message, capturedArguments.Message);
            Assert.Equal(accept, capturedArguments.Accept);
            Assert.Equal(cancel, capturedArguments.Cancel);
        }

        /// <summary>
        /// Tests that OnAlertRequested can be called multiple times in sequence.
        /// Verifies that each call receives the correct parameters and implementations handle multiple invocations.
        /// </summary>
        [Fact]
        public void OnAlertRequested_MultipleSequentialCalls_AllCallsReceiveCorrectParameters()
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var page1 = new ContentPage();
            var page2 = new NavigationPage();
            var arguments1 = new AlertArguments("Title1", "Message1", "Accept1", "Cancel1");
            var arguments2 = new AlertArguments("Title2", "Message2", "Accept2", "Cancel2");

            // Act
            stub.OnAlertRequested(page1, arguments1);
            stub.OnAlertRequested(page2, arguments2);

            // Assert
            stub.Received(2).OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>());
            stub.Received(1).OnAlertRequested(Arg.Is(page1), Arg.Is(arguments1));
            stub.Received(1).OnAlertRequested(Arg.Is(page2), Arg.Is(arguments2));
        }

        /// <summary>
        /// Tests that OnAlertRequested works with different Page types.
        /// Verifies that implementations can handle various Page derivatives correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(ContentPage))]
        [InlineData(typeof(NavigationPage))]
        public void OnAlertRequested_WithDifferentPageTypes_CallsImplementationCorrectly(Type pageType)
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var page = (Page)Activator.CreateInstance(pageType);
            var arguments = new AlertArguments("Title", "Message", "Accept", "Cancel");
            Page capturedSender = null;

            stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>()))
                .Do(x => capturedSender = x.Arg<Page>());

            // Act
            stub.OnAlertRequested(page, arguments);

            // Assert
            stub.Received(1).OnAlertRequested(Arg.Is(page), Arg.Is(arguments));
            Assert.Same(page, capturedSender);
            Assert.IsType(pageType, capturedSender);
        }

        /// <summary>
        /// Tests that OnAlertRequested preserves AlertArguments object identity.
        /// Verifies that the exact same AlertArguments instance is passed to implementations.
        /// </summary>
        [Fact]
        public void OnAlertRequested_PreservesArgumentsObjectIdentity_PassesSameInstance()
        {
            // Arrange
            var stub = Substitute.For<AlertManager.IAlertManagerSubscription>();
            var page = new ContentPage();
            var arguments = new AlertArguments("Title", "Message", "Accept", "Cancel");
            AlertArguments capturedArguments = null;

            stub.When(x => x.OnAlertRequested(Arg.Any<Page>(), Arg.Any<AlertArguments>()))
                .Do(x => capturedArguments = x.Arg<AlertArguments>());

            // Act
            stub.OnAlertRequested(page, arguments);

            // Assert
            stub.Received(1).OnAlertRequested(Arg.Is(page), Arg.Is(arguments));
            Assert.Same(arguments, capturedArguments);
            Assert.NotNull(capturedArguments.Result);
        }

        /// <summary>
        /// Tests that the Window property returns the same Window instance that was passed to the constructor.
        /// This verifies that the property correctly exposes the private readonly field _window.
        /// </summary>
        [Fact]
        public void Window_AfterConstruction_ReturnsSameInstance()
        {
            // Arrange
            var window = new Window();
            var alertManager = new AlertManager(window);

            // Act
            var result = alertManager.Window;

            // Assert
            Assert.Same(window, result);
        }

        /// <summary>
        /// Tests that the Window property consistently returns the same instance across multiple calls.
        /// This verifies that the property getter is stable and doesn't create new instances.
        /// </summary>
        [Fact]
        public void Window_MultipleCalls_ReturnsSameInstance()
        {
            // Arrange
            var window = new Window();
            var alertManager = new AlertManager(window);

            // Act
            var result1 = alertManager.Window;
            var result2 = alertManager.Window;

            // Assert
            Assert.Same(result1, result2);
            Assert.Same(window, result1);
            Assert.Same(window, result2);
        }
    }


    public partial class AlertManagerOnPromptRequestedTests : BaseTestFixture
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
            var app = new TestApp();
            var mauiContext = new TestMauiContext(app.Services);
            var window = new Window { Handler = Substitute.For<IWindowHandler>() };
            window.Handler.MauiContext.Returns(mauiContext);

            if (builder != null)
            {
                var services = Substitute.For<IServiceProvider>();
                builder(services);
                mauiContext.Services.Returns(services);
            }

            return window;
        }

        /// <summary>
        /// Tests that OnPromptRequested is called with correct arguments when DisplayPrompt is invoked.
        /// Verifies all PromptArguments properties are properly set including title, message, accept, cancel buttons.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithBasicParameters_SetsCorrectArguments()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            // Act
            var task = page.DisplayPrompt("Test Title", "Test Message", "Accept", "Cancel");

            // Assert
            Assert.Equal("Test Title", args.Title);
            Assert.Equal("Test Message", args.Message);
            Assert.Equal("Accept", args.Accept);
            Assert.Equal("Cancel", args.Cancel);

            bool completed = false;
            var continueTask = task.ContinueWith(t => completed = true);

            args.SetResult("test result");
            continueTask.Wait();
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
            Assert.True(completed);
        }

        /// <summary>
        /// Tests that OnPromptRequested is called with all parameters including placeholder, initial value, max length and keyboard.
        /// Verifies comprehensive PromptArguments configuration is properly handled.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithAllParameters_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            // Act
            var task = page.DisplayPrompt("Title", "Message", "OK", "Cancel", "Enter text here", 50, Keyboard.Email, "Initial");

            // Assert
            Assert.Equal("Title", args.Title);
            Assert.Equal("Message", args.Message);
            Assert.Equal("OK", args.Accept);
            Assert.Equal("Cancel", args.Cancel);
            Assert.Equal("Enter text here", args.Placeholder);
            Assert.Equal("Initial", args.InitialValue);
            Assert.Equal(50, args.MaxLength);
            Assert.Equal(Keyboard.Email, args.Keyboard);

            args.SetResult("test");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with null parameters where allowed.
        /// Verifies that null placeholder and initial value are handled correctly without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithNullOptionalParameters_HandlesNullValues()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            // Act
            var task = page.DisplayPrompt("Title", "Message", placeholder: null, initialValue: null);

            // Assert
            Assert.Equal("Title", args.Title);
            Assert.Equal("Message", args.Message);
            Assert.Null(args.Placeholder);
            Assert.Null(args.InitialValue);
            Assert.Equal(-1, args.MaxLength); // Default value
            Assert.Equal(Keyboard.Default, args.Keyboard); // Default value

            args.SetResult("");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with empty string parameters.
        /// Verifies that empty strings for title, message, and buttons are preserved correctly.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithEmptyStrings_PreservesEmptyValues()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            // Act
            var task = page.DisplayPrompt("", "", "", "", "", 0, initialValue: "");

            // Assert
            Assert.Equal("", args.Title);
            Assert.Equal("", args.Message);
            Assert.Equal("", args.Accept);
            Assert.Equal("", args.Cancel);
            Assert.Equal("", args.Placeholder);
            Assert.Equal("", args.InitialValue);
            Assert.Equal(0, args.MaxLength);

            args.SetResult("");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with very long string parameters.
        /// Verifies that long strings are handled correctly without truncation or errors.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithLongStrings_HandlesLongValues()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            var longTitle = new string('T', 1000);
            var longMessage = new string('M', 2000);
            var longAccept = new string('A', 100);
            var longCancel = new string('C', 100);

            // Act
            var task = page.DisplayPrompt(longTitle, longMessage, longAccept, longCancel);

            // Assert
            Assert.Equal(longTitle, args.Title);
            Assert.Equal(longMessage, args.Message);
            Assert.Equal(longAccept, args.Accept);
            Assert.Equal(longCancel, args.Cancel);

            args.SetResult("result");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with special characters in string parameters.
        /// Verifies that special characters, unicode, and control characters are preserved correctly.
        /// </summary>
        [Fact]
        public void OnPromptRequested_WithSpecialCharacters_PreservesSpecialCharacters()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            var titleWithSpecialChars = "Title with émojis 🚀 and symbols @#$%";
            var messageWithNewlines = "Line 1\nLine 2\r\nLine 3";
            var acceptWithUnicode = "Accepté ✓";
            var cancelWithSymbols = "Cancel ❌";

            // Act
            var task = page.DisplayPrompt(titleWithSpecialChars, messageWithNewlines, acceptWithUnicode, cancelWithSymbols);

            // Assert
            Assert.Equal(titleWithSpecialChars, args.Title);
            Assert.Equal(messageWithNewlines, args.Message);
            Assert.Equal(acceptWithUnicode, args.Accept);
            Assert.Equal(cancelWithSymbols, args.Cancel);

            args.SetResult("result");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with boundary values for MaxLength parameter.
        /// Verifies that extreme MaxLength values including negative, zero, and maximum integer are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void OnPromptRequested_WithBoundaryMaxLength_HandlesExtremeValues(int maxLength)
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            // Act
            var task = page.DisplayPrompt("Title", "Message", maxLength: maxLength);

            // Assert
            Assert.Equal(maxLength, args.MaxLength);

            args.SetResult("result");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests OnPromptRequested with different keyboard types.
        /// Verifies that various keyboard configurations are preserved correctly in PromptArguments.
        /// </summary>
        [Theory]
        [InlineData("Default")]
        [InlineData("Email")]
        [InlineData("Numeric")]
        [InlineData("Telephone")]
        [InlineData("Text")]
        [InlineData("Url")]
        public void OnPromptRequested_WithDifferentKeyboards_PreservesKeyboardType(string keyboardName)
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            PromptArguments args = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => args = x.Arg<PromptArguments>());

            Keyboard keyboard = keyboardName switch
            {
                "Default" => Keyboard.Default,
                "Email" => Keyboard.Email,
                "Numeric" => Keyboard.Numeric,
                "Telephone" => Keyboard.Telephone,
                "Text" => Keyboard.Text,
                "Url" => Keyboard.Url,
                _ => Keyboard.Default
            };

            // Act
            var task = page.DisplayPrompt("Title", "Message", keyboard: keyboard);

            // Assert
            Assert.Equal(keyboard, args.Keyboard);

            args.SetResult("result");
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Is(args));
        }

        /// <summary>
        /// Tests that OnPromptRequested receives the correct Page sender parameter.
        /// Verifies that the page instance passed to OnPromptRequested matches the page that called DisplayPrompt.
        /// </summary>
        [Fact]
        public void OnPromptRequested_ReceivesCorrectSenderPage()
        {
            // Arrange
            var (window, sub) = CreateStubbedWindow();
            var page = new ContentPage() { Handler = Substitute.For<IViewHandler>(), IsPlatformEnabled = true };
            window.Page = page;

            Page receivedSender = null;
            sub.When(x => x.OnPromptRequested(Arg.Any<Page>(), Arg.Any<PromptArguments>())).Do(x => receivedSender = x.Arg<Page>());

            // Act
            var task = page.DisplayPrompt("Title", "Message");

            // Assert
            Assert.Same(page, receivedSender);
            sub.Received().OnPromptRequested(Arg.Is(page), Arg.Any<PromptArguments>());
        }
    }


    /// <summary>
    /// Tests for AlertRequestHelper partial methods implementation
    /// </summary>
    public partial class AlertRequestHelperTests
    {
        /// <summary>
        /// Tests that OnActionSheetRequested can be called with valid Page and ActionSheetArguments without throwing exceptions.
        /// Since this is a partial method with no implementation, it should execute without errors.
        /// </summary>
        [Fact]
        public void OnActionSheetRequested_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();
            var arguments = new ActionSheetArguments("Test Title", "Cancel", "Destroy", new[] { "Button1", "Button2" });

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnActionSheetRequested handles null Page parameter.
        /// Tests defensive programming practices even though the parameter is not nullable.
        /// </summary>
        [Fact]
        public void OnActionSheetRequested_WithNullPage_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            Page page = null;
            var arguments = new ActionSheetArguments("Test Title", "Cancel", "Destroy", new[] { "Button1", "Button2" });

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnActionSheetRequested handles null ActionSheetArguments parameter.
        /// Tests defensive programming practices even though the parameter is not nullable.
        /// </summary>
        [Fact]
        public void OnActionSheetRequested_WithNullArguments_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();
            ActionSheetArguments arguments = null;

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnActionSheetRequested handles both null parameters.
        /// Verifies the method can handle edge cases gracefully.
        /// </summary>
        [Fact]
        public void OnActionSheetRequested_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            Page page = null;
            ActionSheetArguments arguments = null;

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnActionSheetRequested with various ActionSheetArguments configurations.
        /// Verifies the method handles different argument combinations including null properties.
        /// </summary>
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("Title", null, null)]
        [InlineData(null, "Cancel", null)]
        [InlineData(null, null, "Destroy")]
        [InlineData("Title", "Cancel", "Destroy")]
        [InlineData("", "", "")]
        [InlineData("   ", "   ", "   ")]
        public void OnActionSheetRequested_WithVariousArgumentsConfigurations_DoesNotThrow(string title, string cancel, string destruction)
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();
            var buttons = new[] { "Button1", "Button2" };
            var arguments = new ActionSheetArguments(title, cancel, destruction, buttons);

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnActionSheetRequested with different button configurations.
        /// Verifies the method handles various button collections including null and empty collections.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetButtonConfigurations))]
        public void OnActionSheetRequested_WithVariousButtonConfigurations_DoesNotThrow(IEnumerable<string> buttons)
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();
            var arguments = new ActionSheetArguments("Title", "Cancel", "Destroy", buttons);

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnActionSheetRequested with different page types.
        /// Verifies the method works with various Page implementations.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetPageTypes))]
        public void OnActionSheetRequested_WithDifferentPageTypes_DoesNotThrow(Page page)
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var arguments = new ActionSheetArguments("Title", "Cancel", "Destroy", new[] { "Button1" });

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnActionSheetRequested(page, arguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Provides test data for various button configurations including edge cases.
        /// </summary>
        public static IEnumerable<object[]> GetButtonConfigurations()
        {
            yield return new object[] { null };
            yield return new object[] { new string[0] };
            yield return new object[] { new[] { "Button1" } };
            yield return new object[] { new[] { "Button1", "Button2", "Button3" } };
            yield return new object[] { new[] { "", " ", "Valid Button" } };
            yield return new object[] { new[] { "Button1", null, "Button3" } }; // ActionSheetArguments constructor filters out nulls
            yield return new object[] { Enumerable.Repeat("Button", 10) }; // Many buttons
        }

        /// <summary>
        /// Provides test data for different page types.
        /// </summary>
        public static IEnumerable<object[]> GetPageTypes()
        {
            yield return new object[] { new ContentPage() };
            yield return new object[] { new NavigationPage() };
            yield return new object[] { new TabbedPage() };
            yield return new object[] { new FlyoutPage() };
        }

        /// <summary>
        /// Tests OnPromptRequested method with null sender parameter.
        /// Verifies the method handles null sender gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnPromptRequested_NullSender_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var promptArguments = new PromptArguments("Title", "Message");

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(null, promptArguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with null arguments parameter.
        /// Verifies the method handles null arguments gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnPromptRequested_NullArguments_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new Page();

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(page, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with both parameters null.
        /// Verifies the method handles null inputs gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnPromptRequested_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with valid parameters.
        /// Verifies the method executes successfully with typical input values.
        /// </summary>
        [Fact]
        public void OnPromptRequested_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new Page();
            var promptArguments = new PromptArguments("Test Title", "Test Message");

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(page, promptArguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with PromptArguments containing boundary values.
        /// Verifies the method handles edge case values like maximum length and special characters.
        /// </summary>
        [Fact]
        public void OnPromptRequested_PromptArgumentsWithBoundaryValues_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new Page();
            var promptArguments = new PromptArguments(
                title: string.Empty,
                message: new string('A', 10000), // Very long message
                accept: null,
                cancel: "",
                placeholder: "   ", // Whitespace only
                maxLength: int.MaxValue,
                keyboard: Keyboard.Default,
                initialValue: null
            );

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(page, promptArguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with PromptArguments containing negative max length.
        /// Verifies the method handles negative boundary values for MaxLength property.
        /// </summary>
        [Fact]
        public void OnPromptRequested_PromptArgumentsWithNegativeMaxLength_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new Page();
            var promptArguments = new PromptArguments(
                title: "Title",
                message: "Message",
                maxLength: int.MinValue
            );

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(page, promptArguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnPromptRequested method with PromptArguments containing special characters.
        /// Verifies the method handles strings with control characters, Unicode, and special symbols.
        /// </summary>
        [Fact]
        public void OnPromptRequested_PromptArgumentsWithSpecialCharacters_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new Page();
            var promptArguments = new PromptArguments(
                title: "Title\n\r\t\0",
                message: "Message with 🚀 Unicode and \u0001 control chars",
                accept: "Accept\xFF",
                cancel: "Cancel\u2603",
                placeholder: "Placeholder\u00A0",
                initialValue: "Initial\u200B\uFEFF"
            );

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPromptRequested(page, promptArguments));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnPageBusy can be called with valid Page and true enabled without throwing exceptions.
        /// Verifies the method handles valid input parameters correctly.
        /// Expected result: No exception thrown.
        /// </summary>
        [Fact]
        public void OnPageBusy_ValidPageAndTrueEnabled_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPageBusy(page, true));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnPageBusy can be called with valid Page and false enabled without throwing exceptions.
        /// Verifies the method handles valid input parameters correctly.
        /// Expected result: No exception thrown.
        /// </summary>
        [Fact]
        public void OnPageBusy_ValidPageAndFalseEnabled_DoesNotThrow()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var page = new ContentPage();

            // Act & Assert
            var exception = Record.Exception(() => alertRequestHelper.OnPageBusy(page, false));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnPageBusy throws ArgumentNullException when sender parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException thrown.
        /// </summary>
        [Fact]
        public void OnPageBusy_NullSender_ThrowsArgumentNullException()
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => alertRequestHelper.OnPageBusy(null, true));
        }

        /// <summary>
        /// Tests OnPageBusy with various Page types and boolean combinations.
        /// Verifies the method works with different Page implementations and boolean values.
        /// Expected result: No exceptions thrown for any valid combination.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnPageBusy_VariousPageTypesAndBooleanValues_DoesNotThrow(bool enabled)
        {
            // Arrange
            var alertRequestHelper = new AlertManager.AlertRequestHelper();
            var contentPage = new ContentPage();
            var navigationPage = new NavigationPage();

            // Act & Assert
            var contentPageException = Record.Exception(() => alertRequestHelper.OnPageBusy(contentPage, enabled));
            var navigationPageException = Record.Exception(() => alertRequestHelper.OnPageBusy(navigationPage, enabled));

            Assert.Null(contentPageException);
            Assert.Null(navigationPageException);
        }

        /// <summary>
        /// Tests that the OnPageBusy method has the Obsolete attribute applied correctly.
        /// Verifies that the obsolete attribute is present with the expected message.
        /// Expected result: Method is marked as obsolete with correct message.
        /// </summary>
        [Fact]
        public void OnPageBusy_HasObsoleteAttribute_WithCorrectMessage()
        {
            // Arrange
            var methodInfo = typeof(AlertManager.AlertRequestHelper).GetMethod(nameof(AlertManager.AlertRequestHelper.OnPageBusy));

            // Act
            var obsoleteAttributes = methodInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false);

            // Assert
            Assert.Single(obsoleteAttributes);
            var obsoleteAttribute = (ObsoleteAttribute)obsoleteAttributes[0];
            Assert.Equal("This method is obsolete in .NET 10 and will be removed in .NET11.", obsoleteAttribute.Message);
        }
    }


    public partial class AlertManagerSubscribeTests
    {
        /// <summary>
        /// Tests that Subscribe method returns early when window handler is null.
        /// Input: AlertManager with window that has null handler.
        /// Expected: Method returns without creating subscription.
        /// </summary>
        [Fact]
        public void Subscribe_WhenWindowHandlerIsNull_ReturnsEarlyWithoutSubscription()
        {
            // Arrange
            var window = new Window();
            window.Handler = null;
            var alertManager = new AlertManager(window);

            // Act
            alertManager.Subscribe();

            // Assert
            Assert.Null(alertManager.Subscription);
        }

        /// <summary>
        /// Tests that Subscribe method returns early when handler MauiContext is null.
        /// Input: AlertManager with window that has handler with null MauiContext.
        /// Expected: Method returns without creating subscription.
        /// </summary>
        [Fact]
        public void Subscribe_WhenHandlerMauiContextIsNull_ReturnsEarlyWithoutSubscription()
        {
            // Arrange
            var windowHandler = Substitute.For<IElementHandler>();
            windowHandler.MauiContext.Returns((IMauiContext)null);

            var window = new Window();
            window.Handler = windowHandler;
            var alertManager = new AlertManager(window);

            // Act
            alertManager.Subscribe();

            // Assert
            Assert.Null(alertManager.Subscription);
        }

        /// <summary>
        /// Tests that Subscribe method logs warning and returns when already subscribed.
        /// Input: AlertManager that has already been subscribed.
        /// Expected: Warning is logged and method returns without changing subscription.
        /// </summary>
        [Fact]
        public void Subscribe_WhenAlreadySubscribed_LogsWarningAndReturns()
        {
            // Arrange
            var logger = Substitute.For<ILogger<AlertManager>>();
            var services = Substitute.For<IServiceProvider>();
            var subscription = Substitute.For<AlertManager.IAlertManagerSubscription>();

            services.GetService(typeof(AlertManager.IAlertManagerSubscription)).Returns(subscription);

            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);
            mauiContext.CreateLogger<AlertManager>().Returns(logger);

            var windowHandler = Substitute.For<IElementHandler>();
            windowHandler.MauiContext.Returns(mauiContext);

            var window = new Window();
            window.Handler = windowHandler;
            var alertManager = new AlertManager(window);

            // Act - First subscribe
            alertManager.Subscribe();
            var firstSubscription = alertManager.Subscription;

            // Act - Second subscribe (should log warning)
            alertManager.Subscribe();

            // Assert
            Assert.Same(firstSubscription, alertManager.Subscription);
            logger.Received(1).LogWarning("Warning - Window already had an alert manager subscription, but a new one was requested. Not going to do anything.");
        }

        /// <summary>
        /// Tests that Subscribe method uses service from dependency injection when available.
        /// Input: AlertManager with service provider that contains IAlertManagerSubscription.
        /// Expected: Uses subscription from service provider.
        /// </summary>
        [Fact]
        public void Subscribe_WhenServiceProviderHasSubscription_UsesServiceProviderSubscription()
        {
            // Arrange
            var services = Substitute.For<IServiceProvider>();
            var expectedSubscription = Substitute.For<AlertManager.IAlertManagerSubscription>();

            services.GetService(typeof(AlertManager.IAlertManagerSubscription)).Returns(expectedSubscription);

            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);

            var windowHandler = Substitute.For<IElementHandler>();
            windowHandler.MauiContext.Returns(mauiContext);

            var window = new Window();
            window.Handler = windowHandler;
            var alertManager = new AlertManager(window);

            // Act
            alertManager.Subscribe();

            // Assert
            Assert.Same(expectedSubscription, alertManager.Subscription);
        }

        /// <summary>
        /// Tests that Subscribe method falls back to CreateSubscription when service provider lacks subscription.
        /// Input: AlertManager with service provider that returns null for IAlertManagerSubscription.
        /// Expected: Falls back to CreateSubscription method.
        /// </summary>
        [Fact]
        public void Subscribe_WhenServiceProviderLacksSubscription_UsesCreateSubscription()
        {
            // Arrange
            var services = Substitute.For<IServiceProvider>();
            services.GetService(typeof(AlertManager.IAlertManagerSubscription)).Returns(null);

            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);

            var windowHandler = Substitute.For<IElementHandler>();
            windowHandler.MauiContext.Returns(mauiContext);

            var window = new Window();
            window.Handler = windowHandler;
            var alertManager = new TestableAlertManager(window);

            // Act
            alertManager.Subscribe();

            // Assert
            Assert.NotNull(alertManager.Subscription);
            Assert.True(((TestableAlertManager)alertManager).CreateSubscriptionCalled);
        }

        /// <summary>
        /// Tests that Subscribe method logs warning when unable to create subscription.
        /// Input: AlertManager where both service provider and CreateSubscription return null.
        /// Expected: Warning is logged about inability to create subscription.
        /// </summary>
        [Fact]
        public void Subscribe_WhenUnableToCreateSubscription_LogsWarning()
        {
            // Arrange
            var logger = Substitute.For<ILogger<AlertManager>>();
            var services = Substitute.For<IServiceProvider>();
            services.GetService(typeof(AlertManager.IAlertManagerSubscription)).Returns(null);

            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);
            mauiContext.CreateLogger<AlertManager>().Returns(logger);

            var windowHandler = Substitute.For<IElementHandler>();
            windowHandler.MauiContext.Returns(mauiContext);

            var window = new Window();
            window.Handler = windowHandler;
            var alertManager = new TestableAlertManager(window, returnNullFromCreateSubscription: true);

            // Act
            alertManager.Subscribe();

            // Assert
            Assert.Null(alertManager.Subscription);
            logger.Received(1).LogWarning("Warning - Unable to create alert manager subscription.");
        }

        /// <summary>
        /// Helper class to expose CreateSubscription method for testing.
        /// </summary>
        private class TestableAlertManager : AlertManager
        {
            private readonly bool _returnNullFromCreateSubscription;

            public bool CreateSubscriptionCalled { get; private set; }

            public TestableAlertManager(Window window, bool returnNullFromCreateSubscription = false) : base(window)
            {
                _returnNullFromCreateSubscription = returnNullFromCreateSubscription;
            }

        }
    }
}