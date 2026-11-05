using System.Collections.Concurrent;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

[assembly: TestFramework(typeof(Microsoft.Maui.TestUtils.UnitTests.UiProofFramework))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Microsoft.Maui.TestUtils.UnitTests;

public sealed class UiProofFramework : WindowsUiTestFramework
{
	internal static readonly TestUiContext Context = new();

	public UiProofFramework() => ConfigureDispatcher(Context.InvokeAsync);
}

internal sealed class TestUiContext : SynchronizationContext
{
	readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();
	internal int ThreadId { get; private set; }

	public TestUiContext()
	{
		using (ExecutionContext.SuppressFlow())
		{
			new Thread(() =>
			{
				ThreadId = Environment.CurrentManagedThreadId;
				SetSynchronizationContext(this);
				foreach (var item in _queue.GetConsumingEnumerable())
					item.Callback(item.State);
			})
			{ IsBackground = true, Name = "xUnit UI execution proof" }.Start();
		}
	}

	public override void Post(SendOrPostCallback callback, object? state) => _queue.Add((callback, state));

	internal Task InvokeAsync(Func<Task> action)
	{
		var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		Post(async _ =>
		{
			try
			{ await action(); completion.SetResult(); }
			catch (Exception exception) { completion.SetException(exception); }
		}, null);
		return completion.Task;
	}
}

public class WindowsUiTestFrameworkTests
{
	[Fact]
	public async Task PreExistingUiThreadReceivesTheCallersExecutionContext()
	{
		var marker = new AsyncLocal<string?> { Value = "MTP caller context" };
		await WindowsUiTestFramework.RunOnUiThread(async () =>
		{
			Assert.Equal("MTP caller context", marker.Value);
			Assert.Equal(UiProofFramework.Context.ThreadId, Environment.CurrentManagedThreadId);
			await Task.Yield();
			Assert.Equal("MTP caller context", marker.Value);
			Assert.Same(UiProofFramework.Context, SynchronizationContext.Current);
		});
	}

	[Fact]
	public async Task DispatchedFailuresAreNotConvertedIntoSuccessfulExecution() =>
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			WindowsUiTestFramework.RunOnUiThread(() => throw new InvalidOperationException("UI dispatch failure")));

	[Fact]
	public void PlainFactActuallyExecutesOnTheRegisteredUiThread()
	{
		Assert.Equal(UiProofFramework.Context.ThreadId, Environment.CurrentManagedThreadId);
		Assert.Same(UiProofFramework.Context, SynchronizationContext.Current);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public async Task TheoryRowsKeepTheUiContextAcrossAwait(int value)
	{
		await Task.Yield();
		Assert.Equal(UiProofFramework.Context.ThreadId, Environment.CurrentManagedThreadId);
		Assert.Same(UiProofFramework.Context, SynchronizationContext.Current);
		Assert.InRange(value, 1, 2);
	}
}
