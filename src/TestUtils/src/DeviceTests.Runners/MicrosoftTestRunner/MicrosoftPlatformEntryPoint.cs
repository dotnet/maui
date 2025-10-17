using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

#if ANDROID
using Android.OS;
#endif

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;

  internal class MicrosoftPlatformEntryPoint : ApplicationEntryPoint
{
	protected override int? MaxParallelThreads => int.MaxValue;

	protected override IDevice Device => new TestDevice();

	protected override bool IsXunit => false;

	public override async Task RunAsync()
	{
		var runner = await InternalRunAsync(new LogWriter(Device));
	}

	protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
	{
		return Enumerable.Empty<TestAssemblyInfo>();
	}

	protected override TestRunner GetTestRunner(LogWriter logWriter)
	{
		return new MicrosoftPlatformTestRunner(logWriter);
	}

	protected override void TerminateWithSuccess()
	{

	}

#if ANDROID
	public async Task<Bundle> RunTestsAsync()
	{
		var bundle = new Bundle();

		TestsCompleted += OnTestsCompleted;

		await RunAsync();

		TestsCompleted -= OnTestsCompleted;

		if (bundle.GetLong("return-code", -1) == -1)
			bundle.PutLong("return-code", 1);

		return bundle;

		void OnTestsCompleted(object sender, TestRunResult results)
		{
			var message =
				$"Tests run: {results.ExecutedTests} " +
				$"Passed: {results.PassedTests} " +
				$"Inconclusive: {results.InconclusiveTests} " +
				$"Failed: {results.FailedTests} " +
				$"Ignored: {results.SkippedTests}";

			bundle.PutString("test-execution-summary", message);

			bundle.PutLong("return-code", results.FailedTests == 0 ? 0 : 1);
		}
	}
#endif	
}