using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using WinFormsApplication = System.Windows.Forms.Application;

namespace Microsoft.AspNetCore.Components.WebView.Windows.UnitTests;

[Collection(WindowsDispatcherCollection.Name)]
public sealed class WindowsFormsDispatcherTests
{
	private readonly WindowsDispatcherFixture _fixture;

	public WindowsFormsDispatcherTests(WindowsDispatcherFixture fixture)
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
		ThreadExceptionEventHandler handler = (_, args) => unhandled.Enqueue(args.Exception);
		WinFormsApplication.ThreadException += handler;

		try
		{
			Exception exception = useAggregateException
				? new AggregateException(new InvalidOperationException("first"), new ArgumentException("second"))
				: new InvalidOperationException("sentinel");

			await DispatcherTestHelpers.AssertFailure(_fixture.WindowsFormsDispatcher, workItemKind, exception);
			await _fixture.WindowsFormsDispatcher.InvokeAsync(() => { }).WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Empty(unhandled);
		}
		finally
		{
			WinFormsApplication.ThreadException -= handler;
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
		ThreadExceptionEventHandler handler = (_, args) => unhandled.Enqueue(args.Exception);
		WinFormsApplication.ThreadException += handler;

		try
		{
			using var cancellation = new CancellationTokenSource();
			cancellation.Cancel();

			await DispatcherTestHelpers.AssertCancellation(
				_fixture.WindowsFormsDispatcher,
				workItemKind,
				cancellation.Token);
			await _fixture.WindowsFormsDispatcher.InvokeAsync(() => { }).WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Empty(unhandled);
		}
		finally
		{
			WinFormsApplication.ThreadException -= handler;
		}
	}

	[Theory]
	[InlineData(DispatcherWorkItemKind.AsyncAction)]
	[InlineData(DispatcherWorkItemKind.AsyncFunction)]
	public Task AsyncWorkItemsResumeOnDispatcherThread(DispatcherWorkItemKind workItemKind) =>
		DispatcherTestHelpers.AssertAsyncWorkItemResumesOnDispatcherThread(
			_fixture.WindowsFormsDispatcher,
			workItemKind);
}
