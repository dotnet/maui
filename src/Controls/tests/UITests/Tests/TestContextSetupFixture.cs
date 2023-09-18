using NUnit.Framework;
using TestUtils.Appium.UITests;

// SetupFixture runs once for all tests under the same namespace, if placed outside the namespace it will run once for all tests in the assembly
[SetUpFixture]
public class TestContextSetupFixture
{
	static AppiumContext? _testContext;

	public static IContext TestContext { get { return _testContext ?? throw new InvalidOperationException("Trying to get the TestContext before setup has run"); } }

	[OneTimeSetUp]
	public void RunBeforeAnyTests()
	{
		_testContext = new AppiumContext();
		_testContext.CreateAndStartServer();
	}

	[OneTimeTearDown]
	public void RunAfterAnyTests()
	{
		_testContext?.Dispose();
		_testContext = null;
	}
}
