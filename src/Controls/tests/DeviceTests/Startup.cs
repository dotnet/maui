using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if __ANDROID__
		public static Android.Content.Context CurrentContext { get; private set; }
#endif

		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.ConfigureLifecycleEvents(life =>
				{
#if __ANDROID__
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) => CurrentContext = a);
					});
#endif
				})
				.ConfigureTests(new TestOptions
				{
					Assemblies =
					{
						typeof(MauiProgram).Assembly
					},
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				})
				.UseVisualRunner()
				.Build();
	}
}