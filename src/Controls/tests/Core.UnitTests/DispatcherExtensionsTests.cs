using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

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