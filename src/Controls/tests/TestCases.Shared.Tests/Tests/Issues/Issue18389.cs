using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18389 : _IssuesUITest
{
	public Issue18389(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Incorrect scroll position when scrolling to an item with header in CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToIndexWithHeader()
	{
		App.WaitForElement("Issue18389_ScrollToBtn");
		App.Tap("Issue18389_ScrollToBtn");

		App.WaitForElement("Item 20");

		App.WaitForElement("Issue18389_ResetBtn");
		App.Tap("Issue18389_ResetBtn");

		App.WaitForElement("Item 1");

		App.WaitForElement("Issue18389_RemoveHeaderBtn");
		App.Tap("Issue18389_RemoveHeaderBtn");

		App.Tap("Issue18389_ScrollToBtn");
		App.WaitForElement("Item 20");
	}
}