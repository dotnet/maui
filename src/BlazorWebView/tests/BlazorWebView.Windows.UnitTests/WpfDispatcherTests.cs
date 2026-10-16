using System.Collections.Concurrent;
using DispatcherUnhandledExceptionEventHandler = System.Windows.Threading.DispatcherUnhandledExceptionEventHandler;

namespace Microsoft.AspNetCore.Components.WebView.Windows.UnitTests;

[Collection(WindowsDispatcherCollection.Name)]
public sealed class WpfDispatcherTests
{
	private readonly WindowsDispatcherFixture _fixture;

	public WpfDispatcherTests(WindowsDispatcherFixture fixture)
	{
		_fixture = fixture;
	}

	[Theory]
	[InlineData(DispatcherWorkItemKind.Action, false)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, false)]
	[InlineData(DispatcherWorkItemKind.Function, false)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, false)]
	[InlineData(DispatcherWorkItemKind.Action, true)]
	[InlineData(DispatcherWorkItemKind.AsyncAction, true)]
	[InlineData(DispatcherWorkItemKind.Function, true)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction, true)]
	public async Task ExceptionsOnlyFaultReturnedTask(
		DispatcherWorkItemKind workItemKind,
		bool useAggregateException)
	{
		var unhandled = new ConcurrentQueue<Exception>();
		DispatcherUnhandledExceptionEventHandler handler = (_, args) =>
		{
			unhandled.Enqueue(args.Exception);
			args.Handled = true;
		};
		_fixture.WpfNativeDispatcher.UnhandledException += handler;

		try
		{
			Exception exception = useAggregateException
				? new AggregateException(new InvalidOperationException("first"), new ArgumentException("second"))
				: new InvalidOperationException("sentinel");

			Assert.False(_fixture.WpfDispatcher.CheckAccess());
			await DispatcherTestHelpers.AssertFailure(_fixture.WpfDispatcher, workItemKind, exception);
			await _fixture.WpfNativeDispatcher.InvokeAsync(() => { }).Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Empty(unhandled);
		}
		finally
		{
			_fixture.WpfNativeDispatcher.UnhandledException -= handler;
		}
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
		var unhandled = new ConcurrentQueue<Exception>();
		DispatcherUnhandledExceptionEventHandler handler = (_, args) =>
		{
			unhandled.Enqueue(args.Exception);
			args.Handled = true;
		};
		_fixture.WpfNativeDispatcher.UnhandledException += handler;

		try
		{
			using var cancellation = new CancellationTokenSource();
			if (cancellationRequested)
			{
				cancellation.Cancel();
			}

			Assert.False(_fixture.WpfDispatcher.CheckAccess());
			await DispatcherTestHelpers.AssertCancellation(
				_fixture.WpfDispatcher,
				workItemKind,
				cancellation.Token);
			await _fixture.WpfNativeDispatcher.InvokeAsync(() => { }).Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Empty(unhandled);
		}
		finally
		{
			_fixture.WpfNativeDispatcher.UnhandledException -= handler;
		}
	}

	[Theory]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public Task AsyncWorkItemsResumeOnDispatcherThread(DispatcherWorkItemKind workItemKind) =>
		DispatcherTestHelpers.AssertAsyncWorkItemResumesOnDispatcherThread(
			_fixture.WpfDispatcher,
			workItemKind);

	[Theory]
	[InlineData(DispatcherWorkItemKind.Action)]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.Function)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public Task CheckAccessFastPathPreservesFailureSemantics(DispatcherWorkItemKind workItemKind) =>
		_fixture.InvokeOnWpfDispatcher(async () =>
		{
			Assert.True(_fixture.WpfDispatcher.CheckAccess());

			await DispatcherTestHelpers.AssertFailure(
				_fixture.WpfDispatcher,
				workItemKind,
				new InvalidOperationException("sentinel"));

			using var canceled = new CancellationTokenSource();
			canceled.Cancel();
			await DispatcherTestHelpers.AssertCancellation(
				_fixture.WpfDispatcher,
				workItemKind,
				canceled.Token);
			await DispatcherTestHelpers.AssertCancellation(
				_fixture.WpfDispatcher,
				workItemKind,
				CancellationToken.None);
		});

	[Theory]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public Task AsyncWorkItemsResumeOnDispatcherThreadWhenAlreadyOnDispatcher(
		DispatcherWorkItemKind workItemKind) =>
		_fixture.InvokeOnWpfDispatcher(() =>
			DispatcherTestHelpers.AssertAsyncWorkItemResumesOnDispatcherThread(
				_fixture.WpfDispatcher,
				workItemKind));
}
