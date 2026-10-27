using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class AndroidScreenshotInsetsTests
{
	[Fact]
	public void NotchedPortraitUsesTheNativeViewportRatherThanWindowBarBounds()
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);

		Assert.Equal(171, app.GetAndroidScreenshotTopInset(1440, 2960));
		Assert.Contains(executor.Scripts, s => s == "mobile:viewportRect");
	}

	[Fact]
	public void NonNotchedPortraitKeepsItsReportedInset()
	{
		var executor = new RecordingExecutor
		{
			WindowBarHeight = 60,
			InitialWidth = 1080,
			InitialHeight = 1860,
			InitialTop = 60,
			NativeTop = 60,
			ViewportWidth = 1080,
			ViewportHeight = 1860
		};
		using var app = CreateApp(executor);

		Assert.Equal(60, app.GetAndroidScreenshotTopInset(1080, 1920));
	}

	[Fact]
	public void HiddenStatusBarDoesNotCropFullscreenContent()
	{
		var executor = new RecordingExecutor { Visible = false, WindowBarHeight = 0 };
		using var app = CreateApp(executor);

		Assert.Equal(0, app.GetAndroidScreenshotTopInset(1440, 2960));
		Assert.DoesNotContain("mobile:viewportRect", executor.Scripts);
	}

	[Fact]
	public void RotatedWindowDoesNotApplyTheSessionCachedPortraitInset()
	{
		var executor = new RecordingExecutor { ViewportWidth = 2960, ViewportHeight = 1269 };
		using var app = CreateApp(executor);

		Assert.Equal(84, app.GetAndroidScreenshotTopInset(2960, 1440));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0.5)]
	[InlineData("171")]
	[InlineData(long.MaxValue)]
	public void InvalidNativeInsetDoesNotBecomeAZeroOrGuessedCrop(object top)
	{
		var executor = new RecordingExecutor { NativeTop = top };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void MissingNativeInsetRemainsAFailure()
	{
		var executor = new RecordingExecutor { IncludeTop = false };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void MissingSessionViewportCannotGuessCachedInsetValidity()
	{
		var executor = new RecordingExecutor { IncludeInitialViewport = false };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void ScreenshotAndViewportMismatchIsNotSilentlyResized()
	{
		var executor = new RecordingExecutor { ViewportWidth = 1080 };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void ViewportCommandFailureIsNotReplacedByWindowBarHeight()
	{
		var failure = new WebDriverException("Native viewport command failed");
		var executor = new RecordingExecutor { ViewportFailure = failure };
		using var app = CreateApp(executor);

		Assert.Same(failure, Record.Exception(() => app.GetAndroidScreenshotTopInset(1440, 2960)));
	}

	[Fact]
	public void ResizedWindowRetainsTheCurrentReportedBarInsteadOfCachedInset()
	{
		var executor = new RecordingExecutor { WindowBarHeight = 60, ViewportWidth = 1080, ViewportHeight = 1749 };
		using var app = CreateApp(executor);

		Assert.Equal(60, app.GetAndroidScreenshotTopInset(1080, 1920));
	}

	[Fact]
	public void ReportedZeroViewportInsetIsNotAnAbsentMeasurement()
	{
		var executor = new RecordingExecutor { WindowBarHeight = 0, NativeTop = 0L, InitialTop = 0, InitialHeight = 2960, ViewportHeight = 2960 };
		using var app = CreateApp(executor);

		Assert.Equal(0, app.GetAndroidScreenshotTopInset(1440, 2960));
		Assert.Contains("mobile:viewportRect", executor.Scripts);
	}

	[Fact]
	public void InvalidVisibilityCannotBeTreatedAsHidden()
	{
		var executor = new RecordingExecutor { Visible = "false" };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void ViewportHeightMismatchRemainsAFailure()
	{
		var executor = new RecordingExecutor { ViewportHeight = 2700 };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void DensityChangeDoesNotApplyTheSessionCachedInset()
	{
		var executor = new RecordingExecutor { WindowBarHeight = 60, CurrentDensity = 420 };
		using var app = CreateApp(executor);

		Assert.Equal(60, app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void MissingSessionDensityCannotCertifyAConflictingCachedInset()
	{
		var executor = new RecordingExecutor { IncludeInitialDensity = false };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Fact]
	public void InvalidCurrentDensityRemainsAFailure()
	{
		var executor = new RecordingExecutor { CurrentDensity = float.NaN };
		using var app = CreateApp(executor);

		Assert.Throws<InvalidOperationException>(() => app.GetAndroidScreenshotTopInset(1440, 2960));
	}

	[Theory]
	[InlineData(0, 2960)]
	[InlineData(1440, -1)]
	public void InvalidScreenshotBoundsFailBeforeQuerying(int width, int height)
	{
		var executor = new RecordingExecutor();
		using var app = CreateApp(executor);

		Assert.Throws<ArgumentOutOfRangeException>(() => app.GetAndroidScreenshotTopInset(width, height));
		Assert.Empty(executor.Scripts);
	}

	static TestApp CreateApp(RecordingExecutor executor)
	{
		var config = new Config();
		config.SetProperty("TestDevice", TestDevice.Android);
		var app = new TestApp(new AndroidDriver(executor, new AppiumOptions()), config);
		executor.Scripts.Clear();
		return app;
	}

	sealed class TestApp : AppiumApp
	{
		public TestApp(AppiumDriver driver, IConfig config) : base(driver, config) =>
			_commandExecutor.AddCommandGroup(new AppiumAndroidSpecificActions(this));
		public override ApplicationState AppState => ApplicationState.Running;
	}

	sealed class RecordingExecutor : ICommandExecutor
	{
		public List<string> Scripts { get; } = new();
		public int WindowBarHeight { get; set; } = 84;
		public object Visible { get; set; } = true;
		public int InitialWidth { get; set; } = 1440;
		public int InitialHeight { get; set; } = 2789;
		public int InitialTop { get; set; } = 171;
		public object NativeTop { get; set; } = 171L;
		public int ViewportWidth { get; set; } = 1440;
		public int ViewportHeight { get; set; } = 2789;
		public bool IncludeTop { get; set; } = true;
		public bool IncludeInitialViewport { get; set; } = true;
		public bool IncludeInitialDensity { get; set; } = true;
		public float CurrentDensity { get; set; } = 560;
		public Exception? ViewportFailure { get; set; }
		public bool TryAddCommand(string commandName, CommandInfo? info) => true;
		public Response Execute(Command command) => ExecuteAsync(command).GetAwaiter().GetResult();

		public Task<Response> ExecuteAsync(Command command)
		{
			if (command.Name == DriverCommand.NewSession)
			{
				var capabilities = new Dictionary<string, object> { ["platformName"] = "Android" };
				if (IncludeInitialDensity)
					capabilities["deviceScreenDensity"] = 560;
				if (IncludeInitialViewport)
					capabilities["viewportRect"] = new { left = 0, top = InitialTop, width = InitialWidth, height = InitialHeight };
				return Task.FromResult(Response.FromJson(JsonSerializer.Serialize(new
				{
					value = new { sessionId = "recording", capabilities }
				})));
			}
			if (command.Name == DriverCommand.ExecuteScript)
			{
				var script = Assert.IsType<string>(command.Parameters["script"]).Replace(" ", "", StringComparison.Ordinal);
				Scripts.Add(script);
				if (script == "mobile:getSystemBars")
					return Task.FromResult(new Response("recording", new Dictionary<string, object>
					{
						["statusBar"] = new Dictionary<string, object> { ["height"] = WindowBarHeight, ["visible"] = Visible }
					}, WebDriverResult.Success));
				if (script == "mobile:viewportRect")
				{
					if (ViewportFailure is not null)
						throw ViewportFailure;
					var viewport = new Dictionary<string, object> { ["left"] = 0, ["width"] = ViewportWidth, ["height"] = ViewportHeight };
					if (IncludeTop)
						viewport["top"] = NativeTop;
					return Task.FromResult(new Response("recording", viewport, WebDriverResult.Success));
				}
				if (script == "mobile:getDisplayDensity")
					return Task.FromResult(new Response("recording", CurrentDensity, WebDriverResult.Success));
				throw new InvalidOperationException($"Unexpected script: {script}");
			}
			return Task.FromResult(new Response("recording", true, WebDriverResult.Success));
		}

		public void Dispose() { }
	}
}
