using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class TouchTapTests
{
	[Theory]
	[InlineData(TestDevice.iOS)]
	[InlineData(TestDevice.Android)]
	public void SuccessfulNativeClicksDoNotUseFallback(TestDevice device)
	{
		var executor = new RecordingExecutor { Device = device };
		using var app = CreateApp(executor);

		var response = Tap(app);

		Assert.Equal(CommandResponseResult.Success, response.Result);
		Assert.Single(executor.Commands, c => c.Name == DriverCommand.ClickElement);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions || c.Name == DriverCommand.GetElementAttribute);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.CancelActions);
	}

	[Theory]
	[InlineData(TestDevice.iOS)]
	[InlineData(TestDevice.Android)]
	public void FailedMobileClickUsesTheNativeElementOrigin(TestDevice device)
	{
		var executor = new RecordingExecutor { Device = device, ClickFailure = new WebDriverException("Native click rejected") };
		using var app = CreateApp(executor);

		var response = Tap(app);

		Assert.Equal(CommandResponseResult.Success, response.Result);
		var move = GetPointerMove(executor);
		Assert.Equal("target", move.GetProperty("origin").GetProperty("element-6066-11e4-a52e-4f735466cecf").GetString());
		Assert.Equal(0, move.GetProperty("x").GetInt32());
		Assert.Equal(0, move.GetProperty("y").GetInt32());
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.GetElementAttribute);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.CancelActions);
	}

	[Fact]
	public void InvalidOperationClickUsesTheSameMobileFallback()
	{
		var executor = new RecordingExecutor { ClickFailure = new InvalidOperationException("Native click rejected") };
		using var app = CreateApp(executor);

		Assert.Equal(CommandResponseResult.Success, Tap(app).Result);
		Assert.Equal("target", GetPointerMove(executor).GetProperty("origin")
			.GetProperty("element-6066-11e4-a52e-4f735466cecf").GetString());
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FallbackFailurePreservesBothDiagnosticsAndResetsInput(bool stale)
	{
		WebDriverException failure = stale
			? new StaleElementReferenceException("The element is stale")
			: new WebDriverException("The touch action failed");
		var executor = new RecordingExecutor
		{
			ClickFailure = new WebDriverException("Native click rejected"),
			ActionFailure = failure
		};
		using var app = CreateApp(executor);

		var error = Assert.Throws<AggregateException>(() => Tap(app));
		Assert.Collection(error.InnerExceptions,
			e => Assert.Same(executor.ClickFailure, e),
			e => Assert.Same(failure, e));
		Assert.Single(executor.Commands, c => c.Name == DriverCommand.CancelActions);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.GetElementAttribute);
	}

	[Fact]
	public void ResetFailureCannotHideTheClickOrTouchFailures()
	{
		var executor = new RecordingExecutor
		{
			ClickFailure = new WebDriverException("Native click rejected"),
			ActionFailure = new WebDriverException("Touch action failed"),
			ResetFailure = new WebDriverException("Input reset failed")
		};
		using var app = CreateApp(executor);

		var error = Assert.Throws<AggregateException>(() => Tap(app));

		Assert.Collection(error.InnerExceptions,
			e => Assert.Same(executor.ClickFailure, e),
			e => Assert.Same(executor.ActionFailure, e),
			e => Assert.Same(executor.ResetFailure, e));
	}

	[Fact]
	public void WindowsConfigurationKeepsItsExistingClickablePointFallback()
	{
		var executor = new RecordingExecutor { Device = TestDevice.Windows, ClickFailure = new WebDriverException("Click rejected") };
		using var app = CreateApp(executor);

		Assert.Equal(CommandResponseResult.Success, Tap(app).Result);
		var move = GetPointerMove(executor);
		Assert.Equal("viewport", move.GetProperty("origin").GetString());
		Assert.Equal(12, move.GetProperty("x").GetInt32());
		Assert.Equal(34, move.GetProperty("y").GetInt32());
		Assert.Single(executor.Commands, c => c.Name == DriverCommand.GetElementAttribute);
	}

	[Fact]
	public void ExplicitCoordinatesKeepTheirViewportOrigin()
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);

		var response = app.CommandExecutor.Execute("tapCoordinates", new Dictionary<string, object> { ["x"] = 23, ["y"] = 45 });

		Assert.Equal(CommandResponseResult.Success, response.Result);
		var move = GetPointerMove(executor);
		Assert.Equal("viewport", move.GetProperty("origin").GetString());
		Assert.Equal(23, move.GetProperty("x").GetInt32());
		Assert.Equal(45, move.GetProperty("y").GetInt32());
	}

	static CommandResponse Tap(AppiumApp app) =>
		app.CommandExecutor.Execute("tap", new Dictionary<string, object> { ["element"] = new AppiumElement(app.Driver, "target") });

	static JsonElement GetPointerMove(RecordingExecutor executor)
	{
		var command = Assert.Single(executor.Commands, c => c.Name == DriverCommand.Actions);
		var actions = JsonSerializer.SerializeToElement(command.Parameters["actions"]);
		var pointer = Assert.Single(actions.EnumerateArray());
		Assert.Equal("touch", pointer.GetProperty("parameters").GetProperty("pointerType").GetString());
		var sequence = pointer.GetProperty("actions").EnumerateArray().ToArray();
		Assert.Single(sequence, a => a.GetProperty("type").GetString() == "pointerDown");
		Assert.Single(sequence, a => a.GetProperty("type").GetString() == "pointerUp");
		return Assert.Single(sequence, a => a.GetProperty("type").GetString() == "pointerMove");
	}

	static AppiumApp CreateApp(RecordingExecutor executor)
	{
		var options = new AppiumOptions();
		AppiumDriver driver = executor.Device == TestDevice.Android
			? new AndroidDriver(executor, options)
			: new IOSDriver(executor, options);
		var config = new Config();
		config.SetProperty("TestDevice", executor.Device);
		var app = new TestApp(driver, config);
		executor.Commands.Clear();
		return app;
	}

	sealed class TestApp : AppiumApp
	{
		public TestApp(AppiumDriver driver, IConfig config) : base(driver, config) { }
		public override ApplicationState AppState => ApplicationState.Running;
	}

	sealed class RecordingExecutor : ICommandExecutor
	{
		public List<Command> Commands { get; } = new();
		public TestDevice Device { get; set; } = TestDevice.iOS;
		public Exception? ClickFailure { get; set; }
		public Exception? ActionFailure { get; set; }
		public Exception? ResetFailure { get; set; }
		public bool TryAddCommand(string commandName, CommandInfo? info) => true;
		public Response Execute(Command command) => ExecuteAsync(command).GetAwaiter().GetResult();

		public Task<Response> ExecuteAsync(Command command)
		{
			Commands.Add(command);
			if (command.Name == DriverCommand.ClickElement && ClickFailure is not null)
				throw ClickFailure;
			if (command.Name == DriverCommand.Actions && ActionFailure is not null)
				throw ActionFailure;
			if (command.Name == DriverCommand.CancelActions && ResetFailure is not null)
				throw ResetFailure;
			if (command.Name == DriverCommand.GetElementAttribute)
			{
				if (Device != TestDevice.Windows)
					throw new UnknownErrorException("The attribute 'ClickablePoint' is unknown.");
				return Task.FromResult(new Response("recording", "12,34", WebDriverResult.Success));
			}
			var response = command.Name == DriverCommand.NewSession
				? Response.FromJson(JsonSerializer.Serialize(new
				{
					value = new { sessionId = "recording", capabilities = new { platformName = Device.ToString() } }
				}))
				: new Response("recording", true, WebDriverResult.Success);
			return Task.FromResult(response);
		}

		public void Dispose() { }
	}
}
