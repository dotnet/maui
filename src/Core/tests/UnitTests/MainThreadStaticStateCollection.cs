using Xunit;

namespace Microsoft.Maui.UnitTests
{
	// Tests in this collection share process-wide MainThread / DispatcherProvider state.
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class MainThreadStaticStateCollection
	{
		public const string Name = "MainThreadStaticState";
	}
}
