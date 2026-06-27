using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// Some tests mutate process-global static state: DispatcherProvider.Current
	// (a plain static field, not [ThreadStatic]) and the MainThread custom
	// implementation that MauiApp.Build() wires up on netstandard. When these
	// tests run in parallel with each other (xUnit runs separate test classes in
	// parallel by default), one class can flip the shared static out from under
	// another mid-test -- e.g. clearing the dispatcher provider between
	// MauiApp.Build() and the assertion, which surfaces intermittently as a
	// NotImplementedInReferenceAssemblyException from MainThread.IsMainThread.
	//
	// Assigning every class that touches this shared state to a single collection
	// with parallelization disabled makes their execution deterministic without
	// weakening any assertion.
	[CollectionDefinition("GlobalDispatcherState", DisableParallelization = true)]
	public class GlobalDispatcherStateCollection
	{
	}
}
