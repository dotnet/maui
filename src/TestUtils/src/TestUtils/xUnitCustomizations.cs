using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Microsoft.Maui
{
	/// <summary>
	/// Adds categories to a test method or class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute, ITraitAttribute
	{
		readonly string[] _categories;

		public CategoryAttribute(params string[] categories) => _categories = categories;

		public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
			_categories.Select(category => new KeyValuePair<string, string>("Category", category)).ToArray();
	}

	/// <summary>
	/// Uses the test method's name as the default display name.
	/// </summary>
	public class FactAttribute : Xunit.FactAttribute
	{
		public FactAttribute([CallerMemberName] string displayName = "",
			[CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
			: base(sourceFilePath, sourceLineNumber) => DisplayName = displayName;
	}

	/// <summary>
	/// Uses the test method's name as the default display name.
	/// </summary>
	public class TheoryAttribute : Xunit.TheoryAttribute
	{
		public TheoryAttribute([CallerMemberName] string displayName = "",
			[CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
			: base(sourceFilePath, sourceLineNumber) => DisplayName = displayName;
	}
}
