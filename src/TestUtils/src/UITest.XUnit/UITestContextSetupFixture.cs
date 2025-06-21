using UITest.Core;
using Xunit;

// xUnit equivalent of NUnit's SetUpFixture - we'll use ICollectionFixture
namespace UITest.Appium.XUnit
{
	// Collection fixture for setting up test context once per test collection
	public class UITestContextSetupFixture : IDisposable
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

		public virtual void Initialize()
		{
			// This method should be overridden by derived classes
			// to provide specific initialization logic
		}
	}

	// Collection definition for xUnit to use the setup fixture
	[CollectionDefinition("UITest")]
	public class UITestCollection : ICollectionFixture<UITestContextSetupFixture>
	{
		// This class has no code, and is never created. Its purpose is simply
		// to be the place to apply [CollectionDefinition] and all the
		// ICollectionFixture<> interfaces.
	}
}