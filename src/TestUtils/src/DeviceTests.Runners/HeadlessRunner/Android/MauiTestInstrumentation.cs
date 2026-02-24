#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public abstract class MauiTestInstrumentation : Instrumentation
	{
		readonly TaskCompletionSource<Application> _waitForApplication = new();
		Java.Lang.Class _activityClass = null!;

		protected MauiTestInstrumentation(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
			Current = this;
		}

		public static MauiTestInstrumentation Current { get; private set; } = null!;

		public Bundle? Arguments { get; private set; }

		public IServiceProvider Services { get; private set; } = null!;

		public TestOptions Options { get; private set; } = null!;

		public HeadlessRunnerOptions RunnerOptions { get; private set; } = null!;

		public Context? CurrentExecutionContext { get; private set; }

		public override void OnCreate(Bundle? arguments)
		{
			_activityClass = Java.Lang.Class.ForName(Context!.PackageName + ".TestActivity");
			Arguments = arguments;

			base.OnCreate(arguments);

			Start();
		}

		public override void CallApplicationOnCreate(Application? app)
		{
			base.CallApplicationOnCreate(app);

			if (app == null)
				_waitForApplication.SetException(new ArgumentNullException(nameof(app)));
			else
				_waitForApplication.SetResult(app);
		}

		public override async void OnStart()
		{
			base.OnStart();

			await _waitForApplication.Task;

			Services = IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("Unable to find Application Services");
			Options = Services.GetRequiredService<TestOptions>();
			RunnerOptions = Services.GetRequiredService<HeadlessRunnerOptions>();

			var resultsFilename = Arguments?.GetString("results-file-name");
			if (!string.IsNullOrWhiteSpace(resultsFilename))
				RunnerOptions.TestResultsFilename = resultsFilename;

			var bundle = await RunTestsAsync();

			CopyFile(bundle);

			Finish(Result.Ok, bundle);
		}

		void CopyFile(Bundle bundle)
		{
			var resultsFile = bundle.GetString("test-results-path");
			if (resultsFile == null)
				return;

			var guid = Guid.NewGuid().ToString("N");
			var name = Path.GetFileName(resultsFile);

			string finalPath;
			if (!OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath!;
				var dir = Path.Combine(root, guid);

				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				finalPath = Path.Combine(dir, name);
				File.Copy(resultsFile, finalPath, true);
			}
			else
			{
				var downloads = global::Android.OS.Environment.DirectoryDownloads!;
				var relative = Path.Combine(downloads, Context!.PackageName!, guid);

				var values = new ContentValues();
				values.Put(MediaStore.IMediaColumns.DisplayName, name);
				values.Put(MediaStore.IMediaColumns.MimeType, "text/xml");
				values.Put(MediaStore.IMediaColumns.RelativePath, relative);

				var resolver = Context!.ContentResolver!;
				var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)!;
				using (var dest = resolver.OpenOutputStream(uri)!)
				using (var source = File.OpenRead(resultsFile))
					source.CopyTo(dest);

#pragma warning disable CS0618 // Type or member is obsolete
				var root = global::Android.OS.Environment.ExternalStorageDirectory!.AbsolutePath;
#pragma warning restore CS0618 // Type or member is obsolete
				finalPath = Path.Combine(root, relative, name);
			}

			bundle.PutString("test-results-path", finalPath);
		}

		Task<Bundle> RunTestsAsync()
		{
			if (RunnerOptions.RequiresUIContext)
			{
				var intent = new Intent(TargetContext, _activityClass);
				intent.AddFlags(ActivityFlags.NewTask);

				var activity = StartActivitySync(intent);
				if (activity is not MauiTestActivity testActivity)
					throw new InvalidOperationException($"Unexpected activity type '{activity?.GetType().FullName ?? "<null>"}'.");

				CurrentExecutionContext = activity;

				return testActivity.TaskCompletionSource.Task;
			}
			else
			{
				CurrentExecutionContext = TargetContext;

				var runner = Services.GetRequiredService<HeadlessTestRunner>();

				return runner.RunTestsAsync();
			}
		}
	}
}