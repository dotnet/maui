namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class AppleLaunchTests : IDisposable
{
	readonly string _resultDir = Path.Combine(TestEnvironment.GetLogDirectory(), $"maui-apple-launch-{Guid.NewGuid():N}");
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

		Assert.True(AppLaunch.AppleRunCompleted(0, _resultDir, CompletionMarker));
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
		File.WriteAllText(Path.Combine(_resultDir, "application.log"), $"{CompletionMarker}\n");

		Assert.False(AppLaunch.AppleRunCompleted(exitCode, _resultDir, CompletionMarker));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Launching the app\nRun timed out after 15 seconds")]
	[InlineData("MAUI_APP_COMPLETED_previous")]
	[InlineData("Expected marker: MAUI_APP_COMPLETED_current")]
	[InlineData("[00:56:35.5968960] Expected marker: MAUI_APP_COMPLETED_current")]
	[InlineData("MAUI_APP_COMPLETED_current_extra")]
	public void RejectsSuccessfulExitWithoutThisAppsCompletion(string log)
	{
		File.WriteAllText(Path.Combine(_resultDir, "application.log"), log);

		Assert.False(AppLaunch.AppleRunCompleted(0, _resultDir, CompletionMarker));
	}

	[Fact]
	public void RejectsMissingLogs()
	{
		Assert.False(AppLaunch.AppleRunCompleted(0, _resultDir, CompletionMarker));
		Assert.False(AppLaunch.AppleRunCompleted(0, Path.Combine(_resultDir, "missing"), CompletionMarker));
	}

	[Fact]
	public void RejectsMarkerInCommandDiagnostics()
	{
		File.WriteAllText(Path.Combine(_resultDir, "install.log"), CompletionMarker);
		Assert.False(AppLaunch.AppleRunCompleted(0, _resultDir, CompletionMarker));
	}

	[Theory]
	[InlineData("termination reported by launchd (0, 0, 0)", 0)]
	[InlineData("termination reported by launchd (0, 0, 10752)", 10752)]
	[InlineData("termination reported by launchd (0, 0, 9)", 9)]
	[InlineData("termination reported by launchd (1, 0, 0)", -1)]
	[InlineData("termination reported by launchd (0, 1, 0)", -1)]
	[InlineData("Process exited voluntarily", null)]
	public void RequiresActualSuccessfulApplicationExit(string termination, int? status) =>
		Assert.Equal(status, AppLaunch.AppleExitStatus(
			$"[app<com.example.probe((null))>:123] {termination}", "com.example.probe", 123));

	[Fact]
	public void RejectsAnotherApplicationsOrPreviousRunsExit()
	{
		const string log = "[app<com.example.probe((null))>:123] termination reported by launchd (0, 0, 0)";
		Assert.Null(AppLaunch.AppleExitStatus(log, "com.example.other", 123));
		Assert.Null(AppLaunch.AppleExitStatus(log, "com.example.probe", 124));
	}

	public void Dispose() => Directory.Delete(_resultDir, recursive: true);
}
