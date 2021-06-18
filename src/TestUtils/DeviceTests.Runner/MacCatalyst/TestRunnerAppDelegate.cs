namespace Microsoft.Maui.TestUtils
{
	public abstract class TestRunnerAppDelegate<TStartup> : MauiUIApplicationDelegate<TStartup>
		where TStartup : IStartup, new()
	{
	}
}