using System.Net;
using UITest.Appium;

// SetupFixture runs once for all tests under the same namespace, if placed outside the namespace it will run once for all tests in the assembly
public class AssemblySetupFixture : UITestContextSetupFixture
{
	AppiumServerContext? _appiumServerContext;
	TestAppController? _testAppController;

	public override void Initialize()
	{
		_appiumServerContext = new AppiumServerContext();
		_appiumServerContext.CreateAndStartServer();
		_serverContext = _appiumServerContext;

		// Fire and forget
		_ = StartControllerAsync();
	}

	public async Task StartControllerAsync()
	{
		if (_testAppController is not null)
		{
			throw new InvalidOperationException("Controller is already running.");
		}

		_testAppController = new TestAppController(IPAddress.Loopback);
		await _testAppController.InitializeAsync(TimeSpan.FromSeconds(120));
	}
}
