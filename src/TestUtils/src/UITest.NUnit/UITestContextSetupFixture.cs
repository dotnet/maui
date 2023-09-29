using NUnit.Framework;
using UITest.Core;

[assembly: NonTestAssembly]

// SetupFixture runs once for all tests under the same namespace, if placed outside the namespace it will run once for all tests in the assembly
// Test assemblies that derive from this assembly will need to create a type that derives from this class (or duplicate the code herein)
[SetUpFixture]
public abstract class UITestContextSetupFixture
{
    protected static IServerContext? _serverContext;

    public static IServerContext ServerContext { get { return _serverContext ?? throw new InvalidOperationException($"Trying to get the {nameof(ServerContext)} before setup has run"); } }

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Initialize();
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        _serverContext?.Dispose();
        _serverContext = null;
    }

    public abstract void Initialize();
}