using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DispatcherExtensionsTest : BaseTestFixture
    {
        [Fact]
        public void DispatchIfRequired_ShouldCallDispatch_WhenDispatchIsRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(true);

            int executionCount = 0;
            Action testAction = () => executionCount++;

            // Configure the mock to actually execute the action when Dispatch is called
            dispatcher.Dispatch(Arg.Do<Action>(action => action()));

            // Act
            dispatcher.DispatchIfRequired(testAction);

            // Assert
            dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void DispatchIfRequired_ShouldExecuteAction_WhenDispatchIsNotRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(false);

            int executionCount = 0;
            Action testAction = () => executionCount++;

            // Act
            dispatcher.DispatchIfRequired(testAction);

            // Assert
            dispatcher.DidNotReceive().Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public async Task DispatchIfRequiredAsync_ShouldCallDispatchAsync_WhenDispatchIsRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(true);

            int executionCount = 0;
            Action testAction = () => executionCount++;

            // Configure the mock to actually execute the action when Dispatch is called
            dispatcher.Dispatch(Arg.Do<Action>(action => action())).Returns(true);

            // Act
            await dispatcher.DispatchIfRequiredAsync(testAction);

            // Assert
            dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public async Task DispatchIfRequiredAsync_ShouldExecuteAction_WhenDispatchIsNotRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(false);

            int executionCount = 0;
            Action testAction = () => executionCount++;

            // Act
            await dispatcher.DispatchIfRequiredAsync(testAction);

            // Assert
            dispatcher.DidNotReceive().Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public async Task DispatchIfRequiredAsync_FuncTask_ShouldCallDispatch_WhenDispatchIsRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(true);

            int executionCount = 0;
            Func<Task> testFunc = async () =>
            {
                await Task.Delay(1);
                executionCount++;
            };

            // Configure the mock to actually execute the function when Dispatch is called
            dispatcher.Dispatch(Arg.Do<Action>(action => action())).Returns(true);

            // Act
            await dispatcher.DispatchIfRequiredAsync(testFunc);

            // Assert
            dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public async Task DispatchIfRequiredAsync_FuncTask_ShouldExecuteFunction_WhenDispatchIsNotRequired()
        {
            // Arrange
            var dispatcher = Substitute.For<IDispatcher>();
            dispatcher.IsDispatchRequired.Returns(false);

            int executionCount = 0;
            Func<Task> testFunc = async () =>
            {
                await Task.Delay(1);
                executionCount++;
            };

            // Act
            await dispatcher.DispatchIfRequiredAsync(testFunc);

            // Assert
            dispatcher.DidNotReceive().Dispatch(Arg.Any<Action>());
            Assert.Equal(1, executionCount);
        }
    }

    /// <summary>
    /// Unit tests for the DispatcherExtensions.FindDispatcher method.
    /// Tests various scenarios for finding dispatchers in different BindableObject hierarchies.
    /// </summary>
    public partial class DispatcherExtensionsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that FindDispatcher throws InvalidOperationException when bindableObject is null.
        /// Verifies null input handling and appropriate exception throwing.
        /// </summary>
        [Fact]
        public void FindDispatcher_NullBindableObject_ThrowsInvalidOperationException()
        {
            // Arrange
            BindableObject nullBindableObject = null;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                nullBindableObject.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher returns the dispatcher from Element's MauiContext services.
        /// Verifies the first condition path when Element has a MauiContext with IDispatcher service.
        /// </summary>
        [Fact]
        public void FindDispatcher_ElementWithMauiContextAndDispatcher_ReturnsDispatcher()
        {
            // Arrange
            var expectedDispatcher = Substitute.For<IDispatcher>();
            var mauiContext = Substitute.For<IMauiContext>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var element = Substitute.For<Element>();

            serviceProvider.GetService<IDispatcher>().Returns(expectedDispatcher);
            mauiContext.Services.Returns(serviceProvider);
            element.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act
            var result = element.FindDispatcher();

            // Assert
            Assert.Same(expectedDispatcher, result);
        }

        /// <summary>
        /// Tests that FindDispatcher continues to next conditions when Element has no MauiContext.
        /// Verifies that the method doesn't return early when Element.FindMauiContext returns null.
        /// </summary>
        [Fact]
        public void FindDispatcher_ElementWithoutMauiContext_ContinuesToNextConditions()
        {
            // Arrange
            var element = Substitute.For<Element>();
            element.FindMauiContext(Arg.Any<bool>()).Returns((IMauiContext)null);

            // Act & Assert
            // Should throw InvalidOperationException since no other dispatchers are available
            var exception = Assert.Throws<InvalidOperationException>(() =>
                element.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher continues when Element has MauiContext but no IDispatcher service.
        /// Verifies that the method continues to next conditions when services don't contain IDispatcher.
        /// </summary>
        [Fact]
        public void FindDispatcher_ElementWithMauiContextButNoDispatcherService_ContinuesToNextConditions()
        {
            // Arrange
            var mauiContext = Substitute.For<IMauiContext>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var element = Substitute.For<Element>();

            serviceProvider.GetService<IDispatcher>().Returns((IDispatcher)null);
            mauiContext.Services.Returns(serviceProvider);
            element.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act & Assert
            // Should throw InvalidOperationException since no other dispatchers are available
            var exception = Assert.Throws<InvalidOperationException>(() =>
                element.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher returns optional application dispatcher for Application objects.
        /// Verifies the Application-specific path when GetOptionalApplicationDispatcher returns a dispatcher.
        /// </summary>
        [Fact]
        public void FindDispatcher_ApplicationWithOptionalApplicationDispatcher_ReturnsDispatcher()
        {
            // Arrange
            var expectedDispatcher = Substitute.For<IDispatcher>();
            var mauiContext = Substitute.For<IMauiContext>();
            var keyedServiceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
            var application = Substitute.For<Application>();

            // Setup the keyed service provider to return the dispatcher
            ((IKeyedServiceProvider)keyedServiceProvider).GetKeyedService<IDispatcher>(typeof(IApplication))
                .Returns(expectedDispatcher);

            mauiContext.Services.Returns(keyedServiceProvider);
            application.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act
            var result = application.FindDispatcher();

            // Assert
            Assert.Same(expectedDispatcher, result);
        }

        /// <summary>
        /// Tests that FindDispatcher returns regular IDispatcher service for Application when optional dispatcher is null.
        /// Verifies the fallback path within Application handling when GetOptionalApplicationDispatcher returns null.
        /// </summary>
        [Fact]
        public void FindDispatcher_ApplicationWithoutOptionalButWithRegularDispatcher_ReturnsDispatcher()
        {
            // Arrange
            var expectedDispatcher = Substitute.For<IDispatcher>();
            var mauiContext = Substitute.For<IMauiContext>();
            var keyedServiceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
            var application = Substitute.For<Application>();

            // Setup to return null for optional dispatcher but dispatcher for regular service
            ((IKeyedServiceProvider)keyedServiceProvider).GetKeyedService<IDispatcher>(typeof(IApplication))
                .Returns((IDispatcher)null);
            keyedServiceProvider.GetService<IDispatcher>().Returns(expectedDispatcher);

            mauiContext.Services.Returns(keyedServiceProvider);
            application.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act
            var result = application.FindDispatcher();

            // Assert
            Assert.Same(expectedDispatcher, result);
        }

        /// <summary>
        /// Tests that FindDispatcher continues when Application has MauiContext but no dispatcher services.
        /// Verifies that the method continues to next conditions when Application's services have no dispatchers.
        /// </summary>
        [Fact]
        public void FindDispatcher_ApplicationWithMauiContextButNoDispatchers_ContinuesToNextConditions()
        {
            // Arrange
            var mauiContext = Substitute.For<IMauiContext>();
            var keyedServiceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
            var application = Substitute.For<Application>();

            // Setup to return null for both optional and regular dispatchers
            ((IKeyedServiceProvider)keyedServiceProvider).GetKeyedService<IDispatcher>(typeof(IApplication))
                .Returns((IDispatcher)null);
            keyedServiceProvider.GetService<IDispatcher>().Returns((IDispatcher)null);

            mauiContext.Services.Returns(keyedServiceProvider);
            application.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act & Assert
            // Should throw InvalidOperationException since no other dispatchers are available
            var exception = Assert.Throws<InvalidOperationException>(() =>
                application.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher continues when Application has no MauiContext.
        /// Verifies that the method continues to next conditions when Application.FindMauiContext returns null.
        /// </summary>
        [Fact]
        public void FindDispatcher_ApplicationWithoutMauiContext_ContinuesToNextConditions()
        {
            // Arrange
            var application = Substitute.For<Application>();
            application.FindMauiContext(Arg.Any<bool>()).Returns((IMauiContext)null);

            // Act & Assert
            // Should throw InvalidOperationException since no other dispatchers are available
            var exception = Assert.Throws<InvalidOperationException>(() =>
                application.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher throws InvalidOperationException for BindableObject that is neither Element nor Application.
        /// Verifies behavior when input is a basic BindableObject that doesn't match any specific conditions.
        /// </summary>
        [Fact]
        public void FindDispatcher_BasicBindableObject_ThrowsInvalidOperationException()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                bindableObject.FindDispatcher());

            Assert.Equal("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.",
                exception.Message);
        }

        /// <summary>
        /// Tests that FindDispatcher works with non-keyed service provider for Application.
        /// Verifies Application handling when service provider doesn't implement IKeyedServiceProvider.
        /// </summary>
        [Fact]
        public void FindDispatcher_ApplicationWithNonKeyedServiceProvider_UsesRegularDispatcher()
        {
            // Arrange
            var expectedDispatcher = Substitute.For<IDispatcher>();
            var mauiContext = Substitute.For<IMauiContext>();
            var serviceProvider = Substitute.For<IServiceProvider>(); // Not implementing IKeyedServiceProvider
            var application = Substitute.For<Application>();

            serviceProvider.GetService<IDispatcher>().Returns(expectedDispatcher);
            mauiContext.Services.Returns(serviceProvider);
            application.FindMauiContext(Arg.Any<bool>()).Returns(mauiContext);

            // Act
            var result = application.FindDispatcher();

            // Assert
            Assert.Same(expectedDispatcher, result);
        }
    }
}
