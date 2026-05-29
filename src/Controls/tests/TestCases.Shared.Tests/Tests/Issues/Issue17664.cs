#if TEST_FAILS_ON_WINDOWS // Windows: The Scrolled event is not consistently triggered in the CI environment during automated scrolling, so the label text is not updated. This is a known limitation of the test infrastructure on Windows.
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupedCollectionViewVisibleItemIndices()
	{
		App.WaitForElement("Issue17664ScrollBtn");
		App.Tap("Issue17664ScrollBtn");

		var resultItem = App.WaitForElement("Issue17664DescriptionLabel").GetText();
		Assert.That(resultItem, Is.EqualTo("Category C item #2"));
	}
}
#endif
