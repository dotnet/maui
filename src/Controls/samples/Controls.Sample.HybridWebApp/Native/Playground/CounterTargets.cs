using Maui.HybridApp;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

internal sealed record CounterTargetResult(string InstanceId, int Count);

internal sealed class LocalCounterTarget
{
	readonly string _id = Guid.NewGuid().ToString("D");
	int _count;
	[HybridAction("sample.localCounter")]
	internal CounterTargetResult Count(int increment = 0) => new(_id, Interlocked.Add(ref _count, increment));
}

internal sealed class SharedCounterTarget
{
	readonly string _id = Guid.NewGuid().ToString("D");
	int _count;
	[HybridAction("sample.sharedCounter")]
	internal CounterTargetResult Count(int increment = 0) => new(_id, Interlocked.Add(ref _count, increment));
}
