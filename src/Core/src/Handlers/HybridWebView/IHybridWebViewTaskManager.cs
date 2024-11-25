using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Microsoft.Maui.Handlers
{
	internal interface IHybridWebViewTaskManager
	{
		int GetNextInvokeTaskId();
		ConcurrentDictionary<string, TaskCompletionSource<string>> AsyncTaskCallbacks { get; }
	}
}
