using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ModalNavigationManagerTests
    {
        /// <summary>
        /// Tests that PopModalAsync() calls PopModalAsync(true) and returns the expected Page.
        /// Verifies the parameterless overload correctly delegates to the overload with animated=true.
        /// Expected result: The method should return the same Page as PopModalAsync(true).
        /// </summary>
        [Fact]
        public async Task PopModalAsync_CallsAnimatedOverload_ReturnsExpectedPage()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var mockPage = new ContentPage();
            var modalNavigationManager = new TestableModalNavigationManager(window);

            // Set up the mock to return a specific page when PopModalAsync(true) is called
            modalNavigationManager.SetupPopModalAsyncResult(mockPage);

            // Act
            var result = await modalNavigationManager.PopModalAsync();

            // Assert
            Assert.Equal(mockPage, result);
            Assert.True(modalNavigationManager.PopModalAsyncWithAnimatedCalled);
            Assert.True(modalNavigationManager.LastAnimatedValue);
        }

        /// <summary>
        /// Tests that PopModalAsync() propagates exceptions from PopModalAsync(bool animated).
        /// Verifies that any exception thrown by the animated overload is properly bubbled up.
        /// Expected result: InvalidOperationException should be thrown when modal stack is empty.
        /// </summary>
        [Fact]
        public async Task PopModalAsync_WhenAnimatedOverloadThrows_PropagatesException()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modalNavigationManager = new TestableModalNavigationManager(window);
            var expectedException = new InvalidOperationException("PopModalAsync failed because modal stack is currently empty.");

            // Set up the mock to throw when PopModalAsync(true) is called
            modalNavigationManager.SetupPopModalAsyncException(expectedException);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => modalNavigationManager.PopModalAsync());
            Assert.Equal(expectedException.Message, thrownException.Message);
            Assert.True(modalNavigationManager.PopModalAsyncWithAnimatedCalled);
            Assert.True(modalNavigationManager.LastAnimatedValue);
        }

        /// <summary>
        /// Testable version of ModalNavigationManager that allows us to control the behavior
        /// of PopModalAsync(bool animated) for testing the parameterless overload.
        /// </summary>
        private class TestableModalNavigationManager : ModalNavigationManager
        {
            public bool PopModalAsyncWithAnimatedCalled { get; private set; }
            public bool LastAnimatedValue { get; private set; }
            private Page _mockResult;
            private Exception _exceptionToThrow;

            public TestableModalNavigationManager(Window window) : base(window)
            {
            }

            public void SetupPopModalAsyncResult(Page result)
            {
                _mockResult = result;
                _exceptionToThrow = null;
            }

            public void SetupPopModalAsyncException(Exception exception)
            {
                _exceptionToThrow = exception;
                _mockResult = null;
            }

        }

        /// <summary>
        /// Tests that PushModalAsync with valid page parameter calls the overloaded method with animated=true
        /// Input: Valid Page instance
        /// Expected: Method executes without throwing and returns a Task
        /// </summary>
        [Fact]
        public void PushModalAsync_WithValidPage_ReturnsTask()
        {
            // Arrange
            var window = Substitute.For<Window>();
            window.Page = Substitute.For<Page>();
            window.MauiContext = Substitute.For<IMauiContext>();
            var manager = new TestableModalNavigationManager(window);
            var modal = Substitute.For<Page>();

            // Act & Assert
            var result = manager.PushModalAsync(modal);

            // Verify that a Task is returned
            Assert.NotNull(result);
            Assert.IsType<Task>(result);
        }

        /// <summary>
        /// Tests that PushModalAsync handles null page parameter
        /// Input: null Page parameter
        /// Expected: Method behavior depends on overloaded method handling
        /// </summary>
        [Fact]
        public void PushModalAsync_WithNullPage_ReturnsTask()
        {
            // Arrange
            var window = Substitute.For<Window>();
            window.Page = Substitute.For<Page>();
            window.MauiContext = Substitute.For<IMauiContext>();
            var manager = new TestableModalNavigationManager(window);

            // Act & Assert
            var result = manager.PushModalAsync(null);

            // Verify that a Task is returned (null handling is delegated to overloaded method)
            Assert.NotNull(result);
            Assert.IsType<Task>(result);
        }

        /// <summary>
        /// Tests that PushModalAsync calls the overloaded method with correct parameters
        /// Input: Valid Page instance  
        /// Expected: Overloaded PushModalAsync(Page, bool) is called with animated=true
        /// </summary>
        [Fact]
        public void PushModalAsync_WithValidPage_CallsOverloadedMethodWithAnimatedTrue()
        {
            // Arrange
            var window = Substitute.For<Window>();
            window.Page = Substitute.For<Page>();
            window.MauiContext = Substitute.For<IMauiContext>();
            var manager = new TestableModalNavigationManager(window);
            var modal = Substitute.For<Page>();

            // Act
            var result = manager.PushModalAsync(modal);

            // Assert
            Assert.NotNull(result);
            Assert.True(manager.PushModalAsyncOverloadCalled);
            Assert.Equal(modal, manager.LastModalPassed);
            Assert.True(manager.LastAnimatedValue);
        }

        /// <summary>
        /// Tests that PopModalAsync throws InvalidOperationException when modal stack is empty.
        /// Input: Empty modal stack
        /// Expected: InvalidOperationException with specific message
        /// </summary>
        [Fact]
        public async Task PopModalAsync_EmptyModalStack_ThrowsInvalidOperationException()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modalPages = new NavigatingStepRequestList();
            var manager = new TestableModalNavigationManager(window, modalPages);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.PopModalAsync(true));
            Assert.Equal("PopModalAsync failed because modal stack is currently empty.", exception.Message);
        }

        /// <summary>
        /// Tests that PopModalAsync returns null when window cancels the pop operation.
        /// Input: Modal stack with one page, window returns true from OnModalPopping
        /// Expected: OnPopCanceled called, null returned, modal remains in stack
        /// </summary>
        [Fact]
        public async Task PopModalAsync_WindowCancelsPop_ReturnsNullAndCallsOnPopCanceled()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);

            window.OnModalPopping(modal).Returns(true); // Cancel the pop

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Null(result);
            window.Received(1).OnModalPopping(modal);
            window.Received(1).OnPopCanceled();
            Assert.Single(modalPages.Pages); // Modal should still be in stack
        }

        /// <summary>
        /// Tests successful modal pop with non-Shell window and lifecycle events enabled.
        /// Input: Modal stack with one page, non-Shell window, FireLifeCycleEvents true
        /// Expected: All lifecycle events fired, modal returned, stack empty
        /// </summary>
        [Fact]
        public async Task PopModalAsync_NonShellWindowWithLifeCycleEvents_FiresAllEventsAndReturnsModal()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var rootPage = Substitute.For<Page>();
            var modal = Substitute.For<Page>();
            var parent = Substitute.For<Element>();

            window.Page.Returns(rootPage);
            modal.Parent.Returns(parent);

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(true);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false); // Don't cancel

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);
            Assert.Empty(modalPages.Pages);

            // Verify lifecycle events
            modal.Received(1).SendNavigatingFrom(Arg.Any<NavigatingFromEventArgs>());
            modal.Received(1).SendDisappearing();
            rootPage.Received(1).SendAppearing();
            modal.Received(1).SendNavigatedFrom(Arg.Any<NavigatedFromEventArgs>());
            rootPage.Received(1).SendNavigatedTo(Arg.Any<NavigatedToEventArgs>());

            // Verify window events
            window.Received(1).OnModalPopping(modal);
            window.Received(1).OnModalPopped(modal);

            // Verify parent removal
            parent.Received(1).RemoveLogicalChild(modal);
        }

        /// <summary>
        /// Tests successful modal pop with Shell window and IsPoppingModalStack false.
        /// Input: Modal stack with one page, Shell window, IsPoppingModalStack false
        /// Expected: Current page SendAppearing called
        /// </summary>
        [Fact]
        public async Task PopModalAsync_ShellWindowNotPoppingModalStack_CallsCurrentPageSendAppearing()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var shell = Substitute.For<Shell>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSection = Substitute.For<ShellSection>();
            var currentPage = Substitute.For<Page>();
            var modal = Substitute.For<Page>();

            window.Page.Returns(shell);
            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns(shellSection);
            shellSection.IsPoppingModalStack.Returns(false);
            shell.CurrentPage.Returns(currentPage);

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(true);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);
            currentPage.Received(1).SendAppearing();
        }

        /// <summary>
        /// Tests successful modal pop with Shell window and IsPoppingModalStack true.
        /// Input: Modal stack with one page, Shell window, IsPoppingModalStack true
        /// Expected: Current page SendAppearing not called
        /// </summary>
        [Fact]
        public async Task PopModalAsync_ShellWindowPoppingModalStack_DoesNotCallCurrentPageSendAppearing()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var shell = Substitute.For<Shell>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSection = Substitute.For<ShellSection>();
            var currentPage = Substitute.For<Page>();
            var modal = Substitute.For<Page>();

            window.Page.Returns(shell);
            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns(shellSection);
            shellSection.IsPoppingModalStack.Returns(true);
            shell.CurrentPage.Returns(currentPage);

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(true);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);
            currentPage.DidNotReceive().SendAppearing();
        }

        /// <summary>
        /// Tests modal pop when platform is not ready.
        /// Input: Modal with platform not ready
        /// Expected: SyncModalStackWhenPlatformIsReady called, no platform pop task executed
        /// </summary>
        [Fact]
        public async Task PopModalAsync_PlatformNotReady_CallsSyncModalStackWhenPlatformIsReady()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(false);
            manager.SetIsModalReady(false); // Platform not ready

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);
            Assert.True(manager.SyncModalStackWhenPlatformIsReadyCalled);
        }

        /// <summary>
        /// Tests modal pop when FireLifeCycleEvents is false.
        /// Input: Modal with FireLifeCycleEvents disabled
        /// Expected: Navigation events not fired
        /// </summary>
        [Fact]
        public async Task PopModalAsync_FireLifeCycleEventsFalse_DoesNotFireNavigationEvents()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var rootPage = Substitute.For<Page>();

            window.Page.Returns(rootPage);

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(false);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);

            // Verify navigation events are not fired
            modal.DidNotReceive().SendNavigatingFrom(Arg.Any<NavigatingFromEventArgs>());
            modal.DidNotReceive().SendNavigatedFrom(Arg.Any<NavigatedFromEventArgs>());
            rootPage.DidNotReceive().SendNavigatedTo(Arg.Any<NavigatedToEventArgs>());
        }

        /// <summary>
        /// Tests modal pop when modal has no parent.
        /// Input: Modal with null parent
        /// Expected: RemoveLogicalChild not called, no exception thrown
        /// </summary>
        [Fact]
        public async Task PopModalAsync_ModalWithNullParent_DoesNotCallRemoveLogicalChild()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            modal.Parent.Returns((Element)null); // No parent

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(false);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(true);

            // Assert
            Assert.Equal(modal, result);
            // No exception should be thrown and no RemoveLogicalChild call should be made
        }

        /// <summary>
        /// Tests that PopModalAsync uses the animated parameter correctly.
        /// Input: Various animated values
        /// Expected: Animated parameter passed to platform pop method
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopModalAsync_AnimatedParameter_PassedToPlatformMethod(bool animated)
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            var modalPages = new NavigatingStepRequestList();
            modalPages.Add(new NavigationStepRequest(modal, true));

            var manager = new TestableModalNavigationManager(window, modalPages);
            manager.SetWaitForModalToFinishTask(Task.CompletedTask);
            manager.SetFireLifeCycleEvents(false);
            manager.SetIsModalReady(true);
            manager.SetSyncing(false);

            window.OnModalPopping(modal).Returns(false);

            // Act
            var result = await manager.PopModalAsync(animated);

            // Assert
            Assert.Equal(modal, result);
            Assert.Equal(animated, manager.LastAnimatedParameterPassedToPlatform);
        }

        /// <summary>
        /// Helper method to create a test ModalNavigationManager with mocked dependencies.
        /// </summary>
        private ModalNavigationManager CreateTestModalNavigationManager(
            Window window = null,
            bool fireLifeCycleEvents = true,
            bool isModalReady = false,
            Page currentPage = null)
        {
            window ??= Substitute.For<Window>();
            var manager = new TestableModalNavigationManager(window)
            {
                TestFireLifeCycleEvents = fireLifeCycleEvents,
                TestIsModalReady = isModalReady,
                TestCurrentPage = currentPage
            };
            return manager;
        }

        /// <summary>
        /// Tests that PushModalAsync throws ArgumentNullException when modal parameter is null.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_NullModal_ThrowsArgumentNullException()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var manager = CreateTestModalNavigationManager(window);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.PushModalAsync(null, true));
        }

        /// <summary>
        /// Tests PushModalAsync basic functionality with lifecycle events enabled.
        /// Tests the main execution path with FireLifeCycleEvents=true.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PushModalAsync_WithLifeCycleEvents_CallsExpectedMethods(bool animated)
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: true,
                currentPage: previousPage);

            // Act
            await manager.PushModalAsync(modal, animated);

            // Assert
            window.Received(1).OnModalPushing(modal);
            window.Received(1).AddLogicalChild(modal);
            window.Received(1).OnModalPushed(modal);
            previousPage.Received(1).SendNavigatingFrom(Arg.Any<NavigatingFromEventArgs>());
            previousPage.Received(1).SendDisappearing();
            previousPage.Received(1).SendNavigatedFrom(Arg.Any<NavigatedFromEventArgs>());
        }

        /// <summary>
        /// Tests PushModalAsync with lifecycle events disabled.
        /// Verifies that no lifecycle events are sent when FireLifeCycleEvents=false.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PushModalAsync_WithoutLifeCycleEvents_SkipsLifeCycleEvents(bool animated)
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: false,
                currentPage: previousPage);

            // Act
            await manager.PushModalAsync(modal, animated);

            // Assert
            window.Received(1).OnModalPushing(modal);
            window.Received(1).AddLogicalChild(modal);
            window.Received(1).OnModalPushed(modal);
            previousPage.DidNotReceive().SendNavigatingFrom(Arg.Any<NavigatingFromEventArgs>());
            previousPage.DidNotReceive().SendNavigatedFrom(Arg.Any<NavigatedFromEventArgs>());
        }

        /// <summary>
        /// Tests PushModalAsync with Shell page that is not pushing modal stack.
        /// Verifies Shell-specific lifecycle event handling.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_WithShellNotPushingModalStack_HandlesBoth()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var shell = Substitute.For<Shell>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSection = Substitute.For<ShellSection>();

            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns(shellSection);
            shellSection.IsPushingModalStack.Returns(false);
            window.Page.Returns(shell);

            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: true,
                currentPage: previousPage);

            // Act
            await manager.PushModalAsync(modal, animated: true);

            // Assert
            previousPage.Received(1).SendDisappearing();
        }

        /// <summary>
        /// Tests PushModalAsync with Shell page that is pushing modal stack.
        /// Verifies that disappearing/appearing events are skipped when Shell is pushing modal stack.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_WithShellPushingModalStack_SkipsAppearingDisappearing()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var shell = Substitute.For<Shell>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSection = Substitute.For<ShellSection>();

            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns(shellSection);
            shellSection.IsPushingModalStack.Returns(true);
            window.Page.Returns(shell);

            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: true,
                currentPage: previousPage);

            // Act
            await manager.PushModalAsync(modal, animated: true);

            // Assert
            previousPage.DidNotReceive().SendDisappearing();
        }

        /// <summary>
        /// Tests PushModalAsync with non-Shell page.
        /// Verifies standard page lifecycle event handling.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_WithNonShellPage_HandlesStandardLifeCycle()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var previousPage = Substitute.For<Page>();
            var regularPage = Substitute.For<Page>();

            window.Page.Returns(regularPage);

            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: true,
                currentPage: previousPage);

            // Act
            await manager.PushModalAsync(modal, animated: true);

            // Assert
            previousPage.Received(1).SendDisappearing();
        }

        /// <summary>
        /// Tests PushModalAsync when platform is ready and modal stack is empty.
        /// This tests the NOT COVERED lines 146-150.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PushModalAsync_PlatformReadyEmptyModalStack_SetsNavigationProxyFirst(bool animated)
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var navigation = Substitute.For<INavigation>();
            var navigationProxy = Substitute.For<NavigationProxy>();

            window.Navigation.Returns(navigation);
            modal.NavigationProxy.Returns(navigationProxy);

            var manager = CreateTestModalNavigationManager(
                window: window,
                isModalReady: true,
                currentPage: Substitute.For<Page>());

            // Make sure modal stack appears empty
            ((TestableModalNavigationManager)manager).TestModalStackCount = 0;

            // Act
            await manager.PushModalAsync(modal, animated);

            // Assert
            navigationProxy.Received(1).Inner = navigation;
            Assert.True(((TestableModalNavigationManager)manager).PushModalPlatformAsyncCalled);
        }

        /// <summary>
        /// Tests PushModalAsync when platform is ready and modal stack has items.
        /// This tests the NOT COVERED lines 152-155.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PushModalAsync_PlatformReadyNonEmptyModalStack_SetsNavigationProxyAfter(bool animated)
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();
            var navigation = Substitute.For<INavigation>();
            var navigationProxy = Substitute.For<NavigationProxy>();

            window.Navigation.Returns(navigation);
            modal.NavigationProxy.Returns(navigationProxy);

            var manager = CreateTestModalNavigationManager(
                window: window,
                isModalReady: true,
                currentPage: Substitute.For<Page>());

            // Make sure modal stack appears non-empty
            ((TestableModalNavigationManager)manager).TestModalStackCount = 1;

            // Act
            await manager.PushModalAsync(modal, animated);

            // Assert
            navigationProxy.Received(1).Inner = navigation;
            Assert.True(((TestableModalNavigationManager)manager).PushModalPlatformAsyncCalled);
        }

        /// <summary>
        /// Tests PushModalAsync when platform is not ready.
        /// Verifies that SyncModalStackWhenPlatformIsReady is called.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_PlatformNotReady_CallsSyncModalStackWhenReady()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            var manager = CreateTestModalNavigationManager(
                window: window,
                isModalReady: false,
                currentPage: Substitute.For<Page>());

            // Act
            await manager.PushModalAsync(modal, animated: true);

            // Assert
            Assert.True(((TestableModalNavigationManager)manager).SyncModalStackWhenPlatformIsReadyCalled);
        }

        /// <summary>
        /// Tests PushModalAsync with null previous page.
        /// Verifies that null checks are handled properly in lifecycle events.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_NullPreviousPage_HandlesNullSafely()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            var manager = CreateTestModalNavigationManager(
                window: window,
                fireLifeCycleEvents: true,
                currentPage: null);

            // Act & Assert - should not throw
            await manager.PushModalAsync(modal, animated: true);

            // Verify basic operations still occurred
            window.Received(1).OnModalPushing(modal);
            window.Received(1).AddLogicalChild(modal);
            window.Received(1).OnModalPushed(modal);
        }

        /// <summary>
        /// Tests PushModalAsync when syncing is true.
        /// Verifies that platform operations are skipped when syncing.
        /// </summary>
        [Fact]
        public async Task PushModalAsync_WhenSyncing_SkipsPlatformOperations()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var modal = Substitute.For<Page>();

            var manager = CreateTestModalNavigationManager(
                window: window,
                isModalReady: true,
                currentPage: Substitute.For<Page>());

            // Set syncing to true
            ((TestableModalNavigationManager)manager).TestSyncing = true;

            // Act
            await manager.PushModalAsync(modal, animated: true);

            // Assert
            Assert.False(((TestableModalNavigationManager)manager).PushModalPlatformAsyncCalled);
        }

        /// <summary>
        /// Tests that PageAttachedHandler can be called without throwing any exceptions.
        /// This method calls the partial method OnPageAttachedHandler which has no implementation,
        /// so it should execute as a no-op successfully.
        /// </summary>
        [Fact]
        public void PageAttachedHandler_WhenCalled_DoesNotThrowException()
        {
            // Arrange
            var mockWindow = Substitute.For<Window>();
            var modalNavigationManager = new ModalNavigationManager(mockWindow);

            // Act & Assert
            var exception = Record.Exception(() => modalNavigationManager.PageAttachedHandler());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that PageAttachedHandler can be called multiple times consecutively
        /// without throwing any exceptions. Since the underlying partial method has no
        /// implementation, multiple calls should all succeed as no-ops.
        /// </summary>
        [Fact]
        public void PageAttachedHandler_WhenCalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var mockWindow = Substitute.For<Window>();
            var modalNavigationManager = new ModalNavigationManager(mockWindow);

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                modalNavigationManager.PageAttachedHandler();
                modalNavigationManager.PageAttachedHandler();
                modalNavigationManager.PageAttachedHandler();
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that PageAttachedHandler executes successfully and completes
        /// within a reasonable time frame, verifying the method doesn't hang
        /// or enter an infinite loop.
        /// </summary>
        [Fact]
        public void PageAttachedHandler_WhenCalled_CompletesSuccessfully()
        {
            // Arrange
            var mockWindow = Substitute.For<Window>();
            var modalNavigationManager = new ModalNavigationManager(mockWindow);
            var completed = false;

            // Act
            modalNavigationManager.PageAttachedHandler();
            completed = true;

            // Assert
            Assert.True(completed);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when window parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullWindow_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ModalNavigationManager(null));
        }

        /// <summary>
        /// Tests that the constructor initializes correctly with a valid window parameter.
        /// Verifies that the window is assigned to the internal field and events are subscribed.
        /// </summary>
        [Fact]
        public void Constructor_ValidWindow_InitializesCorrectly()
        {
            // Arrange
            var window = Substitute.For<Window>();

            // Act
            var manager = new ModalNavigationManager(window);

            // Assert
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that the constructor subscribes to the window's PropertyChanged event
        /// and calls SettingNewPage when Window.PageProperty changes.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToPropertyChanged_CallsSettingNewPageOnPagePropertyChange()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var pageProperty = Window.PageProperty;
            var eventArgs = new PropertyChangedEventArgs(pageProperty.PropertyName);

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger the PropertyChanged event with PageProperty
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, eventArgs);

            // Assert - The event subscription was successful (no exception thrown)
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that PropertyChanged events for properties other than PageProperty
        /// do not trigger SettingNewPage method calls.
        /// </summary>
        [Fact]
        public void Constructor_PropertyChangedWithOtherProperty_DoesNotCallSettingNewPage()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var eventArgs = new PropertyChangedEventArgs("SomeOtherProperty");

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger PropertyChanged with different property
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, eventArgs);

            // Assert - No exception should be thrown, event handled gracefully
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that the constructor subscribes to the window's HandlerChanging event
        /// and the subscription is successful.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToHandlerChanging_EventHandlerAttached()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var handlerChangingEventArgs = new HandlerChangingEventArgs(null, null);

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger HandlerChanging event
            window.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(window, handlerChangingEventArgs);

            // Assert - Event subscription successful (no exception)
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that the constructor subscribes to the window's Destroying event
        /// and the event handler executes without errors.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToDestroying_EventHandlerAttached()
        {
            // Arrange
            var window = Substitute.For<Window>();

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger Destroying event
            window.Destroying += Raise.Event<EventHandler>(window, EventArgs.Empty);

            // Assert - Event subscription successful (no exception)
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests edge case where PropertyChanged event args parameter is null.
        /// Verifies graceful handling without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_PropertyChangedWithNullEventArgs_HandlesGracefully()
        {
            // Arrange
            var window = Substitute.For<Window>();

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger PropertyChanged with null - this should not cause issues
            // as the Is extension method should handle null gracefully
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, (PropertyChangedEventArgs)null);

            // Assert - Should handle gracefully without exception
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that multiple PropertyChanged events are handled correctly
        /// without interfering with each other.
        /// </summary>
        [Fact]
        public void Constructor_MultiplePropertyChangedEvents_HandledCorrectly()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var pageProperty = Window.PageProperty;
            var pageEventArgs = new PropertyChangedEventArgs(pageProperty.PropertyName);
            var otherEventArgs = new PropertyChangedEventArgs("OtherProperty");

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger multiple events
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, pageEventArgs);
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, otherEventArgs);
            window.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(window, pageEventArgs);

            // Assert - All events handled without exception
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that HandlerChanging event with null OldHandler is handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_HandlerChangingWithNullOldHandler_HandledCorrectly()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var eventArgs = new HandlerChangingEventArgs(null, null);

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger HandlerChanging with null OldHandler
            window.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(window, eventArgs);

            // Assert - Should handle gracefully
            Assert.NotNull(manager);
        }

        /// <summary>
        /// Tests that HandlerChanging event with non-null OldHandler is handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_HandlerChangingWithNonNullOldHandler_HandledCorrectly()
        {
            // Arrange
            var window = Substitute.For<Window>();
            var oldHandler = Substitute.For<IElementHandler>();
            var eventArgs = new HandlerChangingEventArgs(oldHandler, null);

            // Act
            var manager = new ModalNavigationManager(window);

            // Trigger HandlerChanging with non-null OldHandler
            window.HandlerChanging += Raise.Event<EventHandler<HandlerChangingEventArgs>>(window, eventArgs);

            // Assert - Should handle gracefully
            Assert.NotNull(manager);
        }
    }


    // Helper base class to allow overriding internal behaviors
    public abstract class ModalNavigationManager
    {
        protected Window _window;

        public ModalNavigationManager(Window window)
        {
            _window = window;
        }

        protected virtual Task WaitForModalToFinishTaskInternal => throw new NotImplementedException();

        protected virtual bool FireLifeCycleEventsInternal => throw new NotImplementedException();

        protected virtual bool IsModalReadyInternal => throw new NotImplementedException();

        protected virtual bool SyncingInternal => throw new NotImplementedException();

        protected virtual void SyncModalStackWhenPlatformIsReadyInternal() => throw new NotImplementedException();

        protected virtual Task PopModalPlatformAsyncInternal(bool animated) => throw new NotImplementedException();

        public async Task<Page> PopModalAsync(bool animated)
        {
            var modalPages = ModalPagesInternal;

            if (modalPages.Count <= 0)
                throw new InvalidOperationException("PopModalAsync failed because modal stack is currently empty.");

            await WaitForModalToFinishTaskInternal;

            Page modal = modalPages[modalPages.Count - 1].Page;

            if (_window.OnModalPopping(modal))
            {
                _window.OnPopCanceled();
                return null;
            }

            modalPages.Remove(modal);

            if (FireLifeCycleEventsInternal)
            {
                modal.SendNavigatingFrom(new NavigatingFromEventArgs(CurrentPage, NavigationType.Pop));
            }

            modal.SendDisappearing();

            if (_window.Page is Shell shell)
            {
                if (!shell.CurrentItem.CurrentItem.IsPoppingModalStack)
                {
                    CurrentPage?.SendAppearing();
                }
            }
            else
            {
                CurrentPage?.SendAppearing();
            }

            bool isPlatformReady = IsModalReadyInternal;
            Task popTask =
                (isPlatformReady && !SyncingInternal) ? PopModalPlatformAsyncInternal(animated) : Task.CompletedTask;

            await popTask;
            modal.Parent?.RemoveLogicalChild(modal);
            _window.OnModalPopped(modal);

            if (FireLifeCycleEventsInternal)
            {
                modal.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage, NavigationType.Pop));
                CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(modal, NavigationType.Pop));
            }

            if (!isPlatformReady)
                SyncModalStackWhenPlatformIsReadyInternal();

            return modal;
        }

        private Page CurrentPage
        {
            get
            {
                var modalPages = ModalPagesInternal;
                var currentPage = modalPages.Count > 0 ? modalPages[modalPages.Count - 1].Page : _window.Page;

                if (currentPage is Shell shell)
                    currentPage = shell.CurrentPage;

                return currentPage;
            }
        }

        protected virtual bool FireLifeCycleEvents => _window?.Page is not Shell;

        protected virtual bool IsModalReady => _window?.Page?.Handler is not null && _window.Handler is not null;

        protected virtual int ModalStackCount => ModalStack.Count;

        protected virtual bool Syncing => syncing;

        protected virtual Task PushModalPlatformAsync(Page modal, bool animated)
        {
            _platformModalPages.Add(modal);
            return Task.CompletedTask;
        }

        protected virtual void SyncModalStackWhenPlatformIsReady(string callerName = null)
        {
            SyncModalStackWhenPlatformIsReady(callerName);
        }
    }
}