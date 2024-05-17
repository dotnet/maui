using UITest.Appium;

// SetupFixture runs once for all tests under the same namespace, if placed outside the namespace it will run once for all tests in the assembly
namespace UITests
{
	public class TestContextSetupFixture : UITestContextSetupFixture
	{
		AppiumServerContext? _appiumServerContext;

		public override void Initialize()
		{
			_appiumServerContext = new AppiumServerContext();
			_appiumServerContext.CreateAndStartServer();
			_serverContext = _appiumServerContext;
		}
	}
}