using System.Collections.Generic;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public interface ITestEntryPoint
	{
		void TerminateWithSuccess();

		TestRunner GetTestRunner(TestRunner testRunner, LogWriter logWriter);

		IEnumerable<TestAssemblyInfo> GetTestAssemblies();
	}
}