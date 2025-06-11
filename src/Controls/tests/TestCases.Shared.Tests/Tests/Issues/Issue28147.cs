using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28147 : _IssuesUITest
{
	public Issue28147(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Label LinebreakMode (TailTruncation) for FormattedText does't work in CollectionView after scroll";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void LineBreakModeShouldWorkInCollectionViewAfterScrolling()
	{
		App.WaitForElement("CollectionView");
		App.ScrollDown("CollectionView", ScrollStrategy.Gesture, 0.80);
		VerifyScreenshot();
	}
}