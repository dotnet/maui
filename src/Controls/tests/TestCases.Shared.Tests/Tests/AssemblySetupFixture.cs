using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

// SetupFixture runs once for all tests under the same namespace, if placed outside the namespace it will run once for all tests in the assembly
// Test assemblies that derive from this assembly will need to create a type that derives from this class (or duplicate the code herein)
[SetUpFixture]
public class AssemblySetupFixture
{
	AppiumServerContext? _appiumServerContext;
	
	protected static IServerContext? _serverContext;

	public static IServerContext ServerContext { get { return _serverContext ?? throw new InvalidOperationException($"Trying to get the {nameof(ServerContext)} before setup has run"); } }

	[OneTimeSetUp]
	public void RunBeforeAnyTests()
	{
		_appiumServerContext = new AppiumServerContext();
		_appiumServerContext.CreateAndStartServer();
		_serverContext = _appiumServerContext;
	}

	[OneTimeTearDown]
	public void RunAfterAnyTests()
	{
		_serverContext?.Dispose();
		_serverContext = null;
	}
}
