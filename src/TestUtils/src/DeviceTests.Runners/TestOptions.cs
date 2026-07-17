#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class TestOptions
	{
		/// <summary>
		/// The list of assemblies that contain tests.
		/// </summary>
		public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

		/// <summary>
		/// The list of categories to skip in the form:
		///   [category-name]=[skip-when-value]
		/// </summary>
		public List<string> SkipCategories { get; set; } = new List<string>();

		/// <summary>
		/// The list of fully-qualified test class names to run exclusively (include filter).
		/// When non-empty, only tests declared in these classes are executed. This is an
		/// additive, opt-in narrowing used by the Copilot review gate to verify a PR's
		/// specific test class without running (and being blocked by unrelated crashes in)
		/// the rest of its category. Empty by default, so normal runs are unaffected.
		/// </summary>
		public List<string> IncludeClassNames { get; set; } = new List<string>();
	}
}