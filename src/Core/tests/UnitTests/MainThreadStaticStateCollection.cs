using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class MainThreadStaticStateCollection
	{
		public const string Name = "MainThreadStaticState";
	}
}
