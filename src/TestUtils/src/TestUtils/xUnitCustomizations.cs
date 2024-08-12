using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Maui
{
	/// <summary>
	/// This type provides the assembly name for the xUnit attributes specified in
	/// the linked TestShared\xUnitSharedAttributes.cs file.
	/// </summary>
	internal static class XUnitTypeData
	{
		internal const string XUnitAttributeAssembly = "Microsoft.Maui.TestUtils";
	}

	public class CategoryDiscoverer : ITraitDiscoverer
	{
		public CategoryDiscoverer(IMessageSink diagnosticMessageSink)
		{
		}

		public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
		{
			var args = traitAttribute.GetConstructorArguments().ToList();

			if (args[0] is string[] categories)
			{
				foreach (var category in categories)
				{
					yield return new KeyValuePair<string, string>("Category", category.ToString());
				}
			}
		}
	}

	/// <summary>
	/// Convenience attribute for setting a Category trait on a test or test class
	/// </summary>
	[TraitDiscoverer("Microsoft.Maui.CategoryDiscoverer", "Microsoft.Maui.TestUtils")]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute, ITraitAttribute
	{
		// Yes, it looks like the cateory parameter is not used; CategoryDiscoverer uses it. 
		public CategoryAttribute(params string[] categories) { }
	}

	/// <summary>
	/// Custom Fact attribute which defaults to using the test method name for the DisplayName property
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
	public class FactAttribute : Xunit.FactAttribute
	{
		public FactAttribute([CallerMemberName] string displayName = "")
		{
			base.DisplayName = displayName;
		}
	}

	/// <summary>
	/// Custom Theory attribute which defaults to using the test method name for the DisplayName property
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Xunit.Sdk.TheoryDiscoverer", "xunit.execution.{Platform}")]
	public class TheoryAttribute : Xunit.TheoryAttribute
	{
		public TheoryAttribute([CallerMemberName] string displayName = "")
		{
			base.DisplayName = displayName;
		}
	}
}