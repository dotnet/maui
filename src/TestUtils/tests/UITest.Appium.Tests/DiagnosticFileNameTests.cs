using UITest.Appium.NUnit;
using Xunit;

namespace UITest.Appium.Tests;

public class DiagnosticFileNameTests
{
	static readonly Guid Id = Guid.Parse("804952e2-8d76-4954-8bd0-9dae3c80beba");
	const string Machine = "sat12-jr317-2fcd4ff1-5bda-4b99-b73f-e20a3eaf56dd-E61FF9656ABA";
	const string RelativeResults = "test-results/nunit-bf022d836935420983bbde7fdfe873e4/retry";

	[Fact]
	public void ExistingNamesRemainUnchangedWhenTheyFit()
	{
		var name = DiagnosticFileName.Create("Sample", "Mac", "UITestBaseTearDown", "PageSource.txt", Id, 255);
		Assert.Equal($"Sample-Mac-UITestBaseTearDown-PageSource-{Id:N}.txt", name);
	}

	[Theory]
	[InlineData("drop-maccatalyst_ui_tests_coreclr-Shell1-1", RelativeResults, Machine, 286, 113)]
	[InlineData("drop-maccatalyst_ui_tests_coreclr-CollectionView1-1", RelativeResults, Machine, 295, 104)]
	public void BudgetIncludesTheActualTrxCopyDirectory(string artifact, string results, string machine, int directoryLength, int expected)
	{
		var maximum = DiagnosticFileName.GetMaximumLength(artifact, results, "", machine);
		Assert.Equal(expected, maximum);
		Assert.Equal(DiagnosticFileName.ArtifactPathLimit, directoryLength + maximum);
	}

	[Theory]
	[InlineData(286, "ShellBackgroundTransitionsFromGradientToSolidAndDefault", "PageSource.txt")]
	[InlineData(286, "ShellBackgroundTransitionsFromGradientToSolidAndDefault", "ScreenShot.png")]
	[InlineData(295, "SelectedItemSetDuringShellNavigationUpdatesVisualState", "PageSource.txt")]
	[InlineData(287, "StepperValueChangesWithInheritedRightToLeftFlowDirection", "ScreenShot.png")]
	public void ObservedAndNewNamesFitTheRealPublishedDirectory(int directoryLength, string test, string type)
	{
		var maximum = DiagnosticFileName.ArtifactPathLimit - directoryLength;
		var name = DiagnosticFileName.Create(test, "Mac", "UITestBaseTearDown", type, Id, maximum);
		Assert.True(directoryLength + name.Length <= DiagnosticFileName.ArtifactPathLimit, name);
		Assert.Contains(Path.GetFileNameWithoutExtension(type), name, StringComparison.Ordinal);
		Assert.Contains(Id.ToString("N"), name, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(399)]
	[InlineData(400)]
	public void ExactBoundaryIsPreservedAndOverflowIsShortened(int originalLength)
	{
		const int directoryLength = 286;
		var suffix = $"-Mac-UITestBaseTearDown-PageSource-{Id:N}.txt";
		var test = new string('T', originalLength - directoryLength - suffix.Length);
		var name = DiagnosticFileName.Create(test, "Mac", "UITestBaseTearDown", "PageSource.txt", Id, 113);
		Assert.True(directoryLength + name.Length <= 399);
		if (originalLength == 399)
			Assert.Equal(test + suffix, name);
	}

	[Fact]
	public void LongParameterizedNamesRetainTypeGuidAndDistinctTestIdentity()
	{
		var prefix = new string('T', 500);
		var first = DiagnosticFileName.Create(prefix + "(value: 1)", "Mac", "UITestBaseTearDown", "PageSource.txt", Id, 104);
		var second = DiagnosticFileName.Create(prefix + "(value: 2)", "Mac", "UITestBaseTearDown", "PageSource.txt", Id, 104);
		Assert.True(first.Length <= 104);
		Assert.True(second.Length <= 104);
		Assert.NotEqual(first, second);
		Assert.EndsWith($"-PageSource-{Id:N}.txt", first, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(20)]
	[InlineData(45)]
	public void InsufficientBudgetCannotProduceAnInvalidPath(int maximum)
	{
		Assert.Throws<InvalidOperationException>(() =>
			DiagnosticFileName.Create("Test", "Mac", "UITestBaseTearDown", "PageSource.txt", Id, maximum));
	}

	[Theory]
	[InlineData("../outside")]
	[InlineData("test-results/../../outside")]
	public void ResultsOutsideTheArtifactCannotPretendToFit(string relative)
	{
		Assert.Throws<ArgumentException>(() => DiagnosticFileName.GetMaximumLength("artifact", relative, "", Machine));
	}

	[Theory]
	[InlineData("../PageSource.txt")]
	[InlineData("Page/Source.txt")]
	[InlineData(".")]
	public void InvalidDiagnosticTypeCannotBecomeAValidLookingPath(string type)
	{
		Assert.Throws<ArgumentException>(() => DiagnosticFileName.Create("Test", "Mac", null, type, Id, 255));
	}

	[Fact]
	public void UnicodeNamesRespectCharacterAndFilesystemByteBudgets()
	{
		var test = string.Concat(Enumerable.Repeat("\U0001F9EA", 200));
		var name = DiagnosticFileName.Create(test, "Mac", null, "PageSource.txt", Id, 200);
		Assert.True(name.Length <= 200);
		Assert.True(System.Text.Encoding.UTF8.GetByteCount(name) <= 255);
		Assert.Equal(name, System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(name)));
		Assert.EndsWith($"-PageSource-{Id:N}.txt", name, StringComparison.Ordinal);
	}
}
