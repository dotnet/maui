using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.TestCases.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ShardedTestCategoryAttribute : Attribute, IApplyToTest
{
	readonly string _category;
	readonly int _shard;

	public ShardedTestCategoryAttribute(string category, int shard = 1)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(category);
		ArgumentOutOfRangeException.ThrowIfLessThan(shard, 1);

		_category = category;
		_shard = shard;
	}

	public void ApplyToTest(NUnit.Framework.Internal.Test test)
	{
		test.Properties.Add(PropertyNames.Category, _category);
		test.Properties.Add(PropertyNames.Category, $"{_category}{_shard}");
	}
}
