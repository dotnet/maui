using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.TestUtils.UnitTests;

public class NativeTestArgumentsTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void EmptyFilterPreservesFullSuite(string? filter) =>
		Assert.Empty(NativeTestArguments.GetFilterArguments(filter));

	[Fact]
	public void IncludesOneCategory() =>
		Assert.Equal(["--filter-trait", "Category=Button"], NativeTestArguments.GetFilterArguments("Category=Button"));

	[Fact]
	public void ExcludesEverySpecifiedCategory() =>
		Assert.Equal(
			["--filter-not-trait", "Category=Shell", "--filter-not-trait", "Category=CollectionView", "--filter-not-trait", "Category=HybridWebView"],
			NativeTestArguments.GetFilterArguments("SkipCategories=Shell, CollectionView;HybridWebView"));

	[Theory]
	[InlineData("Category=")]
	[InlineData("Category= ")]
	[InlineData("Category=Button=Other")]
	[InlineData("SkipCategories=")]
	[InlineData("SkipCategories=Shell,,CollectionView")]
	[InlineData("Unknown=Button")]
	public void RejectsInvalidFiltersRatherThanRunningEverything(string filter) =>
		Assert.Throws<ArgumentException>(() => NativeTestArguments.GetFilterArguments(filter));

	[Fact]
	public void NormalizesDistinctClassNamesWithoutGeneratingSource() =>
		Assert.Equal(
			["--filter-class", "Tests.One", "--filter-class", "Tests.Two"],
			NativeTestArguments.GetNameArguments("Tests.One; Tests.Two,Tests.One", "--filter-class"));

	[Fact]
	public void PreservesMethodSelection() =>
		Assert.Equal(["--filter-method", "Tests.One.Method"], NativeTestArguments.GetNameArguments("Tests.One.Method", "--filter-method"));

	[Fact]
	public void SelectsUnqualifiedMethodNamesWithinTheirClass() =>
		Assert.Equal(["--filter-method", "*.Method"], NativeTestArguments.GetNameArguments("Method", "--filter-method"));

	[Theory]
	[InlineData("Tests.One\nTests.Two")]
	[InlineData("Tests.One,,Tests.Two")]
	public void RejectsMalformedNames(string names) =>
		Assert.Throws<ArgumentException>(() => NativeTestArguments.GetNameArguments(names, "--filter-class"));

	[Fact]
	public void RejectsOverlongSelection() =>
		Assert.Throws<ArgumentException>(() => NativeTestArguments.GetNameArguments(new string('a', 32769), "--filter-method"));
}
