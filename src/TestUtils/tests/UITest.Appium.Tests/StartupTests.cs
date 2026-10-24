using UITest.Appium.NUnit;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class StartupTests
{
	[Theory]
	[InlineData("", "\"\"")]
	[InlineData("A test", "\"A test\"")]
	[InlineData("A \"quoted\" test", "\"A \\\"quoted\\\" test\"")]
	[InlineData("A\\B", "\"A\\\\B\"")]
	[InlineData("$HOME `id`", "\"\\$HOME \\`id\\`\"")]
	public void AndroidExtrasPreserveShellArguments(string value, string expected)
	{
		Assert.Equal(expected, AppLaunchArguments.QuoteAndroidShellArgument(value));
		Assert.Equal("--es test " + expected, AppLaunchArguments.AndroidTestExtra(value));
	}

	[Theory]
	[InlineData("", "\"--test=\"")]
	[InlineData("A test", "\"--test=A test\"")]
	[InlineData("A \"quoted\" test", "\"--test=A \\\"quoted\\\" test\"")]
	[InlineData("A\\B", "\"--test=A\\B\"")]
	[InlineData("A\\", "\"--test=A\\\\\"")]
	public void WindowsStartupUsesOneQuotedArgument(string value, string expected)
	{
		Assert.Equal(expected, AppLaunchArguments.WindowsTestArgument(value));
	}

	[Theory]
	[InlineData(TestDevice.Android)]
	[InlineData(TestDevice.iOS)]
	[InlineData(TestDevice.Mac)]
	[InlineData(TestDevice.Windows)]
	public void ReusedFixturesGetTheirOwnStartupTarget(TestDevice device)
	{
		var server = new TestServer();
		var fixture = new TestFixture(device) { Target = "First issue" };
		fixture.InitialSetup(server);
		fixture.Reset();
		var initialContext = UITestContextBase.UITestContext;
		var creations = server.Creations;
		fixture.LaunchedTest = null;

		fixture.Target = "Second gallery";
		fixture.InitialSetup(server);

		Assert.Equal("Second gallery", StartupTest(UITestContextBase.UITestContext!.Config));
		if (device == TestDevice.Windows)
		{
			Assert.NotSame(initialContext, UITestContextBase.UITestContext);
			Assert.Equal(creations + 1, server.Creations);
			Assert.Null(fixture.LaunchedTest);
		}
		else
		{
			Assert.Same(initialContext, UITestContextBase.UITestContext);
			Assert.Equal(creations, server.Creations);
			Assert.Equal("Second gallery", fixture.LaunchedTest);
		}

		creations = server.Creations;
		fixture.InitialSetup(server);
		Assert.Equal(creations, server.Creations);
		Assert.Equal("Second gallery", fixture.LaunchedTest);

		fixture.Target = null;
		fixture.InitialSetup(server);
		Assert.Null(StartupTest(UITestContextBase.UITestContext!.Config));

		fixture.Target = "Retry issue";
		fixture.Reset();
		Assert.Equal("Retry issue", StartupTest(UITestContextBase.UITestContext!.Config));
	}

	[Fact]
	public void FailedSessionRecreationDoesNotReuseDisposedContext()
	{
		var server = new TestServer();
		var fixture = new TestFixture(TestDevice.Windows) { Target = "First issue" };
		fixture.InitialSetup(server);
		fixture.Reset();
		fixture.Target = "Next issue";
		server.FailNext = true;

		Assert.Throws<InvalidOperationException>(() => fixture.InitialSetup(server));
		Assert.Null(UITestContextBase.UITestContext);

		fixture.InitialSetup(server);
		Assert.Equal("Next issue", StartupTest(UITestContextBase.UITestContext!.Config));
	}

	static string? StartupTest(IConfig config)
	{
		var args = config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs");
		return args is not null && args.TryGetValue("test", out var name) ? name : null;
	}

	sealed class TestFixture(TestDevice device) : UITestContextBase(device)
	{
		public string? Target { get; set; }
		public string? LaunchedTest { get; set; }

		public override IConfig GetTestConfig()
		{
			var config = new Config();
			if (Target is not null)
				config.SetProperty("TestConfigurationArgs", new Dictionary<string, string> { ["test"] = Target });
			return config;
		}

		public override void LaunchAppWithTest() => LaunchedTest = StartupTest(App.Config);
		public override void Close() { }
	}

	sealed class TestServer : IServerContext
	{
		public int Creations { get; private set; }
		public bool FailNext { get; set; }
		public IUIClientContext CreateUIClientContext(IConfig config)
		{
			Creations++;
			if (FailNext)
			{
				FailNext = false;
				throw new InvalidOperationException("Session creation failed");
			}
			return new TestContext(config);
		}

		public void Dispose() { }
	}

	sealed class TestContext(IConfig config) : IUIClientContext
	{
		public IConfig Config => config;
		public IApp App { get; } = new TestApp(config);
		public void Dispose() => App.Dispose();
	}

	sealed class TestApp(IConfig config) : IApp
	{
		public IConfig Config => config;
		public ApplicationState AppState => ApplicationState.Running;
		public IUIElementQueryable Query => throw new NotSupportedException();
		public ICommandExecution CommandExecutor => throw new NotSupportedException();
		public string ElementTree => throw new NotSupportedException();
		public IUIElement FindElement(string id) => throw new NotSupportedException();
		public IUIElement FindElementByText(string text) => throw new NotSupportedException();
		public IUIElement FindElement(IQuery query) => throw new NotSupportedException();
		public IReadOnlyCollection<IUIElement> FindElements(string id) => throw new NotSupportedException();
		public IReadOnlyCollection<IUIElement> FindElementsByText(string text) => throw new NotSupportedException();
		public IReadOnlyCollection<IUIElement> FindElements(IQuery query) => throw new NotSupportedException();
		public void Dispose() { }
	}
}
