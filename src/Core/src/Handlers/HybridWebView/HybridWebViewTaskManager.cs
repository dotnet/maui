using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers;

internal class HybridWebViewTaskManager : IHybridWebViewTaskManager
{
	private int _lastTaskId;
	private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _asyncTaskCallbacks = new();

	public HybridWebViewTask CreateTask()
	{
		var taskId = Interlocked.Increment(ref _lastTaskId);
		var taskIdString = taskId.ToString("0", CultureInfo.InvariantCulture);

		var tcs = new TaskCompletionSource<string?>();

		if (!_asyncTaskCallbacks.TryAdd(taskIdString, tcs))
		{
			throw new InvalidOperationException($"Unable to add a new task with new ID {taskIdString} to the task manager.");
		}

		return new(taskIdString, tcs);
	}

	public void SetTaskCompleted(string taskId, string result)
	{
		if (_asyncTaskCallbacks.TryRemove(taskId, out var callback))
		{
			callback.SetResult(result);
		}
	}

	public void SetTaskFailed(string taskId, Exception exception)
	{
		if (_asyncTaskCallbacks.TryRemove(taskId, out var callback))
		{
			callback.SetException(exception);
		}
	}
}
