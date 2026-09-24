namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class AppleLaunchTests : IDisposable
{
	readonly string _resultDir = Path.Combine(Path.GetTempPath(), $"maui-apple-launch-{Guid.NewGuid():N}");
	const string CompletionMarker = "MAUI_APP_COMPLETED_current";

	public AppleLaunchTests() => Directory.CreateDirectory(_resultDir);

	[Theory]
	[InlineData("")]
	[InlineData("[00:56:35.5968960] ")]
	[InlineData("2026-09-25 00:56:35.596729+0200 TestApp[9876:26968677] ")]
	[InlineData("[00:56:35.5968960] 2026-09-25 00:56:35.596729+0200 TestApp[9876:26968677] ")]
	public void RequiresCompletionMarkerAndSuccessfulExit(string prefix)
	{
		File.WriteAllText(Path.Combine(_resultDir, "application.log"), $"{prefix}{CompletionMarker}\n");

		Assert.True(XHarness.AppleRunCompleted(0, _resultDir, CompletionMarker));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(70)]
	[InlineData(78)]
	[InlineData(80)]
	[InlineData(81)]
	[InlineData(83)]
	[InlineData(90)]
	public void RejectsFailuresEvenWithCompletionMarker(int exitCode)
	{
		File.WriteAllText(Path.Combine(_resultDir, "application.log"), $"{CompletionMarker}\nmlaunch exited with 137\n");

		Assert.False(XHarness.AppleRunCompleted(exitCode, _resultDir, CompletionMarker));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Launching the app\nmlaunch exited with 137\nRun timed out after 15 seconds")]
	[InlineData("MAUI_APP_COMPLETED_previous")]
	[InlineData("Expected marker: MAUI_APP_COMPLETED_current")]
	[InlineData("[00:56:35.5968960] Expected marker: MAUI_APP_COMPLETED_current")]
	[InlineData("MAUI_APP_COMPLETED_current_extra")]
	public void RejectsSuccessfulExitWithoutThisAppsCompletion(string log)
	{
		File.WriteAllText(Path.Combine(_resultDir, "application.log"), log);

		Assert.False(XHarness.AppleRunCompleted(0, _resultDir, CompletionMarker));
	}

	[Fact]
	public void RejectsMissingLogs()
	{
		Assert.False(XHarness.AppleRunCompleted(0, _resultDir, CompletionMarker));
		Assert.False(XHarness.AppleRunCompleted(0, Path.Combine(_resultDir, "missing"), CompletionMarker));
	}

	public void Dispose() => Directory.Delete(_resultDir, recursive: true);
}
