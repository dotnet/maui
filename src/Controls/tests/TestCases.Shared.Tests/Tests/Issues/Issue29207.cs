#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // CollectionView Fails to Keep Last Item in View on iOS and macOS https://github.com/dotnet/maui/issues/18029
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29207 : _IssuesUITest
{
	public Issue29207(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "KeepLastItemInView Does Not Scroll to Last Item When Adding Items at Top, Instead Scrolls to SecondLast Item";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ScrollToLastItem()
	{
		App.WaitForElement("InsertItemButton");
		App.Tap("InsertItemButton");
		VerifyScreenshot();
	}
}
#endif