#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

public abstract class MauiTestActivity : MauiAppCompatActivity
{
	int _started;
	public TaskCompletionSource<NativeTestResult> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

	protected override void OnPostResume()
	{
		base.OnPostResume();
		if (Interlocked.Exchange(ref _started, 1) != 0)
			return;

		MauiTestInstrumentation.Current.CurrentExecutionContext = this;
		_ = RunTestsAsync();
	}

	async Task RunTestsAsync()
	{
		try
		{
			var result = await Task.Run(() => MauiTestInstrumentation.Current.RunTestsAsync());
			Completion.TrySetResult(result);
		}
		catch (Exception exception)
		{
			Completion.TrySetException(exception);
		}
		finally
		{
			RunOnUiThread(Finish);
		}
	}
}
