using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Provides all XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] attribute for XamlInflator parameters.
/// </summary>
public class XamlInflatorDataAttribute : DataAttribute
{
	public override IEnumerable<object[]> GetData(MethodInfo testMethod) =>
		Enum.GetValues<XamlInflator>().Select(x => new object[] { x });
}

/// <summary>
/// Provides all pairs of XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] from, [Values] rd pattern for two XamlInflator parameters.
/// </summary>
public class XamlInflatorPairDataAttribute : DataAttribute
{
	public override IEnumerable<object[]> GetData(MethodInfo testMethod)
	{
		var inflators = Enum.GetValues<XamlInflator>();
		foreach (var from in inflators)
		{
			foreach (var rd in inflators)
			{
				yield return new object[] { from, rd };
			}
		}
	}
}

/// <summary>
/// Provides XamlInflator test data for MemberData attributes in xUnit tests.
/// </summary>
public static class XamlInflatorTestData
{
	/// <summary>
	/// Returns all XamlInflator enum values as test data.
	/// </summary>
	public static IEnumerable<object[]> Inflators =>
		Enum.GetValues<XamlInflator>().Select(x => new object[] { x });

	/// <summary>
	/// Returns all pairs of XamlInflator enum values as test data.
	/// Useful for tests that need two inflator parameters like [Values] from, [Values] rd in NUnit.
	/// </summary>
	public static IEnumerable<object[]> InflatorPairs
	{
		get
		{
			var inflators = Enum.GetValues<XamlInflator>();
			foreach (var from in inflators)
			{
				foreach (var rd in inflators)
				{
					yield return new object[] { from, rd };
				}
			}
		}
	}
}
