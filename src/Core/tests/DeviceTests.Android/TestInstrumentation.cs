using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Xamarin.Essentials;

namespace Microsoft.Maui.DeviceTests
{
	[Activity(
		Name = "com.microsoft.maui.devicetests.TestActivity",
		Label = "@string/app_name",
		Theme = "@style/MainTheme",
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class TestActivity : BaseTestActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Platform.Init(this);

			base.OnCreate(savedInstanceState);
		}
	}

	[Instrumentation(Name = "com.microsoft.maui.devicetests.TestInstrumentation")]
	public class TestInstrumentation : BaseTestInstrumentation<TestActivity>
	{
		protected TestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
	}

	// TODO: move the bits below into a shared project for Essentials

	public abstract class BaseTestActivity : AppCompatActivity
	{
		public const string ArgumentsBundleKey = "arguments-bundle";

		public TaskCompletionSource<Bundle> TaskCompletionSource { get; } = new TaskCompletionSource<Bundle>();

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var arguments = Intent?.GetBundleExtra(ArgumentsBundleKey);

			try
			{
				Bundle bundle = await BaseTestInstrumentation.RunTestsAsync(arguments);

				TaskCompletionSource.TrySetResult(bundle);
			}
			catch (Exception ex)
			{
				TaskCompletionSource.TrySetException(ex);
			}

			Finish();
		}
	}

	public abstract class BaseTestInstrumentation<T> : BaseTestInstrumentation
		where T : TestActivity
	{
		protected BaseTestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		protected override async Task<Bundle> OnRunTestsAsync()
		{
			var intent = new Android.Content.Intent(TargetContext, typeof(T));
			intent.PutExtra(BaseTestActivity.ArgumentsBundleKey, Arguments);
			intent.AddFlags(Android.Content.ActivityFlags.NewTask);

			var activity = StartActivitySync(intent);
			if (activity is T testActivity)
				return await testActivity.TaskCompletionSource.Task;

			throw new InvalidOperationException($"Unexpected activity type '{typeof(T).FullName}'.");
		}
	}

	public abstract class BaseTestInstrumentation : Instrumentation
	{
		public const string DefaultTestResultsFilename = "TestResults.xml";

		public Bundle Arguments { get; private set; }

		protected BaseTestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public override void OnCreate(Bundle arguments)
		{
			base.OnCreate(arguments);

			Arguments = arguments;

			Start();
		}

		public override async void OnStart()
		{
			base.OnStart();

			var bundle = await OnRunTestsAsync();

			Finish(Result.Ok, bundle);
		}

		protected virtual Task<Bundle> OnRunTestsAsync()
		{
			return RunTestsAsync(Arguments);
		}

		public static async Task<Bundle> RunTestsAsync(Bundle arguments = null)
		{
			var resultsFileName = arguments?.GetString("results-file-name", DefaultTestResultsFilename) ?? DefaultTestResultsFilename;

			var bundle = new Bundle();

			var entryPoint = new TestsEntryPoint(resultsFileName);
			entryPoint.TestsCompleted += (sender, results) =>
			{
				var message =
					$"Tests run: {results.ExecutedTests} " +
					$"Passed: {results.PassedTests} " +
					$"Inconclusive: {results.InconclusiveTests} " +
					$"Failed: {results.FailedTests} " +
					$"Ignored: {results.SkippedTests}";
				bundle.PutString("test-execution-summary", message);

				bundle.PutLong("return-code", results.FailedTests == 0 ? 0 : 1);
			};

			await entryPoint.RunAsync();

			if (File.Exists(entryPoint.TestsResultsFinalPath))
				bundle.PutString("test-results-path", entryPoint.TestsResultsFinalPath);

			if (bundle.GetLong("return-code", -1) == -1)
				bundle.PutLong("return-code", 1);

			return bundle;
		}

		class TestsEntryPoint : AndroidApplicationEntryPoint
		{
			readonly string resultsPath;

			public TestsEntryPoint(string resultsFileName)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var root = ((int)Build.VERSION.SdkInt) >= 30
					? global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
					: Application.Context.GetExternalFilesDir(null)?.AbsolutePath ?? FileSystem.AppDataDirectory;
#pragma warning restore CS0618 // Type or member is obsolete

				var docsDir = Path.Combine(root, "Documents");

				if (!Directory.Exists(docsDir))
					Directory.CreateDirectory(docsDir);

				resultsPath = Path.Combine(docsDir, resultsFileName);
			}

			protected override bool LogExcludedTests => true;

			public override TextWriter Logger => null;

			public override string TestsResultsFinalPath => resultsPath;

			protected override int? MaxParallelThreads => System.Environment.ProcessorCount;

			protected override IDevice Device { get; } = new TestDevice();

			protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
			{
				yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
				yield return new TestAssemblyInfo(typeof(SliderHandlerTests).Assembly, typeof(SliderHandlerTests).Assembly.Location);
			}

			protected override void TerminateWithSuccess()
			{
			}

			protected override TestRunner GetTestRunner(LogWriter logWriter)
			{
				var testRunner = base.GetTestRunner(logWriter);
				return testRunner;
			}
		}

		class TestDevice : IDevice
		{
			public string BundleIdentifier => AppInfo.PackageName;

			public string UniqueIdentifier => Guid.NewGuid().ToString("N");

			public string Name => DeviceInfo.Name;

			public string Model => DeviceInfo.Model;

			public string SystemName => DeviceInfo.Platform.ToString();

			public string SystemVersion => DeviceInfo.VersionString;

			public string Locale => CultureInfo.CurrentCulture.Name;
		}
	}
}
