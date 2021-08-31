#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	interface ITestRunner
	{
		Task<IReadOnlyList<TestAssemblyViewModel>> DiscoverAsync();

		Task RunAsync(TestCaseViewModel test);

		Task RunAsync(IEnumerable<TestCaseViewModel> tests, string? message = null);

		Task RunAsync(IReadOnlyList<AssemblyRunInfo> runInfos, string? message = null);

		event Action<string> OnDiagnosticMessage;
	}
}