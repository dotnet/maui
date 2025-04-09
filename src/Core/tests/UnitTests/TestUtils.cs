using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public sealed class CategoryAttribute : Attribute
	{
		public CategoryAttribute(params string[] categories)
		{
			Categories = categories;
		}

		public string[] Categories { get; }
	}

	/// <summary>
	/// Custom Fact attribute which defaults to using the test method name for the DisplayName property
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
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
	public class TheoryAttribute : Xunit.TheoryAttribute
	{
		public TheoryAttribute([CallerMemberName] string displayName = "")
		{
			base.DisplayName = displayName;
		}
	}
}