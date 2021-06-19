using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public abstract class MauiTestInstrumentation<TStartup, TActivity> : MauiTestInstrumentation
		where TStartup : IStartup, new()
		where TActivity : MauiTestAppCompatActivity
	{
		protected MauiTestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		protected override IAppHost OnBuildAppHost()
		{
			var startup = new TStartup();

			return startup
				.CreateAppHostBuilder()
				.ConfigureServices(svc => svc.AddSingleton(new AndroidRunnerOptions(typeof(TActivity))))
				.ConfigureUsing(startup)
				.Build();
		}
	}

	public abstract class MauiTestInstrumentation : Instrumentation
	{
		protected MauiTestInstrumentation(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
			Current = this;
		}

		public static MauiTestInstrumentation Current { get; private set; } = null!;

		public Bundle Arguments { get; private set; }

		public IServiceProvider Services { get; private set; }

		public TestOptions Options { get; private set; }

		public HeadlessRunnerOptions RunnerOptions { get; private set; }

		public override void OnCreate(Bundle arguments)
		{
			base.OnCreate(arguments);

			Arguments = arguments;

			var host = OnBuildAppHost();

			Services = host.Services;
			Options = Services.GetRequiredService<TestOptions>();
			RunnerOptions = Services.GetRequiredService<HeadlessRunnerOptions>();

			var resultsFilename = arguments?.GetString("results-file-name");
			if (!string.IsNullOrWhiteSpace(resultsFilename))
				RunnerOptions.TestResultsFilename = resultsFilename;

			Start();
		}

		public override async void OnStart()
		{
			base.OnStart();

			var bundle = await RunTestsAsync();

			Finish(Result.Ok, bundle);
		}

		protected abstract IAppHost OnBuildAppHost();

		Task<Bundle> RunTestsAsync()
		{
			if (RunnerOptions.RequiresUIContext)
			{
				var androidOptions = Services.GetRequiredService<AndroidRunnerOptions>();

				var intent = new Android.Content.Intent(TargetContext, androidOptions.ActivityType);
				intent.AddFlags(Android.Content.ActivityFlags.NewTask);

				var activity = StartActivitySync(intent);
				if (activity is not MauiTestAppCompatActivity testActivity)
					throw new InvalidOperationException($"Unexpected activity type '{activity.GetType().FullName}'.");

				return testActivity.TaskCompletionSource.Task;
			}
			else
			{
				var runner = Services.GetRequiredService<HeadlessTestRunner>();

				return runner.RunTestsAsync();
			}
		}
	}
}