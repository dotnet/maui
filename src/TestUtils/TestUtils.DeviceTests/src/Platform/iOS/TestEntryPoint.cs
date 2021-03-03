using System;
using System.Collections.Generic;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;

namespace Microsoft.Maui.TestUtils
{
	public class TestEntryPoint : iOSApplicationEntryPoint
	{
		readonly ITestEntryPoint _testEntryPoint;

		public TestEntryPoint(ITestEntryPoint testEntryPoint)
		{
			_testEntryPoint = testEntryPoint;
		}

		protected override bool LogExcludedTests => true;

		protected override int? MaxParallelThreads => Environment.ProcessorCount;

		protected override IDevice Device { get; } = new TestDevice();

		protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
			=> _testEntryPoint.GetTestAssemblies();

		protected override void TerminateWithSuccess()
			=> _testEntryPoint.TerminateWithSuccess();

		protected override TestRunner GetTestRunner(LogWriter logWriter)
			=> _testEntryPoint.GetTestRunner(base.GetTestRunner(logWriter), logWriter);
	}
}