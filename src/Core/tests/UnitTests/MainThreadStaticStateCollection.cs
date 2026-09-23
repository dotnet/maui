using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// Tests in this collection mutate or depend on process-wide MainThread /
	// DispatcherProvider state. Keep the collection out of parallel execution so
	// MauiApp.Build() cannot race other default-builder tests.
	[CollectionDefinition("MainThreadStaticState", DisableParallelization = true)]
	public sealed class MainThreadStaticStateCollection
	{
	}
}
