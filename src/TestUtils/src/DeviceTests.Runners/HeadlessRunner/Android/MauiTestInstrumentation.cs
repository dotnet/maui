#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

public abstract class MauiTestInstrumentation : Instrumentation
{
	readonly TaskCompletionSource<Application> _applicationReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

	protected MauiTestInstrumentation(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) => Current = this;

	public static MauiTestInstrumentation Current { get; private set; } = null!;
	public Bundle? Arguments { get; private set; }
	public IServiceProvider Services { get; private set; } = null!;
	public Context? CurrentExecutionContext { get; internal set; }

	public override void OnCreate(Bundle? arguments)
	{
		Arguments = arguments;
		base.OnCreate(arguments);
		Start();
	}

	public override void CallApplicationOnCreate(Application? app)
	{
		base.CallApplicationOnCreate(app);
		if (app is null)
			_applicationReady.TrySetException(new InvalidOperationException("The Android test application was not created."));
		else
			_applicationReady.TrySetResult(app);
	}

	public override async void OnStart()
	{
		base.OnStart();
		var bundle = new Bundle();
		try
		{
			await _applicationReady.Task;
			Services = IPlatformApplication.Current?.Services
				?? throw new InvalidOperationException("The Android test application did not provide services.");

			NativeTestResult result;
			if (Services.GetRequiredService<HeadlessRunnerOptions>().RequiresUIContext)
			{
				var intent = new Intent();
				intent.SetClassName(TargetContext!, TargetContext!.PackageName + ".TestActivity");
				intent.AddFlags(ActivityFlags.NewTask);
				var activity = StartActivitySync(intent) as MauiTestActivity
					?? throw new InvalidOperationException("The Android test activity did not start.");
				CurrentExecutionContext = activity;
				result = await activity.Completion.Task;
			}
			else
			{
				CurrentExecutionContext = TargetContext;
				result = await RunTestsAsync();
			}

			bundle.PutInt("passed", result.Passed);
			bundle.PutInt("failed", result.Failed);
			bundle.PutInt("skipped", result.Skipped);
			bundle.PutString("resultsPath", result.ResultsPath);
			if (result.ExitCode != 0 && result.Failed == 0)
				bundle.PutString("error", $"The test runner exited with code {result.ExitCode}.");
			Finish(Result.Ok, bundle);
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine(exception);
			bundle.PutString("error", exception.ToString());
			Finish(Result.Canceled, bundle);
		}
	}

	internal Task<NativeTestResult> RunTestsAsync()
	{
		var directory = Application.Context.GetExternalFilesDir(null)?.AbsolutePath
			?? throw new InvalidOperationException("Android did not provide a readable test results directory.");
		return Services.GetRequiredService<HeadlessTestRunner>().RunAsync(
			Path.Combine(directory, "TestResults"), []);
	}
}
