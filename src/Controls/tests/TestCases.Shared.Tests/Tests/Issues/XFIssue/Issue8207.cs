using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8207 : _IssuesUITest
{
	public Issue8207(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell System.ObjectDisposedException: 'Cannot access a disposed object. Object name: 'Android.Support.Design.Widget.BottomSheetDialog'.'";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutItemShouldShowTitle()
	//{
	//	TapInFlyout("Dashboard");
	//	Assert.Inconclusive("Flyout title should be visible");
	//}
}