using System.Runtime.CompilerServices;
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
	[InlineData(DispatcherWorkItemKind.Action, true)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, true)]
	[InlineData(DispatcherWorkItemKind.Function, true)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, true)]
	[InlineData(DispatcherWorkItemKind.Action, false)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, false)]
	[InlineData(DispatcherWorkItemKind.Function, false)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, false)]
	public async Task OperationCanceledExceptionCancelsReturnedTask(
		DispatcherWorkItemKind workItemKind,
		bool cancellationRequested)
	{
		using var cancellation = new CancellationTokenSource();
		if (cancellationRequested)
		{
			cancellation.Cancel();
		}
		var task = InvokeFailure(workItemKind, new OperationCanceledException(cancellation.Token));

		var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

		Assert.Equal(cancellation.Token, thrown.CancellationToken);
		Assert.True(task.IsCanceled);
		Assert.False(task.IsFaulted);
	}

	[Fact]
	public async Task ObserveExceptionsLogsFailure()
	{
		var exception = new InvalidOperationException("sentinel");
		Exception? loggedException = null;

		await Task.FromException(exception).ObserveExceptionsAsync(
			new CallbackLogger(ex => loggedException = ex));

		Assert.Same(exception, loggedException);
	}

	[Fact]
	public async Task ObserveExceptionsIgnoresCanceledTask()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		await Task.FromCanceled(cancellation.Token).ObserveExceptionsAsync(
			new CallbackLogger(_ => Assert.Fail("Cancellation should not be logged.")));
	}

	[Fact]
	public async Task ObserveExceptionsLogsFaultedCancellationException()
	{
		var exception = new OperationCanceledException("faulted cancellation");
		Exception? loggedException = null;

		await Task.FromException(exception).ObserveExceptionsAsync(
			new CallbackLogger(ex => loggedException = ex));

		Assert.Same(exception, loggedException);
	}

	[Fact]
	public async Task ObserveExceptionsPropagatesLoggerFailureThroughReturnedTask()
	{
		var loggingException = new InvalidOperationException("logger failure");
		var observer = Task.FromException(new InvalidOperationException("work item failure"))
			.ObserveExceptionsAsync(new CallbackLogger(_ => throw loggingException));

		var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => observer);

		Assert.Same(loggingException, thrown);
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

	[MethodImpl(MethodImplOptions.NoInlining)]
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

	private sealed class CallbackLogger(Action<Exception> logException) : ILogger
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
				logException(exception);
			}
		}
	}
}
