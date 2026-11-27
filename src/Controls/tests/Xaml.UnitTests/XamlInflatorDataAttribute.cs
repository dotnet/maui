using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Provides all XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] attribute for XamlInflator parameters.
/// </summary>
public class XamlInflatorDataAttribute : DataAttribute
{
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
	{
		return new(Enum.GetValues<XamlInflator>().Select(x => (ITheoryDataRow)new TheoryDataRow<XamlInflator>(x)).ToArray());
	}

	public override bool SupportsDiscoveryEnumeration() => true;
}

/// <summary>
/// Provides all pairs of XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] from, [Values] rd pattern for two XamlInflator parameters.
/// </summary>
public class XamlInflatorPairDataAttribute : DataAttribute
{
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
	{
		var inflators = Enum.GetValues<XamlInflator>();
		var rows = new List<ITheoryDataRow>();
		foreach (var from in inflators)
		{
			foreach (var rd in inflators)
			{
				rows.Add(new TheoryDataRow<XamlInflator, XamlInflator>(from, rd));
			}
		}
		return new(rows);
	}

	public override bool SupportsDiscoveryEnumeration() => true;
}

/// <summary>
/// Provides XamlInflator test data for MemberData attributes in xUnit tests.
/// </summary>
public static class XamlInflatorTestData
{
	/// <summary>
	/// Returns all XamlInflator enum values as test data.
	/// </summary>
	public static IEnumerable<object[]> Inflators
	{
		get
		{
			return Enum.GetValues<XamlInflator>().Select(x => new object[] { x });
		}
	}

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
