using Xunit;

namespace Microsoft.Maui.DeviceTests.Services;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EssentialsStaticStateCollection
{
	public const string Name = "EssentialsStaticState";
}
