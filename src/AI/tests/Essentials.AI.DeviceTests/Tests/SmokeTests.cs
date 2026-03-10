using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Basic smoke tests that run on all platforms to ensure the test
/// infrastructure is working correctly (e.g., test discovery on Windows).
/// </summary>
public class SmokeTests
{
	[Fact]
	public void TestInfrastructureWorks()
	{
		Assert.True(true);
	}
}
