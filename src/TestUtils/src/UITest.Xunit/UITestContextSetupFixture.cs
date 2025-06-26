using Xunit;
using UITest.Core;

// xUnit doesn't have direct equivalent of SetUpFixture
// We'll use ICollectionFixture pattern for assembly-level setup
[CollectionDefinition("UITestCollection")]
public class UITestCollectionFixture : ICollectionFixture<UITestContextSetupFixture>
{
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}

public abstract class UITestContextSetupFixture : IDisposable
{
	protected static IServerContext? _serverContext;

	public static IServerContext ServerContext { get { return _serverContext ?? throw new InvalidOperationException($"Trying to get the {nameof(ServerContext)} before setup has run"); } }

	public UITestContextSetupFixture()
	{
		Initialize();
	}

	public void Dispose()
	{
		_serverContext?.Dispose();
		_serverContext = null;
	}

	public abstract void Initialize();
}