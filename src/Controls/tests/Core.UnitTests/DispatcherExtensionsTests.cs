using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class DispatcherExtensionsTest : BaseTestFixture
{
	[Fact]
	public void DispatcherGetterPreservesDispatcherCapturedDuringHandlerAttachment()
	{
		MockBindable parent;
		MockBindable bindable;
		DispatcherProviderStubOptions.SkipDispatcherCreation = true;
		try
		{
			parent = new MockBindable();
			bindable = new MockBindable { Parent = parent };
		}
		finally
		{
			DispatcherProviderStubOptions.SkipDispatcherCreation = false;
		}

		using var lookupEntered = new ManualResetEventSlim();
		using var releaseLookup = new ManualResetEventSlim();
		var backgroundDispatcher = Substitute.For<IDispatcher>();
		var parentServices = Substitute.For<IServiceProvider>();
		parentServices.GetService(typeof(IDispatcher)).Returns(_ =>
		{
			lookupEntered.Set();
			releaseLookup.Wait();
			return backgroundDispatcher;
		});
		var parentMauiContext = Substitute.For<IMauiContext>();
		parentMauiContext.Services.Returns(parentServices);
		var parentHandler = Substitute.For<IViewHandler>();
		IMauiContext currentParentMauiContext = null;
		parentHandler.MauiContext.Returns(_ => currentParentMauiContext);
		parent.Handler = parentHandler;
		currentParentMauiContext = parentMauiContext;

		var attachedDispatcher = Substitute.For<IDispatcher>();
		var attachedServices = Substitute.For<IServiceProvider>();
		attachedServices.GetService(typeof(IDispatcher)).Returns(attachedDispatcher);
		var attachedMauiContext = Substitute.For<IMauiContext>();
		attachedMauiContext.Services.Returns(attachedServices);
		var attachedHandler = Substitute.For<IViewHandler>();
		attachedHandler.MauiContext.Returns(attachedMauiContext);

		IDispatcher resolvedDispatcher = null;
		Exception getterException = null;
		var getterThread = new Thread(() =>
		{
			try
			{
				resolvedDispatcher = bindable.Dispatcher;
			}
			catch (Exception exception)
			{
				getterException = exception;
			}
		});
		getterThread.IsBackground = true;

		getterThread.Start();
		try
		{
			Assert.True(lookupEntered.Wait(TimeSpan.FromSeconds(5)), "Dispatcher lookup did not start.");
			bindable.Handler = attachedHandler;
		}
		finally
		{
			releaseLookup.Set();
		}

		Assert.True(getterThread.Join(TimeSpan.FromSeconds(5)), "Dispatcher lookup did not complete.");
		Assert.Null(getterException);
		Assert.Same(attachedDispatcher, resolvedDispatcher);
		Assert.Same(attachedDispatcher, bindable.Dispatcher);
	}

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