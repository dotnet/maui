using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class LifecycleTests
{
	[Theory]
	[InlineData(TestDevice.Android, "mobile: startActivity")]
	[InlineData(TestDevice.iOS, "mobile: launchApp")]
	[InlineData(TestDevice.Mac, "macos: launchApp")]
	public void RelaunchAndResetCarryCurrentTest(TestDevice device, string launchScript)
	{
		var executor = new RecordingExecutor();
		var options = new AppiumOptions();
		AppiumDriver driver = device switch
		{
			TestDevice.Android => new AndroidDriver(executor, options),
			TestDevice.iOS => new IOSDriver(executor, options),
			TestDevice.Mac => new MacDriver(executor, options),
			_ => throw new NotSupportedException()
		};
		var config = new Config();
		config.SetProperty("TestDevice", device);
		config.SetProperty("AppId", "com.microsoft.maui.uitests");
		config.SetProperty("TestConfigurationArgs", new Dictionary<string, string> { ["test"] = "Previous fixture" });
		using var app = new TestApp(driver, config);
		executor.Commands.Clear();

		app.LaunchApp("Next \"test\" with spaces");
		app.ResetApp();

		var launches = executor.Commands.Where(command =>
			command.Name == DriverCommand.ExecuteScript && Equals(command.Parameters["script"], launchScript)).ToList();
		Assert.Equal(2, launches.Count);
		foreach (var launch in launches)
		{
			var arguments = Assert.IsAssignableFrom<object[]>(launch.Parameters["args"]);
			var payload = JsonSerializer.SerializeToElement(Assert.Single(arguments));
			if (device == TestDevice.Android)
			{
				Assert.True(payload.GetProperty("stop").GetBoolean());
				var extra = Assert.Single(payload.GetProperty("extras").EnumerateArray());
				Assert.Equal(new[] { "s", "test", AppLaunchArguments.QuoteAndroidShellArgument("Next \"test\" with spaces") },
					extra.EnumerateArray().Select(item => item.GetString()));
			}
			else
			{
				Assert.Equal("Next \"test\" with spaces", payload.GetProperty("environment").GetProperty("test").GetString());
			}
		}
		Assert.DoesNotContain(executor.Commands, command =>
			command.Name == DriverCommand.FindElement || command.Name == DriverCommand.FindElements || command.Name == DriverCommand.SendKeysToElement);
	}

	[Fact]
	public void FailedTerminationCannotLaunchWithStaleStartupArguments()
	{
		var executor = new RecordingExecutor { FailTermination = true };
		var config = new Config();
		config.SetProperty("TestDevice", TestDevice.iOS);
		config.SetProperty("AppId", "com.microsoft.maui.uitests");
		using var app = new TestApp(new IOSDriver(executor, new AppiumOptions()), config);

		Assert.Throws<InvalidOperationException>(() => app.LaunchApp("Next issue"));
		Assert.DoesNotContain(executor.Commands, command => command.Name == DriverCommand.ExecuteScript
			&& Equals(command.Parameters["script"], "mobile: launchApp"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FailedSimulatorFallbackCannotLaunchWithStaleStartupArguments(bool reset)
	{
		var executor = new RecordingExecutor { FailTermination = true };
		var config = new Config();
		config.SetProperty("TestDevice", TestDevice.iOS);
		config.SetProperty("AppId", "com.microsoft.maui.uitests");
		config.SetProperty("Udid", "00000000-0000-0000-0000-000000000000");
		config.SetProperty("TestConfigurationArgs", new Dictionary<string, string> { ["test"] = "Next issue" });
		using var app = new TestApp(new IOSDriver(executor, new AppiumOptions()), config);

		var exception = Assert.Throws<InvalidOperationException>(() =>
		{
			if (reset)
				app.ResetApp();
			else
				app.LaunchApp("Next issue");
		});
		Assert.Equal(reset
			? "Could not terminate the app before resetting it."
			: "Could not terminate the app before launching a new startup test.", exception.Message);
		Assert.DoesNotContain(executor.Commands, command => command.Name == DriverCommand.ExecuteScript
			&& Equals(command.Parameters["script"], "mobile: launchApp"));
	}

	[Fact]
	public void ForceCloseReportsFailedSimulatorProcess()
	{
		var executor = new RecordingExecutor();
		var config = new Config();
		config.SetProperty("TestDevice", TestDevice.iOS);
		config.SetProperty("AppId", "com.microsoft.maui.uitests");
		config.SetProperty("Udid", "00000000-0000-0000-0000-000000000000");
		using var app = new TestApp(new IOSDriver(executor, new AppiumOptions()), config);

		var response = app.CommandExecutor.Execute("forceCloseApp", new Dictionary<string, object>());

		Assert.Equal(CommandResponseResult.Failed, response.Result);
	}

	sealed class TestApp(AppiumDriver driver, IConfig config) : AppiumApp(driver, config)
	{
		public override ApplicationState AppState => ApplicationState.Running;
	}

	sealed class RecordingExecutor : ICommandExecutor
	{
		public List<Command> Commands { get; } = new();
		public bool FailTermination { get; set; }

		public bool TryAddCommand(string commandName, CommandInfo? info) => true;
		public Response Execute(Command command) => ExecuteAsync(command).GetAwaiter().GetResult();
		public Task<Response> ExecuteAsync(Command command)
		{
			Commands.Add(command);
			if (FailTermination && (command.Name.Contains("terminate", StringComparison.OrdinalIgnoreCase)
				|| command.Name == DriverCommand.ExecuteScript && Equals(command.Parameters["script"], "mobile: terminateApp")))
				throw new WebDriverException("Termination failed");

			var response = command.Name == DriverCommand.NewSession
				? Response.FromJson("""{"value":{"sessionId":"recording","capabilities":{"platformName":"test"}}}""")
				: new Response("recording", true, WebDriverResult.Success);
			return Task.FromResult(response);
		}

		public void Dispose() { }
	}
}
