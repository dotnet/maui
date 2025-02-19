using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	internal interface IHybridWebViewTaskManager
	{
		int GetNextInvokeTaskId();
		ConcurrentDictionary<string, TaskCompletionSource<string>> AsyncTaskCallbacks { get; }
	}
}
