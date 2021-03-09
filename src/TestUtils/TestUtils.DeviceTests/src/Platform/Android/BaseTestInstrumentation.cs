using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace Microsoft.Maui.TestUtils
{
	public abstract class BaseTestInstrumentation<T> : BaseTestInstrumentation
		where T : BaseTestActivity
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

	public abstract class BaseTestInstrumentation : Instrumentation, ITestEntryPoint
	{
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
			return TestEntryPoint.RunTestsAsync(this, Arguments);
		}

		// ITestEntryPoint

		public virtual string TestResultsFilename => TestEntryPoint.DefaultTestResultsFilename;

		public virtual IEnumerable<TestAssemblyInfo> GetTestAssemblies()
		{
			throw new NotImplementedException();
		}

		public virtual TestRunner GetTestRunner(TestRunner testRunner, LogWriter logWriter)
		{
			return testRunner;
		}

		public virtual void TerminateWithSuccess()
		{
		}
	}
}