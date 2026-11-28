using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using UITest.Appium;
using UITest.Core;
using Xunit;

namespace Microsoft.Maui.TestUtils.UnitTests;

public class WindowsMouseActionsTests
{
	[Theory]
	[InlineData("clickCoordinates", false, 1, 0)]
	[InlineData("doubleClickCoordinates", false, 2, 0)]
	[InlineData("doubleClick", true, 2, 0)]
	[InlineData("longPress", true, 1, 2000)]
	[InlineData("click", true, 1, 0)]
	public void WindowsMouseGesturesUseTheSupportedNativeCommand(string command, bool useElement, int times, int duration)
	{
		using var driver = new RecordingWindowsDriver();
		var actions = new AppiumMouseActions(new TestApp(driver));
		var parameters = useElement
			? new Dictionary<string, object> { ["element"] = new AppiumElement(driver, "target") }
			: new Dictionary<string, object> { ["x"] = 120.5f, ["y"] = 45.5f };

		actions.Execute(command, parameters);

		var script = Assert.Single(driver.Scripts);
		Assert.Equal("windows: click", script["script"]);
		var arguments = Assert.IsAssignableFrom<IEnumerable<object>>(script["args"]);
		var click = Assert.IsAssignableFrom<IDictionary<string, object>>(Assert.Single(arguments));
		if (useElement && command != "click")
			Assert.Equal("target", click["elementId"]);
		else
		{
			Assert.Equal(120, click["x"]);
			Assert.Equal(45, click["y"]);
		}
		Assert.Equal(times, click.TryGetValue("times", out var count) ? count : 1);
		Assert.Equal(duration, click.TryGetValue("durationMs", out var delay) ? delay : 0);
		if (times == 2)
			Assert.Equal(250, click["interClickDelayMs"]);
	}

	[Fact]
	public void NativeClickFailureIsNotReportedAsSuccess()
	{
		using var driver = new RecordingWindowsDriver { FailScript = true };
		var actions = new AppiumMouseActions(new TestApp(driver));
		Assert.Throws<WebDriverException>(() => actions.Execute("clickCoordinates",
			new Dictionary<string, object> { ["x"] = 120, ["y"] = 45 }));
	}

	sealed class TestApp(WindowsDriver driver) : AppiumApp(driver, new Config())
	{
		public override ApplicationState AppState => ApplicationState.Running;
	}

	sealed class RecordingWindowsDriver() : WindowsDriver(new Uri("http://127.0.0.1:1"),
		new AppiumOptions { AutomationName = "Windows" })
	{
		public List<Dictionary<string, object>> Scripts { get; } = new();
		public bool FailScript { get; set; }

		protected override Task<Response> ExecuteAsync(string driverCommandToExecute, Dictionary<string, object> parameters) =>
			Task.FromResult(Execute(driverCommandToExecute, parameters));

		protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
		{
			if (driverCommandToExecute == DriverCommand.NewSession)
				return new Response("test-session", new Dictionary<string, object> { ["platformName"] = "Windows" }, WebDriverResult.Success);
			if (driverCommandToExecute == DriverCommand.ExecuteScript)
			{
				if (FailScript)
					throw new WebDriverException("Native click failed.");
				Scripts.Add(parameters);
				return new Response("test-session", null, WebDriverResult.Success);
			}
			if (driverCommandToExecute == DriverCommand.ClickElement)
				throw new InvalidOperationException("The element requires a coordinate click.");
			if (driverCommandToExecute == DriverCommand.GetElementAttribute)
				return new Response("test-session", "120,45", WebDriverResult.Success);
			if (driverCommandToExecute == DriverCommand.Quit)
				return new Response("test-session", null, WebDriverResult.Success);
			throw new InvalidOperationException($"Unexpected driver command: {driverCommandToExecute}");
		}
	}
}
