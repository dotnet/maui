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
	[InlineData(DispatcherWorkItemKind.Action)]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.Function)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public async Task CancellationCancelsReturnedTask(DispatcherWorkItemKind workItemKind)
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
			cancellation.Cancel();

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
}
