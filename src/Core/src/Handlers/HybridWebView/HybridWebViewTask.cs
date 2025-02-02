using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers;

internal record struct HybridWebViewTask(string TaskId, TaskCompletionSource<string?> TaskCompletionSource);
