using ComponentsDispatcher = Microsoft.AspNetCore.Components.Dispatcher;

namespace Microsoft.AspNetCore.Components.WebView.Windows.UnitTests;

public enum DispatcherWorkItemKind
{
	Action,
	AsyncAction,
	Function,
	AsyncFunction,
}

internal static class DispatcherTestHelpers
{
	public static Task InvokeFailure(
		ComponentsDispatcher dispatcher,
		DispatcherWorkItemKind workItemKind,
		Exception exception) =>
		workItemKind switch
		{
			DispatcherWorkItemKind.Action => dispatcher.InvokeAsync((Action)(() => Throw(exception))),
			DispatcherWorkItemKind.AsyncAction => dispatcher.InvokeAsync(() => ThrowAsync(exception)),
			DispatcherWorkItemKind.Function => dispatcher.InvokeAsync<int>(() =>
			{
				Throw(exception);
				return 0;
			}),
			DispatcherWorkItemKind.AsyncFunction => dispatcher.InvokeAsync<int>(() => ThrowAsync<int>(exception)),
			_ => throw new ArgumentOutOfRangeException(nameof(workItemKind)),
		};

	public static async Task AssertFailure(
		ComponentsDispatcher dispatcher,
		DispatcherWorkItemKind workItemKind,
		Exception exception)
	{
		var task = InvokeFailure(dispatcher, workItemKind, exception);

		var thrown = await Assert.ThrowsAnyAsync<Exception>(
			() => task.WaitAsync(TimeSpan.FromSeconds(5)));

		Assert.Same(exception, thrown);
		Assert.True(task.IsFaulted);
		Assert.False(task.IsCanceled);
		Assert.Same(exception, Assert.Single(task.Exception!.InnerExceptions));
		Assert.Contains(nameof(Throw), thrown.StackTrace, StringComparison.Ordinal);
	}

	public static async Task AssertCancellation(
		ComponentsDispatcher dispatcher,
		DispatcherWorkItemKind workItemKind,
		CancellationToken cancellationToken)
	{
		var exception = new OperationCanceledException(cancellationToken);
		var task = InvokeFailure(dispatcher, workItemKind, exception);

		var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => task.WaitAsync(TimeSpan.FromSeconds(5)));

		Assert.Equal(cancellationToken, thrown.CancellationToken);
		Assert.True(task.IsCanceled);
		Assert.False(task.IsFaulted);
	}

	private static void Throw(Exception exception) => throw exception;

	private static async Task ThrowAsync(Exception exception)
	{
		await Task.Yield();
		Throw(exception);
	}

	private static async Task<TResult> ThrowAsync<TResult>(Exception exception)
	{
		await Task.Yield();
		Throw(exception);
		return default!;
	}
}
