using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// DispatcherProvider stores the "current" provider in a process-global static
	// (DispatcherProvider.SetCurrent). Any test class that mutates it must not run in
	// parallel with the others: when one class clears the provider (SetCurrent(null))
	// while another is calling Dispatcher.GetForCurrentThread(), the latter falls back to
	// the real DispatcherProvider, which returns null on a non-UI background thread. That
	// null then surfaces as an intermittent NullReferenceException in the racing test.
	// Serialize every test class that touches this global state into a single collection
	// that also opts out of cross-collection parallelization.
	[CollectionDefinition(DispatcherProviderGlobalStateCollection.Name, DisableParallelization = true)]
	public class DispatcherProviderGlobalStateCollection
	{
		public const string Name = "DispatcherProviderGlobalState";
	}
}
