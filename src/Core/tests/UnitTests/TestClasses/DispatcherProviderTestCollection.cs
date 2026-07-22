#nullable enable
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// Tests that mutate the process-global DispatcherProvider (via DispatcherProvider.SetCurrent)
	// must not run in parallel with each other. xUnit runs distinct test classes concurrently by
	// default, so without a shared collection one class can reset the global provider to null (or
	// swap in a different stub) while another is mid-test, causing Dispatcher.GetForCurrentThread()
	// to return null and the test to fail with a NullReferenceException. Placing every such class in
	// this single collection serializes them without weakening any assertion.
	[CollectionDefinition(nameof(DispatcherProviderTestCollection), DisableParallelization = true)]
	public class DispatcherProviderTestCollection
	{
	}
}
