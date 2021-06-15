using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.TestUtils;

namespace Microsoft.Maui.DeviceTests
{
	[Register(nameof(TestApplicationDelegate))]
	public class TestApplicationDelegate : BaseTestApplicationDelegate
	{
		public override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
			TestApplicationDelegate.TestAssemblies
				.Distinct()
				.Select(a => new TestAssemblyInfo(a, a.Location));

		public static IEnumerable<Assembly> TestAssemblies
		{
			get
			{
				yield return Assembly.GetExecutingAssembly();
				yield return typeof(SliderHandlerTests).Assembly;
			}
		}
	}
}