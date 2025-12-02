using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class BasicTests
{
	[Fact]
	public void BasicTest()
	{
		// Basic test to verify the test infrastructure is working
		Assert.True(true);
	}

	[Fact]
	public async Task AsyncBasicTest()
	{
		// Basic async test to verify async test infrastructure is working
		await Task.Delay(10);
		Assert.True(true);
	}
}
