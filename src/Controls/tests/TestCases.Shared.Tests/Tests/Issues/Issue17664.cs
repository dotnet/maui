#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Android fix: https://github.com/dotnet/maui/pull/31437// Windows: The Scrolled event is not consistently triggered in the CI environment during automated
// scrolling, so the label text is never updated. This is a test infrastructure limitation on Windows;
// the fix itself targets iOS and MacCatalyst. MacCatalyst test coverage is temporarily excluded above.
//https://github.com/dotnet/maui/issues/39956 fails on Mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17664 : _IssuesUITest
{
	public Issue17664(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Incorrect ItemsViewScrolledEventArgs in CollectionView when IsGrouped is set to true";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyGroupedCollectionViewVisibleItemIndices()
	{
		App.WaitForElement("Issue17664ScrollBtn");
		App.Tap("Issue17664ScrollBtn");

		var resultItem = App.WaitForElement("Issue17664DescriptionLabel").GetText();
		Assert.That(resultItem, Is.EqualTo("Category C item #2"));
	}
}
#endif
