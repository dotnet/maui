using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using AndroidX.AppCompat.App;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace Microsoft.Maui.TestUtils
{
	public abstract class BaseTestActivity : AppCompatActivity, ITestEntryPoint
	{
		public const string ArgumentsBundleKey = "arguments-bundle";

		public TaskCompletionSource<Bundle> TaskCompletionSource { get; } = new TaskCompletionSource<Bundle>();

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var arguments = Intent?.GetBundleExtra(ArgumentsBundleKey);

			try
			{
				Bundle bundle = await TestEntryPoint.RunTestsAsync(this, arguments);

				TaskCompletionSource.TrySetResult(bundle);
			}
			catch (Exception ex)
			{
				TaskCompletionSource.TrySetException(ex);
			}

			Finish();
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