using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Provides all XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] attribute for XamlInflator parameters.
/// Note: SourceGen is automatically excluded when running outside the MAUI repo (e.g., on Helix)
/// where the source generator DLL is not available.
/// </summary>
public class XamlInflatorDataAttribute : DataAttribute
{
	public override IEnumerable<object[]> GetData(MethodInfo testMethod)
	{
		var inflators = Enum.GetValues<XamlInflator>();
		
		// Exclude SourceGen if we can't run source generator tests (e.g., on Helix)
		if (!MockSourceGenerator.CanRunSourceGenTests())
		{
			inflators = inflators.Where(x => x != XamlInflator.SourceGen).ToArray();
		}
		
		return inflators.Select(x => new object[] { x });
	}
}

/// <summary>
/// Provides all pairs of XamlInflator enum values as test data for xUnit Theory tests.
/// This replaces NUnit's [Values] from, [Values] rd pattern for two XamlInflator parameters.
/// Note: SourceGen is automatically excluded when running outside the MAUI repo (e.g., on Helix)
/// where the source generator DLL is not available.
/// </summary>
public class XamlInflatorPairDataAttribute : DataAttribute
{
	public override IEnumerable<object[]> GetData(MethodInfo testMethod)
	{
		var inflators = Enum.GetValues<XamlInflator>();
		
		// Exclude SourceGen if we can't run source generator tests (e.g., on Helix)
		if (!MockSourceGenerator.CanRunSourceGenTests())
		{
			inflators = inflators.Where(x => x != XamlInflator.SourceGen).ToArray();
		}
		
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
	/// Note: SourceGen is automatically excluded when running outside the MAUI repo (e.g., on Helix)
	/// where the source generator DLL is not available.
	/// </summary>
	public static IEnumerable<object[]> Inflators
	{
		get
		{
			var inflators = Enum.GetValues<XamlInflator>();
		
			// Exclude SourceGen if we can't run source generator tests (e.g., on Helix)
			if (!MockSourceGenerator.CanRunSourceGenTests())
			{
				inflators = inflators.Where(x => x != XamlInflator.SourceGen).ToArray();
			}
			
			return inflators.Select(x => new object[] { x });
		}
	}

	/// <summary>
	/// Returns all pairs of XamlInflator enum values as test data.
	/// Useful for tests that need two inflator parameters like [Values] from, [Values] rd in NUnit.
	/// Note: SourceGen is automatically excluded when running outside the MAUI repo (e.g., on Helix)
	/// where the source generator DLL is not available.
	/// </summary>
	public static IEnumerable<object[]> InflatorPairs
	{
		get
		{
			var inflators = Enum.GetValues<XamlInflator>();
			
			// Exclude SourceGen if we can't run source generator tests (e.g., on Helix)
			if (!MockSourceGenerator.CanRunSourceGenTests())
			{
				inflators = inflators.Where(x => x != XamlInflator.SourceGen).ToArray();
			}
			
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
