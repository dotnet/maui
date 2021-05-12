using System.Collections.Generic;
using System.Reflection;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.TestUtils;

namespace Microsoft.Maui.DeviceTests
{
	[Register(nameof(TestApplicationDelegate))]
	public class TestApplicationDelegate : BaseTestApplicationDelegate
	{
		public override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
		{
			yield return new TestAssemblyInfo(typeof(TestApplicationDelegate).Assembly, typeof(TestApplicationDelegate).Assembly.Location);
			yield return new TestAssemblyInfo(typeof(CompatTests).Assembly, typeof(CompatTests).Assembly.Location);
			yield return new TestAssemblyInfo(typeof(CoreTests).Assembly, typeof(CoreTests).Assembly.Location);
		}
	}
}