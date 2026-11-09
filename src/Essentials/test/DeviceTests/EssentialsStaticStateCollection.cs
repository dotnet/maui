using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EssentialsStaticStateCollection
{
	public const string Name = "EssentialsStaticState";
}
