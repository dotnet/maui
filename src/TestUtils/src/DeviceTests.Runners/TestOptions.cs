#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class TestOptions
	{
		public List<Assembly> Assemblies { get; set; } = new List<Assembly>();
	}
}