#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class TestOptions
	{
		/// <summary>
		/// The executable assembly containing this application's tests.
		/// </summary>
		public Assembly TestAssembly { get; set; } = null!;

		/// <summary>
		/// The list of categories to skip in the form:
		///   [category-name]=[skip-when-value]
		/// </summary>
		public List<string> SkipCategories { get; set; } = new List<string>();
	}
}