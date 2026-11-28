namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class AndroidLaunchTests
{
	const string Marker = "MAUI_APP_COMPLETED_current";

	[Theory]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\nINSTRUMENTATION_CODE: -1", true)]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\r\nINSTRUMENTATION_CODE: -1\r\n", true)]
	[InlineData(1, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\nINSTRUMENTATION_CODE: -1", false)]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_previous\nINSTRUMENTATION_CODE: -1", false)]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\nINSTRUMENTATION_CODE: 0", false)]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current", false)]
	[InlineData(0, "INSTRUMENTATION_CODE: -1", false)]
	[InlineData(0, "INSTRUMENTATION_FAILED: Process crashed.", false)]
	[InlineData(0, "INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\nINSTRUMENTATION_CODE: -1\nINSTRUMENTATION_FAILED: crash", false)]
	[InlineData(0, "Expected INSTRUMENTATION_RESULT: completion-marker=MAUI_APP_COMPLETED_current\nINSTRUMENTATION_CODE: -1", false)]
	public void RequiresSuccessfulInstrumentationAndUniqueCompletion(int exitCode, string log, bool expected) =>
		Assert.Equal(expected, AppLaunch.AndroidRunCompleted(exitCode, log, Marker));
}
