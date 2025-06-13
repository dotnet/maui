using System;
using Xunit.Sdk;

namespace Microsoft.Maui.IntegrationTests;

/// <summary>
/// Constants for XUnit trait categories
/// </summary>
static class Categories
{
	/// <summary>
	/// The name of the trait key for categorizing tests
	/// </summary>
	public const string TraitKey = "Category";

	// this is a special job that runs on the samples
	public const string Samples = nameof(Samples);

	// these are special run on "device" jobs
	public const string RunOnAndroid = nameof(RunOnAndroid);
	public const string RunOniOS = nameof(RunOniOS);

	// these are normal jobs
	public const string WindowsTemplates = nameof(WindowsTemplates);
	public const string macOSTemplates = nameof(macOSTemplates);
	public const string Build = nameof(Build);
	public const string Blazor = nameof(Blazor);
	public const string MultiProject = nameof(MultiProject);
	public const string AOT = nameof(AOT);
}

/// <summary>
/// Helper class to make XUnit trait attributes more NUnit-like in usage
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class CategoryAttribute : Attribute, ITraitAttribute
{
	public CategoryAttribute(string category)
	{
		Category = category;
	}

	public string Category { get; }
}
