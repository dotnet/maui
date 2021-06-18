namespace Microsoft.Maui.TestUtils.WinUI
{
	public class TestRunnerWinUIApp<TStartup> : MauiWinUIApplication<TStartup>
		where TStartup : IStartup, new()
	{
	}
}
