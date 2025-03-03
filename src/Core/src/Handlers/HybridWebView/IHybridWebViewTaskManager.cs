using System;

namespace Microsoft.Maui.Handlers;

internal interface IHybridWebViewTaskManager
{
	HybridWebViewTask CreateTask();

	void SetTaskCompleted(string taskId, string result);

	void SetTaskFailed(string taskId, Exception exception);
}
