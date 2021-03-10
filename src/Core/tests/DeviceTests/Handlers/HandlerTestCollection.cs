using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[CollectionDefinition(TestCollections.Handlers)]
	public class HandlerTestCollection : ICollectionFixture<HandlerTestFixture>
	{
	}
}