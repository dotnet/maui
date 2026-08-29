using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests;

public enum DispatcherWorkItemKind
{
	Action,
	AsyncAction,
	Function,
	AsyncFunction,
}

public sealed class MauiDispatcherTests
{
	private readonly MauiDispatcher _dispatcher = new(new ImmediateDispatcher());

	[Theory]
	[InlineData(DispatcherWorkItemKind.Action, false)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, false)]
	[InlineData(DispatcherWorkItemKind.Function, false)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, false)]
	[InlineData(DispatcherWorkItemKind.Action, true)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, true)]
	[InlineData(DispatcherWorkItemKind.Function, true)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, true)]
	public async Task ExceptionsFaultReturnedTask(
		DispatcherWorkItemKind workItemKind,
		bool useAggregateException)
	{
		Exception exception = useAggregateException
			? new AggregateException(new InvalidOperationException("first"), new ArgumentException("second"))
			: new InvalidOperationException("sentinel");
		var task = InvokeFailure(workItemKind, exception);

		var thrown = await Assert.ThrowsAnyAsync<Exception>(() => task);

		Assert.Same(exception, thrown);
		Assert.True(task.IsFaulted);
		Assert.False(task.IsCanceled);
		Assert.Same(exception, Assert.Single(task.Exception!.InnerExceptions));
		Assert.Contains(nameof(Throw), thrown.StackTrace, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(DispatcherWorkItemKind.Action)]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.Function)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public async Task CancellationCancelsReturnedTask(DispatcherWorkItemKind workItemKind)
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var task = InvokeFailure(workItemKind, new OperationCanceledException(cancellation.Token));

		var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

		Assert.Equal(cancellation.Token, thrown.CancellationToken);
		Assert.True(task.IsCanceled);
		Assert.False(task.IsFaulted);
	}

	[Fact]
	public async Task FireAndForgetLogsFailure()
	{
		var exception = new InvalidOperationException("sentinel");
		var loggedException = new TaskCompletionSource<Exception>(
			TaskCreationOptions.RunContinuationsAsynchronously);

		Task.FromException(exception).FireAndForget(new CallbackLogger(loggedException));

		Assert.Same(
			exception,
			await loggedException.Task.WaitAsync(TimeSpan.FromSeconds(5)));
	}

	private Task InvokeFailure(DispatcherWorkItemKind workItemKind, Exception exception) =>
		workItemKind switch
		{
			DispatcherWorkItemKind.Action => _dispatcher.InvokeAsync((Action)(() => Throw(exception))),
			DispatcherWorkItemKind.AsyncAction => _dispatcher.InvokeAsync(() => ThrowAsync(exception)),
			DispatcherWorkItemKind.Function => _dispatcher.InvokeAsync<int>(() =>
			{
				Throw(exception);
				return 0;
			}),
			DispatcherWorkItemKind.AsyncFunction => _dispatcher.InvokeAsync<int>(() => ThrowAsync<int>(exception)),
			_ => throw new ArgumentOutOfRangeException(nameof(workItemKind)),
		};

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

	private sealed class ImmediateDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action) =>
			throw new NotSupportedException();

		public IDispatcherTimer CreateTimer() =>
			throw new NotSupportedException();
	}

	private sealed class CallbackLogger(TaskCompletionSource<Exception> loggedException) : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (exception is not null)
			{
				loggedException.TrySetResult(exception);
			}
		}
	}
}
