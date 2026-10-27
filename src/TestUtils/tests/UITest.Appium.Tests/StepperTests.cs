using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class StepperTests
{
	[Theory]
	[InlineData("increaseStepper", 0, false, 211)]
	[InlineData("decreaseStepper", 0, false, 163)]
	[InlineData("increaseStepper", 1, false, 211)]
	[InlineData("decreaseStepper", 1, false, 163)]
	[InlineData("increaseStepper", 0, true, 163)]
	[InlineData("decreaseStepper", 0, true, 211)]
	[InlineData("increaseStepper", 1, true, 163)]
	[InlineData("decreaseStepper", 1, true, 211)]
	public void MissingButtonChildrenTapTheRequestedHalf(string command, int buttonCount, bool isRightToLeft, int expectedX)
	{
		var executor = new RecordingExecutor { ButtonCount = buttonCount };
		using var app = CreateApp(executor);

		var response = app.CommandExecutor.Execute(command, Parameters(isRightToLeft));

		Assert.Equal(CommandResponseResult.Success, response.Result);
		var action = Assert.Single(executor.Commands, c => c.Name == DriverCommand.Actions);
		var sequences = JsonSerializer.SerializeToElement(action.Parameters["actions"]);
		var pointer = Assert.Single(sequences.EnumerateArray());
		Assert.Equal("touch", pointer.GetProperty("parameters").GetProperty("pointerType").GetString());
		var moves = pointer.GetProperty("actions").EnumerateArray()
			.Where(a => a.GetProperty("type").GetString() == "pointerMove").ToArray();
		var move = Assert.Single(moves);
		Assert.Equal(expectedX, move.GetProperty("x").GetInt32());
		Assert.Equal(89, move.GetProperty("y").GetInt32());
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.ClickElement);
	}

	[Theory]
	[InlineData("increaseStepper", "increment", null)]
	[InlineData("decreaseStepper", "decrement", null)]
	[InlineData("increaseStepper", "increment", true)]
	[InlineData("decreaseStepper", "decrement", true)]
	public void AccessibleButtonsKeepTheirExistingTapPath(string command, string expectedElement, bool? isRightToLeft)
	{
		var executor = new RecordingExecutor { ButtonCount = 2 };
		using var app = CreateApp(executor);

		var response = app.CommandExecutor.Execute(command, Parameters(isRightToLeft));

		Assert.Equal(CommandResponseResult.Success, response.Result);
		var click = Assert.Single(executor.Commands, c => c.Name == DriverCommand.ClickElement);
		Assert.Equal(expectedElement, click.Parameters["id"]);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions);
	}

	[Theory]
	[InlineData(true, false, 211)]
	[InlineData(false, false, 163)]
	[InlineData(true, true, 163)]
	[InlineData(false, true, 211)]
	public void PublicHelpersForwardTheEffectiveDirection(bool increase, bool isRightToLeft, int expectedX)
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);

		if (increase)
			app.IncreaseStepper("DefaultStepper", isRightToLeft);
		else
			app.DecreaseStepper("DefaultStepper", isRightToLeft);

		var action = Assert.Single(executor.Commands, c => c.Name == DriverCommand.Actions);
		var sequences = JsonSerializer.SerializeToElement(action.Parameters["actions"]);
		var pointer = Assert.Single(sequences.EnumerateArray());
		var move = Assert.Single(pointer.GetProperty("actions").EnumerateArray(),
			a => a.GetProperty("type").GetString() == "pointerMove");
		Assert.Equal(expectedX, move.GetProperty("x").GetInt32());
	}

	[Theory]
	[InlineData(0, 32)]
	[InlineData(95, 0)]
	[InlineData(-1, 32)]
	[InlineData(95, -1)]
	public void EmptyBoundsDoNotReportACompletedTap(int width, int height)
	{
		var executor = new RecordingExecutor { Width = width, Height = height };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(false)));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions);
	}

	[Fact]
	public void TapFailureIsNotReportedAsSuccess()
	{
		var executor = new RecordingExecutor { FailActions = true };
		using var app = CreateApp(executor);

		Assert.Throws<WebDriverException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(false)));
	}

	[Theory]
	[InlineData("increaseStepper")]
	[InlineData("decreaseStepper")]
	public void UnknownEffectiveDirectionCannotGuessACoordinate(string command)
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.CommandExecutor.Execute(command, Parameters(null)));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions);
	}

	[Fact]
	public void InvalidDirectionCannotGuessACoordinate()
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);
		var parameters = Parameters(null);
		parameters["isRightToLeft"] = "RightToLeft";

		Assert.Throws<InvalidOperationException>(() => app.CommandExecutor.Execute("increaseStepper", parameters));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions);
	}

	static Dictionary<string, object> Parameters(bool? isRightToLeft)
	{
		var parameters = new Dictionary<string, object> { ["elementId"] = "DefaultStepper" };
		if (isRightToLeft.HasValue)
			parameters["isRightToLeft"] = isRightToLeft.Value;
		return parameters;
	}

	static TestApp CreateApp(RecordingExecutor executor)
	{
		var config = new Config();
		config.SetProperty("TestDevice", TestDevice.iOS);
		var app = new TestApp(new IOSDriver(executor, new AppiumOptions()), config);
		executor.Commands.Clear();
		return app;
	}

	sealed class TestApp : AppiumApp
	{
		public TestApp(AppiumDriver driver, IConfig config) : base(driver, config)
		{
			_commandExecutor.AddCommandGroup(new AppiumAppleStepperActions(this));
		}

		public override ApplicationState AppState => ApplicationState.Running;

		public override IUIElement FindElement(string id)
		{
			Assert.Equal("DefaultStepper", id);
			return new AppiumDriverElement(new AppiumElement(Driver, "stepper"), this);
		}
	}

	sealed class RecordingExecutor : ICommandExecutor
	{
		public List<Command> Commands { get; } = new();
		public int ButtonCount { get; set; }
		public int Width { get; set; } = 95;
		public int Height { get; set; } = 32;
		public bool FailActions { get; set; }

		public bool TryAddCommand(string commandName, CommandInfo? info) => true;
		public Response Execute(Command command) => ExecuteAsync(command).GetAwaiter().GetResult();

		public Task<Response> ExecuteAsync(Command command)
		{
			Commands.Add(command);
			if (command.Name == DriverCommand.Actions && FailActions)
				throw new WebDriverException("Tap failed");

			var response = command.Name switch
			{
				_ when command.Name == DriverCommand.NewSession => Response.FromJson(
					"""{"value":{"sessionId":"recording","capabilities":{"platformName":"iOS"}}}"""),
				_ when command.Name == DriverCommand.FindChildElements => Response.FromJson(ButtonCount switch
				{
					0 => """{"value":[]}""",
					1 => """{"value":[{"element-6066-11e4-a52e-4f735466cecf":"increment"}]}""",
					_ => """{"value":[{"element-6066-11e4-a52e-4f735466cecf":"decrement"},{"element-6066-11e4-a52e-4f735466cecf":"increment"}]}"""
				}),
				_ when command.Name == DriverCommand.GetElementRect => new Response("recording",
					new Dictionary<string, object> { ["x"] = 140, ["y"] = 73, ["width"] = Width, ["height"] = Height },
					WebDriverResult.Success),
				_ => new Response("recording", true, WebDriverResult.Success)
			};
			return Task.FromResult(response);
		}

		public void Dispose() { }
	}
}
