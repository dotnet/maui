using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class AssemblyRunInfo
	{
		public string AssemblyFileName { get; set; }

		public TestAssemblyConfiguration Configuration { get; set; }

		public IList<TestCaseViewModel> TestCases { get; set; }
	}
}