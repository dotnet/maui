using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
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
	[InlineData(TestDevice.Mac, false, "increaseStepper", "increment")]
	[InlineData(TestDevice.Mac, true, "increaseStepper", "increment")]
	[InlineData(TestDevice.Mac, false, "decreaseStepper", "decrement")]
	[InlineData(TestDevice.Mac, true, "decreaseStepper", "decrement")]
	[InlineData(TestDevice.iOS, false, "increaseStepper", "increment")]
	[InlineData(TestDevice.iOS, true, "increaseStepper", "increment")]
	[InlineData(TestDevice.iOS, false, "decreaseStepper", "decrement")]
	[InlineData(TestDevice.iOS, true, "decreaseStepper", "decrement")]
	public void SemanticButtonsDoNotDependOnNativeOrder(TestDevice device, bool incrementFirst, string command, string expectedElement)
	{
		var executor = SemanticExecutor(incrementFirst);
		using var app = CreateApp(executor, device);

		var response = app.CommandExecutor.Execute(command, Parameters(true));

		Assert.Equal(CommandResponseResult.Success, response.Result);
		var click = Assert.Single(executor.Commands, c => c.Name == DriverCommand.ClickElement);
		Assert.Equal(expectedElement, click.Parameters["id"]);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.Actions
			|| c.Name == DriverCommand.ExecuteScript || c.Name == DriverCommand.SendKeysToElement);
	}

	[Theory]
	[InlineData(TestDevice.Mac, "increaseStepper", "increment")]
	[InlineData(TestDevice.Mac, "decreaseStepper", "decrement")]
	[InlineData(TestDevice.iOS, "increaseStepper", "increment")]
	[InlineData(TestDevice.iOS, "decreaseStepper", "decrement")]
	public void DisabledSemanticButtonDoesNotChangeTheRequestedAction(TestDevice device, string command, string expectedElement)
	{
		var executor = SemanticExecutor(incrementFirst: true);
		executor.DisabledElement = expectedElement;
		using var app = CreateApp(executor, device);

		app.CommandExecutor.Execute(command, Parameters(true));

		var click = Assert.Single(executor.Commands, c => c.Name == DriverCommand.ClickElement);
		Assert.Equal(expectedElement, click.Parameters["id"]);
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.IsElementEnabled
			|| c.Name == DriverCommand.Actions || c.Name == DriverCommand.SendKeysToElement);
	}

	[Theory]
	[InlineData("DefaultStepper-Increment", null)]
	[InlineData(null, "DefaultStepper-Decrement")]
	[InlineData("DefaultStepper-Increment", "unidentified")]
	[InlineData("unidentified", "DefaultStepper-Decrement")]
	[InlineData("DefaultStepper-Increment", "DefaultStepper-Increment")]
	[InlineData("DefaultStepper-Decrement", "DefaultStepper-Decrement")]
	[InlineData("OtherStepper-Increment", "OtherStepper-Decrement")]
	public void IncompleteOrConflictingSemanticMetadataCannotChooseAPositionalButton(string? first, string? last)
	{
		var executor = new RecordingExecutor { ButtonCount = 2 };
		executor.Identifiers["decrement"] = first;
		executor.Identifiers["increment"] = last;
		using var app = CreateApp(executor, TestDevice.Mac);

		Assert.Throws<InvalidOperationException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(true)));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.ClickElement
			|| c.Name == DriverCommand.Actions);
	}

	[Fact]
	public void DuplicateSemanticTargetCannotHideBehindACompletePair()
	{
		var executor = SemanticExecutor(incrementFirst: false);
		executor.ButtonOrder = ["decrement", "increment", "duplicate"];
		executor.Identifiers["duplicate"] = "DefaultStepper-Increment";
		using var app = CreateApp(executor, TestDevice.Mac);

		Assert.Throws<InvalidOperationException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(true)));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.ClickElement);
	}

	[Theory]
	[InlineData(TestDevice.iOS)]
	[InlineData(TestDevice.Mac)]
	public void NativeIdentifierErrorsAreNotReportedAsSuccessfulClicks(TestDevice device)
	{
		var executor = SemanticExecutor(incrementFirst: true);
		executor.FailAttributes = true;
		using var app = CreateApp(executor, device);

		Assert.Throws<WebDriverException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(true)));
		Assert.DoesNotContain(executor.Commands, c => c.Name == DriverCommand.ClickElement
			|| c.Name == DriverCommand.Actions);
	}

	[Fact]
	public void NativeSemanticClickAndFallbackErrorsPropagate()
	{
		var executor = SemanticExecutor(incrementFirst: true);
		executor.FailClicks = true;
		executor.FailActions = true;
		using var app = CreateApp(executor);

		var error = Assert.Throws<AggregateException>(() =>
			app.CommandExecutor.Execute("increaseStepper", Parameters(true)));
		Assert.Equal(2, error.InnerExceptions.Count);
	}

	[Fact]
	public void UnidentifiedLegacyButtonsKeepTheirPositionalCompatibility()
	{
		var executor = new RecordingExecutor { ButtonCount = 2 };
		executor.Identifiers["decrement"] = "legacy-left";
		executor.Identifiers["increment"] = "legacy-right";
		using var app = CreateApp(executor, TestDevice.Mac);

		app.CommandExecutor.Execute("increaseStepper", Parameters(null));
		app.CommandExecutor.Execute("decreaseStepper", Parameters(null));

		Assert.Equal(new[] { "increment", "decrement" },
			executor.Commands.Where(c => c.Name == DriverCommand.ClickElement)
				.Select(c => c.Parameters["id"]));
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

	static RecordingExecutor SemanticExecutor(bool incrementFirst)
	{
		var executor = new RecordingExecutor
		{
			ButtonOrder = incrementFirst ? ["increment", "decrement"] : ["decrement", "increment"]
		};
		executor.Identifiers["increment"] = "DefaultStepper-Increment";
		executor.Identifiers["decrement"] = "DefaultStepper-Decrement";
		return executor;
	}

	static TestApp CreateApp(RecordingExecutor executor, TestDevice device = TestDevice.iOS)
	{
		var config = new Config();
		config.SetProperty("TestDevice", device);
		executor.Device = device;
		AppiumDriver driver = device == TestDevice.Mac
			? new MacDriver(executor, new AppiumOptions())
			: new IOSDriver(executor, new AppiumOptions());
		var app = new TestApp(driver, config);
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
		public TestDevice Device { get; set; }
		public string[]? ButtonOrder { get; set; }
		public Dictionary<string, string?> Identifiers { get; } = new();
		public string? DisabledElement { get; set; }
		public bool FailAttributes { get; set; }
		public bool FailClicks { get; set; }

		public bool TryAddCommand(string commandName, CommandInfo? info) => true;
		public Response Execute(Command command) => ExecuteAsync(command).GetAwaiter().GetResult();

		public Task<Response> ExecuteAsync(Command command)
		{
			Commands.Add(command);
			if (command.Name == DriverCommand.Actions && FailActions)
				throw new WebDriverException("Tap failed");
			if (command.Name == DriverCommand.GetElementAttribute && FailAttributes)
				throw new WebDriverException("Native identifier failed");
			if (command.Name == DriverCommand.ClickElement && FailClicks)
				throw new WebDriverException("Native click failed");

			var response = command.Name switch
			{
				_ when command.Name == DriverCommand.NewSession => Response.FromJson(
					"""{"value":{"sessionId":"recording","capabilities":{"platformName":"iOS"}}}"""),
				_ when command.Name == DriverCommand.FindChildElements => Response.FromJson(JsonSerializer.Serialize(new
				{
					value = (ButtonOrder ?? (ButtonCount switch
					{
						0 => [],
						1 => ["increment"],
						_ => new[] { "decrement", "increment" }
					})).Select(id => new Dictionary<string, string>
					{
						["element-6066-11e4-a52e-4f735466cecf"] = id
					})
				})),
				_ when command.Name == DriverCommand.GetElementAttribute => IdentifierResponse(command),
				_ when command.Name == DriverCommand.IsElementEnabled => new Response("recording",
					!Equals(command.Parameters["id"], DisabledElement), WebDriverResult.Success),
				_ when command.Name == DriverCommand.GetElementRect => new Response("recording",
					new Dictionary<string, object> { ["x"] = 140, ["y"] = 73, ["width"] = Width, ["height"] = Height },
					WebDriverResult.Success),
				_ => new Response("recording", true, WebDriverResult.Success)
			};
			return Task.FromResult(response);
		}

		Response IdentifierResponse(Command command)
		{
			Assert.Equal(Device == TestDevice.Mac ? "identifier" : "name", command.Parameters["name"]);
			Identifiers.TryGetValue((string)command.Parameters["id"], out var identifier);
			return Response.FromJson(JsonSerializer.Serialize(new { value = identifier }));
		}

		public void Dispose() { }
	}
}
