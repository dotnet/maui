#nullable enable
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class AssemblyRunInfo
	{
		public AssemblyRunInfo(string assemblyFileName, TestAssemblyConfiguration configuration, IList<TestCaseViewModel> testCases)
		{
			AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
			Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			TestCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
		}

		public string AssemblyFileName { get; }

		public TestAssemblyConfiguration Configuration { get; }

		public IList<TestCaseViewModel> TestCases { get; }
	}
}