using Microsoft.Maui.DeviceTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.TestUtils.UnitTests;

public class DeviceTestAttributeTests
{
	[Fact]
	public void CategoriesExposeAllTraitValues() =>
		Assert.Equal(
			[new KeyValuePair<string, string>("Category", "First"), new KeyValuePair<string, string>("Category", "Second")],
			new CategoryAttribute("First", "Second").GetTraits());

	[Fact]
	[Category("Discovery")]
	public void CategoryPrefixIsPresentDuringRealExecution() =>
		Assert.Equal("[Discovery] CategoryPrefixIsPresentDuringRealExecution", TestContext.Current.TestCase!.TestCaseDisplayName);

	[Theory]
	[Category("Discovery")]
	[InlineData(1)]
	[InlineData(2)]
	public void TheoryRowsKeepTheirArguments(int value)
	{
		Assert.StartsWith("[Discovery] TheoryRowsKeepTheirArguments", TestContext.Current.Test!.TestDisplayName, StringComparison.Ordinal);
		Assert.Contains($"value: {value}", TestContext.Current.Test.TestDisplayName, StringComparison.Ordinal);
	}

	[Fact]
	public async ValueTask RepeatProducesExactlyTheRequestedDistinctRows()
	{
		await using var tracker = new DisposalTracker();
		var method = typeof(DeviceTestAttributeTests).GetMethod(nameof(RepeatProducesExactlyTheRequestedDistinctRows))!;
		var rows = await new RepeatAttribute(3).GetData(method, tracker);
		Assert.Equal([1, 2, 3], rows.Select(row => (int)row.GetData()[0]!));
	}

	[Fact]
	public void VersionSkipDoesNotSkipOtherPlatforms() =>
		Assert.False(new SkipOnIOSVersionAttribute(1).ShouldSkip());
}
