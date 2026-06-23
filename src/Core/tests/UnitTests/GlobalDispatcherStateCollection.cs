using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// Several test classes mutate process-global static state — specifically
	// DispatcherProvider.SetCurrent(...) (DispatcherProvider.s_currentProvider is a plain
	// static, not [ThreadStatic]) and MainThread.SetCustomImplementation(...). xUnit runs
	// distinct test classes in parallel by default, so without a shared collection those
	// classes race on that single global. The race surfaces intermittently as a
	// NullReferenceException (GetForCurrentThread() observing a null provider) or a
	// NotImplementedInReferenceAssemblyException (the MainThread bridge cleared mid-test).
	//
	// Joining every class that touches this global state into one DisableParallelization
	// collection serializes them — and keeps them from running alongside any other
	// collection — so the shared static is never mutated concurrently. This removes the
	// race without weakening any assertion, retrying, or disabling a test.
	[CollectionDefinition("Global Dispatcher State", DisableParallelization = true)]
	public class GlobalDispatcherStateCollection
	{
	}
}
