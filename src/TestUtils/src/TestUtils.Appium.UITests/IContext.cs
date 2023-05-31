using Microsoft.Maui.Appium;

namespace TestUtils.Appium.UITests
{
	public interface IContext : IDisposable
	{
		UITestContext CreateUITestContext(TestConfig testConfig);
	}
}
