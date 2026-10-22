using System.Threading;
using Microsoft.AspNetCore.Components.WebView;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests;

public class StaticContentHotReloadManagerTests
{
	[Fact]
	public async Task ConcurrentAttachRunsRegistrationOnce()
	{
		var registration = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var attachCount = 0;
		var state = new StaticContentHotReloadManager.AttachmentState(
			() =>
			{
				Interlocked.Increment(ref attachCount);
				return registration.Task;
			},
			() => Task.CompletedTask);
		const int callerCount = 8;
		using var barrier = new Barrier(callerCount);

		var calls = Enumerable.Range(0, callerCount)
			.Select(_ => Task.Run(() =>
			{
				Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(30)));
				return new[] { state.Attach() };
			}))
			.ToArray();

		var attachTasks = (await Task.WhenAll(calls))
			.Select(result => result[0])
			.ToArray();

		Assert.Equal(1, attachCount);
		Assert.All(attachTasks, task => Assert.Same(attachTasks[0], task));

		registration.SetResult();
		await Task.WhenAll(attachTasks);
	}

	[Fact]
	public async Task FailedAsynchronousAttachCleansUpAndAllowsRetry()
	{
		var firstRegistration = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var attachCount = 0;
		var removeCount = 0;
		var state = new StaticContentHotReloadManager.AttachmentState(
			() => Interlocked.Increment(ref attachCount) == 1
				? firstRegistration.Task
				: Task.CompletedTask,
			() =>
			{
				Interlocked.Increment(ref removeCount);
				return Task.CompletedTask;
			});

		var failedAttach = state.Attach();
		firstRegistration.SetException(new TestException());

		await Assert.ThrowsAsync<TestException>(() => failedAttach);
		Assert.Equal(1, removeCount);

		var retry = state.Attach();

		await retry;
		Assert.Equal(2, attachCount);
	}

	[Fact]
	public async Task AttachAfterDiscardedDetachWaitsForRemoval()
	{
		var removal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var attachCount = 0;
		var removeCount = 0;
		var state = new StaticContentHotReloadManager.AttachmentState(
			() =>
			{
				Interlocked.Increment(ref attachCount);
				return Task.CompletedTask;
			},
			() => Interlocked.Increment(ref removeCount) == 1
				? removal.Task
				: Task.CompletedTask);

		await state.Attach();
		_ = state.Detach();

		var secondAttach = state.Attach();

		Assert.False(secondAttach.IsCompleted);
		Assert.Equal(1, attachCount);

		removal.SetResult();
		await secondAttach;

		Assert.Equal(2, attachCount);
	}

	[Fact]
	public async Task DetachDoesNotPropagatePendingAttachFailure()
	{
		var registration = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var removeCount = 0;
		var state = new StaticContentHotReloadManager.AttachmentState(
			() => registration.Task,
			() =>
			{
				Interlocked.Increment(ref removeCount);
				return Task.CompletedTask;
			});

		var attach = state.Attach();
		var detach = state.Detach();

		registration.SetException(new TestException());

		await Assert.ThrowsAsync<TestException>(() => attach);
		Assert.NotNull(detach);
		await detach!;

		Assert.True(detach.IsCompletedSuccessfully);
		Assert.Equal(2, removeCount);
	}

	private sealed class TestException : Exception
	{
	}
}
